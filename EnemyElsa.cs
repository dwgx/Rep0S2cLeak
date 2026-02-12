using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyElsa : MonoBehaviour, IPunObservable
{
	public enum State
	{
		SpawnSmall,
		IdleSmall,
		IdleBreakSmall,
		RoamSmall,
		InvestigateSmall,
		NoticeSmall,
		GoToPlayerSmall,
		GoToPlayerOverSmall,
		GoToPlayerUnderSmall,
		BackToNavMeshSmall,
		FlyBackUpSmall,
		FlyBackToNavMeshSmall,
		FlyBackStopSmall,
		PetSmall,
		TryTransformSmall,
		StunSmall,
		LeaveSmall,
		DespawnSmall,
		GoToPlayerBig,
		GoToPlayerOverBig,
		BackToNavMeshBig,
		LookUnderStartBig,
		LookUnderBig,
		LookUnderStopBig,
		TransformSmallToBig,
		TransformBigToSmall
	}

	[Space]
	public State currentState;

	private State previousState;

	public float stateTimer;

	private float stateTicker;

	[Space]
	public EnemyElsaAnim anim;

	public Enemy enemy;

	public MeshRenderer headMesh;

	public AnimationCurve angryCurve;

	public GameObject hurtCollider;

	public Transform visualsTransform;

	private int EmissionColorID = Shader.PropertyToID("_EmissionColor");

	private float eyeEmissionMax = 2f;

	private float eyeEmissionMultiplier = 1f;

	public Transform headWiggleTransform;

	public ParticleSystem[] deathParticlesSmall;

	public ParticleSystem[] deathParticlesBig;

	public SpringQuaternion headSpring;

	public Transform headTransform;

	public Transform headTransformTarget;

	public SpringQuaternion tailFlyingPivotSpring;

	public Transform tailFlyingPivotTransform;

	public Transform tailFlyingPivotTransformTarget;

	private float initialTailFlyingPivotSpringDamping;

	private float initialTailFlyingPivotSpringSpeed;

	public SpringQuaternion tail01Spring;

	public Transform tail01Transform;

	public Transform tail01TransformTarget;

	public SpringQuaternion tail02Spring;

	public Transform tail02Transform;

	public Transform tail02TransformTarget;

	public SpringQuaternion ear01Spring;

	public Transform ear01TransformL;

	public Transform ear01TransformR;

	public Transform ear01TransformLTarget;

	public Transform ear01TransformRTarget;

	public SpringQuaternion ear02Spring;

	public Transform ear02TransformL;

	public Transform ear02TransformR;

	public Transform ear02TransformLTarget;

	public Transform ear02TransformRTarget;

	public AudioSource barkSource;

	public AudioSource stunLoopSource;

	public AudioSource pantingSmallSource;

	[Space]
	public SpringQuaternion jawSpring;

	public Transform jawTransform;

	public Transform jawTransformTarget;

	[Space]
	private Quaternion rotationTarget;

	public SpringQuaternion rotationSpring;

	public Transform headIdleTransform;

	public Transform feetTransform;

	public Transform smallColliderTransform;

	public Transform bigColliderTransform;

	private PlayerAvatar playerTarget;

	private PhotonView photonView;

	private Vector3 agentDestination;

	private bool stateImpulse;

	private bool visionPrevious;

	private float visionTimer;

	private Vector3 moveBackPosition;

	private float angryTimer;

	private float angryTimerMax = 15f;

	private float happyTimer;

	private float happyTimerMax = 15f;

	private float chaseTimer;

	private float chaseTimerMax = 10f;

	private float onGrabbedTimer;

	private float onGrabbedTimerMax = 0.1f;

	private float targetedPlayerTime;

	private float targetedPlayerTimeMax = 120f;

	private float barkTimer;

	private float barkTimerMax = 1f;

	private float barkTimerFrequency;

	private Vector3 targetPosition;

	private float pitCheckTimer;

	private bool pitCheck;

	private float barkClipTimer;

	private float barkClipLoudness;

	private int barkClipSampleDataLength = 1024;

	private float[] barkClipSampleData;

	private float barkDelayTimer;

	private bool tutorialChecked;

	private Vector3 lookUnderPosition;

	private Vector3 lookUnderPositionNavmesh;

	private int idleBreakerIndex;

	private int idleBreakerIndexPrevious;

	private float annoyingJumpPauseTimer;

	private float annoyingJumpPauseFrequency = 2f;

	private bool duckBucketActive;

	private float duckBucketTimer;

	private void Awake()
	{
		hurtCollider.SetActive(value: false);
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
		initialTailFlyingPivotSpringDamping = tailFlyingPivotSpring.damping;
		initialTailFlyingPivotSpringSpeed = tailFlyingPivotSpring.speed;
		barkClipSampleData = new float[barkClipSampleDataLength];
	}

	private void Update()
	{
		HeadRotationLogic();
		TailSpringLogic();
		EarSpringLogic();
		BarkLogic();
		HeadWiggle();
		EyeEmissionLogic();
		TutorialLogic();
		if ((GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient) || !LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (enemy.IsStunned())
		{
			if (IsBig())
			{
				UpdateState(State.TransformBigToSmall);
			}
			else
			{
				UpdateState(State.StunSmall);
			}
		}
		else if (enemy.CurrentState == EnemyState.Despawn && !IsBig())
		{
			UpdateState(State.DespawnSmall);
		}
		if (!playerTarget || playerTarget.isDisabled)
		{
			if (currentState == State.GoToPlayerSmall || currentState == State.GoToPlayerOverSmall || currentState == State.GoToPlayerUnderSmall)
			{
				UpdateState(State.IdleSmall);
			}
			else if (currentState == State.GoToPlayerBig || currentState == State.GoToPlayerOverBig || currentState == State.BackToNavMeshBig || currentState == State.TransformSmallToBig)
			{
				UpdateState(State.TransformBigToSmall);
			}
		}
		RotationLogic();
		GravityLogic();
		TargetPositionLogic();
		FlyBackConditionLogic();
		BackToNavmeshPosition();
		TimerLogic();
		DuckBucketLogic();
		switch (currentState)
		{
		case State.SpawnSmall:
			StateSpawnSmall();
			break;
		case State.IdleSmall:
			StateIdleSmall();
			break;
		case State.IdleBreakSmall:
			StateIdleBreakSmall();
			break;
		case State.RoamSmall:
			StateRoamSmall();
			break;
		case State.InvestigateSmall:
			StateInvestigateSmall();
			break;
		case State.NoticeSmall:
			StateNoticeSmall();
			break;
		case State.GoToPlayerSmall:
			StateGoToPlayerSmall();
			break;
		case State.GoToPlayerOverSmall:
			StateGoToPlayerOverSmall();
			break;
		case State.GoToPlayerUnderSmall:
			StateGoToPlayerUnderSmall();
			break;
		case State.BackToNavMeshSmall:
			StateBackToNavMeshSmall();
			break;
		case State.FlyBackUpSmall:
			StateFlyBackUpSmall();
			break;
		case State.FlyBackToNavMeshSmall:
			StateFlyBackToNavMeshSmall();
			break;
		case State.FlyBackStopSmall:
			StateFlyBackStopSmall();
			break;
		case State.PetSmall:
			StatePetSmall();
			break;
		case State.TryTransformSmall:
			StateTryTransformSmall();
			break;
		case State.StunSmall:
			StateStunSmall();
			break;
		case State.LeaveSmall:
			StateLeaveSmall();
			break;
		case State.DespawnSmall:
			StateDespawnSmall();
			break;
		case State.GoToPlayerBig:
			StateGoToPlayerBig();
			break;
		case State.GoToPlayerOverBig:
			StateGoToPlayerOverBig();
			break;
		case State.BackToNavMeshBig:
			StateBackToNavMeshBig();
			break;
		case State.LookUnderStartBig:
			StateLookUnderStartBig();
			break;
		case State.LookUnderBig:
			StateLookUnderBig();
			break;
		case State.LookUnderStopBig:
			StateLookUnderStopBig();
			break;
		case State.TransformSmallToBig:
			StateTransformSmallToBig();
			break;
		case State.TransformBigToSmall:
			StateTransformBigToSmall();
			break;
		}
	}

	private void StateSpawnSmall()
	{
		if (stateImpulse)
		{
			AgentReset();
			stateImpulse = false;
			stateTimer = 2f;
			angryTimer = 0f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.IdleSmall);
		}
	}

	private void StateIdleSmall()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = Random.Range(2f, 5f);
			AgentReset();
		}
		if (SemiFunc.EnemySpawnIdlePause())
		{
			return;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			if (Random.Range(0, 3) == 0)
			{
				idleBreakerIndexPrevious = (idleBreakerIndexPrevious + 1) % 2;
				idleBreakerIndex = idleBreakerIndexPrevious;
				if (GameManager.Multiplayer())
				{
					photonView.RPC("IdleBreakerSetRPC", RpcTarget.All, idleBreakerIndex);
				}
				else
				{
					IdleBreakerSetRPC(idleBreakerIndex);
				}
				UpdateState(State.IdleBreakSmall);
			}
			else
			{
				UpdateState(State.RoamSmall);
			}
		}
		LeaveCheck(_setLeave: true);
	}

	private void StateIdleBreakSmall()
	{
		if (stateImpulse)
		{
			if (idleBreakerIndex == 0)
			{
				stateTimer = 2.75f;
			}
			else
			{
				stateTimer = 1.5f;
			}
			stateImpulse = false;
			AgentReset();
			RigidbodyReset();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.IdleSmall);
		}
	}

	private void StateRoamSmall()
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
		else
		{
			enemy.NavMeshAgent.SetDestination(agentDestination);
			SemiFunc.EnemyCartJump(enemy);
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 0.1f)
			{
				RigidbodyReset();
				UpdateState(State.IdleSmall);
			}
		}
		LeaveCheck(_setLeave: true);
	}

	private void StateInvestigateSmall()
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
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 2f)
			{
				RigidbodyReset();
				UpdateState(State.IdleSmall);
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.LeaveSmall);
		}
	}

	private void StateNoticeSmall()
	{
		if (stateImpulse)
		{
			Bark();
			AgentReset();
			stateImpulse = false;
			stateTimer = 1f;
			happyTimer = 0f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.GoToPlayerSmall);
		}
	}

	private void StateGoToPlayerSmall()
	{
		float num = 5f;
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = num;
			annoyingJumpPauseTimer = annoyingJumpPauseFrequency;
		}
		OverrideMoveSpeed(3f);
		enemy.NavMeshAgent.SetDestination(targetPosition);
		stateTimer -= Time.deltaTime;
		SemiFunc.EnemyCartJump(enemy);
		enemy.Vision.StandOverride(0.25f);
		if (stateTimer <= 0f || !playerTarget || playerTarget.isDisabled)
		{
			UpdateState(State.IdleSmall);
			return;
		}
		if (!enemy.NavMeshAgent.CanReach(targetPosition, 1f) && Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) < 2f && !IsVisionBlocked() && !NavMesh.SamplePosition(targetPosition, out var hit, 0.5f, -1))
		{
			if (playerTarget.isCrawling && Mathf.Abs(targetPosition.y - enemy.Rigidbody.transform.position.y) < 0.3f && !enemy.Jump.jumping)
			{
				UpdateState(State.GoToPlayerUnderSmall);
				return;
			}
			if (!NavMesh.SamplePosition(playerTarget.transform.position, out hit, 0.5f, -1) && stateTimer < num - 1f && targetPosition.y > enemy.Rigidbody.transform.position.y)
			{
				UpdateState(State.GoToPlayerOverSmall);
				return;
			}
		}
		if (!LeaveCheck(_setLeave: true) && !AngryCheck() && !HappyCheck())
		{
			AnnoyingJumpCheck();
		}
	}

	private void StateGoToPlayerOverSmall()
	{
		if (stateImpulse)
		{
			stateTimer = 3f;
			stateImpulse = false;
		}
		OverrideMoveSpeed(3f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f || (!IsVisionBlocked() && NavMesh.SamplePosition(targetPosition, out var _, 0.5f, -1)))
		{
			UpdateState(State.GoToPlayerSmall);
		}
		else if (!LeaveCheck(_setLeave: true) && !AngryCheck())
		{
			HappyCheck();
		}
	}

	private void StateGoToPlayerUnderSmall()
	{
		if (stateImpulse)
		{
			stateTimer = 2f;
			stateImpulse = false;
		}
		OverrideMoveSpeed(1.5f);
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, enemy.NavMeshAgent.Agent.speed * 1f * Time.deltaTime);
		if (Vector3.Distance(base.transform.position, enemy.Rigidbody.transform.position) > 2f)
		{
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		SemiFunc.EnemyCartJump(enemy);
		enemy.Vision.StandOverride(0.25f);
		if (NavMesh.SamplePosition(targetPosition, out var _, 0.5f, -1))
		{
			UpdateState(State.BackToNavMeshSmall);
		}
		else if (IsVisionBlocked() || !playerTarget || playerTarget.isDisabled)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.BackToNavMeshSmall);
			}
		}
		else
		{
			stateTimer = 2f;
		}
		if (LeaveCheck(_setLeave: false))
		{
			UpdateState(State.BackToNavMeshSmall);
		}
		else if (!AngryCheck())
		{
			HappyCheck();
		}
	}

	private void StateBackToNavMeshSmall()
	{
		if (stateImpulse)
		{
			stateTimer = 15f;
			stateImpulse = false;
			RigidbodyReset();
		}
		enemy.NavMeshAgent.Disable(0.1f);
		if (!enemy.Jump.jumping)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, moveBackPosition, 5f * Time.deltaTime);
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
		if (Vector3.Distance(feetTransform.position, moveBackPosition) <= 0.5f || NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out var _, 0.5f, -1))
		{
			UpdateState(State.GoToPlayerSmall);
		}
		else if (stateTimer <= 0f)
		{
			enemy.EnemyParent.SpawnedTimerSet(0f);
			UpdateState(State.DespawnSmall);
		}
	}

	private void StateFlyBackUpSmall()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 30f;
			if (enemy.Jump.jumping)
			{
				enemy.Jump.JumpingSet(_jumping: false, EnemyJump.Type.None);
			}
		}
		enemy.NavMeshAgent.Disable(0.1f);
		Vector3 target = new Vector3(base.transform.position.x, moveBackPosition.y + 1.5f, base.transform.position.z);
		base.transform.position = Vector3.MoveTowards(base.transform.position, target, 1f * Time.deltaTime);
		enemy.Rigidbody.OverrideFollowPosition(0.1f, 1f);
		enemy.Rigidbody.OverrideFollowRotation(0.1f, 0.25f);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, base.transform.position) > 2f)
		{
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		if (!FlyBackGroundCheck())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.DespawnSmall);
			}
			if (enemy.Rigidbody.transform.position.y - moveBackPosition.y > 1f)
			{
				UpdateState(State.FlyBackToNavMeshSmall);
			}
		}
	}

	private void StateFlyBackToNavMeshSmall()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 10f;
			stateTicker = 0f;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, moveBackPosition + Vector3.up * 0.5f, Time.deltaTime * 2.75f);
		enemy.Rigidbody.OverrideFollowPosition(0.1f, 1f);
		enemy.Rigidbody.OverrideFollowRotation(0.1f, 0.25f);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, base.transform.position) > 2f)
		{
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		if (!FlyBackGroundCheck())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.DespawnSmall);
			}
		}
	}

	private void StateFlyBackStopSmall()
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
			UpdateState(State.IdleSmall);
		}
	}

	private void StatePetSmall()
	{
		if (stateImpulse)
		{
			angryTimer = 0f;
			stateImpulse = false;
			stateTimer = 1f;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(previousState);
		}
	}

	private void StateTryTransformSmall()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			stateTicker = 0f;
			enemy.Rigidbody.notMovingTimer = 0f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			FindTransformationPoint();
		}
		if (stateTicker <= 0f && !enemy.Jump.jumping && !enemy.Jump.jumpingDelay)
		{
			stateTicker = 0.2f;
			Vector3 vector = new Vector3(0f, 0.1f, 0f);
			bigColliderTransform.gameObject.SetActive(value: true);
			bool flag = false;
			if (!SemiFunc.EnemyPhysObjectBoundingBoxCheck(enemy.Rigidbody.transform.position, enemy.Rigidbody.transform.position + vector, enemy.Rigidbody.rb, _checkDefault: true, 1f))
			{
				flag = true;
			}
			bigColliderTransform.gameObject.SetActive(value: false);
			if (flag)
			{
				UpdateState(State.TransformSmallToBig);
				return;
			}
		}
		else
		{
			stateTicker -= Time.deltaTime;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		OverrideMoveSpeed(3f);
		SemiFunc.EnemyCartJump(enemy);
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		if (Vector3.Distance(base.transform.position, agentDestination) < 1f)
		{
			FindTransformationPoint();
			stateTimer = 5f;
		}
		if (duckBucketActive)
		{
			UpdateState(State.IdleSmall);
		}
		if (stateTimer <= 0f)
		{
			RigidbodyReset();
			UpdateState(State.IdleSmall);
		}
	}

	private void StateStunSmall()
	{
		enemy.NavMeshAgent.Disable(0.1f);
		if (stateImpulse)
		{
			stateImpulse = false;
			if (!playerTarget)
			{
				playerTarget = SemiFunc.PlayerGetNearestPlayerAvatarWithinRange(7.5f, base.transform.position, doRaycastCheck: true, LayerMask.GetMask("Default"));
			}
		}
		if (!enemy.IsStunned())
		{
			UpdateState(State.BackToNavMeshSmall);
		}
	}

	private void StateLeaveSmall()
	{
		if (stateImpulse)
		{
			angryTimer = 0f;
			stateTimer = 5f;
			if (SemiFunc.EnemyLeavePoint(enemy, out agentDestination))
			{
				SemiFunc.EnemyLeaveStart(enemy);
				stateImpulse = false;
			}
			return;
		}
		OverrideMoveSpeed(3f);
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		SemiFunc.EnemyCartJump(enemy);
		if (Vector3.Distance(base.transform.position, agentDestination) < 1f || stateTimer <= 0f)
		{
			RigidbodyReset();
			UpdateState(State.IdleSmall);
		}
	}

	private void StateDespawnSmall()
	{
		if (stateImpulse)
		{
			AgentReset();
			stateImpulse = false;
		}
	}

	private void StateGoToPlayerBig()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			RigidbodyReset();
		}
		ChaseStop();
		BigOverrides();
		enemy.NavMeshAgent.SetDestination(targetPosition);
		if (!enemy.Jump.jumping && playerTarget.isGrounded && ((!enemy.NavMeshAgent.CanReach(targetPosition, 1f) && Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) < 3f) || enemy.Rigidbody.notMovingTimer > 1.5f) && !IsVisionBlocked() && !NavMesh.SamplePosition(targetPosition, out var _, 0.5f, -1) && targetPosition.y > enemy.Rigidbody.transform.position.y - 0.5f)
		{
			UpdateState(State.GoToPlayerOverBig);
		}
		if (SemiFunc.EnemyLookUnderCondition(enemy, stateTimer, 0f, playerTarget))
		{
			UpdateState(State.LookUnderStartBig);
		}
	}

	private void StateGoToPlayerOverBig()
	{
		bool flag = false;
		if ((bool)playerTarget && !playerTarget.isDisabled)
		{
			flag = true;
		}
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			RigidbodyReset();
		}
		if (!flag || chaseTimer >= chaseTimerMax)
		{
			UpdateState(State.BackToNavMeshBig);
			return;
		}
		if (IsVisionBlocked() || enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.BackToNavMeshBig);
				return;
			}
		}
		if (SemiFunc.FPSImpulse5() && !enemy.Jump.jumping && !enemy.Jump.jumpingDelay && !enemy.Jump.landDelay)
		{
			if (NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out var hit, 0.5f, -1))
			{
				UpdateState(State.GoToPlayerBig);
				return;
			}
			if (NavMesh.SamplePosition(targetPosition, out hit, 0.5f, -1))
			{
				UpdateState(State.BackToNavMeshBig);
				return;
			}
		}
		enemy.NavMeshAgent.Disable(0.1f);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, targetPosition) > 1.5f)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
		}
		else
		{
			base.transform.position = enemy.Rigidbody.transform.position;
			enemy.Rigidbody.DisableFollowPosition(0.1f, 5f);
		}
		if (targetPosition.y > enemy.Rigidbody.transform.position.y + 0.3f && !enemy.Jump.jumping && !enemy.Jump.jumpingDelay && !enemy.Jump.landDelay && !enemy.Jump.stuckJumpImpulse)
		{
			Vector3 normalized = (targetPosition - enemy.Rigidbody.transform.position).normalized;
			enemy.Jump.StuckTrigger(normalized);
			enemy.Rigidbody.WarpDisable(0.25f);
			base.transform.position = enemy.Rigidbody.transform.position;
			base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, 2f);
			stateTimer -= 0.5f;
		}
		base.transform.position = Vector3.MoveTowards(base.transform.position, playerTarget.localCamera.transform.position + Vector3.down * 0.31f, 5f * Time.deltaTime);
		BigOverrides();
	}

	private void StateBackToNavMeshBig()
	{
		if (stateImpulse)
		{
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
			UpdateState(State.GoToPlayerBig);
		}
		else
		{
			BigOverrides();
		}
	}

	private void StateLookUnderStartBig()
	{
		if (!playerTarget)
		{
			UpdateState(State.TransformBigToSmall);
			return;
		}
		if (stateImpulse)
		{
			lookUnderPosition = playerTarget.transform.position;
			lookUnderPositionNavmesh = playerTarget.LastNavmeshPosition;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateTimer = 0f;
			stateImpulse = false;
		}
		if (!playerTarget.isCrawling)
		{
			UpdateState(State.GoToPlayerBig);
		}
		enemy.NavMeshAgent.OverrideAgent(3f, 10f, 0.2f);
		enemy.NavMeshAgent.SetDestination(lookUnderPositionNavmesh);
		if (Vector3.Distance(base.transform.position, lookUnderPositionNavmesh) < 0.5f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.LookUnderBig);
			}
		}
		else if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			UpdateState(State.TransformBigToSmall);
		}
	}

	private void StateLookUnderBig()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
		}
		if (IsVisionBlocked())
		{
			stateTimer -= Time.deltaTime * 1f;
		}
		else
		{
			stateTimer -= Time.deltaTime * 0.5f;
		}
		enemy.Vision.StandOverride(0.05f);
		if (!playerTarget.isCrawling)
		{
			UpdateState(State.LookUnderStopBig);
			chaseTimer *= 0.5f;
		}
		else if (stateTimer <= 0f || !playerTarget || playerTarget.isDisabled)
		{
			UpdateState(State.LookUnderStopBig);
			chaseTimer = chaseTimerMax;
		}
	}

	private void StateLookUnderStopBig()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 0.25f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			if (chaseTimer < chaseTimerMax)
			{
				UpdateState(State.GoToPlayerBig);
			}
			else
			{
				UpdateState(State.TransformBigToSmall);
			}
		}
	}

	private void StateTransformSmallToBig()
	{
		if (stateImpulse)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("TransformSmallToBigRPC", RpcTarget.All);
			}
			else
			{
				TransformSmallToBigRPC();
			}
			stateImpulse = false;
			stateTimer = 4.1f;
			angryTimer = angryTimerMax;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
		}
		enemy.NavMeshAgent.Disable(0.1f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.GoToPlayerBig);
		}
		BigOverrides();
	}

	private void StateTransformBigToSmall()
	{
		if (stateImpulse)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("TransformBigToSmallRPC", RpcTarget.All, true);
			}
			else
			{
				TransformBigToSmallRPC(_triggerAnimation: true);
			}
			stateImpulse = false;
			stateTimer = 0.3f;
			angryTimer = 0f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			if (enemy.IsStunned())
			{
				UpdateState(State.StunSmall);
			}
			else
			{
				UpdateState(State.LeaveSmall);
			}
		}
		BigOverrides();
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("TransformBigToSmallRPC", RpcTarget.All, false);
			}
			else
			{
				TransformBigToSmallRPC(_triggerAnimation: false);
			}
			UpdateState(State.SpawnSmall);
		}
		if (anim.isActiveAndEnabled)
		{
			anim.OnSpawn();
		}
	}

	public void OnGrabbed()
	{
		if (currentState == State.StunSmall)
		{
			return;
		}
		onGrabbedTimer += Time.deltaTime;
		if (onGrabbedTimer >= onGrabbedTimerMax)
		{
			happyTimer += 3f;
			onGrabbedTimer = 0f;
			enemy.Rigidbody.GrabRelease(_effects: false, 1f);
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("PetRPC", RpcTarget.All);
			}
			else
			{
				PetRPC();
			}
			if (SemiFunc.IsMasterClientOrSingleplayer() && !IsBig() && !IsTransforming() && currentState != State.FlyBackUpSmall && currentState != State.FlyBackStopSmall && currentState != State.FlyBackToNavMeshSmall && currentState != State.LeaveSmall)
			{
				UpdateState(State.PetSmall);
			}
		}
	}

	public void OnInvestigate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && (currentState == State.IdleSmall || currentState == State.RoamSmall || currentState == State.InvestigateSmall))
		{
			agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			UpdateState(State.InvestigateSmall);
		}
	}

	public void OnVision()
	{
		if ((currentState == State.IdleSmall || currentState == State.RoamSmall || currentState == State.InvestigateSmall || currentState == State.LeaveSmall) && !enemy.Jump.jumping)
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				playerTarget = enemy.Vision.onVisionTriggeredPlayer;
				if (SemiFunc.IsMultiplayer())
				{
					photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, playerTarget.photonView.ViewID);
				}
				UpdateState(State.NoticeSmall);
			}
		}
		else if (currentState == State.GoToPlayerSmall || currentState == State.GoToPlayerUnderSmall)
		{
			stateTimer = Mathf.Max(stateTimer, 2f);
		}
		else if (currentState == State.LookUnderStartBig)
		{
			if (playerTarget.isCrawling)
			{
				lookUnderPosition = playerTarget.transform.position;
			}
		}
		else if (currentState == State.LookUnderBig && playerTarget == enemy.Vision.onVisionTriggeredPlayer && playerTarget.isCrawling)
		{
			lookUnderPosition = playerTarget.transform.position;
		}
	}

	public void OnHurt()
	{
		anim.soundHurtPauseTimer = 0.5f;
		if (IsBig())
		{
			anim.SFXHurtBig();
		}
		else
		{
			anim.SFXHurtSmall();
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		angryTimer = angryTimerMax;
		PlayerAvatar playerAvatar = SemiFunc.PlayerGetNearestPlayerAvatarWithinRange(15f, base.transform.position, doRaycastCheck: true, LayerMask.GetMask("Default"));
		if ((bool)playerAvatar && playerAvatar != playerTarget)
		{
			playerTarget = playerAvatar;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, playerTarget.photonView.ViewID);
			}
		}
		if (!enemy.IsStunned() && (bool)playerTarget && (currentState == State.GoToPlayerSmall || currentState == State.GoToPlayerOverSmall || currentState == State.GoToPlayerUnderSmall))
		{
			UpdateState(State.TryTransformSmall);
		}
	}

	public void OnDeath()
	{
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.05f);
		if (IsBig())
		{
			anim.SFXDeathBig();
			ParticleSystem[] array = deathParticlesBig;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: true);
			}
		}
		else
		{
			anim.SFXDeathSmall();
			ParticleSystem[] array = deathParticlesSmall;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: true);
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnDespawn()
	{
	}

	private void UpdateState(State _state)
	{
		if (currentState != _state)
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

	private void TimerLogic()
	{
		visionTimer -= Time.deltaTime;
		if (currentState == State.GoToPlayerSmall || currentState == State.GoToPlayerOverSmall || currentState == State.GoToPlayerUnderSmall || currentState == State.TryTransformSmall || currentState == State.StunSmall || currentState == State.TransformSmallToBig)
		{
			if (angryTimer <= angryTimerMax)
			{
				angryTimer += Time.deltaTime;
				angryTimer = Mathf.Min(angryTimerMax, angryTimer);
			}
		}
		else if (angryTimer >= 0f)
		{
			angryTimer -= Time.deltaTime * 0.5f;
			angryTimer = Mathf.Max(0f, angryTimer);
		}
		if (currentState == State.GoToPlayerBig || currentState == State.GoToPlayerOverBig || currentState == State.BackToNavMeshBig || IsLookingUnder())
		{
			chaseTimer += Time.deltaTime;
		}
		else
		{
			chaseTimer = 0f;
		}
		if (currentState == State.SpawnSmall)
		{
			targetedPlayerTime = 0f;
		}
		if (currentState == State.GoToPlayerSmall || currentState == State.GoToPlayerOverSmall || currentState == State.GoToPlayerUnderSmall)
		{
			targetedPlayerTime += Time.deltaTime;
		}
		else
		{
			targetedPlayerTime -= 5f * Time.deltaTime;
			targetedPlayerTime = Mathf.Max(0f, targetedPlayerTime);
		}
		annoyingJumpPauseTimer -= Time.deltaTime;
	}

	private void GravityLogic()
	{
		if (currentState == State.FlyBackUpSmall || currentState == State.FlyBackToNavMeshSmall)
		{
			enemy.Rigidbody.gravity = false;
		}
		else
		{
			enemy.Rigidbody.gravity = true;
		}
	}

	private void TargetPositionLogic()
	{
		if ((currentState == State.GoToPlayerSmall || currentState == State.GoToPlayerOverSmall || currentState == State.GoToPlayerUnderSmall || currentState == State.GoToPlayerBig || currentState == State.GoToPlayerOverBig) && (bool)playerTarget)
		{
			Vector3 vector = playerTarget.transform.position + playerTarget.transform.forward * 1f;
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

	private void FlyBackConditionLogic()
	{
		if ((currentState == State.IdleSmall || currentState == State.RoamSmall || currentState == State.InvestigateSmall || currentState == State.NoticeSmall || currentState == State.GoToPlayerSmall || currentState == State.GoToPlayerOverSmall || currentState == State.GoToPlayerUnderSmall || currentState == State.BackToNavMeshSmall || currentState == State.TryTransformSmall || IsTransforming()) && enemy.Rigidbody.transform.position.y - moveBackPosition.y < -4f)
		{
			UpdateState(State.FlyBackUpSmall);
		}
	}

	private bool LeaveCheck(bool _setLeave)
	{
		if (SemiFunc.EnemyForceLeave(enemy) || targetedPlayerTime >= targetedPlayerTimeMax)
		{
			if (_setLeave)
			{
				UpdateState(State.LeaveSmall);
			}
			return true;
		}
		return false;
	}

	private bool AngryCheck()
	{
		if (angryTimer >= angryTimerMax)
		{
			UpdateState(State.TryTransformSmall);
			return true;
		}
		return false;
	}

	private bool HappyCheck()
	{
		if (happyTimer >= happyTimerMax)
		{
			UpdateState(State.LeaveSmall);
			return true;
		}
		return false;
	}

	private void ChaseStop()
	{
		if (!enemy.Jump.jumping && !enemy.Jump.jumpingDelay && (chaseTimer >= chaseTimerMax || !playerTarget || playerTarget.isDisabled))
		{
			UpdateState(State.TransformBigToSmall);
		}
	}

	private void TailSpringLogic()
	{
		tail01Transform.rotation = SemiFunc.SpringQuaternionGet(tail01Spring, tail01TransformTarget.rotation);
		tail02Transform.rotation = SemiFunc.SpringQuaternionGet(tail02Spring, tail02TransformTarget.rotation);
		if (currentState == State.FlyBackUpSmall || currentState == State.FlyBackToNavMeshSmall || currentState == State.FlyBackStopSmall)
		{
			if (currentState == State.FlyBackStopSmall)
			{
				tailFlyingPivotSpring.damping = 0.8f;
				tailFlyingPivotSpring.speed = 20f;
				tailFlyingPivotTransform.rotation = tailFlyingPivotTransformTarget.rotation;
			}
			else
			{
				tailFlyingPivotSpring.damping = initialTailFlyingPivotSpringDamping;
				tailFlyingPivotSpring.speed = initialTailFlyingPivotSpringSpeed;
			}
			tailFlyingPivotTransform.rotation = SemiFunc.SpringQuaternionGet(tailFlyingPivotSpring, tailFlyingPivotTransformTarget.rotation);
		}
		else
		{
			tailFlyingPivotTransform.rotation = tailFlyingPivotTransformTarget.rotation;
		}
	}

	private void EarSpringLogic()
	{
		ear01TransformL.rotation = SemiFunc.SpringQuaternionGet(ear01Spring, ear01TransformLTarget.rotation);
		ear02TransformL.rotation = SemiFunc.SpringQuaternionGet(ear02Spring, ear02TransformLTarget.rotation);
		ear01TransformR.rotation = SemiFunc.SpringQuaternionGet(ear01Spring, ear01TransformRTarget.rotation);
		ear02TransformR.rotation = SemiFunc.SpringQuaternionGet(ear02Spring, ear02TransformRTarget.rotation);
	}

	private void RotationLogic()
	{
		if ((bool)playerTarget && (currentState == State.NoticeSmall || currentState == State.GoToPlayerSmall || currentState == State.GoToPlayerOverSmall || currentState == State.GoToPlayerUnderSmall || currentState == State.PetSmall || currentState == State.GoToPlayerOverBig))
		{
			if (currentState == State.PetSmall || currentState == State.GoToPlayerOverBig || currentState == State.GoToPlayerUnderSmall || (!IsVisionBlocked() && !playerTarget.isMoving && !IsMoving()) || enemy.Jump.jumping)
			{
				FollowTargetRotation(playerTarget.transform.position);
			}
			else
			{
				FollowControllerRotation();
			}
		}
		else if (currentState == State.FlyBackUpSmall || currentState == State.FlyBackToNavMeshSmall || currentState == State.BackToNavMeshSmall)
		{
			rotationTarget = Quaternion.LookRotation(moveBackPosition - enemy.Rigidbody.transform.position);
			rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
		}
		else if ((bool)playerTarget && (currentState == State.GoToPlayerBig || currentState == State.GoToPlayerOverBig))
		{
			FollowControllerRotation();
		}
		else if (IsTransforming())
		{
			if (stateImpulse)
			{
				if (playerTarget != null)
				{
					FollowTargetRotation(playerTarget.transform.position);
				}
				else
				{
					FollowControllerRotation();
				}
			}
		}
		else if (IsLookingUnder())
		{
			if (Vector3.Distance(lookUnderPosition, base.transform.position) > 0.1f)
			{
				FollowTargetRotation(lookUnderPosition);
			}
		}
		else
		{
			FollowControllerRotation();
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(rotationSpring, rotationTarget);
	}

	private void HeadRotationLogic()
	{
		headTransformTarget.localRotation = Quaternion.identity;
		if ((bool)playerTarget && (currentState == State.NoticeSmall || currentState == State.GoToPlayerSmall || currentState == State.GoToPlayerOverSmall || currentState == State.GoToPlayerUnderSmall || currentState == State.PetSmall || currentState == State.GoToPlayerBig || currentState == State.GoToPlayerOverBig || currentState == State.BackToNavMeshBig) && !enemy.IsStunned())
		{
			Vector3 position = playerTarget.PlayerVisionTarget.VisionTransform.position;
			if (playerTarget.isLocal)
			{
				position = playerTarget.localCamera.transform.position;
			}
			position += Vector3.down * 0.3f;
			position = SemiFunc.ClampDirection(position - headTransform.position, headIdleTransform.forward, 60f);
			headTransformTarget.rotation = Quaternion.LookRotation(position);
		}
		headTransform.rotation = SemiFunc.SpringQuaternionGet(headSpring, headTransformTarget.rotation);
	}

	private void BarkLogic()
	{
		if (HasTarget())
		{
			barkTimerFrequency = 0.5f + angryCurve.Evaluate(angryTimer / angryTimerMax) * 2f;
			barkTimer += Time.deltaTime * barkTimerFrequency;
			if (barkTimer >= barkTimerMax)
			{
				barkTimer = 0f;
				Bark();
			}
		}
		if (barkClipTimer <= 0f)
		{
			barkClipTimer = 0.01f;
			barkClipLoudness = 0f;
			if ((bool)barkSource.clip && barkSource.isPlaying)
			{
				barkSource.clip.GetData(barkClipSampleData, barkSource.timeSamples);
				float[] array = barkClipSampleData;
				foreach (float f in array)
				{
					barkClipLoudness += Mathf.Abs(f);
				}
				barkClipLoudness /= barkClipSampleDataLength;
			}
			if ((bool)stunLoopSource.clip && stunLoopSource.isPlaying)
			{
				stunLoopSource.clip.GetData(barkClipSampleData, stunLoopSource.timeSamples);
				float[] array = barkClipSampleData;
				foreach (float num in array)
				{
					barkClipLoudness += Mathf.Abs(num * 10f);
				}
				barkClipLoudness /= barkClipSampleDataLength;
			}
			if ((bool)pantingSmallSource.clip && pantingSmallSource.isPlaying)
			{
				pantingSmallSource.clip.GetData(barkClipSampleData, pantingSmallSource.timeSamples);
				float[] array = barkClipSampleData;
				foreach (float num2 in array)
				{
					barkClipLoudness += Mathf.Abs(num2 * 2.5f);
				}
				barkClipLoudness /= barkClipSampleDataLength;
			}
		}
		else
		{
			barkClipTimer -= Time.deltaTime;
		}
		if (barkClipLoudness >= 0.05f)
		{
			barkDelayTimer = 2f;
		}
		if (barkClipLoudness < 0.05f && barkDelayTimer >= 0f)
		{
			barkDelayTimer -= Time.deltaTime;
		}
		if (barkDelayTimer >= 0f)
		{
			jawTransformTarget.localRotation = Quaternion.Euler(barkClipLoudness * 72f - 6f, 0f, 0f);
		}
		else
		{
			jawTransformTarget.localRotation = Quaternion.identity;
		}
		jawTransform.localRotation = SemiFunc.SpringQuaternionGet(jawSpring, jawTransformTarget.localRotation);
	}

	private void Bark()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("BarkRPC", RpcTarget.All);
		}
		else
		{
			BarkRPC();
		}
	}

	private bool FlyBackGroundCheck()
	{
		if (stateTicker <= 0f)
		{
			stateTicker = 0.25f;
			if (Physics.Raycast(enemy.Rigidbody.transform.position, Vector3.down, out var hitInfo, 5f, LayerMask.GetMask("Default")) && Physics.Raycast(enemy.Rigidbody.transform.position + enemy.Rigidbody.transform.forward * 0.5f, Vector3.down, out hitInfo, 5f, LayerMask.GetMask("Default")) && NavMesh.SamplePosition(hitInfo.point, out var _, 0.5f, -1))
			{
				UpdateState(State.FlyBackStopSmall);
				return true;
			}
		}
		else
		{
			stateTicker -= Time.deltaTime;
		}
		return false;
	}

	private void AnnoyingJumpCheck()
	{
		if (!enemy.Jump.jumping && !(annoyingJumpPauseTimer > 0f) && Vector3.Distance(base.transform.position, playerTarget.transform.position) < 1.5f)
		{
			enemy.Jump.StuckTrigger(targetPosition - enemy.Vision.VisionTransform.position);
			annoyingJumpPauseTimer = annoyingJumpPauseFrequency;
		}
	}

	private void FindTransformationPoint()
	{
		LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 5f, 25f);
		if (!levelPoint)
		{
			levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
		}
		if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
		{
			agentDestination = hit.position;
		}
	}

	private void OverrideMoveSpeed(float _speed, float _time = 0.2f)
	{
		if (!duckBucketActive)
		{
			enemy.NavMeshAgent.OverrideAgent(_speed, 999f, _time);
		}
	}

	private void BigOverrides()
	{
		enemy.Rigidbody.OverrideFollowPosition(0.2f, 10f);
		enemy.Rigidbody.OverrideFollowRotation(0.2f, 5f);
		OverrideMoveSpeed(4f);
		enemy.OverrideType(EnemyType.Heavy, 1.5f);
		enemy.Jump.GapJumpOverride(0.2f, 11f, 10f);
		enemy.Jump.SurfaceJumpDisable(0.2f);
	}

	private void BackToNavmeshPosition()
	{
		if (SemiFunc.FPSImpulse15() && enemy.NavMeshAgent.OnNavmesh(enemy.Rigidbody.transform.position, 0.5f, _checkPit: true))
		{
			moveBackPosition = enemy.Rigidbody.transform.position;
		}
	}

	private void FollowControllerRotation()
	{
		if (IsMoving())
		{
			rotationTarget = Quaternion.LookRotation(enemy.Rigidbody.velocity.normalized);
			rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
		}
	}

	private bool IsMoving()
	{
		if (enemy.Rigidbody.velocity.magnitude > 0.25f)
		{
			return true;
		}
		return false;
	}

	private void FollowTargetRotation(Vector3 _target)
	{
		rotationTarget = Quaternion.LookRotation(_target - enemy.Rigidbody.transform.position);
		rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
	}

	private void RigidbodyReset()
	{
		enemy.Rigidbody.StuckReset();
		enemy.Jump.StuckReset();
		SemiFunc.EnemyCartJumpReset(enemy);
	}

	private void AgentReset()
	{
		enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
		enemy.NavMeshAgent.ResetPath();
	}

	private void HeadWiggle()
	{
		if (IsBig())
		{
			headWiggleTransform.localPosition = Vector3.zero;
			return;
		}
		float num = angryCurve.Evaluate(angryTimer / angryTimerMax) * 0.004f;
		float num2 = angryCurve.Evaluate(angryTimer / angryTimerMax) * 5.5f;
		headWiggleTransform.localPosition = new Vector3(Mathf.Sin(Time.time * num2 * 1f) * num, Mathf.Cos(Time.time * num2 * 0.8f) * num, 0f);
	}

	private void EyeEmissionLogic()
	{
		enemy.Health.instancedMaterials[0].EnableKeyword("_EMISSION");
		enemy.Health.instancedMaterials[0].SetColor(EmissionColorID, Color.red * eyeEmissionMultiplier);
		eyeEmissionMultiplier = Mathf.Lerp(0f, eyeEmissionMax, angryCurve.Evaluate(angryTimer / angryTimerMax));
	}

	internal bool IsBig()
	{
		if (currentState != State.BackToNavMeshBig && currentState != State.GoToPlayerBig && currentState != State.GoToPlayerOverBig && currentState != State.LookUnderStartBig && currentState != State.LookUnderBig && currentState != State.LookUnderStopBig && currentState != State.TransformBigToSmall)
		{
			return currentState == State.TransformSmallToBig;
		}
		return true;
	}

	private bool HasTarget()
	{
		if (currentState != State.GoToPlayerSmall && currentState != State.GoToPlayerOverSmall)
		{
			return currentState == State.GoToPlayerUnderSmall;
		}
		return true;
	}

	internal bool IsTransforming()
	{
		if (currentState != State.TransformSmallToBig)
		{
			return currentState == State.TransformBigToSmall;
		}
		return true;
	}

	private bool IsLookingUnder()
	{
		if (currentState != State.LookUnderStartBig && currentState != State.LookUnderBig)
		{
			return currentState == State.LookUnderStopBig;
		}
		return true;
	}

	private bool IsVisionBlocked()
	{
		if (visionTimer <= 0f)
		{
			visionTimer = 0.25f;
			Vector3 direction = playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.Vision.VisionTransform.position;
			visionPrevious = Physics.Raycast(enemy.Vision.VisionTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
		}
		return visionPrevious;
	}

	private void TutorialLogic()
	{
		if (!tutorialChecked && enemy.OnScreen.OnScreenLocal)
		{
			if (TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialEnemyElsa, 1))
			{
				TutorialDirector.instance.ActivateTip("EnemyElsa", 0f, _interrupt: true, 6f);
			}
			tutorialChecked = true;
		}
	}

	private void DuckBucketLogic()
	{
		Vector3 vector = Vector3.one;
		if (duckBucketActive)
		{
			float num = 0.8f;
			vector = new Vector3(num, num, num);
			enemy.Vision.DisableVision(0.25f);
			enemy.EnemyParent.SpawnedTimerPause(0.25f);
			enemy.NavMeshAgent.OverrideAgent(0.5f, 5f, 0.25f);
			enemy.Jump.GapJumpOverride(0.25f, 1f, 1f);
			enemy.Jump.StuckJumpOverride(0.25f, 1f, 1f);
			enemy.Jump.SurfaceJumpOverride(0.25f, 1f, 1f);
			if (duckBucketTimer <= 0f)
			{
				duckBucketActive = false;
			}
			duckBucketTimer -= Time.deltaTime;
		}
		if (visualsTransform.localScale != vector)
		{
			visualsTransform.localScale = Vector3.Lerp(visualsTransform.localScale, vector, Time.deltaTime * 3f);
		}
	}

	public void DuckBucketActive()
	{
		if (currentState == State.IdleSmall || currentState == State.IdleBreakSmall || currentState == State.RoamSmall || currentState == State.InvestigateSmall || currentState == State.NoticeSmall || currentState == State.GoToPlayerSmall || currentState == State.GoToPlayerOverSmall || currentState == State.GoToPlayerUnderSmall || currentState == State.BackToNavMeshSmall || currentState == State.LeaveSmall)
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
	private void TransformSmallToBigRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.TransformSmallToBig();
			bigColliderTransform.gameObject.SetActive(value: true);
			smallColliderTransform.gameObject.SetActive(value: false);
			enemy.Grounded.colliderSizeOriginal = new Vector3(0.5f, 0.125f, 0.5f);
		}
	}

	[PunRPC]
	private void TransformBigToSmallRPC(bool _triggerAnimation, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			if (_triggerAnimation)
			{
				anim.TransformBigToSmall();
			}
			bigColliderTransform.gameObject.SetActive(value: false);
			smallColliderTransform.gameObject.SetActive(value: true);
			enemy.Grounded.colliderSizeOriginal = new Vector3(0.25f, 0.125f, 0.25f);
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
	private void BarkRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.soundHurtPauseTimer = 0.2f;
			if (IsBig())
			{
				anim.SFXBarkBig();
				return;
			}
			anim.barkSmallSound.Pitch = Mathf.Lerp(1.1f, 0.7f, angryCurve.Evaluate(angryTimer / angryTimerMax));
			anim.barkSmallSound.PitchRandom = Mathf.Lerp(0.1f, 0.05f, angryCurve.Evaluate(angryTimer / angryTimerMax));
			anim.SFXBarkSmall();
			EnemyDirector.instance.SetInvestigate(base.transform.position, 7.5f);
		}
	}

	[PunRPC]
	private void PetRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.VFXPetParticles();
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(angryTimer);
			}
			else
			{
				angryTimer = (float)stream.ReceiveNext();
			}
		}
	}
}
