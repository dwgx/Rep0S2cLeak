using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHeadGrabber : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		IdleBreaker,
		Roam,
		RoamFast,
		Investigate,
		InvestigateFast,
		Notice,
		Attack,
		CantReach,
		GotoPlayer,
		GotoPlayerOver,
		GotoHead,
		GotoHeadOver,
		BackToNavmesh,
		GrabHead,
		ReleaseHead,
		Leave,
		Stun,
		Despawn
	}

	public State currentState;

	public float stateTimer;

	private bool stateImpulse;

	internal State previousState;

	internal Enemy enemy;

	internal PhotonView photonView;

	public EnemyHeadGrabberAnim anim;

	public SphereCollider topCollider;

	private bool topColliderActive;

	public SpringQuaternion horizontalRotationSpring;

	private Quaternion horizontalRotationTarget = Quaternion.identity;

	internal PlayerAvatar playerTarget;

	internal PlayerDeathHead headTarget;

	internal bool headTargetActive;

	internal bool headTargetRelease;

	internal float headTargetTime;

	private bool headTargetActivePrevious;

	internal bool headTargetActiveLocal;

	private float headTargetCooldown;

	private bool nearbyHeadLogic;

	[Space]
	public Transform headTargetPositionTransform;

	public Transform headTargetPositionFollowTransform;

	public Transform headTargetPositionReleaseTransform;

	public AnimationCurve headTargetPositionCurve;

	[Space]
	public GameObject localCameraPrefab;

	private float headTargetPositionLerp;

	private Vector3 headTargetOriginalPosition;

	private Quaternion headTargetOriginalRotation;

	private Vector3 agentDestination;

	private Vector3 backToNavmeshPosition;

	private bool visionPrevious;

	private float visionPreviousTime;

	private int idleBreakerIndex;

	private int idleBreakerIndexPrevious;

	private int attackIndex;

	private int attackIndexPrevious;

	private float attackCooldown;

	private int attacksInRow;

	internal float rotationStopTimer;

	private float dropkickHitTimeLast;

	private bool localMeshHidden;

	private EnemyHeadGrabberLocalCamera localCameraObject;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		DeathHeadLogicShared();
		LocalMeshLogicShared();
		if ((!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient) && LevelGenerator.Instance.Generated)
		{
			if (enemy.IsStunned())
			{
				UpdateState(State.Stun);
			}
			else if (enemy.CurrentState == EnemyState.Despawn)
			{
				UpdateState(State.Despawn);
			}
			switch (currentState)
			{
			case State.Spawn:
				StateSpawn();
				break;
			case State.Idle:
				StateIdle();
				break;
			case State.IdleBreaker:
				StateIdleBreaker();
				break;
			case State.Roam:
				StateRoam();
				break;
			case State.RoamFast:
				StateRoamFast();
				break;
			case State.Investigate:
				StateInvestigate();
				break;
			case State.InvestigateFast:
				StateInvestigateFast();
				break;
			case State.Notice:
				StateNotice();
				break;
			case State.CantReach:
				StateCantReach();
				break;
			case State.GotoPlayer:
				StateGotoPlayer();
				break;
			case State.GotoPlayerOver:
				StateGotoPlayerOver();
				break;
			case State.GotoHead:
				StateGotoHead();
				break;
			case State.GotoHeadOver:
				StateGotoHeadOver();
				break;
			case State.BackToNavmesh:
				StateBackToNavmesh();
				break;
			case State.GrabHead:
				StateGrabHead();
				break;
			case State.ReleaseHead:
				StateReleaseHead();
				break;
			case State.Attack:
				StateAttack();
				break;
			case State.Leave:
				StateLeave();
				break;
			case State.Stun:
				StateStun();
				break;
			case State.Despawn:
				StateDespawn();
				break;
			}
			RotationLogic();
			BackToNavmeshPosition();
			DeathHeadLogic();
			TimerLogic();
		}
	}

	private void StateSpawn()
	{
		if (stateImpulse)
		{
			ResetAgent();
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
			ResetAgent();
			stateImpulse = false;
			stateTimer = Random.Range(2f, 8f);
		}
		if (SemiFunc.EnemySpawnIdlePause() || NearbyHeadLogic())
		{
			return;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			if (headTargetTime >= 60f)
			{
				UpdateState(State.ReleaseHead);
			}
			else if (Random.Range(0, 3) == 0)
			{
				for (idleBreakerIndex = idleBreakerIndexPrevious; idleBreakerIndex == idleBreakerIndexPrevious; idleBreakerIndex = Random.Range(0, 3))
				{
				}
				idleBreakerIndexPrevious = idleBreakerIndex;
				if (GameManager.Multiplayer())
				{
					photonView.RPC("IdleBreakerSetRPC", RpcTarget.All, idleBreakerIndex);
				}
				else
				{
					IdleBreakerSetRPC(idleBreakerIndex);
				}
				UpdateState(State.IdleBreaker);
			}
			else if (Random.Range(0, 5) == 0)
			{
				UpdateState(State.RoamFast);
			}
			else
			{
				UpdateState(State.Roam);
			}
		}
		else if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateIdleBreaker()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1.5f;
			ResetAgent();
			RigidbodyReset();
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateRoam()
	{
		if (stateImpulse)
		{
			stateTimer = 5f;
			if (!SemiFunc.EnemyRoamPoint(enemy, out agentDestination))
			{
				return;
			}
			RigidbodyReset();
			stateImpulse = false;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (!NearbyHeadLogic())
		{
			RigidbodyNotMovingTickTimer();
			if (!GoToIdleOnDestinationReached() && SemiFunc.EnemyForceLeave(enemy))
			{
				UpdateState(State.Leave);
			}
		}
	}

	private void StateRoamFast()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			if (!SemiFunc.EnemyRoamPoint(enemy, out agentDestination))
			{
				return;
			}
			RigidbodyReset();
			stateImpulse = false;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		AgentMoveFast();
		RigidbodyNotMovingTickTimer();
		if (!NearbyHeadLogic() && !GoToIdleOnDestinationReached() && SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateInvestigate()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			RigidbodyReset();
		}
		else
		{
			enemy.NavMeshAgent.SetDestination(agentDestination);
			RigidbodyNotMovingTickTimer();
			if (GoToIdleOnDestinationReached())
			{
				return;
			}
		}
		if (!NearbyHeadLogic() && SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateInvestigateFast()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			RigidbodyReset();
		}
		else
		{
			enemy.NavMeshAgent.SetDestination(agentDestination);
			AgentMoveFast();
			RigidbodyNotMovingTickTimer();
			if (GoToIdleOnDestinationReached())
			{
				return;
			}
		}
		if (!NearbyHeadLogic() && SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateNotice()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			attacksInRow = 0;
			ResetAgent();
			RigidbodyReset();
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			PlayerDeathHead closestDeathHead = GetClosestDeathHead(30f);
			if ((bool)closestDeathHead)
			{
				UpdateHeadTarget(closestDeathHead);
				UpdateState(State.GotoHead);
			}
			else
			{
				UpdateState(State.GotoPlayer);
			}
		}
	}

	private void StateCantReach()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2.5f;
			ResetAgent();
			RigidbodyReset();
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			if (previousState == State.GotoHead)
			{
				UpdateState(State.GotoPlayer);
			}
			else
			{
				UpdateState(State.Leave);
			}
		}
	}

	private void StateGotoPlayer()
	{
		GotoLogic();
	}

	private void StateGotoPlayerOver()
	{
		GotoOverLogic();
	}

	private void StateGotoHead()
	{
		GotoLogic();
	}

	private void StateGotoHeadOver()
	{
		GotoOverLogic();
	}

	private void StateBackToNavmesh()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 15f;
			RigidbodyReset();
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		if (!enemy.Jump.jumping)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, backToNavmeshPosition, 6f * Time.deltaTime);
		}
		SemiFunc.EnemyCartJump(enemy);
		if (!enemy.Jump.jumping && (Vector3.Distance(base.transform.position, enemy.Rigidbody.transform.position) > 2f || enemy.Rigidbody.notMovingTimer > 2f))
		{
			Vector3 normalized = (base.transform.position - backToNavmeshPosition).normalized;
			enemy.Jump.StuckTrigger(base.transform.position - backToNavmeshPosition);
			base.transform.position = enemy.Rigidbody.transform.position;
			base.transform.position += normalized * 2f;
		}
		if (NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out var _, 0.5f, -1))
		{
			if (!headTargetActive && attacksInRow > 3)
			{
				UpdateState(State.Leave);
				return;
			}
			if (previousState == State.Stun)
			{
				UpdateState(State.Idle);
				return;
			}
			if (nearbyHeadLogic)
			{
				PlayerAvatar playerAvatar = SemiFunc.PlayerGetNearestPlayerAvatarWithinRange(999f, base.transform.position);
				if ((bool)playerAvatar)
				{
					UpdatePlayerTarget(playerAvatar);
				}
			}
			UpdateState(State.GotoPlayer);
		}
		else
		{
			RigidbodyNotMovingTickTimer();
			if (stateTimer <= 0f)
			{
				enemy.EnemyParent.SpawnedTimerSet(0f);
			}
		}
	}

	private void StateGrabHead()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1.5f;
			attacksInRow = 0;
			ResetAgent();
			RigidbodyReset();
			stateImpulse = false;
			Vector3 normalized = (headTarget.physGrabObject.centerPoint - enemy.Rigidbody.transform.position).normalized;
			normalized.y = 0f;
			enemy.Rigidbody.rb.AddForce(normalized * 15f, ForceMode.Impulse);
			enemy.Rigidbody.DisableFollowPosition(1.5f, 5f);
			headTargetActive = true;
		}
		stateTimer -= Time.deltaTime;
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = enemy.Rigidbody.transform.position;
		if (stateTimer <= 0f)
		{
			UpdateState(State.BackToNavmesh);
		}
	}

	private void StateReleaseHead()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			ResetAgent();
			RigidbodyReset();
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = enemy.Rigidbody.transform.position;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateAttack()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			attacksInRow++;
			if (attackIndex == 2)
			{
				stateTimer = 2f;
			}
			else
			{
				stateTimer = 0.5f;
			}
			ResetAgent();
			RigidbodyReset();
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = enemy.Rigidbody.transform.position;
		if (stateTimer <= 0f)
		{
			if (!headTargetActive && attacksInRow > 3)
			{
				UpdateState(State.BackToNavmesh);
			}
			else
			{
				UpdateState(previousState);
			}
		}
	}

	private void StateLeave()
	{
		if (stateImpulse)
		{
			stateTimer = 5f;
			if (!SemiFunc.EnemyLeavePoint(enemy, out agentDestination))
			{
				return;
			}
			SemiFunc.EnemyLeaveStart(enemy);
			RigidbodyReset();
			stateImpulse = false;
		}
		RigidbodyNotMovingTickTimer();
		AgentMoveFast();
		enemy.NavMeshAgent.SetDestination(agentDestination);
		GoToIdleOnDestinationReached();
	}

	private void StateStun()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			ResetAgent();
		}
		enemy.Vision.DisableVision(1f);
		if (!enemy.IsStunned())
		{
			UpdateState(State.BackToNavmesh);
		}
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			ResetAgent();
			stateImpulse = false;
		}
	}

	private void UpdateState(State _state)
	{
		if (_state != currentState)
		{
			previousState = currentState;
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

	private void RotationLogic()
	{
		bool flag = true;
		if (rotationStopTimer <= 0f)
		{
			if ((bool)playerTarget && currentState == State.Attack)
			{
				Vector3 position = playerTarget.transform.position;
				position += playerTarget.rbVelocityRaw * 0.5f;
				horizontalRotationTarget = Quaternion.LookRotation(position - enemy.Rigidbody.transform.position);
				horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
			}
			else if ((bool)playerTarget && (currentState == State.GotoPlayer || currentState == State.GotoPlayerOver))
			{
				if (!enemy.Jump.jumping && !VisionBlocked())
				{
					flag = false;
					if (Vector3.Distance(playerTarget.transform.position, enemy.Rigidbody.transform.position) > 0.1f)
					{
						horizontalRotationTarget = Quaternion.LookRotation(playerTarget.transform.position - enemy.Rigidbody.transform.position);
						horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
					}
				}
			}
			else if ((bool)playerTarget && currentState == State.Notice)
			{
				flag = false;
				if (stateTimer >= 1f && Vector3.Distance(playerTarget.transform.position, enemy.Rigidbody.transform.position) > 0.1f)
				{
					horizontalRotationTarget = Quaternion.LookRotation(playerTarget.transform.position - enemy.Rigidbody.transform.position);
					horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
				}
			}
			else if (currentState == State.CantReach)
			{
				flag = false;
				if (previousState == State.GotoHead)
				{
					if ((bool)headTarget && Vector3.Distance(headTarget.transform.position, enemy.Rigidbody.transform.position) > 0.1f)
					{
						horizontalRotationTarget = Quaternion.LookRotation(headTarget.transform.position - enemy.Rigidbody.transform.position);
						horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
					}
				}
				else if ((bool)playerTarget && Vector3.Distance(playerTarget.transform.position, enemy.Rigidbody.transform.position) > 0.1f)
				{
					horizontalRotationTarget = Quaternion.LookRotation(playerTarget.transform.position - enemy.Rigidbody.transform.position);
					horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
				}
			}
			if (flag && enemy.Rigidbody.velocity.magnitude > 0.5f)
			{
				horizontalRotationTarget = Quaternion.LookRotation(enemy.Rigidbody.velocity.normalized);
				horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
			}
			if (currentState == State.Attack)
			{
				horizontalRotationSpring.speed = 20f;
				horizontalRotationSpring.damping = 0.8f;
			}
			else if (currentState == State.Idle || currentState == State.IdleBreaker || currentState == State.Roam || currentState == State.Investigate)
			{
				horizontalRotationSpring.speed = 5f;
				horizontalRotationSpring.damping = 0.7f;
			}
			else
			{
				horizontalRotationSpring.speed = 10f;
				horizontalRotationSpring.damping = 0.7f;
			}
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(horizontalRotationSpring, horizontalRotationTarget);
	}

	private void ResetAgent()
	{
		enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
		enemy.NavMeshAgent.ResetPath();
	}

	private void AgentMoveFast()
	{
		enemy.NavMeshAgent.OverrideAgent(6f, 15f, 0.1f);
		enemy.Rigidbody.OverrideFollowPosition(0.1f, 10f, 30f);
	}

	private void RigidbodyReset()
	{
		enemy.Rigidbody.StuckReset();
		enemy.Jump.StuckReset();
		SemiFunc.EnemyCartJumpReset(enemy);
	}

	private void RigidbodyNotMovingTickTimer()
	{
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
	}

	private bool GoToIdleOnDestinationReached()
	{
		if (!enemy.Jump.jumping && (stateTimer <= 0f || Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) <= 1f || Vector3.Distance(enemy.Rigidbody.transform.position, agentDestination) <= 1f))
		{
			UpdateState(State.Idle);
			return true;
		}
		return false;
	}

	private bool VisionBlocked()
	{
		if (Time.time - visionPreviousTime > 0.2f && (bool)playerTarget)
		{
			Vector3 vector = playerTarget.PlayerVisionTarget.VisionTransform.position;
			if (currentState == State.GotoHead || currentState == State.GotoHeadOver)
			{
				vector = headTarget.physGrabObject.centerPoint;
			}
			visionPreviousTime = Time.time;
			Vector3 direction = vector - enemy.Vision.VisionTransform.position;
			visionPrevious = Physics.Raycast(enemy.Vision.VisionTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
		}
		return visionPrevious;
	}

	private void BackToNavmeshPosition()
	{
		if (SemiFunc.FPSImpulse15() && enemy.NavMeshAgent.OnNavmesh(enemy.Rigidbody.transform.position, 0.5f, _checkPit: true))
		{
			backToNavmeshPosition = enemy.Rigidbody.transform.position;
		}
	}

	private void UpdatePlayerTarget(PlayerAvatar _player)
	{
		playerTarget = _player;
		int num = -1;
		if ((bool)playerTarget)
		{
			num = playerTarget.photonView.ViewID;
		}
		if (GameManager.Multiplayer())
		{
			photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.Others, num);
		}
	}

	private void UpdateHeadTarget(PlayerDeathHead _head)
	{
		if (!headTargetActive)
		{
			headTarget = _head;
			headTargetCooldown = 120f;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateHeadTargetRPC", RpcTarget.Others, headTarget.photonView.ViewID);
			}
		}
	}

	private void GotoLogic()
	{
		Vector3 vector = Vector3.zero;
		bool flag = false;
		if (currentState == State.GotoHead)
		{
			vector = headTarget.transform.position;
			flag = headTarget;
			if ((bool)headTarget && headTarget.overrideSpectated)
			{
				flag = false;
			}
		}
		else if ((bool)playerTarget && !playerTarget.isDisabled)
		{
			vector = playerTarget.transform.position;
			flag = true;
		}
		else if (!headTargetActive && (bool)playerTarget && playerTarget.isDisabled)
		{
			UpdateHeadTarget(playerTarget.playerDeathHead);
			UpdateState(State.GotoHead);
			return;
		}
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			RigidbodyReset();
		}
		if (currentState == State.GotoHead)
		{
			RigidbodyNotMovingTickTimer();
		}
		else if (!nearbyHeadLogic)
		{
			stateTimer -= Time.deltaTime;
		}
		if (!flag || stateTimer <= 0f)
		{
			UpdateState(State.Idle);
			return;
		}
		enemy.NavMeshAgent.SetDestination(vector);
		AgentMoveFast();
		if (currentState == State.GotoHead && Vector3.Distance(enemy.Rigidbody.transform.position, vector) < 1.5f)
		{
			UpdateState(State.GrabHead);
		}
		else
		{
			if (currentState == State.GotoPlayer && AttackSet(vector))
			{
				return;
			}
			SemiFunc.EnemyCartJump(enemy);
			if (currentState == State.GotoPlayer && Vector3.Distance(enemy.Rigidbody.transform.position, vector) < 1f)
			{
				enemy.NavMeshAgent.Stop(0.1f);
			}
			else
			{
				if (((enemy.NavMeshAgent.CanReach(vector, 1f) || !(Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) < 2f)) && !(enemy.Rigidbody.notMovingTimer > 2f)) || enemy.Jump.jumping || (currentState != State.GotoHead && VisionBlocked()) || NavMesh.SamplePosition(vector, out var _, 1f, -1) || (currentState != State.GotoHead && !playerTarget.isGrounded))
				{
					return;
				}
				if (vector.y > enemy.Rigidbody.transform.position.y + 0.5f)
				{
					if (currentState == State.GotoHead)
					{
						UpdateState(State.GotoHeadOver);
					}
					else
					{
						UpdateState(State.GotoPlayerOver);
					}
				}
				else
				{
					UpdateState(State.CantReach);
				}
			}
		}
	}

	private void GotoOverLogic()
	{
		Vector3 position = base.transform.position;
		bool flag = false;
		if (currentState == State.GotoHeadOver)
		{
			position = headTarget.transform.position;
			flag = headTarget;
			if ((bool)headTarget && headTarget.overrideSpectated)
			{
				flag = false;
			}
		}
		else if ((bool)playerTarget && !playerTarget.isDisabled)
		{
			position = playerTarget.transform.position;
			flag = true;
		}
		else if (!headTargetActive && (bool)playerTarget && playerTarget.isDisabled)
		{
			UpdateHeadTarget(playerTarget.playerDeathHead);
			UpdateState(State.GotoHeadOver);
			return;
		}
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			RigidbodyReset();
		}
		if (!flag)
		{
			UpdateState(State.BackToNavmesh);
			return;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, position) > 1.5f)
		{
			float num = enemy.NavMeshAgent.DefaultSpeed;
			if (headTargetActive)
			{
				num = 6f;
			}
			base.transform.position = Vector3.MoveTowards(base.transform.position, position, num * Time.deltaTime);
		}
		else
		{
			base.transform.position = enemy.Rigidbody.transform.position;
			enemy.Rigidbody.DisableFollowPosition(0.1f, 5f);
			if (currentState == State.GotoHeadOver)
			{
				UpdateState(State.GrabHead);
				return;
			}
		}
		if (currentState == State.GotoPlayerOver && AttackSet(position))
		{
			return;
		}
		SemiFunc.EnemyCartJump(enemy);
		if (position.y > enemy.Rigidbody.transform.position.y + 0.3f && !enemy.Jump.jumping && !enemy.Jump.jumpingDelay && !enemy.Jump.landDelay && !enemy.Jump.stuckJumpImpulse)
		{
			Vector3 normalized = (position - enemy.Rigidbody.transform.position).normalized;
			enemy.Jump.StuckTrigger(normalized);
			enemy.Rigidbody.WarpDisable(0.25f);
			base.transform.position = enemy.Rigidbody.transform.position;
			base.transform.position = Vector3.MoveTowards(base.transform.position, position, 2f);
			stateTimer -= 0.5f;
		}
		if (enemy.Jump.jumping || enemy.Jump.jumpingDelay || enemy.Jump.landDelay)
		{
			return;
		}
		if (NavMesh.SamplePosition(position, out var _, 0.5f, -1))
		{
			UpdateState(State.BackToNavmesh);
		}
		else if (currentState == State.GotoPlayerOver || VisionBlocked() || enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.BackToNavmesh);
			}
		}
		else
		{
			stateTimer = 2f;
		}
	}

	private void DeathHeadLogic()
	{
		if (headTargetActive)
		{
			headTargetTime += Time.deltaTime;
			enemy.EnemyParent.SpawnedTimerPause(0.5f);
			if (!headTarget || headTargetRelease || !headTarget.playerAvatar.isDisabled || currentState == State.Stun || currentState == State.Despawn)
			{
				DeathHeadRelease();
			}
			else
			{
				headTarget.OverrideSpectated(0.5f);
				if (!topColliderActive && SemiFunc.FPSImpulse5())
				{
					bool flag = false;
					Collider[] array = Physics.OverlapSphere(topCollider.transform.position, topCollider.radius, SemiFunc.LayerMaskGetVisionObstruct());
					foreach (Collider collider in array)
					{
						if (!collider.gameObject.CompareTag("Phys Grab Object") || !(collider.GetComponentInParent<PhysGrabObject>() == enemy.Rigidbody.physGrabObject))
						{
							flag = true;
						}
					}
					if (!flag)
					{
						ToggleTopCollider(_active: true);
					}
				}
			}
		}
		else
		{
			headTargetTime = 0f;
			headTargetRelease = false;
		}
		if (headTargetActivePrevious != headTargetActive)
		{
			headTargetActivePrevious = headTargetActive;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateHeadTargetActiveRPC", RpcTarget.All, headTargetActive);
			}
			else
			{
				UpdateHeadTargetActiveRPC(headTargetActive);
			}
		}
		if (nearbyHeadLogic && currentState != State.GotoHead && currentState != State.GotoHeadOver && currentState != State.GrabHead && currentState != State.GotoPlayer && currentState != State.BackToNavmesh)
		{
			nearbyHeadLogic = false;
		}
	}

	private void DeathHeadLogicShared()
	{
		if (headTargetActive && (bool)headTarget)
		{
			headTargetPositionLerp += 2f * Time.deltaTime;
			headTargetPositionLerp = Mathf.Clamp01(headTargetPositionLerp);
			Transform followTransform = headTargetPositionTransform;
			if (headTargetPositionLerp < 1f)
			{
				followTransform = headTargetPositionFollowTransform;
				Vector3 position = Vector3.LerpUnclamped(headTargetOriginalPosition, headTargetPositionTransform.position, headTargetPositionCurve.Evaluate(headTargetPositionLerp));
				Quaternion rotation = Quaternion.SlerpUnclamped(headTargetOriginalRotation, headTargetPositionTransform.rotation, headTargetPositionCurve.Evaluate(headTargetPositionLerp));
				headTargetPositionFollowTransform.position = position;
				headTargetPositionFollowTransform.rotation = rotation;
			}
			headTarget.OverridePositionRotation(followTransform, headTargetPositionReleaseTransform.position, headTargetPositionReleaseTransform.rotation, 0.1f);
		}
		else
		{
			headTargetPositionLerp = 0f;
		}
	}

	public void DeathHeadRelease()
	{
		if (headTargetActive)
		{
			headTargetActive = false;
			headTargetCooldown = 120f;
			if ((bool)headTarget)
			{
				headTarget.OverridePositionRotationReset();
				headTarget.OverrideSpectatedReset();
			}
			ToggleTopCollider(_active: false);
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("DeathHeadReleaseRPC", RpcTarget.Others);
			}
			headTarget = null;
		}
	}

	private bool AttackSet(Vector3 _targetPosition)
	{
		if (attackCooldown > 0f || enemy.Jump.jumping || enemy.Jump.jumpingDelay || enemy.Jump.landDelay)
		{
			return false;
		}
		if (Vector3.Distance(enemy.Rigidbody.transform.position, _targetPosition) > 2f)
		{
			return false;
		}
		for (attackIndex = attackIndexPrevious; attackIndex == attackIndexPrevious; attackIndex = Random.Range(0, 3))
		{
		}
		attackIndexPrevious = attackIndex;
		if (attackIndex == 2)
		{
			attackCooldown = 2f;
		}
		else
		{
			attackCooldown = 1f;
		}
		if (GameManager.Multiplayer())
		{
			photonView.RPC("AttackSetRPC", RpcTarget.All, attackIndex);
		}
		else
		{
			AttackSetRPC(attackIndex);
		}
		UpdateState(State.Attack);
		return true;
	}

	private void TimerLogic()
	{
		if (attackCooldown > 0f)
		{
			attackCooldown -= Time.deltaTime;
		}
		if (headTargetCooldown > 0f)
		{
			headTargetCooldown -= Time.deltaTime;
		}
		if (rotationStopTimer > 0f)
		{
			rotationStopTimer -= Time.deltaTime;
		}
	}

	private PlayerDeathHead GetClosestDeathHead(float _distanceMax)
	{
		PlayerDeathHead result = null;
		float num = _distanceMax;
		if (headTargetCooldown <= 0f)
		{
			foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
			{
				if (item.isDisabled && (bool)item.playerDeathHead && !item.playerDeathHead.overrideSpectated)
				{
					float num2 = Vector3.Distance(enemy.Rigidbody.transform.position, item.playerDeathHead.transform.position);
					if (num2 < num)
					{
						num = num2;
						result = item.playerDeathHead;
					}
				}
			}
		}
		return result;
	}

	private bool NearbyHeadLogic()
	{
		if (SemiFunc.FPSImpulse1() && !headTargetActive)
		{
			PlayerDeathHead closestDeathHead = GetClosestDeathHead(10f);
			if ((bool)closestDeathHead)
			{
				UpdatePlayerTarget(null);
				UpdateHeadTarget(closestDeathHead);
				nearbyHeadLogic = true;
				UpdateState(State.GotoHead);
				return true;
			}
		}
		return false;
	}

	private void LocalMeshLogicShared()
	{
		if (headTargetActive && headTargetActiveLocal && SpectateCamera.instance.CheckState(SpectateCamera.State.Head))
		{
			if (!localMeshHidden)
			{
				foreach (MeshRenderer renderer in enemy.Health.renderers)
				{
					renderer.gameObject.layer = LayerMask.NameToLayer("HideTriggers");
				}
				localMeshHidden = true;
			}
			if (!localCameraObject)
			{
				GameObject gameObject = Object.Instantiate(localCameraPrefab, Camera.main.transform);
				localCameraObject = gameObject.GetComponent<EnemyHeadGrabberLocalCamera>();
			}
			else
			{
				localCameraObject.Active();
			}
		}
		else
		{
			if (!localMeshHidden)
			{
				return;
			}
			if ((bool)localCameraObject)
			{
				localCameraObject.ActiveReset();
			}
			foreach (MeshRenderer renderer2 in enemy.Health.renderers)
			{
				renderer2.gameObject.layer = LayerMask.NameToLayer("Default");
			}
			localMeshHidden = false;
		}
	}

	[PunRPC]
	private void UpdateStateRPC(State _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
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
	private void UpdateHeadTargetRPC(int _photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item.playerDeathHead.photonView.ViewID == _photonViewID)
			{
				headTarget = item.playerDeathHead;
				break;
			}
		}
	}

	[PunRPC]
	private void UpdateHeadTargetActiveRPC(bool _active, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		headTargetActive = _active;
		if (headTargetActive)
		{
			if (headTarget.playerAvatar.isLocal)
			{
				headTargetActiveLocal = true;
			}
			else
			{
				headTargetActiveLocal = false;
			}
			headTargetOriginalPosition = headTarget.transform.position;
			headTargetOriginalRotation = headTarget.transform.rotation;
			headTargetPositionLerp = 0f;
		}
		LocalMeshLogicShared();
	}

	private void ToggleTopCollider(bool _active)
	{
		if (topColliderActive != _active)
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("ToggleTopColliderRPC", RpcTarget.All, _active);
			}
			else
			{
				ToggleTopColliderRPC(_active);
			}
		}
	}

	[PunRPC]
	private void ToggleTopColliderRPC(bool _active, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			topColliderActive = _active;
			topCollider.enabled = _active;
		}
	}

	[PunRPC]
	private void DeathHeadReleaseRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info) && (bool)headTarget)
		{
			headTarget.OverridePositionRotationReset();
		}
	}

	[PunRPC]
	private void AttackSetRPC(int _index, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			attackIndex = _index;
			anim.AttackSet(attackIndex);
		}
	}

	[PunRPC]
	private void IdleBreakerSetRPC(int _index, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			idleBreakerIndex = _index;
			anim.IdleBreakerSet(idleBreakerIndex);
		}
	}

	[PunRPC]
	private void OnDropKickHitRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!(Time.time - dropkickHitTimeLast < 0.5f))
		{
			dropkickHitTimeLast = Time.time;
			anim.EventDropkickHit();
		}
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
	}

	public void OnVision()
	{
		if (enemy.Jump.jumpingDelay || enemy.Jump.jumping)
		{
			return;
		}
		if (currentState == State.Idle || currentState == State.IdleBreaker || currentState == State.Roam || currentState == State.RoamFast || currentState == State.Investigate || currentState == State.InvestigateFast || currentState == State.Leave)
		{
			UpdatePlayerTarget(enemy.Vision.onVisionTriggeredPlayer);
			if ((bool)playerTarget)
			{
				UpdateState(State.Notice);
			}
		}
		else
		{
			if (currentState != State.GotoPlayer && currentState != State.GotoPlayerOver)
			{
				return;
			}
			if (playerTarget == enemy.Vision.onVisionTriggeredPlayer)
			{
				stateTimer = Mathf.Max(2f, stateTimer);
			}
			else if (nearbyHeadLogic)
			{
				UpdatePlayerTarget(enemy.Vision.onVisionTriggeredPlayer);
				if ((bool)playerTarget)
				{
					UpdateState(State.Notice);
				}
			}
			nearbyHeadLogic = false;
		}
	}

	public void OnInvestigate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && (currentState == State.Idle || currentState == State.Roam || currentState == State.RoamFast || currentState == State.Investigate || currentState == State.InvestigateFast))
		{
			agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			if (Vector3.Distance(base.transform.position, agentDestination) > 10f)
			{
				UpdateState(State.InvestigateFast);
			}
			else if (Vector3.Distance(base.transform.position, agentDestination) > 2f)
			{
				UpdateState(State.Investigate);
			}
		}
	}

	public void OnHurt()
	{
		anim.soundHurt.Play(enemy.CenterTransform.position);
		anim.soundLoopPauseTimer = 0.5f;
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.Leave)
		{
			UpdateState(State.Idle);
		}
	}

	public void OnDeath()
	{
		anim.EventDeath();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			DeathHeadRelease();
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnDropkickHit()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("OnDropKickHitRPC", RpcTarget.All);
		}
		else
		{
			OnDropKickHitRPC();
		}
	}
}
