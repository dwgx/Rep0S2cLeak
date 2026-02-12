using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTricycle : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		RoamAggro,
		Investigate,
		BellRing,
		BellRing2,
		BellRing3,
		StateBeforeAttack,
		AttackTell,
		AttackTellShort,
		Attack,
		AttackShort,
		AttackDive,
		AttackOutro,
		BackToNavmesh,
		Leave,
		Stun,
		TeleportOut,
		TeleportIn,
		Despawn
	}

	public State currentState;

	public float stateTimer;

	public float stateTimerMax;

	private bool stateStart;

	private float stateTicker;

	internal Enemy enemy;

	internal PhotonView photonView;

	public EnemyTricycleVisuals visuals;

	public Transform teleportTransform;

	public List<Transform> bikeRiderTransforms;

	private EnemyPitCheck enemyPitCheck;

	public Sound audioBell;

	public Sound audioBellGlobal;

	public Sound audioFinalBellBuildup;

	public Sound audioIdle;

	public Sound audioMove;

	public Sound audioAttack;

	public Sound audioAttackLoop;

	public Sound audioAttackOutro;

	public Sound audioDeath;

	public Sound audioDeathBikeCrash;

	public Sound audioWheelRattle;

	public Sound audioWheelSqueak;

	public Sound audioTeleportOut;

	public Sound audioTeleportIn;

	public Sound audioRiderGlitchIn;

	public Sound audioRiderGlitchOut;

	public Sound audioRiderStunnedLoop;

	public Sound[] audioIdleBreaker;

	public Sound audioTell;

	public Sound audioTellGlobal;

	public Sound audioHurt;

	public Sound audioHitPlayer;

	public AnimationCurve animationCurveTeleportOut;

	public AnimationCurve animationCurveTeleportIn;

	public ParticleSystem particlesDeath;

	public ParticleSystem particlesTeleportOut;

	public ParticleSystem particlesTeleportIn;

	public ParticleSystem particlesRiderGlitchIn;

	public ParticleSystem particlesRiderGlitchOut;

	public GameObject[] handMeshes;

	private bool handMeshesActive;

	public GameObject[] handMeshesAttached;

	private bool handMeshesAttachedActive = true;

	public GameObject[] footMeshes;

	private bool footMeshesActive;

	public GameObject[] footMeshesAttached;

	private bool footMeshesAttachedActive = true;

	public Transform hurtColliderTransform;

	public Transform followTargetTransform;

	private float followTargetRotationOverrideTimer;

	private Quaternion followTargetRotationOverride;

	internal PlayerAvatar playerTarget;

	private Vector3 agentDestination;

	private Vector3 agentDirection;

	private int bellRingCount;

	private float bellRingDelay;

	private float bellRingResetTimer;

	private bool bellHasRung;

	private int bellRingCurrentRing;

	private int bellRingMaxRings;

	private float bellRingSeparation;

	private Vector3 blockedPosition;

	private float blockedTimer;

	private bool shouldRetryDestination;

	private float closeToPlayerTime;

	private PlayerAvatar nearbyPlayer;

	public Rigidbody rb;

	private float lastSeenPlayerTimer;

	private float attackCooldown;

	private float attackTimer;

	private float attackTimerMax;

	private Vector3 backToNavmeshPosition;

	private Vector3 previousPosition;

	private Vector3 currentVelocity;

	private Quaternion previousRotation;

	private float currentAngularVelocity;

	private float flyingOverPitTimer;

	private bool isFlyingOverPit;

	private float flyingOverPitYPosition;

	private float smoothedSpeed;

	private bool rattlePlayingState;

	private bool squeakPlayingState;

	private float moveSoundCooldown;

	private float previousVelocityMagnitude;

	private float previousAngularVelocity;

	private float riderGlitchTimer;

	private float riderFlickerTimer;

	private float riderHideDelayTimer;

	private bool riderGlitchParticlesPlayed;

	private bool riderGlitchOutParticlesPlayed;

	private float riderGlitchOutHideDelay;

	private float riderGlitchInShowDelay;

	private float activateRiderStunnedVisualsTimer;

	private float activeStunnedVisualsCooldown;

	private float handsLetGoTimer;

	private float feetLetGoTimer;

	private float attackVisualsTimer;

	private float idleBreakerTimer = -1f;

	private float idleBreakerTimerMax;

	private float idleBreakerActiveTimer;

	private int idleBreakerIndex;

	public List<AudioSource> talkingAudioSources;

	private float[] audioSourceSpectrum = new float[1024];

	public Transform jawTransform;

	private Quaternion jawStartRotation;

	private Quaternion jawTargetRotation;

	private SpringQuaternion jawSpring;

	private float talkVolume;

	private bool isBlockedByPlayer;

	private PlayerAvatar isBlockedByPlayerAvatar;

	private float isBlockedByPlayerCheckLast;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
		enemyPitCheck = GetComponent<EnemyPitCheck>();
		previousPosition = rb.position;
		previousRotation = rb.rotation;
		InitializeJawAnimation();
	}

	private void Update()
	{
		PitCheckLogic();
		UpdateVelocity();
		BlockedDetectionLogic();
		PlayLoopSoundLogic();
		SendVisualImpulses();
		UpdateRiderGlitch();
		UpdateFollowTargetRotation();
		CodeAnimatedTalk();
		audioRiderStunnedLoop.PlayLoop(activateRiderStunnedVisualsTimer > 0f, 5f, 5f);
		if (attackVisualsTimer <= 0f && hurtColliderTransform.gameObject.activeSelf)
		{
			hurtColliderTransform.gameObject.SetActive(value: false);
		}
		if (attackVisualsTimer > 0f)
		{
			attackVisualsTimer -= Time.deltaTime;
			HandsLetGo();
			visuals.botSystemSpringPoseAnimator.SetPoseByName("attack");
			ActivateRiderGlitch();
			if (!hurtColliderTransform.gameObject.activeSelf)
			{
				hurtColliderTransform.gameObject.SetActive(value: true);
			}
		}
		if (idleBreakerActiveTimer > 0f)
		{
			idleBreakerActiveTimer -= Time.deltaTime;
			ActivateRiderGlitch();
			visuals.botSystemSpringPoseAnimator.SetPoseByName("idle breaker");
		}
		if (LevelGenerator.Instance.Generated)
		{
			if (stateTimer <= stateTimerMax)
			{
				stateTimer += Time.deltaTime;
			}
			UpdateTimers();
			UpdateBackToNavmeshPosition();
			UpdateAgentDirection();
			if (enemy.IsStunned())
			{
				SetState(State.Stun);
			}
			else if (enemy.CurrentState == EnemyState.Despawn && currentState != State.TeleportOut)
			{
				SetState(State.Despawn);
			}
			StateMachine();
		}
	}

	private void FixedUpdate()
	{
		if (currentState == State.Attack)
		{
			FixedUpdateAttack();
		}
		if (currentState == State.AttackShort)
		{
			FixedUpdateAttack();
		}
		if (currentState == State.AttackDive)
		{
			FixedUpdateAttackDive();
		}
	}

	private void StateMachine()
	{
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
		case State.RoamAggro:
			StateRoamAggro();
			break;
		case State.Investigate:
			StateInvestigate();
			break;
		case State.BellRing:
			StateBellRing();
			break;
		case State.BellRing2:
			StateBellRing2();
			break;
		case State.BellRing3:
			StateBellRing3();
			break;
		case State.StateBeforeAttack:
			StateStateBeforeAttack();
			break;
		case State.AttackTell:
			StateAttackTell();
			break;
		case State.AttackTellShort:
			StateAttackTellShort();
			break;
		case State.Attack:
			StateAttack();
			break;
		case State.AttackShort:
			StateAttackShort();
			break;
		case State.AttackDive:
			StateAttackDive();
			break;
		case State.AttackOutro:
			StateAttackOutro();
			break;
		case State.BackToNavmesh:
			StateBackToNavmesh();
			break;
		case State.Leave:
			StateLeave();
			break;
		case State.Stun:
			StateStun();
			break;
		case State.TeleportOut:
			StateTeleportOut();
			break;
		case State.TeleportIn:
			StateTeleportIn();
			break;
		case State.Despawn:
			StateDespawn();
			break;
		}
	}

	private void StateSpawn()
	{
		if (stateStart)
		{
			stateTimerMax = 0.1f;
			stateTimer = 0f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			teleportTransform.localScale = Vector3.zero;
			stateStart = false;
		}
		enemy.StateStunned.OverrideDisable(0.5f);
		if (stateTimer >= stateTimerMax)
		{
			SetState(State.TeleportIn);
		}
	}

	private void StateIdle()
	{
		if (stateStart)
		{
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				stateTimerMax = Random.Range(4f, 8f);
			}
			stateTimer = 0f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateStart = false;
		}
		CanDoIdleBreakers();
		if (SemiFunc.EnemySpawnIdlePause())
		{
			return;
		}
		if (stateTimer >= stateTimerMax && SemiFunc.IsMasterClientOrSingleplayer())
		{
			Vector3 position = enemy.CenterTransform.position;
			List<PlayerAvatar> list = SemiFunc.PlayerGetAllPlayerAvatarWithinRange(25f, position);
			if (list != null && list.Count > 0)
			{
				SetState(State.RoamAggro);
			}
			else
			{
				SetState(State.Roam);
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemyForceLeave(enemy))
		{
			SetState(State.Leave);
		}
	}

	private void StateRoam()
	{
		if (stateStart)
		{
			stateTimerMax = 999f;
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			if (SemiFunc.IsMasterClientOrSingleplayer() && !SemiFunc.EnemyRoamPoint(enemy, out agentDestination))
			{
				SetState(State.Idle);
				return;
			}
			enemy.Rigidbody.notMovingTimer = 0f;
			stateStart = false;
		}
		CanDoIdleBreakers();
		enemy.Rigidbody.OverrideFollowPosition(0.2f, 5f, 30f);
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (enemy.NavMeshAgent.AgentVelocity.normalized.magnitude > 0.1f)
		{
			Quaternion quaternion = Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized);
			quaternion.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
			followTargetTransform.rotation = Quaternion.Lerp(followTargetTransform.rotation, quaternion, 5f * Time.deltaTime);
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (enemy.Rigidbody.notMovingTimer > 5f)
		{
			SetState(State.Despawn);
			return;
		}
		if (Vector3.Distance(enemy.CenterTransform.position, enemy.NavMeshAgent.GetPoint()) < 1f)
		{
			SetState(State.Idle);
		}
		else if (HandleBlocked() || StuckCheck())
		{
			return;
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			SetState(State.Leave);
		}
	}

	private void StateRoamAggro()
	{
		if (stateStart)
		{
			stateTimerMax = 999f;
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				if (!shouldRetryDestination)
				{
					Vector3 position = enemy.CenterTransform.position;
					bool flag = false;
					List<PlayerAvatar> list = SemiFunc.PlayerGetAllPlayerAvatarWithinRange(25f, position);
					if (list == null || list.Count == 0)
					{
						stateStart = false;
						SetState(State.Roam);
						return;
					}
					Vector3 zero = Vector3.zero;
					foreach (PlayerAvatar item in list)
					{
						zero += item.transform.position;
					}
					zero /= (float)list.Count;
					Vector3 normalized = (zero - position).normalized;
					List<LevelPoint> list2 = SemiFunc.LevelPointGetWithinDistance(zero, 0f, 25f);
					if (list2 != null && list2.Count > 0)
					{
						List<LevelPoint> list3 = new List<LevelPoint>();
						foreach (LevelPoint item2 in list2)
						{
							if (!(Vector3.Distance(item2.transform.position, zero) > 15f))
							{
								Vector3 normalized2 = (item2.transform.position - position).normalized;
								if (Vector3.Dot(normalized, normalized2) > 0.3f)
								{
									list3.Add(item2);
								}
							}
						}
						if (list3.Count > 0)
						{
							agentDestination = list3[Random.Range(0, list3.Count)].transform.position;
							flag = true;
						}
					}
					if (!flag)
					{
						stateStart = false;
						SetState(State.Roam);
						return;
					}
				}
				shouldRetryDestination = false;
			}
			enemy.Rigidbody.notMovingTimer = 0f;
			stateStart = false;
		}
		CanDoIdleBreakers();
		enemy.Rigidbody.OverrideFollowPosition(0.2f, 5f, 30f);
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (enemy.NavMeshAgent.AgentVelocity.normalized.magnitude > 0.1f)
		{
			Quaternion quaternion = Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized);
			quaternion.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
			followTargetTransform.rotation = Quaternion.Lerp(followTargetTransform.rotation, quaternion, 5f * Time.deltaTime);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (enemy.Rigidbody.notMovingTimer > 5f)
			{
				SetState(State.Despawn);
			}
			else if (Vector3.Distance(enemy.CenterTransform.position, enemy.NavMeshAgent.GetPoint()) < 1f)
			{
				SetState(State.Idle);
			}
			else if (!HandleBlocked())
			{
				StuckCheck();
			}
		}
	}

	private void StateInvestigate()
	{
		if (stateStart)
		{
			stateTimerMax = 999f;
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateStart = false;
		}
		enemy.Rigidbody.OverrideFollowPosition(0.2f, 5f, 30f);
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (enemy.NavMeshAgent.AgentVelocity.normalized.magnitude > 0.1f)
		{
			Quaternion quaternion = Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized);
			quaternion.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
			followTargetTransform.rotation = Quaternion.Lerp(followTargetTransform.rotation, quaternion, 5f * Time.deltaTime);
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (enemy.Rigidbody.notMovingTimer > 5f)
		{
			SetState(State.Despawn);
			return;
		}
		if (Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) < 1f || Vector3.Distance(enemy.Rigidbody.transform.position, agentDestination) < 1f)
		{
			FindNextWaypointFromInvestigation();
		}
		else if (HandleBlocked() || StuckCheck())
		{
			return;
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			SetState(State.Leave);
		}
	}

	private void StateBellRing()
	{
		if (stateStart)
		{
			bellRingCount++;
			stateTimerMax = 2.5f;
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			bellRingDelay = Random.Range(0.8f, 1.5f);
			bellHasRung = false;
			stateStart = false;
		}
		CanDoIdleBreakers();
		Quaternion quaternion = Quaternion.LookRotation(agentDirection);
		quaternion.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
		followTargetTransform.rotation = Quaternion.Lerp(followTargetTransform.rotation, quaternion, 5f * Time.deltaTime);
		if (stateTimer >= bellRingDelay && !bellHasRung)
		{
			DoBellRing();
			bellHasRung = true;
		}
		if (stateTimer >= stateTimerMax)
		{
			if (IsPlayerBlockingNavmeshPath())
			{
				SetState(State.BellRing2);
				return;
			}
			shouldRetryDestination = true;
			SetState(State.Roam);
		}
	}

	private void StateBellRing2()
	{
		if (stateStart)
		{
			bellRingCount++;
			bellRingMaxRings = 2;
			bellRingSeparation = 0.25f;
			bellRingCurrentRing = 0;
			bellRingDelay = Random.Range(0.8f, 1.5f);
			bellHasRung = false;
			stateTimerMax = bellRingDelay + (float)bellRingMaxRings * bellRingSeparation + 0.5f;
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateStart = false;
		}
		Quaternion quaternion = Quaternion.LookRotation(agentDirection);
		quaternion.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
		followTargetTransform.rotation = Quaternion.Lerp(followTargetTransform.rotation, quaternion, 5f * Time.deltaTime);
		CanDoIdleBreakers();
		if (stateTimer >= bellRingDelay && bellRingCurrentRing < bellRingMaxRings)
		{
			float num = bellRingDelay + (float)bellRingCurrentRing * bellRingSeparation;
			if (stateTimer >= num && !bellHasRung)
			{
				DoBellRing();
				bellRingCurrentRing++;
				bellHasRung = true;
			}
			if (bellHasRung && stateTimer >= num + 0.05f)
			{
				bellHasRung = false;
			}
		}
		if (stateTimer >= stateTimerMax)
		{
			if (IsPlayerBlockingNavmeshPath())
			{
				SetState(State.BellRing3);
				return;
			}
			shouldRetryDestination = true;
			SetState(State.Roam);
		}
	}

	private void StateBellRing3()
	{
		if (stateStart)
		{
			bellRingCount++;
			bellRingMaxRings = 3;
			bellRingSeparation = 0.2f;
			bellRingCurrentRing = 0;
			bellRingDelay = Random.Range(0.8f, 1.5f);
			bellHasRung = false;
			stateTimerMax = bellRingDelay + (float)bellRingMaxRings * bellRingSeparation + 1f;
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			audioFinalBellBuildup.Play(enemy.CenterTransform.position);
			stateStart = false;
		}
		CanDoIdleBreakers();
		Quaternion quaternion = Quaternion.LookRotation(agentDirection);
		quaternion.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
		followTargetTransform.rotation = Quaternion.Lerp(followTargetTransform.rotation, quaternion, 5f * Time.deltaTime);
		if (stateTimer >= bellRingDelay)
		{
			float num = bellRingDelay + (float)bellRingCurrentRing * bellRingSeparation;
			if (stateTimer >= num && !bellHasRung)
			{
				DoBellRing();
				bellRingCurrentRing++;
				bellHasRung = true;
			}
			if (bellHasRung && stateTimer >= num + 0.05f)
			{
				bellHasRung = false;
			}
		}
		if (stateTimer >= stateTimerMax)
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				bellRingCount = 0;
				bellRingResetTimer = 0f;
			}
			SetState(State.StateBeforeAttack);
		}
	}

	private void StateStateBeforeAttack()
	{
		if (stateStart)
		{
			attackTimer = 0f;
			stateTimerMax = 0f;
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			IdleBreakerStop();
			stateStart = false;
			bool flag = true;
			if ((bool)isBlockedByPlayerAvatar && !isBlockedByPlayerAvatar.isDisabled)
			{
				Vector3 direction = isBlockedByPlayerAvatar.PlayerVisionTarget.VisionTransform.position - enemy.CenterTransform.position;
				if (!Physics.Raycast(enemy.CenterTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
				{
					playerTarget = isBlockedByPlayerAvatar;
					flag = false;
				}
			}
			if (flag)
			{
				playerTarget = SemiFunc.PlayerGetNearestPlayerAvatarWithinRange(10f, enemy.CenterTransform.position, doRaycastCheck: true, LayerMask.GetMask("Default"));
			}
		}
		if ((bool)playerTarget && !playerTarget.isDisabled)
		{
			lastSeenPlayerTimer = 10f;
			attackTimer = 0f;
			attackTimerMax = 5f;
			SetState(State.AttackTell);
		}
		else if (lastSeenPlayerTimer <= 0f)
		{
			attackTimer = 0f;
			attackTimerMax = Random.Range(1f, 2f);
			SetState(State.AttackTellShort);
		}
		else
		{
			SetState(State.AttackTell);
		}
	}

	private void StateAttackTell()
	{
		if (stateStart)
		{
			stateTimerMax = 0.5f;
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			audioTell.Play(enemy.CenterTransform.position);
			audioTellGlobal.Play(enemy.CenterTransform.position);
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateStart = false;
		}
		ActivateAttackVisuals();
		RotationFollowTargetOrVelocity();
		if (stateTimer >= stateTimerMax)
		{
			SetState(State.Attack);
		}
	}

	private void StateAttackTellShort()
	{
		if (stateStart)
		{
			stateTimerMax = 0.5f;
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			audioAttack.Play(enemy.CenterTransform.position);
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateStart = false;
		}
		ActivateAttackVisuals();
		RotationFollowTargetOrVelocity();
		if (stateTimer >= stateTimerMax)
		{
			SetState(State.AttackShort);
		}
	}

	private void StateAttack()
	{
		if (stateStart)
		{
			stateTimerMax = Random.Range(0.5f, 2f);
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			audioAttack.Play(enemy.CenterTransform.position);
			visuals.ImpulseAttackImpact();
			stateStart = false;
		}
		RotationFollowTargetOrVelocity();
		attackTimer += Time.deltaTime;
		enemy.OverrideType(EnemyType.Heavy, 0.2f);
		ActivateAttackVisuals();
		enemy.Rigidbody.DeactivateCustomGravity(0.2f);
		enemy.Rigidbody.physGrabObject.OverrideZeroGravity(0.2f);
		visuals.ImpulseBodyShake(2f, 15f);
		visuals.ImpulseWheelShake(1.5f, 18f);
		if (attackTimer >= attackTimerMax)
		{
			SetState(State.AttackOutro);
		}
		else if (stateTimer >= stateTimerMax && (bool)playerTarget && Vector3.Dot(enemy.Rigidbody.transform.forward, (playerTarget.transform.position - rb.position).normalized) > 0.5f)
		{
			SetState(State.AttackDive);
		}
	}

	private void StateAttackShort()
	{
		if (stateStart)
		{
			stateTimerMax = 3f;
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			audioAttack.Play(enemy.CenterTransform.position);
			visuals.ImpulseAttackImpact();
			stateStart = false;
		}
		enemy.OverrideType(EnemyType.Heavy, 0.2f);
		ActivateAttackVisuals();
		RotationFollowTargetOrVelocity();
		enemy.Rigidbody.DeactivateCustomGravity(0.2f);
		enemy.Rigidbody.physGrabObject.OverrideZeroGravity(0.2f);
		visuals.ImpulseBodyShake(2f, 15f);
		visuals.ImpulseWheelShake(1.5f, 18f);
		if (stateTimer >= stateTimerMax)
		{
			SetState(State.AttackOutro);
		}
	}

	private void StateAttackDive()
	{
		if (stateStart)
		{
			stateTimerMax = Random.Range(1f, 3f);
			stateTimer = 0f;
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			bool flag = true;
			if ((bool)playerTarget && !playerTarget.isDisabled)
			{
				Vector3 direction = playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.CenterTransform.position;
				if (!Physics.Raycast(enemy.CenterTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
				{
					flag = false;
				}
			}
			if (flag)
			{
				playerTarget = SemiFunc.PlayerGetNearestPlayerAvatarWithinRange(10f, enemy.CenterTransform.position, doRaycastCheck: true, LayerMask.GetMask("Default"));
			}
			if (!playerTarget || playerTarget.isDisabled)
			{
				SetState(State.Attack);
				return;
			}
			audioAttack.Play(enemy.CenterTransform.position);
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateStart = false;
		}
		RotationFollowTargetOrVelocity();
		attackTimer += Time.deltaTime;
		ActivateAttackVisuals();
		enemy.OverrideType(EnemyType.Heavy, 0.2f);
		enemy.Rigidbody.DeactivateCustomGravity(0.2f);
		enemy.Rigidbody.physGrabObject.OverrideZeroGravity(0.2f);
		visuals.ImpulseBodyShake(2f, 15f);
		visuals.ImpulseWheelShake(1.5f, 18f);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!playerTarget || playerTarget.isDisabled)
			{
				SetState(State.Attack);
			}
			else if (Vector3.Distance(rb.position, playerTarget.transform.position) < 2f || stateTimer >= stateTimerMax)
			{
				stateTimerMax = Random.Range(0.5f, 2f);
				stateTimer = 0f;
				SetState(State.Attack);
			}
		}
	}

	private void FixedUpdateAttackDive()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && (bool)playerTarget && !playerTarget.isDisabled)
		{
			Vector3 force = (playerTarget.transform.position - rb.position).normalized * 50f;
			rb.AddForce(force, ForceMode.Impulse);
		}
	}

	private void FixedUpdateAttack()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if ((bool)playerTarget && !playerTarget.isDisabled)
			{
				Vector3 normalized = (playerTarget.transform.position - rb.position).normalized;
				float x = Mathf.PerlinNoise(Time.time * 1.5f, 0f) * 2f - 1f;
				float z = Mathf.PerlinNoise(0f, Time.time * 1.5f) * 2f - 1f;
				Vector3 vector = new Vector3(x, 0f, z) * 0.2f;
				Vector3 force = (normalized + vector).normalized * 300f;
				rb.AddForce(force, ForceMode.Force);
			}
			else
			{
				float num = Mathf.PerlinNoise(Time.time * 1.5f, 0f) * 2f - 1f;
				float num2 = Mathf.PerlinNoise(0f, Time.time * 1.5f) * 2f - 1f;
				float num3 = 45f;
				float num4 = num * num3;
				float num5 = num2 * num3;
				float x2 = num4 * 0.4f;
				float y = num5 * 0.4f;
				Vector3 forward = followTargetTransform.forward;
				Vector3 force2 = Quaternion.Euler(x2, y, 0f) * forward * 300f;
				rb.AddForce(force2, ForceMode.Force);
			}
		}
	}

	private void StateAttackOutro()
	{
		if (stateStart)
		{
			stateTimerMax = 1f;
			stateTimer = 0f;
			attackTimer = 0f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			audioAttackOutro.Play(enemy.CenterTransform.position);
			stateStart = false;
		}
		if (stateTimer >= stateTimerMax)
		{
			attackCooldown = 5f;
			SetState(State.BackToNavmesh);
		}
	}

	private void StateBackToNavmesh()
	{
		if (stateStart)
		{
			stateTimerMax = 3f;
			stateTimer = 0f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateStart = false;
		}
		Vector3 forward = backToNavmeshPosition - enemy.Rigidbody.transform.position;
		if (forward.magnitude > 0.1f)
		{
			Quaternion quaternion = Quaternion.LookRotation(forward);
			quaternion.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
			followTargetTransform.rotation = Quaternion.Lerp(followTargetTransform.rotation, quaternion, 5f * Time.deltaTime);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.NavMeshAgent.Disable(0.1f);
			enemy.Rigidbody.OverrideFollowPosition(0.2f, 5f, 30f);
			base.transform.position = Vector3.MoveTowards(base.transform.position, backToNavmeshPosition, 3f * Time.deltaTime);
			if (Vector3.Distance(base.transform.position, enemy.Rigidbody.transform.position) > 2f)
			{
				base.transform.position = enemy.Rigidbody.transform.position;
			}
			if (SemiFunc.FPSImpulse15() && NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out var _, 0.5f, -1))
			{
				SetState(State.Idle);
			}
			else if (enemy.Rigidbody.notMovingTimer > 3f && stateTimer >= stateTimerMax)
			{
				SetState(State.Despawn);
			}
		}
	}

	public void StateLeave()
	{
		if (stateStart)
		{
			stateTimerMax = 999f;
			stateTimer = 0f;
			if (!SemiFunc.EnemyLeavePoint(enemy, out agentDestination))
			{
				SetState(State.Roam);
				return;
			}
			SemiFunc.EnemyLeaveStart(enemy);
			enemy.Rigidbody.notMovingTimer = 0f;
			bellRingCount = 0;
			bellRingResetTimer = 0f;
			stateStart = false;
		}
		enemy.Rigidbody.OverrideFollowPosition(0.2f, 5f, 30f);
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (enemy.NavMeshAgent.AgentVelocity.normalized.magnitude > 0.1f)
		{
			Quaternion quaternion = Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized);
			quaternion.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
			followTargetTransform.rotation = Quaternion.Lerp(followTargetTransform.rotation, quaternion, 5f * Time.deltaTime);
		}
		if (Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) <= 1f || Vector3.Distance(enemy.Rigidbody.transform.position, agentDestination) <= 1f)
		{
			SetState(State.Idle);
		}
	}

	private void StateStun()
	{
		if (stateStart)
		{
			stateTimerMax = 0f;
			stateTimer = 0f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			IdleBreakerStop();
			stateStart = false;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && activeStunnedVisualsCooldown <= 0f)
		{
			float num = Random.Range(0.35f, 2f);
			activeStunnedVisualsCooldown = Random.Range(0.2f, 8f);
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ActivateRiderStunnedVisualsRPC", RpcTarget.All, num);
			}
			else
			{
				ActivateRiderStunnedVisualsRPC(num);
			}
		}
		if (!enemy.IsStunned())
		{
			SetState(State.BackToNavmesh);
		}
	}

	private void StateTeleportOut()
	{
		if (stateStart)
		{
			stateTimerMax = 1f;
			stateTimer = 0f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			particlesTeleportOut.Play(withChildren: true);
			audioTeleportOut.Play(enemy.CenterTransform.position);
			stateStart = false;
		}
		enemy.StateStunned.OverrideDisable(0.5f);
		ActivateRiderGlitch();
		float num = stateTimer / stateTimerMax;
		float num2 = num * num * num;
		float num3 = 1f - animationCurveTeleportOut.Evaluate(num);
		teleportTransform.localScale = Vector3.one * num3;
		float num4 = 1440f * num2;
		teleportTransform.localRotation = Quaternion.Euler(num4 * 0.5f, num4, num4 * 0.3f);
		if (stateTimer >= stateTimerMax)
		{
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.zero;
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				enemy.EnemyParent.Despawn();
			}
		}
	}

	private void StateTeleportIn()
	{
		if (stateStart)
		{
			stateTimerMax = 1f;
			stateTimer = 0f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			teleportTransform.localScale = Vector3.zero;
			particlesTeleportIn.Play(withChildren: true);
			audioTeleportIn.Play(enemy.CenterTransform.position);
			stateStart = false;
		}
		enemy.StateStunned.OverrideDisable(0.5f);
		ActivateRiderGlitch();
		float num = stateTimer / stateTimerMax;
		float num2 = num * num * num;
		float num3 = 1f - animationCurveTeleportIn.Evaluate(1f - num);
		teleportTransform.localScale = Vector3.one * num3;
		float num4 = 1440f * num2;
		teleportTransform.localRotation = Quaternion.Euler(num4 * 0.5f, num4, num4 * 0.3f);
		if (stateTimer >= stateTimerMax)
		{
			teleportTransform.localRotation = Quaternion.identity;
			teleportTransform.localScale = Vector3.one;
			SetState(State.Idle);
		}
	}

	private void AnimatedDespawn()
	{
		if (teleportTransform.localScale.x > 0.1f)
		{
			SetState(State.TeleportOut);
		}
		else if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	private void StateDespawn()
	{
		if (stateStart)
		{
			AnimatedDespawn();
		}
		else if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			SetState(State.Spawn);
		}
	}

	public void OnInvestigate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && (currentState == State.Idle || currentState == State.Roam || currentState == State.RoamAggro || currentState == State.Investigate) && Vector3.Distance(base.transform.position, enemy.StateInvestigate.onInvestigateTriggeredPosition) >= 5f)
		{
			agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			SetState(State.Investigate);
		}
	}

	public void OnHurt()
	{
		audioHurt.Play(enemy.CenterTransform.position);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			GotoBellState(_increaseFirst: true);
		}
	}

	public void OnGrabbed()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			agentDirection = (enemy.Rigidbody.onGrabbedPlayerAvatar.transform.position - base.transform.position).normalized;
			GotoBellState(_increaseFirst: true);
		}
	}

	public void OnDeath()
	{
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.05f);
		particlesDeath.Play(withChildren: true);
		audioDeath.Play(enemy.CenterTransform.position);
		audioDeathBikeCrash.Play(enemy.CenterTransform.position);
		visuals.ImpulseImpact(15f);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnHitPlayer()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("OnHitPlayerRPC", RpcTarget.All);
		}
		else
		{
			OnHitPlayerRPC();
		}
	}

	private void SetState(State newState)
	{
		if (newState != currentState && SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("SetStateRPC", RpcTarget.All, (int)newState);
			}
			else
			{
				SetStateRPC((int)newState);
			}
		}
	}

	private void GotoBellState(bool _increaseFirst = false)
	{
		if (currentState != State.Attack && currentState != State.AttackShort && currentState != State.AttackTell && currentState != State.AttackTellShort && currentState != State.StateBeforeAttack && currentState != State.Despawn && currentState != State.BellRing && currentState != State.BellRing2 && currentState != State.BellRing3)
		{
			if (_increaseFirst)
			{
				bellRingCount++;
			}
			if (bellRingCount == 0)
			{
				SetState(State.BellRing);
			}
			else if (bellRingCount == 1)
			{
				SetState(State.BellRing2);
			}
			else
			{
				SetState(State.BellRing3);
			}
		}
	}

	private bool HandleBlocked()
	{
		bool result = false;
		if (IsPlayerBlockingNavmeshPath())
		{
			lastSeenPlayerTimer = 10f;
			GotoBellState();
			result = true;
		}
		return result;
	}

	private bool StuckCheck()
	{
		return false;
	}

	private bool IsPlayerBlockingNavmeshPath()
	{
		if (Time.time - isBlockedByPlayerCheckLast > 0.2f)
		{
			isBlockedByPlayer = false;
			isBlockedByPlayerAvatar = null;
			isBlockedByPlayerCheckLast = Time.time;
			Vector3 normalized = agentDirection.normalized;
			float maxDistance = 3f;
			float radius = 0.5f;
			RaycastHit[] array = Physics.SphereCastAll(enemy.CenterTransform.position, radius, normalized, maxDistance, LayerMask.GetMask("Player", "PhysGrabObject"));
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit = array[i];
				PlayerController componentInParent = raycastHit.collider.GetComponentInParent<PlayerController>();
				PlayerAvatar playerAvatar;
				if ((bool)componentInParent)
				{
					playerAvatar = componentInParent.playerAvatarScript;
				}
				else
				{
					playerAvatar = raycastHit.collider.GetComponentInParent<PlayerAvatar>();
					if (!playerAvatar)
					{
						PlayerTumble componentInParent2 = raycastHit.collider.GetComponentInParent<PlayerTumble>();
						if ((bool)componentInParent2)
						{
							playerAvatar = componentInParent2.playerAvatar;
						}
					}
				}
				if ((bool)playerAvatar && enemy.NavMeshAgent.OnNavmesh(playerAvatar.transform.position, 1f, _checkPit: true))
				{
					isBlockedByPlayerAvatar = playerAvatar;
					isBlockedByPlayer = true;
				}
			}
		}
		return isBlockedByPlayer;
	}

	private void BlockedDetectionLogic()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || currentState != State.Roam)
		{
			return;
		}
		if (enemy.NavMeshAgent.AgentVelocity.magnitude < 0.1f)
		{
			blockedTimer += Time.deltaTime;
			if (blockedTimer > 1f)
			{
				HandleBlocked();
			}
		}
		else
		{
			blockedTimer = 0f;
		}
	}

	private void PlayBellSound()
	{
		audioBell.Play(enemy.CenterTransform.position);
	}

	private void DoBellRing()
	{
		PlayBellSound();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			bellRingResetTimer = 10f;
		}
		if (bellRingCount >= 3)
		{
			audioBellGlobal.Play(enemy.CenterTransform.position);
		}
		visuals.ImpulseBellRing();
	}

	private void FindNextWaypointFromInvestigation()
	{
		Vector3 normalized = (agentDestination - enemy.CenterTransform.position).normalized;
		LevelPoint levelPoint = FindLevelPointInCone(agentDestination, normalized, 65f, 15f, 35f);
		NavMeshHit hit;
		if (levelPoint != null)
		{
			agentDestination = levelPoint.transform.position;
			SetState(State.Roam);
		}
		else if (NavMesh.SamplePosition(agentDestination + normalized * 8f, out hit, 10f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
		{
			agentDestination = hit.position;
			SetState(State.Roam);
		}
	}

	private LevelPoint FindLevelPointInCone(Vector3 fromPosition, Vector3 forwardDirection, float coneAngle, float minDistance, float maxDistance)
	{
		LevelPoint result = null;
		float num = float.MaxValue;
		foreach (LevelPoint item in SemiFunc.LevelPointsGetAll())
		{
			Vector3 normalized = (item.transform.position - fromPosition).normalized;
			float num2 = Vector3.Distance(fromPosition, item.transform.position);
			if (!(num2 < minDistance) && !(num2 > maxDistance) && Vector3.Angle(forwardDirection, normalized) <= coneAngle * 0.5f && num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	private void UpdateTimers()
	{
		if (lastSeenPlayerTimer > 0f)
		{
			lastSeenPlayerTimer -= Time.deltaTime;
		}
		if (attackCooldown > 0f && currentState != State.Attack && currentState != State.AttackOutro)
		{
			attackCooldown -= Time.deltaTime;
		}
		if (flyingOverPitTimer <= 0f)
		{
			isFlyingOverPit = false;
		}
		if (flyingOverPitTimer > 0f)
		{
			flyingOverPitTimer -= Time.deltaTime;
		}
		if (activateRiderStunnedVisualsTimer > 0f)
		{
			activateRiderStunnedVisualsTimer -= Time.deltaTime;
		}
		if (activeStunnedVisualsCooldown > 0f)
		{
			activeStunnedVisualsCooldown -= Time.deltaTime;
		}
		if (handsLetGoTimer > 0f)
		{
			handsLetGoTimer -= Time.deltaTime;
			if (!handMeshesActive)
			{
				GameObject[] array = handMeshes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: true);
				}
				handMeshesActive = true;
			}
			if (handMeshesAttachedActive)
			{
				GameObject[] array = handMeshesAttached;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
				handMeshesAttachedActive = false;
			}
		}
		else
		{
			if (handMeshesActive)
			{
				GameObject[] array = handMeshes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
				handMeshesActive = false;
			}
			if (riderGlitchTimer > 0f && !handMeshesAttachedActive)
			{
				GameObject[] array = handMeshesAttached;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: true);
				}
				handMeshesAttachedActive = true;
			}
		}
		if (feetLetGoTimer > 0f)
		{
			feetLetGoTimer -= Time.deltaTime;
			if (!footMeshesActive)
			{
				GameObject[] array = footMeshes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: true);
				}
				footMeshesActive = true;
			}
			if (footMeshesAttachedActive)
			{
				GameObject[] array = footMeshesAttached;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
				footMeshesAttachedActive = false;
			}
		}
		else
		{
			if (footMeshesActive)
			{
				GameObject[] array = footMeshes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
				footMeshesActive = false;
			}
			if (riderGlitchTimer > 0f && !footMeshesAttachedActive)
			{
				GameObject[] array = footMeshesAttached;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: true);
				}
				footMeshesAttachedActive = true;
			}
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (currentState == State.AttackTell || currentState == State.AttackTellShort || currentState == State.Attack || currentState == State.AttackDive || currentState == State.AttackOutro)
		{
			bellRingResetTimer = 0f;
			bellRingCount = 0;
		}
		if (bellRingResetTimer > 0f)
		{
			bellRingResetTimer -= Time.deltaTime;
			if (bellRingResetTimer <= 0f)
			{
				bellRingCount = 0;
			}
		}
	}

	private void UpdateBackToNavmeshPosition()
	{
		if (SemiFunc.FPSImpulse15() && enemy.NavMeshAgent.OnNavmesh(enemy.Rigidbody.transform.position, 0.5f, _checkPit: true))
		{
			backToNavmeshPosition = enemy.Rigidbody.transform.position;
		}
	}

	private void UpdateAgentDirection()
	{
		if (enemy.NavMeshAgent.AgentVelocity.magnitude > 0.1f)
		{
			agentDirection = enemy.NavMeshAgent.AgentVelocity.normalized;
		}
	}

	private void PlayLoopSoundLogic()
	{
		if (currentState == State.AttackTell)
		{
			_ = 1;
		}
		else
			_ = currentState == State.AttackTellShort;
		float t = Mathf.Clamp01(stateTimer / 0.5f);
		Mathf.Lerp(0.8f, 1.5f, t);
		bool playing = currentState == State.Attack || currentState == State.AttackShort;
		audioAttackLoop.PlayLoop(playing, 1f, 1f);
		float num = currentVelocity.magnitude * 10f;
		smoothedSpeed = Mathf.Lerp(smoothedSpeed, num, Time.deltaTime * 5f);
		float t2 = Mathf.Clamp01(smoothedSpeed / 5f);
		float pitchMultiplier = Mathf.Lerp(1f, 1.6f, t2);
		if (rattlePlayingState)
		{
			if (num < 1f)
			{
				rattlePlayingState = false;
			}
		}
		else if (num > 1.2f)
		{
			rattlePlayingState = true;
		}
		audioWheelRattle.PlayLoop(rattlePlayingState, 2f, 1f, pitchMultiplier);
		float t3 = Mathf.Clamp01(smoothedSpeed);
		float pitchMultiplier2 = Mathf.Lerp(0.9f, 1.5f, t3);
		if (squeakPlayingState)
		{
			if (num < 0.1f)
			{
				squeakPlayingState = false;
			}
		}
		else if (num > 0.2f)
		{
			squeakPlayingState = true;
		}
		audioWheelSqueak.PlayLoop(squeakPlayingState, 2f, 1f, pitchMultiplier2);
		if (SemiFunc.FPSImpulse5())
		{
			float num2 = currentVelocity.magnitude * 3f;
			float num3 = Mathf.Abs(num2 - previousVelocityMagnitude);
			float num4 = Mathf.Abs(currentAngularVelocity - previousAngularVelocity);
			if ((num3 > 0.15f || num4 > 5f) && moveSoundCooldown <= 0f)
			{
				audioMove.Play(enemy.CenterTransform.position);
				moveSoundCooldown = 0.3f;
			}
			previousVelocityMagnitude = num2;
			previousAngularVelocity = currentAngularVelocity;
		}
		if (moveSoundCooldown > 0f)
		{
			moveSoundCooldown -= Time.deltaTime;
		}
	}

	private void UpdateVelocity()
	{
		if (SemiFunc.FPSImpulse30())
		{
			currentVelocity = visuals.transform.position - previousPosition;
			previousPosition = visuals.transform.position;
			float f = Mathf.DeltaAngle(previousRotation.eulerAngles.y, visuals.transform.rotation.eulerAngles.y);
			currentAngularVelocity = Mathf.Abs(f);
			previousRotation = visuals.transform.rotation;
		}
	}

	private void SendVisualImpulses()
	{
		float num = currentVelocity.magnitude * 100f;
		float num2 = num + currentAngularVelocity;
		float speed = num2 * 100f;
		visuals.ImpulseWheelRotation(speed);
		float num3 = Mathf.Lerp(5f, 25f, Mathf.Clamp01(num2 / 5f));
		float num4 = num / 2f;
		float num5 = currentAngularVelocity / 30f;
		float num6 = num4 + num5;
		if (num6 > 0.01f)
		{
			visuals.ImpulseWheelShake(num6 * 2f, num3);
			visuals.ImpulseBodyShake(num6 * 1f, num3 * 0.7f);
			visuals.ImpulseHandlebarShake(num6 * 1f, num3 * 0.8f);
		}
	}

	private void PitCheckLogic()
	{
		if (isFlyingOverPit)
		{
			followTargetTransform.localPosition = Vector3.up * 0.5f;
		}
		else
		{
			followTargetTransform.localPosition = Vector3.zero;
		}
		if (!enemy.IsStunned())
		{
			enemyPitCheck.CheckPit();
			if (enemyPitCheck.isOverPit)
			{
				enemy.Rigidbody.DeactivateCustomGravity(0.2f);
				enemy.Rigidbody.physGrabObject.OverrideZeroGravity(0.2f);
				enemy.Rigidbody.OverrideFollowPositionGravityDisable(0.2f);
				SetFlyingOverPitTimer(0.1f);
			}
		}
	}

	public void SetFlyingOverPitTimer(float duration)
	{
		ActivateRiderGlitch();
		flyingOverPitTimer = duration;
		isFlyingOverPit = true;
		flyingOverPitYPosition = enemy.Rigidbody.transform.position.y;
	}

	private void ActivateRiderGlitch(bool _letGoOfBike = false)
	{
		if (riderGlitchTimer <= 0f)
		{
			riderGlitchTimer = 0.1f;
			riderGlitchParticlesPlayed = false;
			riderGlitchOutParticlesPlayed = false;
			riderGlitchInShowDelay = 0.1f;
		}
		else
		{
			riderGlitchTimer = 0.1f;
		}
	}

	private void GlitchInEffects()
	{
		particlesRiderGlitchIn.Play(withChildren: true);
		audioRiderGlitchIn.Play(enemy.CenterTransform.position);
		visuals.botSystemSpringPoseAnimator.SetRandomForceOnAllBones(100f, 300f);
		visuals.botSystemSpringPoseAnimator.SetTempSpeedAndDampingOnAllBones(65f, 0.3f, 0.4f);
	}

	private void GlitchOutEffects()
	{
		particlesRiderGlitchOut.Play(withChildren: true);
		audioRiderGlitchOut.Play(enemy.CenterTransform.position);
		visuals.botSystemSpringPoseAnimator.SetRandomForceOnAllBones(100f, 300f);
		visuals.botSystemSpringPoseAnimator.SetTempSpeedAndDampingOnAllBones(65f, 0.3f, 0.4f);
	}

	private void SetFollowTargetRotation(Quaternion rotation)
	{
		followTargetRotationOverride = rotation;
		followTargetRotationOverrideTimer = 0.1f;
	}

	private void UpdateFollowTargetRotation()
	{
		if (followTargetRotationOverrideTimer <= 0f && followTargetRotationOverrideTimer != -123f)
		{
			followTargetTransform.localRotation = Quaternion.identity;
			followTargetRotationOverrideTimer = -123f;
		}
		if (followTargetRotationOverrideTimer > 0f)
		{
			followTargetRotationOverrideTimer -= Time.deltaTime;
			Quaternion localRotation = Quaternion.Inverse(followTargetTransform.parent.rotation) * followTargetRotationOverride;
			followTargetTransform.localRotation = localRotation;
		}
	}

	private void UpdateRiderGlitch()
	{
		if (riderGlitchTimer > 0f)
		{
			riderGlitchOutParticlesPlayed = false;
			riderGlitchTimer -= Time.deltaTime;
			if (riderGlitchTimer <= 0f && !riderGlitchOutParticlesPlayed)
			{
				GlitchOutEffects();
				riderGlitchOutParticlesPlayed = true;
				riderGlitchOutHideDelay = 0.2f;
			}
		}
		if (riderGlitchOutHideDelay > 0f)
		{
			riderGlitchOutHideDelay -= Time.deltaTime;
			if (riderGlitchOutHideDelay <= 0f)
			{
				foreach (Transform bikeRiderTransform in bikeRiderTransforms)
				{
					if ((bool)bikeRiderTransform && bikeRiderTransform.gameObject.activeSelf)
					{
						bikeRiderTransform.gameObject.SetActive(value: false);
					}
				}
				riderFlickerTimer = 0f;
				riderHideDelayTimer = 0f;
			}
		}
		if (riderGlitchTimer <= 0f && riderGlitchOutHideDelay <= 0f)
		{
			foreach (Transform bikeRiderTransform2 in bikeRiderTransforms)
			{
				if ((bool)bikeRiderTransform2 && bikeRiderTransform2.gameObject.activeSelf)
				{
					bikeRiderTransform2.gameObject.SetActive(value: false);
				}
			}
			riderFlickerTimer = 0f;
			riderHideDelayTimer = 0f;
		}
		if (riderGlitchInShowDelay > 0f)
		{
			riderGlitchInShowDelay -= Time.deltaTime;
			if (riderGlitchInShowDelay <= 0f)
			{
				if (!riderGlitchParticlesPlayed)
				{
					GlitchInEffects();
					riderGlitchParticlesPlayed = true;
				}
				foreach (Transform bikeRiderTransform3 in bikeRiderTransforms)
				{
					if ((bool)bikeRiderTransform3)
					{
						bikeRiderTransform3.gameObject.SetActive(value: true);
					}
				}
			}
		}
		if (riderGlitchTimer > 0f && riderGlitchInShowDelay <= 0f)
		{
			if (riderHideDelayTimer > 0f)
			{
				riderHideDelayTimer -= Time.deltaTime;
				if (riderHideDelayTimer <= 0f)
				{
					foreach (Transform bikeRiderTransform4 in bikeRiderTransforms)
					{
						if ((bool)bikeRiderTransform4)
						{
							bikeRiderTransform4.gameObject.SetActive(value: false);
						}
					}
					riderFlickerTimer = 0f;
				}
			}
			if (riderFlickerTimer > 0f)
			{
				riderFlickerTimer -= Time.deltaTime;
				if (riderFlickerTimer <= 0f && riderHideDelayTimer <= 0f)
				{
					bool flag = false;
					foreach (Transform bikeRiderTransform5 in bikeRiderTransforms)
					{
						if ((bool)bikeRiderTransform5 && bikeRiderTransform5.gameObject.activeSelf)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						riderHideDelayTimer = 0.1f;
					}
				}
			}
			if (riderFlickerTimer <= 0f && riderHideDelayTimer <= 0f)
			{
				bool flag2 = false;
				foreach (Transform bikeRiderTransform6 in bikeRiderTransforms)
				{
					if ((bool)bikeRiderTransform6 && !bikeRiderTransform6.gameObject.activeSelf)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					foreach (Transform bikeRiderTransform7 in bikeRiderTransforms)
					{
						if ((bool)bikeRiderTransform7)
						{
							bikeRiderTransform7.gameObject.SetActive(value: true);
						}
					}
					riderFlickerTimer = Random.Range(0.1f, 0.5f);
				}
			}
		}
		if (activateRiderStunnedVisualsTimer > 0f)
		{
			visuals.botSystemSpringPoseAnimator.SetPoseByName("stunned");
			ActivateRiderGlitch(_letGoOfBike: true);
		}
	}

	public void HandsLetGo()
	{
		handsLetGoTimer = 0.1f;
		visuals.HandsLetGo(0.1f);
	}

	public void FeetLetGo()
	{
		feetLetGoTimer = 0.1f;
		visuals.FeetLetGo(0.1f);
	}

	public void ActivateAttackVisuals()
	{
		attackVisualsTimer = 0.1f;
	}

	private void CanDoIdleBreakers()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		idleBreakerTimer += Time.deltaTime;
		if (idleBreakerTimer <= 0f)
		{
			idleBreakerTimer = 0f;
			idleBreakerTimerMax = Random.Range(1f, 20f);
		}
		else if (idleBreakerTimer >= idleBreakerTimerMax)
		{
			idleBreakerTimer = 0f;
			idleBreakerTimerMax = Random.Range(1f, 20f);
			int num = Random.Range(0, 6);
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("IdleBreakerRPC", RpcTarget.All, num);
			}
			else
			{
				IdleBreakerRPC(num);
			}
		}
	}

	private void IdleBreakerStop()
	{
		idleBreakerActiveTimer = 0f;
		if (audioIdleBreaker.Length > idleBreakerIndex && idleBreakerIndex >= 0)
		{
			audioIdleBreaker[idleBreakerIndex].Stop();
		}
	}

	private void RotationFollowTargetOrVelocity()
	{
		Vector3 zero = Vector3.zero;
		zero = ((!playerTarget || playerTarget.isDisabled) ? rb.velocity.normalized : (playerTarget.transform.position - rb.position).normalized);
		if (zero.sqrMagnitude > float.Epsilon)
		{
			Quaternion quaternion = Quaternion.LookRotation(zero);
			followTargetTransform.rotation = Quaternion.Lerp(followTargetTransform.rotation, quaternion, 50f * Time.deltaTime);
		}
	}

	private void InitializeJawAnimation()
	{
		jawStartRotation = jawTransform.localRotation;
		jawTargetRotation = jawStartRotation;
		jawSpring = new SpringQuaternion();
		jawSpring.damping = 0.6f;
		jawSpring.speed = 30f;
	}

	private void CodeAnimatedTalk()
	{
		if (SemiFunc.FPSImpulse30())
		{
			float num = 0f;
			foreach (AudioSource talkingAudioSource in talkingAudioSources)
			{
				talkingAudioSource.GetSpectrumData(audioSourceSpectrum, 0, FFTWindow.Hamming);
				float num2 = audioSourceSpectrum[0] * 100000f;
				num += num2;
			}
			if (num > 46f)
			{
				num = 46f;
			}
			talkVolume = num;
			jawSpring.springVelocity += Random.insideUnitSphere * 0.5f;
		}
		float x = talkVolume;
		jawTargetRotation = jawStartRotation * Quaternion.Euler(x, 0f, 0f);
		jawTransform.localRotation = SemiFunc.SpringQuaternionGet(jawSpring, jawTargetRotation);
		if (talkVolume > 0f)
		{
			talkVolume = Mathf.Lerp(talkVolume, 0f, Time.deltaTime * 5f);
		}
	}

	[PunRPC]
	private void SetStateRPC(int newStateInt, PhotonMessageInfo info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			stateStart = true;
			currentState = (State)newStateInt;
		}
	}

	[PunRPC]
	private void ActivateRiderStunnedVisualsRPC(float duration, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			activateRiderStunnedVisualsTimer = duration;
		}
	}

	[PunRPC]
	private void IdleBreakerRPC(int index, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			idleBreakerIndex = index;
			if (audioIdleBreaker.Length > index && index >= 0)
			{
				Sound sound = audioIdleBreaker[index];
				sound.Play(enemy.CenterTransform.position);
				idleBreakerActiveTimer = sound.Source.clip.length - 0.5f;
			}
		}
	}

	[PunRPC]
	private void OnHitPlayerRPC()
	{
		audioHitPlayer.Play(rb.position);
	}
}
