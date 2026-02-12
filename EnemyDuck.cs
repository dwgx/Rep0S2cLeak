using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDuck : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		Notice,
		GoToPlayer,
		GoToPlayerOver,
		GoToPlayerUnder,
		FlyBackToNavmesh,
		FlyBackToNavmeshStop,
		MoveBackToNavmesh,
		AttackStart,
		Transform,
		ChaseNavmesh,
		ChaseTowards,
		ChaseMoveBack,
		DeTransform,
		Leave,
		Stun,
		Despawn
	}

	private PhotonView photonView;

	public State currentState;

	private bool stateImpulse;

	public float stateTimer;

	private float stateTicker;

	private Vector3 targetPosition;

	private float pitCheckTimer;

	private bool pitCheck;

	private Vector3 agentDestination;

	private bool visionPrevious;

	private float visionTimer;

	private Vector3 moveBackPosition;

	private float moveBackTimer;

	private float targetForwardOffset = 1.5f;

	public Transform followOffsetTransform;

	[Space]
	public SpringQuaternion bodySpring;

	public Transform bodyTransform;

	public Transform bodyTargetTransform;

	[Space]
	public EnemyDuckAnim anim;

	public Enemy enemy;

	public ParticleSystem featherParticles;

	private PlayerAvatar playerTarget;

	[Space]
	private Quaternion rotationTarget;

	public SpringQuaternion rotationSpring;

	[Space]
	public SpringQuaternion headLookAtSpring;

	public Transform headLookAtTarget;

	public Transform headLookAtSource;

	private float targetedPlayerTime;

	private float targetedPlayerTimeMax = 120f;

	internal bool idleBreakerTrigger;

	private float chaseTimer;

	private float annoyingJumpPauseTimer;

	private bool duckBucketActive;

	private float duckBucketTimer;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		bodyTransform.rotation = SemiFunc.SpringQuaternionGet(bodySpring, bodyTargetTransform.rotation);
		HeadLookAtLogic();
		if ((GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient) || !LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (enemy.IsStunned())
		{
			UpdateState(State.Stun);
		}
		else if (enemy.CurrentState == EnemyState.Despawn)
		{
			UpdateState(State.Despawn);
		}
		if (!playerTarget)
		{
			if (currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder)
			{
				UpdateState(State.Idle);
			}
			else if (currentState == State.ChaseNavmesh || currentState == State.ChaseTowards || currentState == State.ChaseMoveBack || currentState == State.Transform)
			{
				UpdateState(State.DeTransform);
			}
		}
		RotationLogic();
		TimerLogic();
		GravityLogic();
		TargetPositionLogic();
		FollowOffsetLogic();
		FlyBackConditionLogic();
		DuckBucketLogic();
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
		case State.Notice:
			StateNotice();
			break;
		case State.GoToPlayer:
			StateGoToPlayer();
			break;
		case State.GoToPlayerUnder:
			StateGoToPlayerUnder();
			break;
		case State.GoToPlayerOver:
			StateGoToPlayerOver();
			break;
		case State.MoveBackToNavmesh:
			StateMoveBackToNavMesh();
			break;
		case State.FlyBackToNavmesh:
			StateFlyBackToNavmesh();
			break;
		case State.FlyBackToNavmeshStop:
			StateFlyBackToNavmeshStop();
			break;
		case State.AttackStart:
			StateAttackStart();
			break;
		case State.Transform:
			StateTransform();
			break;
		case State.ChaseNavmesh:
			StateChaseNavmesh();
			break;
		case State.ChaseTowards:
			StateChaseTowards();
			break;
		case State.ChaseMoveBack:
			StateChaseMoveBack();
			break;
		case State.DeTransform:
			StateDeTransform();
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
	}

	private void StateSpawn()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateImpulse = false;
			stateTimer = 2f;
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
			stateTimer = Random.Range(2f, 5f);
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
			LeaveCheck(_setLeave: true);
		}
	}

	private void StateRoam()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			playerTarget = null;
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
			enemy.Rigidbody.notMovingTimer = 0f;
		}
		else
		{
			SemiFunc.EnemyCartJump(enemy);
			MoveBackPosition();
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f || !enemy.NavMeshAgent.HasPath())
			{
				SemiFunc.EnemyCartJumpReset(enemy);
				UpdateState(State.Idle);
			}
		}
		LeaveCheck(_setLeave: true);
	}

	private void StateInvestigate()
	{
		if (stateImpulse)
		{
			stateTimer = 5f;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
		}
		else
		{
			enemy.NavMeshAgent.SetDestination(agentDestination);
			SemiFunc.EnemyCartJump(enemy);
			MoveBackPosition();
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f || !enemy.NavMeshAgent.HasPath())
			{
				SemiFunc.EnemyCartJumpReset(enemy);
				UpdateState(State.Idle);
			}
		}
		LeaveCheck(_setLeave: true);
	}

	private void StateNotice()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			stateImpulse = false;
			stateTimer = 1f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.GoToPlayer);
		}
	}

	private void StateGoToPlayer()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			annoyingJumpPauseTimer = 1f;
		}
		enemy.NavMeshAgent.SetDestination(targetPosition);
		stateTimer -= Time.deltaTime;
		SemiFunc.EnemyCartJump(enemy);
		MoveBackPosition();
		enemy.Vision.StandOverride(0.25f);
		if (stateTimer <= 0f || !playerTarget || playerTarget.isDisabled)
		{
			UpdateState(State.Idle);
			return;
		}
		if (!enemy.NavMeshAgent.CanReach(targetPosition, 1f) && Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) < 2f && !VisionBlocked() && !NavMesh.SamplePosition(targetPosition, out var _, 0.5f, -1))
		{
			if (playerTarget.isCrawling && Mathf.Abs(targetPosition.y - enemy.Rigidbody.transform.position.y) < 0.3f && !enemy.Jump.jumping)
			{
				UpdateState(State.GoToPlayerUnder);
				return;
			}
			if (targetPosition.y > enemy.Rigidbody.transform.position.y)
			{
				UpdateState(State.GoToPlayerOver);
				return;
			}
		}
		AnnoyingJump();
		LeaveCheck(_setLeave: true);
	}

	private void StateGoToPlayerUnder()
	{
		if (stateImpulse)
		{
			stateTimer = 2f;
			stateImpulse = false;
			annoyingJumpPauseTimer = 1f;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, enemy.NavMeshAgent.DefaultSpeed * 0.5f * Time.deltaTime);
		SemiFunc.EnemyCartJump(enemy);
		enemy.Vision.StandOverride(0.25f);
		if (NavMesh.SamplePosition(targetPosition, out var _, 0.5f, -1))
		{
			UpdateState(State.MoveBackToNavmesh);
		}
		else if (VisionBlocked() || !playerTarget || playerTarget.isDisabled)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.MoveBackToNavmesh);
			}
		}
		else
		{
			stateTimer = 2f;
		}
		if (LeaveCheck(_setLeave: false))
		{
			UpdateState(State.MoveBackToNavmesh);
		}
	}

	private void StateGoToPlayerOver()
	{
		if (stateImpulse)
		{
			stateTimer = 2f;
			stateImpulse = false;
			annoyingJumpPauseTimer = 1f;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, enemy.NavMeshAgent.DefaultSpeed * 0.5f * Time.deltaTime);
		SemiFunc.EnemyCartJump(enemy);
		enemy.Vision.StandOverride(0.25f);
		if (playerTarget.PlayerVisionTarget.VisionTransform.position.y > enemy.Rigidbody.transform.position.y + 1.5f)
		{
			if (!enemy.Jump.jumping)
			{
				Vector3 normalized = (playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.Rigidbody.transform.position).normalized;
				enemy.Jump.StuckTrigger(normalized);
				base.transform.position = enemy.Rigidbody.transform.position;
				base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, 2f);
			}
		}
		else
		{
			AnnoyingJump();
		}
		if (NavMesh.SamplePosition(targetPosition, out var _, 0.5f, -1))
		{
			UpdateState(State.MoveBackToNavmesh);
		}
		else if (VisionBlocked() || !playerTarget || playerTarget.isDisabled)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f || enemy.Rigidbody.notMovingTimer > 1f)
			{
				UpdateState(State.MoveBackToNavmesh);
			}
		}
		else
		{
			stateTimer = 2f;
		}
		if (LeaveCheck(_setLeave: false))
		{
			UpdateState(State.MoveBackToNavmesh);
		}
	}

	private void StateMoveBackToNavMesh()
	{
		if (stateImpulse)
		{
			stateTimer = 30f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		if (!enemy.Jump.jumping)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, moveBackPosition, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
		}
		SemiFunc.EnemyCartJump(enemy);
		enemy.Vision.StandOverride(0.25f);
		if ((Vector3.Distance(base.transform.position, enemy.Rigidbody.transform.position) > 2f || enemy.Rigidbody.notMovingTimer > 2f) && !enemy.Jump.jumping)
		{
			Vector3 normalized = (moveBackPosition - enemy.Rigidbody.transform.position).normalized;
			enemy.Jump.StuckTrigger(normalized);
			base.transform.position = enemy.Rigidbody.transform.position;
			base.transform.position += normalized * 2f;
		}
		stateTimer -= Time.deltaTime;
		if (Vector3.Distance(enemy.Rigidbody.transform.position, moveBackPosition) <= 0.5f || NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out var _, 0.5f, -1))
		{
			UpdateState(State.GoToPlayer);
		}
		else if (stateTimer <= 0f)
		{
			enemy.EnemyParent.SpawnedTimerSet(0f);
			UpdateState(State.Despawn);
		}
	}

	private void StateFlyBackToNavmesh()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 30f;
			stateTicker = 0f;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, moveBackPosition + Vector3.up * 0.5f, 0.75f * Time.deltaTime);
		enemy.Rigidbody.OverrideFollowPosition(0.1f, 1f);
		enemy.Rigidbody.OverrideFollowRotation(0.1f, 0.25f);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, base.transform.position) > 2f)
		{
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		if (stateTicker <= 0f)
		{
			stateTicker = 0.25f;
			if (Physics.Raycast(enemy.Rigidbody.transform.position, Vector3.down, out var hitInfo, 5f, LayerMask.GetMask("Default")) && Physics.Raycast(enemy.Rigidbody.transform.position + enemy.Rigidbody.transform.forward * 0.5f, Vector3.down, out hitInfo, 5f, LayerMask.GetMask("Default")) && NavMesh.SamplePosition(hitInfo.point, out var hit, 0.5f, -1))
			{
				moveBackPosition = hit.position;
				UpdateState(State.FlyBackToNavmeshStop);
				return;
			}
		}
		else
		{
			stateTicker -= Time.deltaTime;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Despawn);
		}
	}

	private void StateFlyBackToNavmeshStop()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.Warp(moveBackPosition);
			enemy.NavMeshAgent.ResetPath();
			base.transform.position = moveBackPosition;
			stateTimer = 1f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateAttackStart()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 0.5f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			enemy.Rigidbody.GrabRelease();
			UpdateState(State.Transform);
		}
	}

	private void StateTransform()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1.4f;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		Vector3 target = new Vector3(base.transform.position.x, playerTarget.PlayerVisionTarget.VisionTransform.position.y, base.transform.position.z);
		base.transform.position = Vector3.MoveTowards(base.transform.position, target, Time.deltaTime);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.ChaseNavmesh);
		}
	}

	private void StateChaseNavmesh()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
		}
		enemy.NavMeshAgent.OverrideAgent(10f, 10f, 0.1f);
		enemy.NavMeshAgent.SetDestination(playerTarget.transform.position);
		if (!VisionBlocked())
		{
			UpdateState(State.ChaseTowards);
			return;
		}
		MoveBackPosition();
		ChaseStop();
	}

	private void StateChaseTowards()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
		}
		if (VisionBlocked())
		{
			UpdateState(State.ChaseMoveBack);
			return;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, playerTarget.localCamera.transform.position + Vector3.down * 0.31f, 5f * Time.deltaTime);
		ChaseStop();
	}

	private void StateChaseMoveBack()
	{
		if (stateImpulse)
		{
			if (Physics.Raycast(enemy.Rigidbody.transform.position, Vector3.down, out var hitInfo, 5f, LayerMask.GetMask("Default")) && NavMesh.SamplePosition(hitInfo.point, out var hit, 0.5f, -1))
			{
				moveBackPosition = hit.position;
			}
			stateImpulse = false;
			stateTimer = 10f;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, moveBackPosition, 5f * Time.deltaTime);
		if (Vector3.Distance(base.transform.position, enemy.Rigidbody.transform.position) > 2f || enemy.Rigidbody.notMovingTimer > 2f)
		{
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		stateTimer -= Time.deltaTime;
		if (Vector3.Distance(enemy.Rigidbody.transform.position, moveBackPosition) <= 1f || NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out var _, 0.5f, -1))
		{
			UpdateState(State.ChaseNavmesh);
		}
		else
		{
			ChaseStop();
		}
	}

	private void StateDeTransform()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateImpulse = false;
			stateTimer = 2f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.MoveBackToNavmesh);
		}
	}

	private void StateStun()
	{
		if (enemy.IsStunned())
		{
			return;
		}
		PlayerAvatar playerAvatar = null;
		float num = 999f;
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if ((bool)item && !item.isDisabled)
			{
				float num2 = Vector3.Distance(base.transform.position, item.transform.position);
				if (num2 < 10f && num2 < num)
				{
					num = num2;
					playerAvatar = item;
				}
			}
		}
		if ((bool)playerAvatar)
		{
			if ((bool)enemy.Vision.onVisionTriggeredPlayer)
			{
				playerTarget = enemy.Vision.onVisionTriggeredPlayer;
			}
			else
			{
				playerTarget = playerAvatar;
			}
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, playerTarget.photonView.ViewID);
			}
			UpdateState(State.AttackStart);
		}
		else
		{
			UpdateState(State.Idle);
		}
	}

	private void StateLeave()
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
			if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 1f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				agentDestination = hit.position;
				flag = true;
			}
			if (flag)
			{
				enemy.NavMeshAgent.SetDestination(agentDestination);
				enemy.Rigidbody.notMovingTimer = 0f;
				stateImpulse = false;
			}
			SemiFunc.EnemyLeaveStart(enemy);
		}
		else
		{
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			SemiFunc.EnemyCartJump(enemy);
			if (Vector3.Distance(base.transform.position, agentDestination) < 1f || stateTimer <= 0f)
			{
				SemiFunc.EnemyCartJumpReset(enemy);
				UpdateState(State.Idle);
			}
		}
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateImpulse = false;
		}
	}

	public void OnInvestigate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && (currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate))
		{
			agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			UpdateState(State.Investigate);
		}
	}

	public void OnVision()
	{
		if ((currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate || currentState == State.Leave) && !enemy.Jump.jumping)
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				playerTarget = enemy.Vision.onVisionTriggeredPlayer;
				if (SemiFunc.IsMultiplayer())
				{
					photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, playerTarget.photonView.ViewID);
				}
				UpdateState(State.Notice);
			}
		}
		else if (currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder)
		{
			stateTimer = 2f;
		}
	}

	public void OnHurt()
	{
		anim.soundHurtPauseTimer = 0.5f;
		anim.hurtSound.Play(enemy.CenterTransform.position);
		if (!enemy.IsStunned() && (bool)playerTarget && (currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder))
		{
			UpdateState(State.AttackStart);
		}
	}

	public void OnDeath()
	{
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.05f);
		featherParticles.transform.position = enemy.CenterTransform.position;
		featherParticles.Play();
		anim.deathSound.Play(enemy.CenterTransform.position);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnGrabbed()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState != State.AttackStart && currentState != State.Transform && currentState != State.ChaseNavmesh && currentState != State.ChaseTowards && currentState != State.ChaseMoveBack && currentState != State.DeTransform && currentState != State.Stun && currentState != State.Despawn)
		{
			playerTarget = enemy.Rigidbody.onGrabbedPlayerAvatar;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, playerTarget.photonView.ViewID);
			}
			UpdateState(State.AttackStart);
		}
	}

	public void OnObjectHurt()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && (bool)enemy.Health.onObjectHurtPlayer && currentState != State.AttackStart && currentState != State.Transform && currentState != State.ChaseNavmesh && currentState != State.ChaseTowards && currentState != State.ChaseMoveBack && currentState != State.DeTransform && currentState != State.Stun && currentState != State.Despawn)
		{
			playerTarget = enemy.Health.onObjectHurtPlayer;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, playerTarget.photonView.ViewID);
			}
			UpdateState(State.AttackStart);
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

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
	}

	public void TargetPositionLogic()
	{
		if ((currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder) && (bool)playerTarget)
		{
			Vector3 vector = playerTarget.transform.position + playerTarget.transform.forward * targetForwardOffset;
			if (pitCheckTimer <= 0f)
			{
				pitCheckTimer = 0.1f;
				pitCheck = !Physics.Raycast(vector + Vector3.up, Vector3.down, 4f, LayerMask.GetMask("Default"));
			}
			else
			{
				pitCheckTimer -= Time.deltaTime;
			}
			if (pitCheck)
			{
				vector = playerTarget.transform.position;
			}
			targetPosition = Vector3.Lerp(targetPosition, vector, 20f * Time.deltaTime);
		}
	}

	private void AnnoyingJump()
	{
		if (!enemy.Jump.jumping && !(annoyingJumpPauseTimer > 0f) && playerTarget.PlayerVisionTarget.VisionTransform.position.y > enemy.Rigidbody.transform.position.y && enemy.Rigidbody.timeSinceStun > 2f)
		{
			Vector3 vector = playerTarget.localCamera.transform.position + playerTarget.localCamera.transform.forward;
			float num = new Vector3(enemy.Rigidbody.transform.position.x, vector.y, enemy.Rigidbody.transform.position.z).y - enemy.CenterTransform.position.y;
			if (!enemy.OnScreen.GetOnScreen(playerTarget) && num > 1f && !playerTarget.isMoving)
			{
				enemy.Jump.StuckTrigger(targetPosition - enemy.Vision.VisionTransform.position);
			}
		}
	}

	private void RotationLogic()
	{
		if ((bool)playerTarget && (currentState == State.Notice || currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder))
		{
			if ((!VisionBlocked() && !playerTarget.isMoving && enemy.Rigidbody.velocity.magnitude < 0.5f) || enemy.Jump.jumping)
			{
				rotationTarget = Quaternion.LookRotation(playerTarget.transform.position - enemy.Rigidbody.transform.position);
				rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
			}
			else if (enemy.Rigidbody.velocity.magnitude > 0.1f)
			{
				rotationTarget = Quaternion.LookRotation(enemy.Rigidbody.velocity.normalized);
				rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
			}
		}
		else if ((bool)playerTarget && (currentState == State.ChaseNavmesh || currentState == State.ChaseTowards || currentState == State.Transform))
		{
			rotationTarget = Quaternion.LookRotation(playerTarget.transform.position - enemy.Rigidbody.transform.position);
			rotationTarget.eulerAngles = new Vector3(rotationTarget.eulerAngles.x, rotationTarget.eulerAngles.y, rotationTarget.eulerAngles.z);
		}
		else if (enemy.Rigidbody.velocity.magnitude > 0.1f)
		{
			rotationTarget = Quaternion.LookRotation(enemy.Rigidbody.velocity.normalized);
			if (currentState != State.ChaseMoveBack)
			{
				rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
			}
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(rotationSpring, rotationTarget);
	}

	private void GravityLogic()
	{
		if (currentState == State.ChaseNavmesh || currentState == State.ChaseTowards || currentState == State.ChaseMoveBack || currentState == State.Transform || currentState == State.FlyBackToNavmesh)
		{
			enemy.Rigidbody.gravity = false;
		}
		else
		{
			enemy.Rigidbody.gravity = true;
		}
	}

	private void MoveBackPosition()
	{
		if (moveBackTimer <= 0f)
		{
			moveBackTimer = 0.1f;
			if (NavMesh.SamplePosition(base.transform.position, out var hit, 0.5f, -1) && Physics.Raycast(base.transform.position, Vector3.down, 2f, LayerMask.GetMask("Default")))
			{
				moveBackPosition = hit.position;
			}
		}
	}

	private bool VisionBlocked()
	{
		if (visionTimer <= 0f)
		{
			visionTimer = 0.25f;
			Vector3 direction = playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.CenterTransform.position;
			visionPrevious = Physics.Raycast(enemy.CenterTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
		}
		return visionPrevious;
	}

	private void TimerLogic()
	{
		visionTimer -= Time.deltaTime;
		moveBackTimer -= Time.deltaTime;
		annoyingJumpPauseTimer -= Time.deltaTime;
		if (currentState == State.ChaseNavmesh || currentState == State.ChaseTowards || currentState == State.ChaseMoveBack)
		{
			chaseTimer += Time.deltaTime;
		}
		else
		{
			chaseTimer = 0f;
		}
		if (currentState == State.Spawn)
		{
			targetedPlayerTime = 0f;
		}
		if (currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder)
		{
			targetedPlayerTime += Time.deltaTime;
			return;
		}
		targetedPlayerTime -= 5f * Time.deltaTime;
		targetedPlayerTime = Mathf.Max(0f, targetedPlayerTime);
	}

	private void HeadLookAtLogic()
	{
		bool flag = false;
		if ((currentState == State.Notice || currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder) && (bool)playerTarget && !playerTarget.isDisabled)
		{
			flag = true;
		}
		if (flag)
		{
			Vector3 direction = playerTarget.PlayerVisionTarget.VisionTransform.position - headLookAtTarget.position;
			direction = SemiFunc.ClampDirection(direction, headLookAtTarget.forward, 60f);
			headLookAtSource.rotation = SemiFunc.SpringQuaternionGet(headLookAtSpring, Quaternion.LookRotation(direction));
		}
		else
		{
			headLookAtSource.rotation = SemiFunc.SpringQuaternionGet(headLookAtSpring, headLookAtTarget.rotation);
		}
	}

	private bool LeaveCheck(bool _setLeave)
	{
		if (SemiFunc.EnemyForceLeave(enemy) || targetedPlayerTime >= targetedPlayerTimeMax)
		{
			if (_setLeave)
			{
				UpdateState(State.Leave);
			}
			return true;
		}
		return false;
	}

	public void IdleBreakerSet()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("IdleBreakerSetRPC", RpcTarget.All);
		}
		else
		{
			IdleBreakerSetRPC();
		}
	}

	private void ChaseStop()
	{
		if (chaseTimer >= 10f || !playerTarget || playerTarget.isDisabled)
		{
			UpdateState(State.DeTransform);
		}
	}

	private void FollowOffsetLogic()
	{
		if (currentState == State.ChaseNavmesh || currentState == State.ChaseMoveBack || currentState == State.FlyBackToNavmesh)
		{
			followOffsetTransform.localPosition = Vector3.Lerp(followOffsetTransform.localPosition, Vector3.up * 0.75f, 5f * Time.deltaTime);
		}
		else if (currentState == State.FlyBackToNavmeshStop)
		{
			followOffsetTransform.localPosition = Vector3.zero;
		}
		else
		{
			followOffsetTransform.localPosition = Vector3.Lerp(followOffsetTransform.localPosition, Vector3.zero, 10f * Time.deltaTime);
		}
	}

	private void FlyBackConditionLogic()
	{
		if ((currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate || currentState == State.Notice || currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder || currentState == State.MoveBackToNavmesh) && enemy.Rigidbody.transform.position.y - moveBackPosition.y < -4f)
		{
			UpdateState(State.FlyBackToNavmesh);
		}
	}

	private void DuckBucketLogic()
	{
		if (duckBucketActive)
		{
			enemy.Vision.DisableVision(0.25f);
			enemy.EnemyParent.SpawnedTimerPause(0.25f);
			enemy.NavMeshAgent.OverrideAgent(0.5f, 0.5f, 0.25f);
			enemy.Jump.GapJumpOverride(0.25f, 1f, 1f);
			enemy.Jump.StuckJumpOverride(0.25f, 1f, 1f);
			enemy.Jump.SurfaceJumpOverride(0.25f, 1f, 1f);
			if (duckBucketTimer <= 0f)
			{
				duckBucketActive = false;
			}
			duckBucketTimer -= Time.deltaTime;
		}
	}

	public void DuckBucketActive()
	{
		if (currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate || currentState == State.Notice || currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder || currentState == State.MoveBackToNavmesh || currentState == State.Leave)
		{
			duckBucketActive = true;
			duckBucketTimer = 0.25f;
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
				anim.OnSpawn();
			}
		}
	}

	[PunRPC]
	private void UpdatePlayerTargetRPC(int _photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item.photonView.ViewID == _photonViewID)
			{
				playerTarget = item;
				break;
			}
		}
	}

	[PunRPC]
	private void IdleBreakerSetRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			idleBreakerTrigger = true;
		}
	}
}
