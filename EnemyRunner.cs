using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRunner : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		SeekPlayer,
		Sneak,
		Notice,
		AttackPlayer,
		AttackPlayerOver,
		AttackPlayerBackToNavMesh,
		StuckAttackNotice,
		StuckAttack,
		LookUnderStart,
		LookUnder,
		LookUnderStop,
		Stun,
		Leave,
		Despawn
	}

	public SpringQuaternion headSpring;

	public Transform headSpringTarget;

	public Transform headSpringSource;

	public SpringQuaternion rotationSpring;

	private Quaternion rotationTarget;

	public State currentState;

	private bool stateImpulse = true;

	private float stateTimer;

	internal PlayerAvatar targetPlayer;

	public Enemy enemy;

	public EnemyRunnerAnim animator;

	public ParticleSystem hayParticlesBig;

	public ParticleSystem hayParticlesSmall;

	public ParticleSystem bitsParticlesFar;

	public ParticleSystem bitsParticlesShort;

	private PhotonView photonView;

	public HurtCollider hurtCollider;

	private float hurtColliderTimer;

	private Vector3 agentDestination;

	private Vector3 backToNavMeshPosition;

	private Vector3 stuckAttackTarget;

	private float agentSpeed = 3f;

	private float agentSpeedCurrent;

	private float rbCheckHeightOffset = 0.8f;

	private Vector3 targetPosition;

	private float visionTimer;

	private bool visionPrevious;

	public Transform feetTransform;

	private float sampleNavMeshTimer;

	private Vector3 lookUnderPosition;

	private Vector3 lookUnderPositionNavmesh;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		hurtCollider.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		HeadSpringUpdate();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (enemy.CurrentState == EnemyState.Despawn && !enemy.IsStunned())
			{
				UpdateState(State.Despawn);
			}
			if (enemy.IsStunned())
			{
				UpdateState(State.Stun);
			}
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
			case State.SeekPlayer:
				StateSeekPlayer();
				break;
			case State.Sneak:
				StateSneak();
				break;
			case State.Notice:
				StateNotice();
				break;
			case State.AttackPlayer:
				StateAttackPlayer();
				break;
			case State.AttackPlayerOver:
				StateAttackPlayerOver();
				break;
			case State.AttackPlayerBackToNavMesh:
				StateAttackPlayerBackToNavMesh();
				break;
			case State.StuckAttackNotice:
				StateStuckAttackNotice();
				break;
			case State.StuckAttack:
				StateStuckAttack();
				break;
			case State.LookUnderStart:
				StateLookUnderStart();
				break;
			case State.LookUnder:
				StateLookUnder();
				break;
			case State.LookUnderStop:
				StateLookUnderStop();
				break;
			case State.Stun:
				StateStun();
				break;
			case State.Leave:
				StateLeave();
				break;
			case State.Despawn:
				StateDespawn();
				break;
			}
			RotationLogic();
			TimerLogic();
		}
	}

	public void StateSpawn()
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

	public void StateIdle()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = Random.Range(2f, 6f);
			enemy.NavMeshAgent.Warp(feetTransform.position);
			enemy.NavMeshAgent.ResetPath();
			StoreBackToNavMeshPosition();
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
				UpdateState(State.Leave);
			}
		}
	}

	public void StateRoam()
	{
		if (stateImpulse)
		{
			stateTimer = 5f;
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 10f, 25f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
			}
			if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				agentDestination = hit.position;
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
		}
		else
		{
			StoreBackToNavMeshPosition();
			enemy.NavMeshAgent.SetDestination(agentDestination);
			if (enemy.Rigidbody.notMovingTimer > 3f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (!enemy.Jump.jumping && (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 1f))
			{
				UpdateState(State.Idle);
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	public void StateInvestigate()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			enemy.Rigidbody.notMovingTimer = 0f;
		}
		else
		{
			StoreBackToNavMeshPosition();
			enemy.NavMeshAgent.SetDestination(agentDestination);
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f)
			{
				AttackNearestPhysObjectOrGoToIdle();
				return;
			}
			if (Vector3.Distance(base.transform.position, agentDestination) < 2f)
			{
				UpdateState(State.Idle);
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	public void StateSeekPlayer()
	{
		if (stateImpulse)
		{
			stateTimer = 20f;
			stateImpulse = false;
			targetPosition = base.transform.position;
			LevelPoint levelPointAhead = enemy.GetLevelPointAhead(targetPosition);
			if ((bool)levelPointAhead)
			{
				targetPosition = levelPointAhead.transform.position;
			}
			enemy.Rigidbody.notMovingTimer = 0f;
		}
		StoreBackToNavMeshPosition();
		enemy.NavMeshAgent.OverrideAgent(1f, enemy.NavMeshAgent.DefaultAcceleration, 0.2f);
		if (Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetPoint()) < 2f)
		{
			LevelPoint levelPointAhead2 = enemy.GetLevelPointAhead(targetPosition);
			if ((bool)levelPointAhead2)
			{
				targetPosition = levelPointAhead2.transform.position;
			}
		}
		if (enemy.Rigidbody.notMovingTimer >= 3f)
		{
			AttackNearestPhysObjectOrGoToIdle();
			return;
		}
		enemy.NavMeshAgent.SetDestination(targetPosition);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f || enemy.Rigidbody.notMovingTimer > 3f)
		{
			UpdateState(State.Roam);
		}
	}

	public void StateNotice()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 0.9f;
			enemy.NavMeshAgent.Warp(feetTransform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.AttackPlayer);
		}
	}

	public void StateSneak()
	{
		if (!targetPlayer)
		{
			UpdateState(State.Idle);
			return;
		}
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			enemy.Rigidbody.notMovingTimer = 0f;
			enemy.NavMeshAgent.Warp(feetTransform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		targetPosition = targetPlayer.transform.position;
		enemy.NavMeshAgent.SetDestination(targetPosition);
		enemy.NavMeshAgent.OverrideAgent(1f, enemy.NavMeshAgent.DefaultAcceleration, 0.2f);
		StoreBackToNavMeshPosition();
		stateTimer -= Time.deltaTime;
		if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			AttackNearestPhysObjectOrGoToIdle();
		}
		else if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
		else if (Vector3.Distance(feetTransform.position, enemy.NavMeshAgent.GetPoint()) < 2f || enemy.OnScreen.OnScreenAny)
		{
			UpdateState(State.Notice);
		}
	}

	public void StateAttackPlayer()
	{
		if (!targetPlayer)
		{
			UpdateState(State.SeekPlayer);
			return;
		}
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			agentSpeedCurrent = 0f;
			targetPosition = targetPlayer.transform.position;
			enemy.NavMeshAgent.SetDestination(targetPosition);
			return;
		}
		StoreBackToNavMeshPosition();
		agentSpeedCurrent = Mathf.Lerp(agentSpeedCurrent, agentSpeed, Time.deltaTime * 2f);
		enemy.NavMeshAgent.OverrideAgent(agentSpeedCurrent, enemy.NavMeshAgent.DefaultAcceleration, 0.2f);
		targetPosition = targetPlayer.transform.position;
		enemy.NavMeshAgent.SetDestination(targetPosition);
		stateTimer -= Time.deltaTime;
		if (!enemy.NavMeshAgent.CanReach(targetPlayer.transform.position, 0.25f) && Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) < 2f)
		{
			if (targetPlayer.transform.position.y > enemy.Rigidbody.transform.position.y - rbCheckHeightOffset)
			{
				enemy.Jump.StuckTrigger(targetPlayer.transform.position - enemy.Vision.VisionTransform.position);
			}
			if (!VisionBlocked() && !NavMesh.SamplePosition(targetPlayer.transform.position, out var _, 0.5f, -1) && targetPlayer.transform.position.y > feetTransform.position.y && !targetPlayer.isCrawling)
			{
				UpdateState(State.AttackPlayerOver);
				return;
			}
		}
		if (!enemy.Jump.jumping && enemy.Rigidbody.notMovingTimer > 2f)
		{
			enemy.Jump.StuckTrigger(targetPlayer.transform.position - feetTransform.position);
		}
		if (SemiFunc.EnemyLookUnderCondition(enemy, stateTimer, 1.5f, targetPlayer))
		{
			UpdateState(State.LookUnderStart);
		}
		else if (stateTimer <= 0f)
		{
			UpdateState(State.SeekPlayer);
		}
		else if (targetPlayer.isDisabled)
		{
			UpdateState(State.Idle);
		}
	}

	public void StateAttackPlayerOver()
	{
		if (!targetPlayer)
		{
			UpdateState(State.AttackPlayerBackToNavMesh);
			return;
		}
		if (stateImpulse)
		{
			stateTimer = 2f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, targetPlayer.transform.position, agentSpeed * Time.deltaTime);
		if (!enemy.Jump.jumping && (targetPlayer.transform.position.y > enemy.Rigidbody.transform.position.y - rbCheckHeightOffset || enemy.Rigidbody.notMovingTimer > 2f))
		{
			enemy.Jump.StuckTrigger(targetPlayer.transform.position - feetTransform.position);
			base.transform.position = Vector3.MoveTowards(base.transform.position, targetPlayer.transform.position, agentSpeed);
			enemy.Rigidbody.OverrideFollowRotation(0.5f, 0.25f);
		}
		if (NavMesh.SamplePosition(targetPlayer.transform.position, out var _, 0.5f, -1))
		{
			UpdateState(State.AttackPlayerBackToNavMesh);
		}
		else if (VisionBlocked() || targetPlayer.isDisabled || enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f || targetPlayer.isDisabled)
			{
				UpdateState(State.AttackPlayerBackToNavMesh);
			}
		}
	}

	public void StateAttackPlayerBackToNavMesh()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 30f;
		}
		stateTimer -= Time.deltaTime;
		if ((Vector3.Distance(base.transform.position, feetTransform.position) > 2f || enemy.Rigidbody.notMovingTimer > 2f) && !enemy.Jump.jumping)
		{
			Vector3 normalized = (feetTransform.position - backToNavMeshPosition).normalized;
			enemy.Jump.StuckTrigger(normalized);
			base.transform.position = feetTransform.position;
			base.transform.position += normalized * 2f;
			enemy.Rigidbody.OverrideFollowRotation(0.5f, 0.25f);
		}
		enemy.NavMeshAgent.Disable(0.1f);
		if (!enemy.Jump.jumping)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, backToNavMeshPosition, agentSpeed * Time.deltaTime);
		}
		if (Vector3.Distance(feetTransform.position, backToNavMeshPosition) <= 0.2f || NavMesh.SamplePosition(feetTransform.position, out var _, 0.5f, -1))
		{
			UpdateState(State.AttackPlayer);
		}
		else if (stateTimer <= 0f)
		{
			enemy.EnemyParent.SpawnedTimerSet(0f);
			UpdateState(State.Despawn);
		}
	}

	public void StateStuckAttackNotice()
	{
		if (stateImpulse)
		{
			stateTimer = 0.9f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.StuckAttack);
		}
	}

	public void StateStuckAttack()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(feetTransform.position);
			stateTimer = 1.5f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.Stop(0.2f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	public void StateLookUnderStart()
	{
		if (!targetPlayer)
		{
			UpdateState(State.SeekPlayer);
			return;
		}
		if (stateImpulse)
		{
			lookUnderPosition = targetPlayer.transform.position;
			lookUnderPositionNavmesh = targetPlayer.LastNavmeshPosition;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateTimer = 1f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.OverrideAgent(3f, 10f, 0.2f);
		enemy.NavMeshAgent.SetDestination(lookUnderPositionNavmesh);
		if (Vector3.Distance(base.transform.position, lookUnderPositionNavmesh) < 0.5f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.LookUnder);
			}
		}
		else if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			UpdateState(State.SeekPlayer);
		}
	}

	public void StateLookUnder()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
		}
		stateTimer -= Time.deltaTime;
		enemy.Vision.StandOverride(0.25f);
		if (stateTimer <= 0f)
		{
			UpdateState(State.LookUnderStop);
		}
	}

	public void StateLookUnderStop()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 0.9f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.SeekPlayer);
		}
	}

	public void StateStun()
	{
		StoreBackToNavMeshPosition();
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = enemy.Rigidbody.transform.position;
		if (!enemy.IsStunned())
		{
			UpdateState(State.Idle);
		}
	}

	public void StateLeave()
	{
		if (stateImpulse)
		{
			stateTimer = 5f;
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGetPlayerDistance(base.transform.position, 30f, 50f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
			}
			if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				agentDestination = hit.position;
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			SemiFunc.EnemyLeaveStart(enemy);
			stateImpulse = false;
		}
		if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			stateTimer -= Time.deltaTime;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		enemy.NavMeshAgent.OverrideAgent(1f, enemy.NavMeshAgent.DefaultAcceleration, 0.2f);
		if (Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetPoint()) < 1f || stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	public void StateDespawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			enemy.NavMeshAgent.Warp(feetTransform.position);
			enemy.NavMeshAgent.ResetPath();
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
		animator.sfxHurt.Play(animator.transform.position);
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.Leave)
		{
			UpdateState(State.Idle);
		}
	}

	public void OnDeath()
	{
		hayParticlesBig.transform.position = enemy.CenterTransform.position;
		hayParticlesBig.Play();
		hayParticlesSmall.transform.position = enemy.CenterTransform.position;
		hayParticlesSmall.Play();
		bitsParticlesFar.transform.position = enemy.CenterTransform.position;
		bitsParticlesFar.Play();
		bitsParticlesShort.transform.position = enemy.CenterTransform.position;
		bitsParticlesShort.Play();
		animator.sfxDeath.Play(animator.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.05f);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnInvestigate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate)
			{
				agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
				UpdateState(State.Investigate);
			}
			else if (currentState == State.SeekPlayer)
			{
				targetPosition = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			}
		}
	}

	public void OnVision()
	{
		if (enemy.CurrentState == EnemyState.Despawn)
		{
			return;
		}
		if (currentState == State.Roam || currentState == State.Idle || currentState == State.Investigate || currentState == State.SeekPlayer || currentState == State.Leave)
		{
			targetPlayer = enemy.Vision.onVisionTriggeredPlayer;
			if (!enemy.OnScreen.OnScreenAny)
			{
				UpdateState(State.Sneak);
			}
			else
			{
				UpdateState(State.Notice);
			}
			if (GameManager.Multiplayer())
			{
				photonView.RPC("TargetPlayerRPC", RpcTarget.All, targetPlayer.photonView.ViewID);
			}
		}
		else if (currentState == State.AttackPlayer || currentState == State.AttackPlayerOver || currentState == State.Sneak)
		{
			if (targetPlayer == enemy.Vision.onVisionTriggeredPlayer)
			{
				stateTimer = 2f;
			}
		}
		else if (currentState == State.LookUnderStart)
		{
			if (targetPlayer == enemy.Vision.onVisionTriggeredPlayer && !targetPlayer.isCrawling)
			{
				UpdateState(State.AttackPlayer);
			}
		}
		else if (currentState == State.LookUnder && targetPlayer == enemy.Vision.onVisionTriggeredPlayer)
		{
			if (targetPlayer.isCrawling)
			{
				lookUnderPosition = targetPlayer.transform.position;
			}
			else
			{
				UpdateState(State.LookUnderStop);
			}
		}
	}

	public void OnGrabbed()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.Leave)
		{
			targetPlayer = enemy.Rigidbody.onGrabbedPlayerAvatar;
			UpdateState(State.Notice);
			if (GameManager.Multiplayer())
			{
				photonView.RPC("TargetPlayerRPC", RpcTarget.All, targetPlayer.photonView.ViewID);
			}
		}
	}

	private void UpdateState(State _state)
	{
		if (currentState != _state)
		{
			enemy.Rigidbody.notMovingTimer = 0f;
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

	private void AttackNearestPhysObjectOrGoToIdle()
	{
		stuckAttackTarget = Vector3.zero;
		if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			stuckAttackTarget = SemiFunc.EnemyGetNearestPhysObject(enemy);
		}
		if (stuckAttackTarget != Vector3.zero)
		{
			UpdateState(State.StuckAttackNotice);
		}
		else
		{
			UpdateState(State.Idle);
		}
	}

	private void HeadSpringUpdate()
	{
		headSpringSource.rotation = SemiFunc.SpringQuaternionGet(headSpring, headSpringTarget.rotation);
	}

	private void RotationLogic()
	{
		if (currentState != State.LookUnderStop)
		{
			if (currentState == State.Notice || currentState == State.AttackPlayer || currentState == State.AttackPlayerOver)
			{
				if ((bool)targetPlayer && Vector3.Distance(targetPlayer.transform.position, enemy.Rigidbody.transform.position) > 0.1f)
				{
					rotationTarget = Quaternion.LookRotation(targetPlayer.transform.position - enemy.Rigidbody.transform.position);
					rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
				}
			}
			else if (currentState == State.StuckAttack)
			{
				if (Vector3.Distance(stuckAttackTarget, enemy.Rigidbody.transform.position) > 0.1f)
				{
					rotationTarget = Quaternion.LookRotation(stuckAttackTarget - enemy.Rigidbody.transform.position);
					rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
				}
			}
			else if (currentState == State.LookUnderStart || currentState == State.LookUnder)
			{
				if (Vector3.Distance(lookUnderPosition, base.transform.position) > 0.1f)
				{
					rotationTarget = Quaternion.LookRotation(lookUnderPosition - base.transform.position);
					rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
				}
			}
			else if (enemy.NavMeshAgent.AgentVelocity.normalized.magnitude > 0.1f)
			{
				rotationTarget = Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized);
				rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
			}
		}
		if (currentState == State.Roam || currentState == State.Investigate)
		{
			rotationSpring.speed = 3f;
		}
		else
		{
			rotationSpring.speed = 10f;
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(rotationSpring, rotationTarget);
	}

	private bool VisionBlocked()
	{
		if (visionTimer <= 0f && (bool)targetPlayer)
		{
			visionTimer = 0.1f;
			Vector3 direction = targetPlayer.PlayerVisionTarget.VisionTransform.position - enemy.Vision.VisionTransform.position;
			visionPrevious = Physics.Raycast(enemy.Vision.VisionTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"));
		}
		return visionPrevious;
	}

	private void TimerLogic()
	{
		visionTimer -= Time.deltaTime;
		sampleNavMeshTimer -= Time.deltaTime;
	}

	private void StoreBackToNavMeshPosition()
	{
		if (sampleNavMeshTimer <= 0f)
		{
			sampleNavMeshTimer = 0.5f;
			if (NavMesh.SamplePosition(base.transform.position, out var hit, 0.5f, -1))
			{
				backToNavMeshPosition = hit.position;
			}
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
				animator.OnSpawn();
			}
		}
	}

	[PunRPC]
	private void TargetPlayerRPC(int _playerID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.photonView.ViewID == _playerID)
			{
				targetPlayer = player;
				break;
			}
		}
	}
}
