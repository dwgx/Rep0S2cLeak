using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHunter : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		InvestigateWalk,
		Aim,
		Shoot,
		ShootEnd,
		LeaveStart,
		Leave,
		Despawn,
		Stun
	}

	public bool debugSpawn;

	[Space]
	public State currentState;

	private bool stateImpulse;

	internal float stateTimer;

	[Space]
	public Enemy enemy;

	public EnemyHunterAnim enemyHunterAnim;

	public EnemyHunterAlwaysActive enemyHunterAlwaysActive;

	private PhotonView photonView;

	public Transform investigateRayTransform;

	public Transform verticalAimTransform;

	public Transform gunAimTransform;

	public Transform gunTipTransform;

	public HurtCollider hurtCollider;

	private float hurtColliderTimer;

	private bool shootFast;

	[Space]
	public LineRenderer lineRenderer;

	public AnimationCurve lineRendererWidthCurve;

	private float lineRendererLerp;

	private bool lineRendererActive;

	[Space]
	public SpringQuaternion horizontalAimSpring;

	private Quaternion horizontalAimTarget = Quaternion.identity;

	public SpringQuaternion verticalAimSpring;

	private float pitCheckTimer;

	private int shotsFired;

	private int shotsFiredMax = 4;

	private Vector3 leavePosition;

	private Vector3 investigatePoint;

	private bool investigatePathfindOnly;

	private Quaternion investigateAimHorizontal = Quaternion.identity;

	private Quaternion investigateAimVertical = Quaternion.identity;

	private Quaternion investigateAimVerticalPrevious = Quaternion.identity;

	private float investigateAimVerticalRPCTimer;

	private bool investigatePointHasTransform;

	private Transform investigatePointTransform;

	private Vector3 investigatePointTransformPrevious;

	private Vector3 investigatePointSpread;

	private Vector3 investigatePointSpreadTarget;

	private float investigatePointSpreadTimer;

	private int leaveInterruptCounter;

	private float leaveInterruptTimer;

	private float tripTimer;

	[Space]
	public Transform shootEffectTransform;

	public List<ParticleSystem> shootEffects;

	[Space]
	public Transform hitEffectTransform;

	public List<ParticleSystem> hitEffects;

	[Space]
	public List<ParticleSystem> deathEffects;

	[Space]
	public Sound soundHurt;

	public Sound soundDeath;

	public Sound soundShoot;

	public Sound soundShootGlobal;

	public Sound soundHit;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		if (!Application.isEditor || (SemiFunc.IsMultiplayer() && !GameManager.instance.localTest))
		{
			debugSpawn = false;
		}
	}

	private void Update()
	{
		VerticalRotationLogic();
		HurtColliderTimer();
		LineRendererLogic();
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (enemy.Rigidbody.physGrabObject.rbVelocity.y < -0.5f && (enemy.Rigidbody.timeSinceStun == 0f || enemy.Rigidbody.timeSinceStun > 3f))
		{
			tripTimer += Time.deltaTime;
			if (enemy.Rigidbody.physGrabObject.rbVelocity.y <= -2f)
			{
				tripTimer = 999f;
			}
		}
		else
		{
			tripTimer = 0f;
		}
		if (tripTimer > 0.5f)
		{
			enemy.StateStunned.Set(2f);
			tripTimer = 0f;
		}
		if (enemy.IsStunned())
		{
			UpdateState(State.Stun);
		}
		if (enemy.CurrentState == EnemyState.Despawn && !enemy.IsStunned())
		{
			UpdateState(State.Despawn);
		}
		ShotsFiredLogic();
		HorizontalRotationLogic();
		LeaveInterruptLogic();
		switch (currentState)
		{
		case State.Spawn:
			StateSpawn();
			break;
		case State.Idle:
			StateIdle();
			break;
		case State.Roam:
			StateRoam();
			break;
		case State.Investigate:
			StateInvestigate();
			break;
		case State.InvestigateWalk:
			StateInvestigateWalk();
			break;
		case State.Aim:
			AimLogic();
			StateAim();
			break;
		case State.Shoot:
			StateShoot();
			break;
		case State.ShootEnd:
			StateShootEnd();
			break;
		case State.LeaveStart:
			StateLeaveStart();
			break;
		case State.Leave:
			StateLeave();
			break;
		case State.Despawn:
			StateDespawn();
			break;
		case State.Stun:
			StateStun();
			break;
		}
	}

	private void StateSpawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateIdle()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = Random.Range(2f, 8f);
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		if (!SemiFunc.EnemySpawnIdlePause())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.Roam);
			}
			if (SemiFunc.EnemyForceLeave(enemy))
			{
				UpdateState(State.LeaveStart);
			}
		}
	}

	private void StateRoam()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 10f, 25f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
			}
			if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				enemy.NavMeshAgent.SetDestination(hit.position);
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			pitCheckTimer = 0.1f;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
		}
		else
		{
			if (enemy.Rigidbody.notMovingTimer > 1f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (PitCheckLogic())
			{
				enemy.NavMeshAgent.ResetPath();
			}
			if (stateTimer <= 0f || !enemy.NavMeshAgent.HasPath())
			{
				UpdateState(State.Idle);
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.LeaveStart);
		}
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
		}
	}

	private void StateStun()
	{
		if (!enemy.IsStunned())
		{
			UpdateState(State.Idle);
		}
	}

	private void StateInvestigate()
	{
		if (!stateImpulse)
		{
			return;
		}
		stateImpulse = false;
		float num = 12f;
		Vector3 direction = investigatePoint - investigateRayTransform.position;
		bool flag = false;
		if (direction.magnitude < num && !investigatePathfindOnly)
		{
			flag = true;
			RaycastHit[] array = Physics.RaycastAll(investigateRayTransform.position, direction, direction.magnitude, SemiFunc.LayerMaskGetVisionObstruct());
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit = array[i];
				if (!(Vector3.Distance(investigatePoint, raycastHit.point) < 1f) && !raycastHit.transform.CompareTag("Player") && !raycastHit.transform.GetComponent<EnemyRigidbody>())
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			UpdateState(State.Aim);
		}
		else
		{
			UpdateState(State.InvestigateWalk);
		}
	}

	private void StateInvestigateWalk()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.SetDestination(investigatePoint);
			pitCheckTimer = 0f;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateTimer = 1f;
			stateImpulse = false;
		}
		else
		{
			if (enemy.Rigidbody.notMovingTimer > 1f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (PitCheckLogic())
			{
				enemy.NavMeshAgent.ResetPath();
			}
			if (stateTimer <= 0f || !enemy.NavMeshAgent.HasPath())
			{
				UpdateState(State.Idle);
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.LeaveStart);
		}
	}

	private void StateAim()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateImpulse = false;
			investigatePointSpread = Vector3.zero;
			investigatePointSpreadTarget = Vector3.zero;
			investigatePointSpreadTimer = 0f;
			if (shootFast)
			{
				stateTimer = 0.5f;
			}
			else
			{
				stateTimer = Random.Range(0.25f, 1f);
			}
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Shoot);
		}
	}

	private void StateShoot()
	{
		if (stateImpulse)
		{
			Vector3 vector = gunAimTransform.position + gunAimTransform.forward * 50f;
			float radius = 1f;
			if (shootFast)
			{
				radius = 1.5f;
			}
			if (Vector3.Distance(base.transform.position, investigatePoint) > 10f)
			{
				radius = 0.5f;
			}
			bool flag = false;
			RaycastHit[] array = Physics.SphereCastAll(gunAimTransform.position, radius, gunAimTransform.forward, 50f, LayerMask.GetMask("Player") + LayerMask.GetMask("PhysGrabObject"));
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit = array[i];
				PlayerAvatar playerAvatar = null;
				bool flag2 = false;
				if (raycastHit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
				{
					flag2 = true;
					PlayerController componentInParent = raycastHit.transform.GetComponentInParent<PlayerController>();
					playerAvatar = ((!componentInParent) ? raycastHit.transform.GetComponentInParent<PlayerAvatar>() : componentInParent.playerAvatarScript);
				}
				else
				{
					PlayerTumble componentInParent2 = raycastHit.transform.GetComponentInParent<PlayerTumble>();
					if ((bool)componentInParent2)
					{
						playerAvatar = componentInParent2.playerAvatar;
						flag2 = true;
					}
				}
				if (!flag2)
				{
					continue;
				}
				bool flag3 = true;
				if (raycastHit.point != Vector3.zero)
				{
					Vector3 direction = raycastHit.point - gunAimTransform.position;
					RaycastHit[] array2 = Physics.RaycastAll(gunAimTransform.position, direction, direction.magnitude, SemiFunc.LayerMaskGetVisionObstruct(), QueryTriggerInteraction.Ignore);
					for (int j = 0; j < array2.Length; j++)
					{
						RaycastHit raycastHit2 = array2[j];
						if (raycastHit2.transform.gameObject.layer != LayerMask.NameToLayer("Player") && (raycastHit2.transform.gameObject.layer != LayerMask.NameToLayer("PhysGrabObject") || !GetComponentInParent<PlayerTumble>()))
						{
							flag3 = false;
							break;
						}
					}
				}
				if (flag3)
				{
					if (raycastHit.point != Vector3.zero)
					{
						vector = raycastHit.point;
						flag = true;
					}
					else if ((bool)playerAvatar)
					{
						vector = playerAvatar.PlayerVisionTarget.VisionTransform.position;
						flag = true;
					}
					break;
				}
			}
			if (!flag && Physics.Raycast(gunAimTransform.position, gunAimTransform.forward, out var hitInfo, 50f, (int)SemiFunc.LayerMaskGetVisionObstruct() + LayerMask.GetMask("Enemy")))
			{
				vector = hitInfo.point;
			}
			vector -= gunAimTransform.forward * 0.1f;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ShootRPC", RpcTarget.All, vector);
			}
			else
			{
				ShootRPC(vector);
			}
			stateImpulse = false;
			stateTimer = 2f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.ShootEnd);
		}
	}

	private void StateShootEnd()
	{
		if (stateImpulse)
		{
			shotsFired++;
			stateImpulse = false;
			stateTimer = 2f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			if (shotsFired >= shotsFiredMax)
			{
				UpdateState(State.LeaveStart);
			}
			else
			{
				UpdateState(State.Idle);
			}
		}
	}

	private void StateLeaveStart()
	{
		if (stateImpulse)
		{
			shotsFired++;
			stateImpulse = false;
			stateTimer = 3f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Leave);
		}
	}

	private void StateLeave()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			if (!enemy.EnemyParent.playerClose)
			{
				UpdateState(State.Idle);
				return;
			}
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGetPlayerDistance(base.transform.position, 25f, 50f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
			}
			if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				leavePosition = hit.position;
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			stateImpulse = false;
			stateTimer = 5f;
		}
		enemy.NavMeshAgent.SetDestination(leavePosition);
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		if (PitCheckLogic())
		{
			stateImpulse = true;
		}
		else if (stateTimer <= 0f || Vector3.Distance(base.transform.position, leavePosition) < 1f)
		{
			UpdateState(State.Idle);
		}
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
	}

	public void OnHurt()
	{
		soundHurt.Play(enemy.CenterTransform.position);
		enemyHunterAnim.StopHumming(1f);
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.Leave)
		{
			UpdateState(State.Idle);
		}
	}

	public void OnDeath()
	{
		soundDeath.Play(enemy.CenterTransform.position);
		foreach (ParticleSystem deathEffect in deathEffects)
		{
			deathEffect.Play();
		}
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.05f);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnInvestigate()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || !(enemy.Rigidbody.timeSinceStun > 1.5f))
		{
			return;
		}
		if (currentState == State.Idle || currentState == State.Roam || currentState == State.InvestigateWalk || (currentState == State.ShootEnd && shotsFired < shotsFiredMax) || (currentState == State.Leave && leaveInterruptCounter >= 3))
		{
			investigatePathfindOnly = enemy.StateInvestigate.onInvestigateTriggeredPathfindOnly;
			investigatePoint = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			if (Vector3.Distance(investigatePoint, base.transform.position) < 4f && !investigatePathfindOnly)
			{
				shootFast = true;
			}
			else
			{
				shootFast = false;
			}
			InvestigateTransformGet();
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdateInvestigationPoint", RpcTarget.Others, investigatePoint);
			}
			UpdateState(State.Investigate);
		}
		else if (currentState == State.Leave && Vector3.Distance(base.transform.position, enemy.StateInvestigate.onInvestigateTriggeredPosition) < 5f)
		{
			leaveInterruptCounter++;
			leaveInterruptTimer = 3f;
		}
	}

	public void OnTouchPlayer()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && enemy.Rigidbody.timeSinceStun > 1.5f && (currentState == State.Idle || currentState == State.Roam || currentState == State.InvestigateWalk || currentState == State.ShootEnd || currentState == State.LeaveStart || currentState == State.Leave))
		{
			shootFast = true;
			investigatePoint = enemy.Rigidbody.onTouchPlayerAvatar.PlayerVisionTarget.VisionTransform.position;
			investigatePointHasTransform = true;
			investigatePointTransform = enemy.Rigidbody.onTouchPlayerAvatar.PlayerVisionTarget.VisionTransform;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdateInvestigationPoint", RpcTarget.Others, investigatePoint);
			}
			UpdateState(State.Aim);
		}
	}

	public void OnTouchPlayerGrabbedObject()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && enemy.Rigidbody.timeSinceStun > 1.5f && (currentState == State.Idle || currentState == State.Roam || currentState == State.InvestigateWalk || currentState == State.ShootEnd || currentState == State.LeaveStart || currentState == State.Leave))
		{
			shootFast = true;
			investigatePoint = enemy.Rigidbody.onTouchPlayerGrabbedObjectPosition;
			investigatePointHasTransform = true;
			investigatePointTransform = enemy.Rigidbody.onTouchPlayerGrabbedObjectAvatar.PlayerVisionTarget.VisionTransform;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdateInvestigationPoint", RpcTarget.Others, investigatePoint);
			}
			UpdateState(State.Aim);
		}
	}

	public void OnGrabbed()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && enemy.Rigidbody.timeSinceStun > 1.5f && (currentState == State.Idle || currentState == State.Roam || currentState == State.InvestigateWalk || currentState == State.ShootEnd || currentState == State.LeaveStart || currentState == State.Leave))
		{
			shootFast = true;
			investigatePoint = enemy.Rigidbody.onGrabbedPlayerAvatar.PlayerVisionTarget.VisionTransform.position;
			investigatePointHasTransform = true;
			investigatePointTransform = enemy.Rigidbody.onGrabbedPlayerAvatar.PlayerVisionTarget.VisionTransform;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdateInvestigationPoint", RpcTarget.Others, investigatePoint);
			}
			UpdateState(State.Aim);
		}
	}

	private void UpdateState(State _state)
	{
		if (currentState != _state)
		{
			currentState = _state;
			stateImpulse = true;
			stateTimer = 0f;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateStateRPC", RpcTarget.All, currentState);
			}
			else
			{
				UpdateStateRPC(currentState);
			}
		}
	}

	private void AimLogic()
	{
		if (!investigatePointTransform)
		{
			investigatePointHasTransform = false;
		}
		Vector3 vector = investigatePoint;
		if (investigatePointHasTransform)
		{
			Vector3 vector2 = investigatePointTransform.position - investigatePointTransformPrevious;
			vector2.y = 0f;
			vector = investigatePointTransform.position + vector2 * 25f;
			investigatePointTransformPrevious = investigatePointTransform.position;
		}
		if (investigatePointSpreadTimer <= 0f)
		{
			Vector3 vector3 = Random.insideUnitSphere * Random.Range(0f, 0.5f);
			if (Vector3.Distance(base.transform.position, vector) > 10f)
			{
				vector3 = Random.insideUnitSphere * Random.Range(0.5f, 1f);
			}
			investigatePointSpreadTimer = Random.Range(0.1f, 0.5f);
			investigatePointSpreadTarget = vector3;
		}
		else
		{
			investigatePointSpreadTimer -= Time.deltaTime;
		}
		investigatePointSpread = Vector3.Lerp(investigatePointSpread, investigatePointSpreadTarget, Time.deltaTime * 20f);
		vector += investigatePointSpread;
		float num = 5f;
		if (shootFast)
		{
			num = 20f;
		}
		investigatePoint = Vector3.Lerp(investigatePoint, vector, num * Time.deltaTime);
		Vector3 position = base.transform.position;
		base.transform.position += gunAimTransform.position - verticalAimTransform.position;
		Quaternion rotation = base.transform.rotation;
		base.transform.LookAt(investigatePoint);
		base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
		Quaternion rotation2 = base.transform.rotation;
		base.transform.rotation = rotation;
		base.transform.position = position;
		investigateAimHorizontal = rotation2;
		Vector3 position2 = verticalAimTransform.position;
		verticalAimTransform.position += gunAimTransform.position - verticalAimTransform.position;
		verticalAimTransform.LookAt(investigatePoint);
		float num2 = 45f;
		float x = verticalAimTransform.localEulerAngles.x;
		x = ((!(x < 180f)) ? Mathf.Clamp(x, 360f - num2, 360f) : Mathf.Clamp(x, 0f, num2));
		verticalAimTransform.localEulerAngles = new Vector3(x, 0f, 0f);
		Quaternion localRotation = verticalAimTransform.localRotation;
		verticalAimTransform.position = position2;
		investigateAimVertical = localRotation;
		if (!SemiFunc.IsMultiplayer())
		{
			return;
		}
		if (investigateAimVerticalRPCTimer <= 0f)
		{
			if (investigateAimVerticalPrevious != investigateAimVertical)
			{
				investigateAimVerticalRPCTimer = 1f;
				photonView.RPC("UpdateVerticalAimRPC", RpcTarget.Others, investigateAimVertical);
				investigateAimVerticalPrevious = investigateAimVertical;
			}
		}
		else
		{
			investigateAimVerticalRPCTimer -= Time.deltaTime;
		}
	}

	private void HorizontalRotationLogic()
	{
		if (currentState == State.Idle || currentState == State.Roam || currentState == State.InvestigateWalk || currentState == State.LeaveStart || currentState == State.Leave)
		{
			horizontalAimSpring.damping = 0.7f;
			horizontalAimSpring.speed = 3f;
			if (enemy.NavMeshAgent.AgentVelocity.magnitude > 0.01f)
			{
				Quaternion rotation = base.transform.rotation;
				base.transform.rotation = Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized);
				base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
				Quaternion rotation2 = base.transform.rotation;
				base.transform.rotation = rotation;
				horizontalAimTarget = rotation2;
			}
		}
		else if (currentState == State.Aim)
		{
			if (shootFast)
			{
				horizontalAimSpring.damping = 0.9f;
				horizontalAimSpring.speed = 30f;
			}
			else
			{
				horizontalAimSpring.damping = 0.8f;
				horizontalAimSpring.speed = 20f;
			}
			horizontalAimTarget = investigateAimHorizontal;
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(horizontalAimSpring, horizontalAimTarget);
	}

	private void VerticalRotationLogic()
	{
		if (currentState == State.Aim || currentState == State.Shoot)
		{
			verticalAimTransform.localRotation = SemiFunc.SpringQuaternionGet(verticalAimSpring, investigateAimVertical);
		}
		else
		{
			verticalAimTransform.localRotation = SemiFunc.SpringQuaternionGet(verticalAimSpring, Quaternion.identity);
		}
	}

	private bool PitCheckLogic()
	{
		if (pitCheckTimer <= 0f && enemy.NavMeshAgent.AgentVelocity.normalized.magnitude > 0.1f)
		{
			pitCheckTimer = 0.5f;
			Vector3 normalized = enemy.NavMeshAgent.AgentVelocity.normalized;
			normalized.y = 0f;
			bool num = Physics.Raycast(base.transform.position + normalized + Vector3.up * 1f, Vector3.down, 5f, SemiFunc.LayerMaskGetVisionObstruct());
			if (!num)
			{
				enemy.NavMeshAgent.Warp(base.transform.position - normalized * 0.5f);
				enemy.NavMeshAgent.ResetPath();
				enemy.NavMeshAgent.Agent.velocity = Vector3.zero;
			}
			return !num;
		}
		pitCheckTimer -= Time.deltaTime;
		return false;
	}

	private void HurtColliderTimer()
	{
		if (hurtColliderTimer > 0f)
		{
			hurtColliderTimer -= Time.deltaTime;
			if (hurtColliderTimer <= 0f)
			{
				hurtCollider.gameObject.SetActive(value: false);
			}
		}
	}

	private void LineRendererLogic()
	{
		if (lineRendererActive)
		{
			lineRenderer.widthMultiplier = lineRendererWidthCurve.Evaluate(lineRendererLerp);
			lineRendererLerp += Time.deltaTime * 5f;
			if (lineRendererLerp >= 1f)
			{
				lineRenderer.gameObject.SetActive(value: false);
				lineRendererActive = false;
			}
		}
	}

	private void ShotsFiredLogic()
	{
		if (currentState == State.Spawn || currentState == State.Leave)
		{
			shotsFired = 0;
		}
	}

	private void InvestigateTransformGet()
	{
		investigatePointHasTransform = false;
		if (investigatePathfindOnly)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(investigatePoint, 1.5f, LayerMask.GetMask("Player") + LayerMask.GetMask("PhysGrabObject"));
		foreach (Collider collider in array)
		{
			if (collider.CompareTag("Player"))
			{
				PlayerController componentInParent = collider.GetComponentInParent<PlayerController>();
				if ((bool)componentInParent)
				{
					investigatePointHasTransform = true;
					investigatePointTransform = componentInParent.playerAvatarScript.PlayerVisionTarget.VisionTransform;
					continue;
				}
				PlayerAvatar componentInParent2 = collider.GetComponentInParent<PlayerAvatar>();
				if ((bool)componentInParent2)
				{
					investigatePointHasTransform = true;
					investigatePointTransform = componentInParent2.PlayerVisionTarget.VisionTransform;
				}
			}
			else
			{
				PlayerTumble componentInParent3 = collider.GetComponentInParent<PlayerTumble>();
				if ((bool)componentInParent3)
				{
					investigatePointHasTransform = true;
					investigatePointTransform = componentInParent3.playerAvatar.PlayerVisionTarget.VisionTransform;
				}
			}
		}
	}

	private void LeaveInterruptLogic()
	{
		if (currentState == State.Leave)
		{
			if (leaveInterruptTimer <= 0f)
			{
				leaveInterruptCounter = 0;
			}
			else
			{
				leaveInterruptTimer -= Time.deltaTime;
			}
		}
		else
		{
			leaveInterruptTimer = 0f;
		}
	}

	[PunRPC]
	private void UpdateStateRPC(State _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
			if (currentState == State.Spawn)
			{
				enemyHunterAnim.OnSpawn();
			}
			if (currentState == State.Stun)
			{
				enemyHunterAnim.StopHumming(1f);
			}
		}
	}

	[PunRPC]
	private void UpdateVerticalAimRPC(Quaternion _rotation, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			investigateAimVertical = _rotation;
		}
	}

	[PunRPC]
	private void ShootRPC(Vector3 _hitPosition, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		Vector3 vector = _hitPosition - gunTipTransform.position;
		lineRenderer.gameObject.SetActive(value: true);
		lineRenderer.SetPosition(0, gunTipTransform.position);
		lineRenderer.SetPosition(1, gunTipTransform.position + vector.normalized * 0.5f);
		lineRenderer.SetPosition(2, _hitPosition - vector.normalized * 0.5f);
		lineRenderer.SetPosition(3, _hitPosition);
		lineRendererActive = true;
		lineRendererLerp = 0f;
		hurtCollider.transform.position = _hitPosition;
		hurtCollider.transform.rotation = Quaternion.LookRotation(gunTipTransform.forward);
		hurtCollider.gameObject.SetActive(value: true);
		hurtColliderTimer = 0.25f;
		shootEffectTransform.position = gunTipTransform.position;
		shootEffectTransform.rotation = gunTipTransform.rotation;
		foreach (ParticleSystem shootEffect in shootEffects)
		{
			shootEffect.Play();
		}
		hitEffectTransform.position = _hitPosition;
		hitEffectTransform.rotation = gunTipTransform.rotation;
		foreach (ParticleSystem hitEffect in hitEffects)
		{
			hitEffect.Play();
		}
		enemyHunterAlwaysActive.Trigger();
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 15f, gunTipTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 15f, gunTipTransform.position, 0.05f);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, _hitPosition, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, _hitPosition, 0.05f);
		soundShoot.Play(gunTipTransform.position);
		soundShootGlobal.Play(gunTipTransform.position);
		soundHit.Play(_hitPosition);
	}

	[PunRPC]
	private void UpdateInvestigationPoint(Vector3 _point, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			investigatePoint = _point;
		}
	}
}
