using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class EnemyRigidbody : MonoBehaviour
{
	public Enemy enemy;

	public Transform followTarget;

	internal PhysGrabObject physGrabObject;

	internal PhysGrabObjectImpactDetector impactDetector;

	internal PhotonView photonView;

	internal Rigidbody rb;

	internal Vector3 velocity;

	[Space]
	public bool gravity = true;

	public float customGravity;

	[Space]
	public GrabForce grabForceNeeded;

	public float grabTimeNeeded = 0.5f;

	private float grabForceTimer;

	private float grabShakeReleaseTimer;

	public bool grabStun;

	public bool grabOverride;

	public float grabPositionStrength;

	public float grabRotationStrength;

	public float grabStrengthTime = 1f;

	public bool hasShakeRelease = true;

	internal float grabStrengthTimer;

	public float grabTimeMax = 3f;

	private float grabTimeMaxRandom;

	private float grabTimeCurrent;

	internal bool grabbed;

	private bool grabbedPrevious;

	[Space]
	public float positionSpeedIdle = 1f;

	public float positionSpeedLerpIdle = 10f;

	public float positionSpeedChase = 2f;

	public float positionSpeedLerpChase = 50f;

	private float positionSpeed;

	private float positionSpeedCurrent;

	internal float positionSpeedLerp = 1f;

	private float positionSpeedLerpCurrent = 1f;

	private Vector3 positionForce;

	[Space]
	public float rotationSpeedIdle = 1f;

	public float rotationSpeedChase = 2f;

	public float rotationSpeedDamping = 2f;

	private float rotationSpeed;

	private float rotationSpeedCurrent;

	private float rotationSpeedLerp = 1f;

	[Space]
	public float distanceWarpIdle = 1f;

	public float distanceWarpChase = 2f;

	private float distanceWarp;

	private float timeSinceLastWarp;

	[Space]
	public float notMovingDistance = 1f;

	internal float notMovingTimer;

	private Vector3 lastMovingPosition;

	[Space]
	public bool stunFromFall = true;

	private float stunFromFallTime = 1f;

	private float stunFromFallTimer;

	public bool stunMassOverride;

	public float stunMassOverrideMultiplier = 1f;

	[Space]
	public AnimationCurve speedResetCurve;

	public float stunResetSpeed = 10f;

	internal float disableFollowPositionTimer;

	internal float disableFollowPositionResetSpeed;

	internal float disableFollowRotationTimer;

	internal float disableFollowRotationResetSpeed;

	internal float disableNoGravityTimer;

	internal float overrideFollowPositionTimer;

	internal float overrideFollowPositionSpeed;

	internal float overrideFollowPositionLerp;

	internal float overrideFollowPositionGravityDisableTimer;

	internal float overrideFollowRotationTimer;

	internal float overrideFollowRotationSpeed;

	internal float deactivateFollowTargetPhysicsTimer;

	internal float deactivateCustomGravityTimer;

	private float idleTimer;

	internal float timeSinceStun;

	[Space]
	public PhysicMaterial ColliderMaterialDefault;

	public PhysicMaterial ColliderMaterialDisabled;

	public PhysicMaterial ColliderMaterialStunned;

	public PhysicMaterial ColliderMaterialGrabbed;

	public PhysicMaterial ColliderMaterialJumping;

	private float colliderMaterialStunnedOverrideTimer;

	[Space]
	public Collider playerCollision;

	private bool hasPlayerCollision;

	private bool playerCollisionActive;

	private int materialState = -1;

	internal float teleportedTimer;

	internal float touchingCartTimer;

	internal bool frozen;

	private Vector3 freezeVelocity;

	private Vector3 freezeAngularVelocity;

	private Vector3 freezeForce;

	private Vector3 freezeTorque;

	internal float yOffset;

	private float overchargeMultiplier;

	[Space]
	public float impactShakeLight = 1f;

	public float impactShakeMedium = 2f;

	public float impactShakeHeavy = 4f;

	public float impactFragility = 1f;

	[Space]
	public UnityEvent onImpactLight;

	public UnityEvent onImpactMedium;

	public UnityEvent onImpactHeavy;

	public UnityEvent onTouchPlayer;

	internal PlayerAvatar onTouchPlayerAvatar;

	public UnityEvent onTouchPlayerGrabbedObject;

	internal PlayerAvatar onTouchPlayerGrabbedObjectAvatar;

	internal PhysGrabObject onTouchPlayerGrabbedObjectPhysObject;

	internal Vector3 onTouchPlayerGrabbedObjectPosition;

	public UnityEvent onTouchPhysObject;

	internal PhysGrabObject onTouchPhysObjectPhysObject;

	internal Vector3 onTouchPhysObjectPosition;

	public UnityEvent onGrabbed;

	internal PlayerAvatar onGrabbedPlayerAvatar;

	internal Vector3 onGrabbedPosition;

	private float warpDisableTimer;

	private EnemyParent enemyParent;

	private void Awake()
	{
		enemyParent = GetComponentInParent<EnemyParent>();
		yOffset = base.transform.position.y - followTarget.position.y;
		enemy.Rigidbody = this;
		enemy.HasRigidbody = true;
		physGrabObject = GetComponent<PhysGrabObject>();
		impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		impactDetector.impactFragilityMultiplier = impactFragility;
		if ((bool)playerCollision)
		{
			hasPlayerCollision = true;
			playerCollisionActive = true;
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				playerCollision.enabled = false;
			}
		}
		rb = GetComponent<Rigidbody>();
		photonView = GetComponent<PhotonView>();
		overchargeMultiplier = enemyParent.overchargeMultiplier;
	}

	public void IdleSet(float time)
	{
		idleTimer = time;
	}

	private void Update()
	{
		if (physGrabObject.playerGrabbing.Count > 0)
		{
			PhysGrabOverCharge();
			if (physGrabObject.grabbedLocal)
			{
				ItemInfoUI.instance.ItemInfoText(null, enemyParent.enemyName, enemy: true);
			}
			onGrabbedPlayerAvatar = physGrabObject.playerGrabbing[0].playerAvatar;
			onGrabbedPosition = physGrabObject.playerGrabbing[0].physGrabPoint.position;
			onGrabbed.Invoke();
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			physGrabObject.enemyInteractTimer = 10f;
			if (touchingCartTimer > 0f)
			{
				touchingCartTimer -= Time.deltaTime;
			}
			if (stunMassOverride && enemy.IsStunned())
			{
				physGrabObject.OverrideMass(physGrabObject.massOriginal * stunMassOverrideMultiplier);
			}
			positionSpeed = positionSpeedChase;
			rotationSpeed = rotationSpeedChase;
			distanceWarp = distanceWarpChase;
			positionSpeedLerpCurrent = positionSpeedLerpChase;
			if (idleTimer > 0f)
			{
				positionSpeed = positionSpeedIdle;
				rotationSpeed = rotationSpeedIdle;
				distanceWarp = distanceWarpIdle;
				positionSpeedLerpCurrent = positionSpeedLerpIdle;
				idleTimer -= Time.deltaTime;
			}
			if (overrideFollowPositionTimer > 0f)
			{
				positionSpeed = overrideFollowPositionSpeed;
				if (overrideFollowPositionLerp != -1f)
				{
					positionSpeedLerpCurrent = overrideFollowPositionLerp;
				}
				overrideFollowPositionTimer -= Time.deltaTime;
			}
			if (overrideFollowRotationTimer > 0f)
			{
				rotationSpeed = overrideFollowRotationSpeed;
				overrideFollowRotationTimer -= Time.deltaTime;
			}
			if (disableNoGravityTimer > 0f)
			{
				disableNoGravityTimer -= Time.deltaTime;
			}
			else if (!gravity)
			{
				physGrabObject.OverrideZeroGravity();
			}
		}
		if (!enemy.IsStunned() && (!enemy.HasHealth || enemy.Health.nonStunHurtTimer <= 0f))
		{
			impactDetector.ImpactDisable(0.25f);
		}
	}

	private void FixedUpdate()
	{
		if (!frozen)
		{
			velocity = physGrabObject.rbVelocity;
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer() || !physGrabObject.spawned)
		{
			return;
		}
		if (teleportedTimer > 0f)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			teleportedTimer -= Time.fixedDeltaTime;
			return;
		}
		if (hasPlayerCollision)
		{
			if (enemy.IsStunned())
			{
				if (playerCollisionActive)
				{
					playerCollisionActive = false;
					playerCollision.enabled = false;
				}
			}
			else if (!playerCollisionActive)
			{
				playerCollisionActive = true;
				playerCollision.enabled = true;
			}
		}
		if (enemy.FreezeTimer > 0f)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			return;
		}
		if (frozen)
		{
			rb.AddForce(freezeVelocity, ForceMode.VelocityChange);
			rb.AddTorque(freezeAngularVelocity, ForceMode.VelocityChange);
			rb.AddForce(freezeForce, ForceMode.Impulse);
			rb.AddTorque(freezeTorque, ForceMode.Impulse);
			freezeForce = Vector3.zero;
			freezeTorque = Vector3.zero;
			frozen = false;
			return;
		}
		bool flag = false;
		if (physGrabObject.playerGrabbing.Count > 0)
		{
			float num = 1f + (float)(physGrabObject.playerGrabbing.Count - 1) * 1.5f;
			enemy.SetChaseTarget(physGrabObject.playerGrabbing[0].playerAvatar);
			if (physGrabObject.grabDisplacementCurrent.magnitude >= grabForceNeeded.amount || EnemyDirector.instance.debugEasyGrab)
			{
				grabShakeReleaseTimer = 0f;
				grabForceTimer += Time.fixedDeltaTime * num;
				if (grabForceTimer >= grabTimeNeeded)
				{
					flag = true;
					if (grabOverride)
					{
						grabStrengthTimer = grabStrengthTime;
					}
				}
			}
			else
			{
				grabShakeReleaseTimer += Time.fixedDeltaTime;
			}
			if (grabShakeReleaseTimer > 3f * num && enemy.StateStunned.stunTimer <= 0.25f && !grabbed)
			{
				GrabReleaseShake();
			}
			grabTimeCurrent += Time.fixedDeltaTime;
			if (!EnemyDirector.instance.debugNoGrabMaxTime && enemy.StateStunned.stunTimer <= 0.25f && grabTimeCurrent >= grabTimeMaxRandom * num)
			{
				GrabReleaseShake();
			}
		}
		else
		{
			grabTimeCurrent = 0f;
			grabTimeMaxRandom = grabTimeMax * Random.Range(0.9f, 1.1f);
			grabForceTimer = 0f;
			grabShakeReleaseTimer = 0f;
		}
		if (grabStrengthTimer > 0f)
		{
			flag = true;
			if (grabStun && enemy.HasStateStunned)
			{
				enemy.StateStunned.Set(0.1f);
			}
			if (rb.velocity.magnitude < 2f)
			{
				grabStrengthTimer -= Time.fixedDeltaTime;
				if (grabStrengthTimer <= 0f)
				{
					GrabReleaseShake();
				}
			}
		}
		if (flag)
		{
			enemy.StuckCount = 0;
			if (enemy.HasJump)
			{
				enemy.Jump.jumpCooldown = 1f;
			}
		}
		if (grabbedPrevious != flag)
		{
			GrabbedSet(flag);
		}
		if (customGravity > 0f && gravity && disableNoGravityTimer <= 0f && deactivateCustomGravityTimer <= 0f && rb.useGravity && physGrabObject.playerGrabbing.Count <= 0)
		{
			rb.AddForce(-Vector3.up * customGravity, ForceMode.Force);
		}
		if (deactivateCustomGravityTimer > 0f)
		{
			deactivateCustomGravityTimer -= Time.fixedDeltaTime;
		}
		if (grabbed)
		{
			if (materialState != 0)
			{
				Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].material = ColliderMaterialGrabbed;
				}
				materialState = 0;
			}
		}
		else if (enemy.IsStunned() || colliderMaterialStunnedOverrideTimer > 0f)
		{
			if (materialState != 1)
			{
				Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].material = ColliderMaterialStunned;
				}
				materialState = 1;
			}
		}
		else if (enemy.HasJump && enemy.Jump.jumping)
		{
			if (materialState != 3)
			{
				Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].material = ColliderMaterialJumping;
				}
				materialState = 3;
			}
		}
		else if (disableFollowPositionTimer > 0f || disableFollowRotationTimer > 0f)
		{
			if (materialState != 2)
			{
				Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].material = ColliderMaterialDisabled;
				}
				materialState = 2;
			}
		}
		else if (materialState != 4)
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material = ColliderMaterialDefault;
			}
			materialState = 4;
		}
		if (colliderMaterialStunnedOverrideTimer > 0f)
		{
			colliderMaterialStunnedOverrideTimer -= Time.fixedDeltaTime;
		}
		if (deactivateFollowTargetPhysicsTimer > 0f)
		{
			followTarget.position = base.transform.position;
			followTarget.rotation = base.transform.rotation;
			deactivateFollowTargetPhysicsTimer -= Time.fixedDeltaTime;
		}
		if (deactivateFollowTargetPhysicsTimer <= 0f && disableFollowRotationTimer <= 0f && !enemy.IsStunned())
		{
			rotationSpeedLerp += disableFollowRotationResetSpeed * Time.fixedDeltaTime;
			rotationSpeedLerp = Mathf.Clamp01(rotationSpeedLerp);
			rotationSpeedCurrent = Mathf.Lerp(0f, rotationSpeed, speedResetCurve.Evaluate(rotationSpeedLerp));
			rotationSpeedCurrent *= Mathf.Clamp(Quaternion.Angle(base.transform.rotation, followTarget.rotation) / rotationSpeedDamping, 0f, 1f);
			Vector3 torque = SemiFunc.PhysFollowRotation(base.transform, followTarget.rotation, rb, rotationSpeedCurrent);
			if (grabStrengthTimer > 0f)
			{
				torque = Vector3.Lerp(Vector3.zero, torque, grabRotationStrength);
			}
			rb.AddTorque(torque, ForceMode.Impulse);
		}
		else
		{
			rotationSpeedLerp = 0f;
			disableFollowRotationTimer -= Time.fixedDeltaTime;
		}
		if (deactivateFollowTargetPhysicsTimer <= 0f && disableFollowPositionTimer <= 0f && !enemy.IsStunned())
		{
			timeSinceStun += Time.fixedDeltaTime;
			positionSpeedLerp += disableFollowPositionResetSpeed * Time.fixedDeltaTime;
			positionSpeedLerp = Mathf.Clamp01(positionSpeedLerp);
			positionSpeedCurrent = Mathf.Lerp(0f, positionSpeed, speedResetCurve.Evaluate(positionSpeedLerp));
			Vector3 vector = SemiFunc.PhysFollowPosition(rb.transform.position, followTarget.position, rb.velocity, positionSpeedCurrent);
			if (grabStrengthTimer > 0f)
			{
				vector = Vector3.Lerp(Vector3.zero, vector, grabPositionStrength);
			}
			if (overrideFollowPositionGravityDisableTimer <= 0f && (gravity || disableNoGravityTimer > 0f) && physGrabObject.playerGrabbing.Count <= 0)
			{
				vector.y = 0f;
			}
			vector = Vector3.Lerp(positionForce, vector, positionSpeedLerpCurrent * Time.fixedDeltaTime);
			rb.AddForce(vector, ForceMode.Impulse);
		}
		else
		{
			timeSinceStun = 0f;
			positionSpeedLerp = 0f;
			disableFollowPositionTimer -= Time.fixedDeltaTime;
		}
		if (overrideFollowPositionGravityDisableTimer > 0f)
		{
			overrideFollowPositionGravityDisableTimer -= Time.fixedDeltaTime;
		}
		if (!grabbed && Vector3.Distance(lastMovingPosition, base.transform.position) < notMovingDistance)
		{
			notMovingTimer += Time.fixedDeltaTime;
		}
		else
		{
			lastMovingPosition = base.transform.position;
			notMovingTimer = 0f;
		}
		if (enemy.HasNavMeshAgent && !grabbed)
		{
			float num2 = Vector3.Distance(new Vector3(followTarget.position.x, 0f, followTarget.position.z), new Vector3(rb.position.x, 0f, rb.position.z));
			bool flag2 = false;
			if (enemy.HasJump && enemy.Jump.jumping)
			{
				flag2 = true;
			}
			if (warpDisableTimer <= 0f && num2 >= distanceWarp && !flag2)
			{
				if (enemy.NavMeshAgent.IsDisabled() || enemy.NavMeshAgent.IsStopped())
				{
					enemy.transform.position = rb.position;
					timeSinceLastWarp = 0f;
					if (LevelGenerator.Instance.Generated && (!enemy.HasAttackPhysObject || !enemy.AttackStuckPhysObject.Active) && notMovingTimer >= 1f)
					{
						enemy.StuckCount++;
					}
				}
				else if (enemy.NavMeshAgent.Agent.velocity.magnitude > 0.1f || num2 >= distanceWarp * 2f)
				{
					if (Physics.Raycast(rb.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, 10f, LayerMask.GetMask("Default", "NavmeshOnly", "PlayerOnlyCollision")))
					{
						enemy.NavMeshAgent.AgentMove(hitInfo.point);
					}
					else
					{
						enemy.NavMeshAgent.AgentMove(rb.position);
					}
					timeSinceLastWarp = 0f;
					if (LevelGenerator.Instance.Generated && (!enemy.HasAttackPhysObject || !enemy.AttackStuckPhysObject.Active) && notMovingTimer >= 1f)
					{
						enemy.StuckCount++;
					}
				}
			}
			else if (!enemy.NavMeshAgent.IsDisabled() && !enemy.NavMeshAgent.IsStopped())
			{
				timeSinceLastWarp += Time.fixedDeltaTime;
				if (timeSinceLastWarp >= 3f)
				{
					enemy.StuckCount = 0;
				}
			}
		}
		if (warpDisableTimer > 0f)
		{
			warpDisableTimer -= Time.fixedDeltaTime;
		}
		if (stunFromFall && (!enemy.HasJump || !enemy.Jump.jumping) && !grabbed && gravity && disableNoGravityTimer <= 0f && rb.useGravity && (!enemy.HasGrounded || !enemy.Grounded.grounded))
		{
			if (rb.velocity.y < -2f)
			{
				if (stunFromFallTimer >= stunFromFallTime && enemy.HasStateStunned)
				{
					if (!enemy.IsStunned())
					{
						rb.AddTorque(-base.transform.right * (rb.mass * 0.5f), ForceMode.Impulse);
					}
					enemy.StateStunned.Set(3f);
				}
				stunFromFallTimer += Time.fixedDeltaTime;
			}
			else
			{
				stunFromFallTimer = 0f;
			}
		}
		else
		{
			stunFromFallTimer = 0f;
		}
	}

	private void PhysGrabOverCharge()
	{
		SemiFunc.EnemyOvercharge(physGrabObject, enemy.EnemyParent.difficulty, overchargeMultiplier);
	}

	private void OnCollisionStay(Collision other)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || enemy.CurrentState == EnemyState.Despawn)
		{
			return;
		}
		if (other.gameObject.CompareTag("Phys Grab Object"))
		{
			PhysGrabObject physGrabObject = other.gameObject.GetComponent<PhysGrabObject>();
			if (!physGrabObject)
			{
				physGrabObject = other.gameObject.GetComponentInParent<PhysGrabObject>();
			}
			if (!physGrabObject)
			{
				return;
			}
			onTouchPhysObjectPhysObject = physGrabObject;
			onTouchPhysObjectPosition = other.GetContact(0).point;
			onTouchPhysObject.Invoke();
			physGrabObject.EnemyInteractTimeSet();
			PhysGrabCart component = physGrabObject.GetComponent<PhysGrabCart>();
			if ((bool)component)
			{
				touchingCartTimer = 0.25f;
				foreach (PhysGrabInCart.CartObject item in component.physGrabInCart.inCartObjects.ToList())
				{
					item.physGrabObject.EnemyInteractTimeSet();
				}
			}
			if (enemy.CheckChase())
			{
				if (!(enemy.FreezeTimer <= 0f))
				{
					return;
				}
				Vector3 normalized = (physGrabObject.centerPoint - this.physGrabObject.centerPoint).normalized;
				if (Vector3.Dot((followTarget.position - this.physGrabObject.centerPoint).normalized, normalized) > 0f)
				{
					Vector3 force = normalized * 10f;
					force.y = 5f;
					physGrabObject.rb.AddForce(force, ForceMode.Impulse);
					physGrabObject.rb.AddTorque(Random.insideUnitSphere * force.magnitude, ForceMode.Impulse);
					physGrabObject.lightBreakImpulse = true;
					PhysGrabHinge component2 = physGrabObject.GetComponent<PhysGrabHinge>();
					if ((bool)component2 && component2.brokenTimer >= 1.5f)
					{
						component2.DestroyHinge();
					}
					GameDirector.instance.CameraImpact.ShakeDistance(5f, 5f, 15f, base.transform.position, 0.1f);
					GameDirector.instance.CameraShake.ShakeDistance(5f, 5f, 15f, base.transform.position, 0.1f);
					rb.AddForce(-normalized * 2f, ForceMode.Impulse);
					DisableFollowPosition(0.1f, 5f);
				}
			}
			else
			{
				PlayerTumble component3 = physGrabObject.GetComponent<PlayerTumble>();
				if ((bool)component3)
				{
					onTouchPlayerAvatar = component3.playerAvatar;
					onTouchPlayer.Invoke();
					enemy.SetChaseTarget(component3.playerAvatar);
				}
				else if (physGrabObject.playerGrabbing.Count > 0)
				{
					PlayerAvatar chaseTarget = (onTouchPlayerGrabbedObjectAvatar = physGrabObject.playerGrabbing[0].playerAvatar);
					onTouchPlayerGrabbedObjectPhysObject = physGrabObject;
					onTouchPlayerGrabbedObjectPosition = other.GetContact(0).point;
					onTouchPlayerGrabbedObject.Invoke();
					enemy.SetChaseTarget(chaseTarget);
				}
			}
		}
		else
		{
			if (!other.gameObject.CompareTag("Player"))
			{
				return;
			}
			PlayerController componentInParent = other.gameObject.GetComponentInParent<PlayerController>();
			if ((bool)componentInParent)
			{
				onTouchPlayerAvatar = componentInParent.playerAvatarScript;
				onTouchPlayer.Invoke();
				enemy.SetChaseTarget(componentInParent.playerAvatarScript);
				return;
			}
			PlayerAvatar componentInParent2 = other.gameObject.GetComponentInParent<PlayerAvatar>();
			if ((bool)componentInParent2)
			{
				onTouchPlayerAvatar = componentInParent2;
				onTouchPlayer.Invoke();
				enemy.SetChaseTarget(componentInParent2);
			}
		}
	}

	public void DisableFollowPosition(float time, float resetSpeed)
	{
		disableFollowPositionTimer = Mathf.Max(disableFollowPositionTimer, time);
		disableFollowPositionResetSpeed = resetSpeed;
	}

	public void DisableFollowRotation(float time, float resetSpeed)
	{
		disableFollowRotationTimer = Mathf.Max(disableFollowRotationTimer, time);
		disableFollowRotationResetSpeed = resetSpeed;
	}

	public void DisableNoGravity(float time)
	{
		disableNoGravityTimer = time;
	}

	public void OverrideFollowPosition(float time, float speed, float lerp = -1f)
	{
		overrideFollowPositionTimer = time;
		overrideFollowPositionSpeed = speed;
		overrideFollowPositionLerp = lerp;
	}

	public void OverrideFollowPositionGravityDisable(float time)
	{
		overrideFollowPositionGravityDisableTimer = time;
	}

	public void OverrideFollowRotation(float time, float speed)
	{
		overrideFollowRotationTimer = time;
		overrideFollowRotationSpeed = speed;
	}

	public void DeactivateFollowTargetPhysics(float time)
	{
		deactivateFollowTargetPhysicsTimer = time;
	}

	public void DeactivateCustomGravity(float time)
	{
		deactivateCustomGravityTimer = time;
	}

	public void Teleport()
	{
		enemy.Rigidbody.GrabRelease(_effects: false, 0.5f);
		physGrabObject.Teleport(followTarget.position + new Vector3(0f, yOffset, 0f), followTarget.rotation);
		if (!rb.isKinematic)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
		freezeForce = Vector3.zero;
		freezeTorque = Vector3.zero;
		frozen = false;
	}

	public void FreezeForces(Vector3 force, Vector3 torque)
	{
		if (!frozen)
		{
			freezeVelocity = rb.velocity;
			freezeAngularVelocity = rb.angularVelocity;
			frozen = true;
		}
		freezeForce += force;
		freezeTorque += torque;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
	}

	public void JumpImpulse()
	{
		if (materialState != 3)
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material = ColliderMaterialJumping;
			}
			materialState = 3;
		}
	}

	public void StuckReset()
	{
		notMovingTimer = 0f;
		enemy.StuckCount = 0;
	}

	public void WarpDisable(float time)
	{
		warpDisableTimer = time;
	}

	public void OverrideColliderMaterialStunned(float _time)
	{
		colliderMaterialStunnedOverrideTimer = _time;
	}

	public void LightImpact()
	{
		if (enemy.HasHealth)
		{
			enemy.Health.LightImpact();
		}
		GameDirector.instance.CameraShake.ShakeDistance(impactShakeLight, 5f, 15f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(impactShakeLight, 5f, 15f, base.transform.position, 0.1f);
		onImpactLight.Invoke();
	}

	public void MediumImpact()
	{
		if (enemy.HasHealth)
		{
			enemy.Health.MediumImpact();
		}
		GameDirector.instance.CameraShake.ShakeDistance(impactShakeMedium, 5f, 15f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(impactShakeMedium, 5f, 15f, base.transform.position, 0.1f);
		onImpactMedium.Invoke();
	}

	public void HeavyImpact()
	{
		if (enemy.HasHealth)
		{
			enemy.Health.HeavyImpact();
		}
		GameDirector.instance.CameraShake.ShakeDistance(impactShakeHeavy, 5f, 15f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(impactShakeHeavy, 5f, 15f, base.transform.position, 0.1f);
		onImpactHeavy.Invoke();
	}

	public void GrabRelease(bool _effects = true, float _grabDisableTime = 0.1f)
	{
		bool flag = false;
		foreach (PhysGrabber item in physGrabObject.playerGrabbing.ToList())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				item.ReleaseObject(photonView.ViewID, _grabDisableTime);
			}
			else
			{
				item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, _grabDisableTime, photonView.ViewID);
			}
			flag = true;
		}
		if (flag && _effects)
		{
			if (GameManager.instance.gameMode == 0)
			{
				GrabReleaseRPC();
			}
			else
			{
				photonView.RPC("GrabReleaseRPC", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	private void GrabReleaseRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
			physGrabObject.grabDisableTimer = 1f;
		}
	}

	private void GrabReleaseShake()
	{
		if (hasShakeRelease)
		{
			grabStrengthTimer = 0f;
			GrabbedSet(_grabbed: false);
			float num = 1f * rb.mass;
			rb.AddRelativeTorque(Vector3.up * num, ForceMode.Impulse);
			GrabRelease();
			DisableFollowRotation(0.5f, 50f);
		}
	}

	private void GrabbedSet(bool _grabbed)
	{
		grabbed = _grabbed;
		grabbedPrevious = _grabbed;
		if (GameManager.Multiplayer() && PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("GrabbedSetRPC", RpcTarget.All, grabbed);
		}
	}

	[PunRPC]
	private void GrabbedSetRPC(bool _grabbed, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			grabbed = _grabbed;
		}
	}
}
