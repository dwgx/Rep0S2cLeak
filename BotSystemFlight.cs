using System;
using System.Collections.Generic;
using UnityEngine;

public class BotSystemFlight : MonoBehaviour
{
	[Header("Flight Configuration")]
	public FlightSettings flightSettings = new FlightSettings();

	[Header("Physics References")]
	public Rigidbody targetRigidbody;

	public BotPhysicsController physicsController;

	public Transform flightTransform;

	private SpringVector3 positionSpring = new SpringVector3();

	private SpringQuaternion rotationSpring = new SpringQuaternion();

	private SpringFloat stabilizationSpring = new SpringFloat();

	private Vector3 currentTarget = Vector3.zero;

	private Vector3 currentLookDirection = Vector3.forward;

	private bool isInitialized;

	private Vector3 spawnPoint;

	private float currentStabilizationForce;

	private float stuckTimer;

	private Vector3 prevStuckPosition;

	private Vector3 currentStuckPosition;

	public EnemyRigidbody enemyRigidbody;

	private void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (!isInitialized)
		{
			if (!targetRigidbody)
			{
				targetRigidbody = GetComponent<Rigidbody>();
			}
			if (!physicsController)
			{
				physicsController = GetComponent<BotPhysicsController>();
			}
			if (!flightTransform)
			{
				flightTransform = base.transform;
			}
			if (!enemyRigidbody)
			{
				enemyRigidbody = GetComponent<EnemyRigidbody>();
			}
			spawnPoint = flightTransform.position;
			SetupSprings();
			InitializeStuckDetection();
			isInitialized = true;
		}
	}

	private void SetupSprings()
	{
		positionSpring.damping = flightSettings.positionDamping;
		positionSpring.speed = flightSettings.positionSpeed;
		rotationSpring.damping = flightSettings.rotationDamping;
		rotationSpring.speed = flightSettings.rotationSpeed;
		stabilizationSpring.damping = 0.8f;
		stabilizationSpring.speed = 10f;
	}

	public void FlyToPosition(Vector3 targetPosition, float customForce = -1f)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		currentTarget = targetPosition;
		float spring = ((customForce > 0f) ? customForce : flightSettings.moveForce);
		if ((bool)physicsController)
		{
			SemiFunc.PhysFollowPoint(targetRigidbody, targetPosition, spring, flightSettings.turnSmoothness);
		}
	}

	public void FlyInDirection(Vector3 direction, float force, float drag = 0.2f)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		if ((bool)physicsController)
		{
			Vector3 normalized = direction.normalized;
			physicsController.PhysMoveTowards(normalized, force, drag);
		}
	}

	public void RotateTowards(Vector3 direction, float turnSpeed = -1f, float verticalTurnSpeed = -1f, float drag = 1f)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		if ((bool)physicsController)
		{
			float spring = ((turnSpeed > 0f) ? turnSpeed : 100f);
			float damping = ((verticalTurnSpeed > 0f) ? verticalTurnSpeed : 10f);
			Vector3 normalized = direction.normalized;
			physicsController.PhysRotateTowards(normalized, spring, damping, drag);
		}
	}

	public void LookAtTarget(Vector3 targetPosition, float smoothness = -1f)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		Vector3 forward = (currentLookDirection = (targetPosition - flightTransform.position).normalized);
		if (smoothness > 0f)
		{
			Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);
			flightTransform.rotation = SemiFunc.SpringQuaternionGet(rotationSpring, targetRotation);
		}
		else
		{
			flightTransform.LookAt(targetPosition, Vector3.up);
		}
	}

	public void Stabilize()
	{
		if (!isInitialized)
		{
			Initialize();
		}
		if ((bool)physicsController)
		{
			ApplyGroundAvoidance();
			ApplyOrientationStabilization();
		}
	}

	public void Hover(float intensity = 1f, float speed = 2f)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		Vector3 vector = Vector3.up * (Mathf.Sin(Time.time * speed) * 0.5f * intensity);
		Vector3 targetPosition = spawnPoint + Vector3.up * flightSettings.preferredHeight + vector;
		FlyToPosition(targetPosition);
	}

	public void CircleAround(Vector3 centerPoint, float radius, float speed = 1f, float height = -1f)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		float num = ((height > 0f) ? height : flightSettings.preferredHeight);
		float f = Time.time * speed;
		Vector3 vector = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)) * radius;
		Vector3 targetPosition = centerPoint + vector + Vector3.up * num;
		FlyToPosition(targetPosition);
		Vector3 direction = new Vector3(0f - Mathf.Sin(f), 0f, Mathf.Cos(f));
		RotateTowards(direction);
	}

	public void PatrolBetweenPoints(List<Vector3> patrolPoints, ref int currentPatrolIndex, float arrivalDistance = 3f)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		if (patrolPoints != null && patrolPoints.Count != 0)
		{
			if (currentPatrolIndex >= patrolPoints.Count)
			{
				currentPatrolIndex = 0;
			}
			Vector3 targetPosition = patrolPoints[currentPatrolIndex];
			FlyToPosition(targetPosition);
			if (Vector3.Distance(flightTransform.position, targetPosition) < arrivalDistance)
			{
				currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
			}
		}
	}

	public void ChaseTarget(Vector3 targetPosition, float chaseForce, float predictionAmount = 0f)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		Vector3 targetPosition2 = targetPosition;
		if (predictionAmount > 0f && (bool)targetRigidbody)
		{
			Vector3 velocity = targetRigidbody.velocity;
			targetPosition2 += velocity * predictionAmount;
		}
		FlyToPosition(targetPosition2, chaseForce);
		LookAtTarget(targetPosition2);
	}

	public void FleeFrom(Vector3 dangerPosition, float fleeForce, float fleeDistance = 20f)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		Vector3 normalized = (flightTransform.position - dangerPosition).normalized;
		Vector3 targetPosition = dangerPosition + normalized * fleeDistance;
		targetPosition.y = Mathf.Max(targetPosition.y, flightSettings.preferredHeight);
		FlyToPosition(targetPosition, fleeForce);
	}

	public void RandomRoam(float roamRadius, ref Vector3 currentRoamTarget, ref bool roamTargetReached, float arrivalDistance = 6f)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		if ((currentRoamTarget == Vector3.zero) | roamTargetReached)
		{
			currentRoamTarget = GenerateRandomRoamTarget(roamRadius);
			roamTargetReached = false;
		}
		FlyToPosition(currentRoamTarget);
		if (Vector3.Distance(flightTransform.position, currentRoamTarget) < arrivalDistance)
		{
			roamTargetReached = true;
		}
	}

	public void PerformLoop(Vector3 centerPoint, float loopRadius, float loopSpeed, ref float loopProgress)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		loopProgress += Time.deltaTime * loopSpeed;
		if (loopProgress >= MathF.PI * 2f)
		{
			loopProgress = 0f;
		}
		Vector3 vector = new Vector3(Mathf.Cos(loopProgress) * loopRadius, Mathf.Sin(loopProgress * 2f) * loopRadius * 0.5f, Mathf.Sin(loopProgress) * loopRadius);
		Vector3 targetPosition = centerPoint + vector;
		FlyToPosition(targetPosition);
		Vector3 direction = new Vector3(0f - Mathf.Sin(loopProgress), Mathf.Cos(loopProgress * 2f), Mathf.Cos(loopProgress));
		RotateTowards(direction);
	}

	public Vector3 GenerateRandomRoamTarget(float radius)
	{
		int num = 10;
		for (int i = 0; i < num; i++)
		{
			float f = UnityEngine.Random.Range(0f, MathF.PI * 2f);
			float num2 = UnityEngine.Random.Range(5f, radius);
			float y = UnityEngine.Random.Range(flightSettings.minFlightHeight, flightSettings.maxFlightHeight);
			Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * num2;
			Vector3 vector2 = spawnPoint + new Vector3(vector.x, y, vector.y);
			if (IsPositionSafe(vector2))
			{
				return vector2;
			}
		}
		return spawnPoint + Vector3.up * flightSettings.preferredHeight;
	}

	public bool IsPositionSafe(Vector3 position)
	{
		Vector3 normalized = (position - flightTransform.position).normalized;
		float maxDistance = Vector3.Distance(flightTransform.position, position);
		LayerMask layerMask = SemiFunc.LayerMaskGetVisionObstruct();
		if (Physics.Raycast(flightTransform.position, normalized, maxDistance, layerMask))
		{
			return false;
		}
		int mask = LayerMask.GetMask("Default");
		if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out var hitInfo, 50f, mask))
		{
			float num = Vector3.Distance(position, hitInfo.point);
			if (num >= 4f)
			{
				return num <= 30f;
			}
			return false;
		}
		return false;
	}

	public float GetDistanceToGround()
	{
		int mask = LayerMask.GetMask("Default");
		if (Physics.Raycast(flightTransform.position + Vector3.up, Vector3.down, out var hitInfo, 100f, mask))
		{
			return Vector3.Distance(flightTransform.position, hitInfo.point);
		}
		return float.MaxValue;
	}

	public void SetSpawnPoint(Vector3 newSpawnPoint)
	{
		spawnPoint = newSpawnPoint;
	}

	public Vector3 GetSpawnPoint()
	{
		return spawnPoint;
	}

	public void SetFlightSettings(FlightSettings newSettings)
	{
		flightSettings = newSettings;
		if (isInitialized)
		{
			SetupSprings();
		}
	}

	public void InitializeStuckDetection()
	{
		prevStuckPosition = flightTransform.position;
		currentStuckPosition = flightTransform.position;
		stuckTimer = 0f;
	}

	public void UpdateStuckDetection()
	{
		if (!isInitialized)
		{
			Initialize();
		}
		currentStuckPosition = flightTransform.position;
		if (Vector3.Distance(prevStuckPosition, currentStuckPosition) > 2f)
		{
			prevStuckPosition = currentStuckPosition;
			stuckTimer = 0f;
		}
		else
		{
			stuckTimer += Time.deltaTime;
		}
	}

	public bool IsStuck(float stuckTimeThreshold = 1.5f)
	{
		return stuckTimer > stuckTimeThreshold;
	}

	public float GetStuckTimer()
	{
		return stuckTimer;
	}

	public void EscapeUpwards(float upwardForce = 800f, float speedMultiplier = 1f)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		if ((bool)physicsController)
		{
			Vector3 vector = Quaternion.Euler(UnityEngine.Random.Range(-25f, 25f), UnityEngine.Random.Range(0f, 360f), 0f) * Vector3.up;
			Vector3 vector2 = flightTransform.position + vector * 10f;
			vector2.y = spawnPoint.y + flightSettings.maxFlightHeight;
			physicsController.PhysRotateTowards(vector, 100f, 10f, 1f);
			physicsController.PhysMoveTowards(vector, upwardForce * speedMultiplier, 0.5f);
		}
	}

	private void ApplyGroundAvoidance()
	{
		if (!physicsController)
		{
			return;
		}
		float targetFloat = 0f;
		int mask = LayerMask.GetMask("Default");
		if (Physics.Raycast(flightTransform.position + Vector3.up, Vector3.down, out var hitInfo, 100f, mask))
		{
			float num = Vector3.Distance(flightTransform.position, hitInfo.point);
			if (num < flightSettings.groundAvoidanceDistance)
			{
				float t = Mathf.Clamp01(num / flightSettings.stabilizationRange);
				float num2 = 1f - Mathf.Clamp01((num - flightSettings.stabilizationRange) / 2f);
				targetFloat = Mathf.Lerp(100f, flightSettings.stabilizationForce, t) * num2;
			}
		}
		currentStabilizationForce = SemiFunc.SpringFloatGet(stabilizationSpring, targetFloat);
		if (currentStabilizationForce > 0.1f)
		{
			physicsController.PhysMoveTowards(Vector3.up, currentStabilizationForce, 0.8f);
		}
	}

	private void ApplyOrientationStabilization()
	{
		if ((bool)physicsController)
		{
			Vector3 up = flightTransform.up;
			Vector3 up2 = Vector3.up;
			if (Vector3.Dot(up, up2) < 0.7f)
			{
				Quaternion quaternion = Quaternion.LookRotation(flightTransform.forward, Vector3.up);
				physicsController.PhysRotateTowards(quaternion * Vector3.forward, 30f, 15f, 0.8f);
			}
		}
	}
}
