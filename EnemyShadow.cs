using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyShadow : MonoBehaviour
{
	public enum State
	{
		Spawn,
		ChooseTarget,
		Despawn,
		Follow,
		Stun,
		Slap,
		CoolDown,
		StuckAttack,
		Leave
	}

	[Header("OTHER SCRIPTS")]
	public EnemyShadowAnim anim;

	public BotSystemSpringPoseAnimator springPoseAnimator;

	public Transform torsoBend;

	public Transform headBend;

	public Transform rightArmBend;

	public Transform leftArmBend;

	public Transform leftEyePivot;

	public Transform rightEyePivot;

	public Transform rightWristPivot;

	public Transform leftWristPivot;

	public Transform leftArmPoint;

	public Transform rightArmPoint;

	public Transform neckBend;

	public Transform leftHandDownPos;

	public Transform rightHandDownPos;

	public Transform visionTransform;

	[Min(0f)]
	public float headTurnSpeed = 2f;

	[Min(0f)]
	public float eyeTurnSpeed = 12f;

	[Space]
	public AnimationCurve slapAnimationCurve = new AnimationCurve();

	[Header("Hand animators")]
	public Animator leftHandAnimator;

	public Animator rightHandAnimator;

	[Space]
	[Header("COLLIDERS")]
	public Collider topCollider;

	public List<Collider> crouchColliders = new List<Collider>();

	public List<Transform> shootingColliders = new List<Transform>();

	[Header("Collision Checkers (From Top to Bottom)")]
	public List<Transform> collisionCheckers = new List<Transform>();

	[Header("Hurt Collider")]
	public GameObject hurtCollider;

	public GameObject hurtColliderWave;

	[Space]
	[Header("Hand movement range")]
	public Transform handArea;

	public Transform handAreaUnder;

	public Transform handAreaUnderSlapReady;

	[Space]
	private Quaternion horizontalRotationTarget = Quaternion.identity;

	public SpringQuaternion horizontalRotationSpring;

	[Header("Hair Springs")]
	public SpringQuaternion hairSpring1 = new SpringQuaternion();

	public Transform hairPivot1;

	public Transform hairTarget1;

	public SpringQuaternion hairSpring2 = new SpringQuaternion();

	public Transform hairPivot2;

	public Transform hairTarget2;

	public SpringQuaternion hairSpring3 = new SpringQuaternion();

	public Transform hairPivot3;

	public Transform hairTarget3;

	public SpringQuaternion hairSpring4 = new SpringQuaternion();

	public Transform hairPivot4;

	public Transform hairTarget4;

	[HideInInspector]
	public State currentState;

	[Space]
	public GameObject screenVeinEffect;

	private HurtCollider hurtColliderScript;

	private HurtCollider hurtColliderWaveScript;

	private State previousState;

	private PlayerAvatar actualPlayerTarget;

	private PlayerAvatar playerTarget;

	private PlayerAvatar playerTargetPrevious;

	private Enemy enemy;

	private PhotonView photonView;

	private GameObject activeCollider;

	private EnemyShadowScreenVeinEffect activeVeinEffect;

	private List<float> torsoBends = new List<float> { 57f, 88f, 130f };

	private List<float> headBends = new List<float> { -79f, -100f, -108f };

	private List<float> neckBends = new List<float> { 21f, 18f, -20f };

	private int bendState;

	private bool stateImpulse = true;

	private bool closeEnoughToLook;

	private bool seesTarget = true;

	private bool closeEnoughToReachOut;

	private bool closeEnoughToSlap;

	private bool bendStateChangedCollider;

	private bool stateChangedClient;

	private bool reachDownToPlayer;

	private bool isStuckAttacking;

	private bool annoyed;

	private bool skipTargetTell;

	private bool seesTargetPrevious = true;

	private bool slapNetValid;

	private bool firstTimeColliderCheck;

	private Vector3 originalRightWristPosition;

	private Vector3 originalRightWristRotation;

	private Vector3 originalLeftWristPosition;

	private Vector3 originalLeftWristRotation;

	private Vector3 rightHandSlapLocation;

	private Vector3 leftHandSlapLocation;

	private Vector3 rightWristSlapRotation = new Vector3(6.04f, -12f, 354f);

	private Vector3 leftWristSlapRotation = new Vector3(6.45f, 12f, 9f);

	private Vector3 _rwVel;

	private Vector3 _lwVel;

	private Vector3 agentDestination;

	private Vector3 stuckAttackTarget;

	private List<Vector3> rightBendHandPoses = new List<Vector3>
	{
		new Vector3(0.17f, 2.05f, 0.3f),
		new Vector3(0.185f, 1.9f, 0.5f),
		new Vector3(0.25f, 1.6f, 0.7f)
	};

	private List<Vector3> leftBendHandPoses = new List<Vector3>
	{
		new Vector3(-0.17f, 2.05f, 0.3f),
		new Vector3(-0.185f, 1.9f, 0.5f),
		new Vector3(-0.25f, 1.6f, 0.7f)
	};

	private float stateTimer;

	private float stateTimerClient;

	private float handMoveDistance = 3f;

	private float slapDistance = 1.5f;

	private float slapDuration = 0.3f;

	private float torsoBendingSpeed = 1.7f;

	private float maxHandSize = 3f;

	private float distanceFromPlayer;

	private float postStunTimer;

	private float noSeeTimer;

	private float annoyedTimer;

	private float annoyedFollowTimer;

	private float waveColliderActiveTime;

	private int stuckAttackAttempts;

	private double slapStartTimeNetwork;

	private bool visionBlocked;

	private float visionBlockedCheckTime;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
		hurtColliderScript = hurtCollider.GetComponent<HurtCollider>();
		hurtColliderWaveScript = hurtColliderWave.GetComponent<HurtCollider>();
		originalRightWristPosition = rightWristPivot.localPosition;
		originalRightWristRotation = rightWristPivot.localEulerAngles;
		originalLeftWristPosition = leftWristPivot.localPosition;
		originalLeftWristRotation = leftWristPivot.localEulerAngles;
		activeCollider = topCollider.gameObject;
		previousState = currentState;
	}

	private void Update()
	{
		distanceFromPlayer = DistanceFromPlayer();
		closeEnoughToReachOut = distanceFromPlayer < handMoveDistance;
		closeEnoughToLook = distanceFromPlayer < 7f;
		reachDownToPlayer = closeEnoughToReachOut && playerTarget.transform.position.y - base.transform.position.y < 0.2f;
		stateChangedClient = false;
		if (previousState != currentState)
		{
			stateChangedClient = true;
			previousState = currentState;
			stateTimerClient = 0f;
		}
		if (postStunTimer > 0f)
		{
			postStunTimer -= Time.deltaTime;
		}
		stateTimerClient += Time.deltaTime;
		if (playerTarget != playerTargetPrevious)
		{
			if (!skipTargetTell)
			{
				if ((bool)playerTargetPrevious && playerTargetPrevious.isLocal && (!playerTarget || !playerTarget.isLocal))
				{
					PlayerTargetStopTell();
				}
				else if ((bool)playerTarget && playerTarget.isLocal)
				{
					PlayerTargetTell();
				}
			}
			playerTargetPrevious = playerTarget;
			skipTargetTell = false;
		}
		BendLogic();
		GeneralRotationLogic();
		SpringLogic();
		HandLogic();
		anim.TurnSoundLogic(headBend, enemy.Rigidbody.rb, torsoBend);
		if (currentState == State.Slap)
		{
			if (stateChangedClient)
			{
				anim.ToggleWooshLines(_state: true);
			}
			Slap();
		}
		else
		{
			if (hurtCollider.activeSelf)
			{
				hurtCollider.SetActive(value: false);
			}
			if (hurtColliderWave.activeSelf && waveColliderActiveTime > 0f)
			{
				waveColliderActiveTime -= Time.deltaTime;
				if (waveColliderActiveTime <= 0f)
				{
					hurtColliderWave.SetActive(value: false);
					hurtColliderWaveScript.playerLogic = true;
				}
			}
			anim.ToggleWooshLines(_state: false);
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (closeEnoughToReachOut)
		{
			if (!VisionBlocked())
			{
				noSeeTimer = 0f;
			}
			else
			{
				noSeeTimer += Time.deltaTime;
			}
			seesTarget = noSeeTimer < 1f;
		}
		else
		{
			seesTarget = false;
			noSeeTimer = 1f;
		}
		if (seesTarget != seesTargetPrevious && closeEnoughToReachOut)
		{
			seesTargetPrevious = seesTarget;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdateSeesTargetRPC", RpcTarget.Others, seesTarget);
			}
		}
		if (annoyedTimer > 0f)
		{
			annoyedTimer -= Time.deltaTime;
		}
		if (enemy.CurrentState == EnemyState.Despawn)
		{
			UpdateState(State.Despawn);
		}
		else if (enemy.IsStunned())
		{
			UpdateState(State.Stun);
		}
		else if (currentState == State.Follow && (!playerTarget || playerTarget.isDisabled))
		{
			UpdateState(State.ChooseTarget);
		}
		switch (currentState)
		{
		case State.Spawn:
			StateSpawn();
			break;
		case State.Despawn:
			StateDespawn();
			break;
		case State.Follow:
			StateFollow();
			break;
		case State.Stun:
			StateStun();
			break;
		case State.ChooseTarget:
			StateChooseTarget();
			break;
		case State.Slap:
			StateSlap();
			break;
		case State.CoolDown:
			StateCoolDown();
			break;
		case State.StuckAttack:
			StateStuckAttack();
			break;
		case State.Leave:
			StateLeave();
			break;
		}
	}

	private void UpdateState(State _nextState)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || _nextState == currentState)
		{
			return;
		}
		SemiLogger.LogMonika("Change state to : " + _nextState);
		stateImpulse = true;
		currentState = _nextState;
		if (GameManager.Multiplayer())
		{
			photonView.RPC("UpdateStateRPC", RpcTarget.Others, _nextState);
		}
		if (_nextState == State.Slap)
		{
			Vector3 localPosition = rightWristPivot.localPosition;
			Vector3 localPosition2 = leftWristPivot.localPosition;
			double num = PhotonNetwork.Time + 0.06;
			ApplySlapStart(num, localPosition, localPosition2);
			if (GameManager.Multiplayer())
			{
				photonView.RPC("StartSlapRPC", RpcTarget.Others, num, localPosition, localPosition2);
			}
		}
	}

	[PunRPC]
	private void UpdateStateRPC(State _nextState, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _nextState;
		}
	}

	private void UpdatePlayerTarget(PlayerAvatar _player)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (SemiFunc.IsMultiplayer() && _player != playerTarget)
		{
			int num = -1;
			if ((bool)_player)
			{
				num = _player.photonView.ViewID;
			}
			photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.Others, num, skipTargetTell);
		}
		playerTarget = _player;
	}

	[PunRPC]
	private void UpdatePlayerTargetRPC(int photonViewID, bool _skipTell, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			skipTargetTell = _skipTell;
			playerTarget = SemiFunc.PlayerAvatarGetFromPhotonID(photonViewID);
		}
	}

	private void UpdateBendState(int _bendState)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && _bendState != bendState)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdateBendStateRPC", RpcTarget.All, _bendState);
			}
			else
			{
				UpdateBendStateRPC(_bendState);
			}
			bendStateChangedCollider = true;
			firstTimeColliderCheck = true;
		}
	}

	[PunRPC]
	private void UpdateBendStateRPC(int _bendState, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		bendState = _bendState;
		if (activeCollider.activeSelf)
		{
			activeCollider.SetActive(value: false);
		}
		foreach (Transform shootingCollider in shootingColliders)
		{
			if (shootingCollider.gameObject.activeSelf)
			{
				shootingCollider.gameObject.SetActive(value: false);
			}
		}
		if (bendState != -1)
		{
			shootingColliders[bendState].gameObject.SetActive(value: true);
		}
	}

	private void EnableHurtCollider(Vector3 _pos)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("EnableHurtColliderRPC", RpcTarget.All, _pos);
			}
			else
			{
				HurtColliderLogic(_pos);
			}
		}
	}

	private IEnumerator EnableWaveColliderDelayed(Vector3 _pos)
	{
		yield return new WaitForSeconds(0.06f);
		hurtColliderWave.transform.position = _pos + base.transform.forward * 1f;
		hurtColliderWave.SetActive(value: true);
		waveColliderActiveTime = 0.2f;
	}

	[PunRPC]
	private void EnableHurtColliderRPC(Vector3 _pos)
	{
		HurtColliderLogic(_pos);
	}

	private void HurtColliderLogic(Vector3 _pos)
	{
		hurtCollider.transform.position = _pos;
		hurtCollider.SetActive(value: true);
		StartCoroutine(EnableWaveColliderDelayed(_pos));
		anim.PlayClapSound();
		anim.PlaySlapParticles(_pos);
		anim.ClapCameraShake();
	}

	[PunRPC]
	private void ToggleTopColliderRPC(bool _active, int _bendState, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			Collider collider = ((_bendState != -1) ? crouchColliders[_bendState] : topCollider);
			collider.gameObject.SetActive(_active);
			activeCollider = collider.gameObject;
		}
	}

	private void ApplySlapStart(double _slapStartTimeNetwork, Vector3 _rightStartLocal, Vector3 _leftStartLocal)
	{
		slapStartTimeNetwork = _slapStartTimeNetwork;
		slapNetValid = PhotonNetwork.IsConnected && PhotonNetwork.InRoom;
		rightHandSlapLocation = _rightStartLocal;
		leftHandSlapLocation = _leftStartLocal;
		anim.ToggleWooshLines(_state: true);
		anim.clapTell.Play(base.transform.position);
	}

	[PunRPC]
	private void StartSlapRPC(double t0, Vector3 rightStartLocal, Vector3 leftStartLocal, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			ApplySlapStart(t0, rightStartLocal, leftStartLocal);
		}
	}

	[PunRPC]
	private void UpdateSeesTargetRPC(bool _seesTarget, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			seesTarget = _seesTarget;
		}
	}

	private void StateSpawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			springPoseAnimator.CullingOverride(2f);
			UpdatePlayerTarget(null);
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f && !SemiFunc.EnemySpawnIdlePause())
		{
			if (!playerTarget)
			{
				UpdateState(State.ChooseTarget);
			}
			else
			{
				UpdateState(State.Follow);
			}
		}
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			ResetNavMesh();
		}
	}

	private void StateStun()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			ResetNavMesh();
			UpdatePlayerTarget(null);
		}
		if (!enemy.IsStunned())
		{
			postStunTimer = 2f;
			UpdateState(State.ChooseTarget);
		}
	}

	private void StateFollow()
	{
		if (stateImpulse)
		{
			if (!isStuckAttacking)
			{
				enemy.Rigidbody.notMovingTimer = 0f;
			}
			else
			{
				enemy.Rigidbody.notMovingTimer = 1f;
			}
			stateImpulse = false;
			stateTimer = 5f;
		}
		if (annoyed && annoyedFollowTimer > 0f)
		{
			if (currentState == State.Leave)
			{
				annoyedFollowTimer = 0f;
				annoyed = false;
			}
			else
			{
				annoyedFollowTimer -= Time.deltaTime;
				if (annoyedFollowTimer <= 0f)
				{
					UpdateState(State.ChooseTarget);
					return;
				}
			}
		}
		if (!enemy.OnScreen.OnScreenAny && enemy.PlayerDistance.PlayerDistanceClosest >= 15f && SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
			return;
		}
		if (reachDownToPlayer && distanceFromPlayer < 1f)
		{
			if (!HandsInPositionToSlap())
			{
				Vector3 normalized = (playerTarget.transform.position - base.transform.position).normalized;
				agentDestination = base.transform.position - normalized * 3f;
				enemy.NavMeshAgent.SetDestination(agentDestination);
			}
			else
			{
				enemy.NavMeshAgent.ResetPath();
			}
		}
		else
		{
			enemy.NavMeshAgent.SetDestination(playerTarget.transform.position);
		}
		if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			if (!isStuckAttacking)
			{
				AttackNearestPhysObjectOrFollow();
				return;
			}
			stuckAttackAttempts++;
			if (stuckAttackAttempts >= 5)
			{
				stuckAttackTarget = Vector3.zero;
				stuckAttackAttempts = 0;
				isStuckAttacking = false;
				UpdateState(State.Leave);
				return;
			}
			stuckAttackTarget = SemiFunc.EnemyGetNearestPhysObject(enemy);
			if (!(stuckAttackTarget == Vector3.zero) && !(Vector3.Distance(base.transform.position, stuckAttackTarget) > 5f))
			{
				UpdateState(State.StuckAttack);
				return;
			}
			stuckAttackTarget = Vector3.zero;
			stuckAttackAttempts = 0;
			isStuckAttacking = false;
		}
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer < 0f)
			{
				UpdateState(State.Leave);
				return;
			}
		}
		if (HandsInPositionToSlap() || (HandsInDownPositionToSlap() && postStunTimer <= 0f))
		{
			isStuckAttacking = false;
			UpdateState(State.Slap);
		}
	}

	private void StateSlap()
	{
		if (stateImpulse)
		{
			ResetNavMesh();
			stateTimer = 0f;
			stateImpulse = false;
		}
		if (slapNetValid)
		{
			if (NetNow() >= slapStartTimeNetwork + (double)slapDuration)
			{
				UpdateState(State.CoolDown);
			}
			return;
		}
		stateTimer += Time.deltaTime;
		if (stateTimer >= slapDuration)
		{
			UpdateState(State.CoolDown);
		}
	}

	private void StateChooseTarget()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
			if (annoyed && (bool)actualPlayerTarget && !actualPlayerTarget.isDisabled)
			{
				skipTargetTell = true;
				UpdatePlayerTarget(actualPlayerTarget);
				annoyed = false;
				actualPlayerTarget = null;
			}
			else
			{
				List<PlayerAvatar> list = SemiFunc.PlayerGetList();
				int count = list.Count;
				int index = Random.Range(0, count);
				UpdatePlayerTarget(list[index]);
			}
			ResetNavMesh();
		}
		if (!playerTarget || playerTarget.isDisabled)
		{
			stateImpulse = true;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Follow);
		}
	}

	private void StateCoolDown()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			if (isStuckAttacking)
			{
				stateTimer = 0f;
			}
			else
			{
				stateTimer = 2f;
			}
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			if (isStuckAttacking)
			{
				UpdateState(State.Follow);
			}
			else
			{
				UpdateState(State.ChooseTarget);
			}
		}
	}

	private void StateStuckAttack()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			isStuckAttacking = true;
			stateTimer = 1.5f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.Stop(0.2f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Slap);
		}
	}

	private void StateLeave()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 30f;
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
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
			enemy.Rigidbody.notMovingTimer = 0f;
			annoyedTimer = 5f;
			UpdatePlayerTarget(null);
		}
		enemy.NavMeshAgent.OverrideAgent(2.5f, 30f, 0.1f);
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (Vector3.Distance(base.transform.position, agentDestination) <= 1f)
		{
			if (stateTimer > 0f)
			{
				stateTimer -= Time.deltaTime;
				if (enemy.PlayerDistance.PlayerDistanceClosest <= 15f)
				{
					stateTimer = 0f;
				}
			}
			else if ((bool)playerTarget)
			{
				UpdateState(State.Follow);
			}
			else
			{
				UpdateState(State.ChooseTarget);
			}
		}
		else if (enemy.Rigidbody.notMovingTimer >= 3f)
		{
			AttackNearestPhysObjectOrFollow();
			if (enemy.Rigidbody.notMovingTimer >= 10f)
			{
				enemy.EnemyParent.SpawnedTimerSet(0f);
				UpdateState(State.Despawn);
			}
		}
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
		anim.spawnTriggerImpulse = true;
	}

	public void OnTouchPlayer()
	{
		if (!(playerTarget == enemy.Rigidbody.onTouchPlayerAvatar) || currentState == State.Leave)
		{
			GetAnnoyed(enemy.Rigidbody.onTouchPlayerAvatar);
		}
	}

	public void OnTouchPlayerGrabbedObject()
	{
		if (!(playerTarget == enemy.Rigidbody.onTouchPlayerGrabbedObjectAvatar) || currentState == State.Leave)
		{
			GetAnnoyed(enemy.Rigidbody.onTouchPlayerGrabbedObjectAvatar);
		}
	}

	public void OnGrabbed()
	{
		if (!(playerTarget == enemy.Rigidbody.onGrabbedPlayerAvatar) || currentState == State.Leave)
		{
			GetAnnoyed(enemy.Rigidbody.onGrabbedPlayerAvatar);
		}
	}

	public void OnHurt()
	{
		anim.PlayHurtSound();
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.Leave)
		{
			if ((bool)playerTarget)
			{
				UpdateState(State.Follow);
			}
			else
			{
				UpdateState(State.ChooseTarget);
			}
		}
	}

	public void OnDeath()
	{
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		anim.PlayDeathParticles();
		anim.PlayDeathSound();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateState(State.Despawn);
			enemy.EnemyParent.SpawnedTimerSet(0f);
			enemy.EnemyParent.Despawn();
		}
	}

	private void BendLogic()
	{
		if (SemiFunc.FPSImpulse5() && SemiFunc.IsMasterClientOrSingleplayer())
		{
			CheckBendState();
		}
		BendingColliderLogic();
		TorsoRotationLogic();
		NeckRotationLogic();
		ArmRotationLogic();
		HeadRotationLogic();
		EyeRotationLogic();
		FaceHairDown();
	}

	private void CheckBendState()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState != State.CoolDown)
		{
			int num = GetBendState();
			if (num != bendState)
			{
				UpdateBendState(num);
			}
		}
	}

	private int GetBendState()
	{
		if (reachDownToPlayer || currentState == State.Leave)
		{
			return 2;
		}
		for (int num = collisionCheckers.Count - 1; num >= 0; num--)
		{
			if (IsColliding(collisionCheckers[num], collisionCheckers[num].position))
			{
				return num;
			}
		}
		return -1;
	}

	private void BendingColliderLogic()
	{
		if (!bendStateChangedCollider || (!SemiFunc.FPSImpulse1() && !firstTimeColliderCheck))
		{
			return;
		}
		firstTimeColliderCheck = false;
		Collider collider = ((bendState != -1) ? crouchColliders[bendState] : topCollider);
		if (!IsCollidingCapsule(collider.transform.position, collider.GetComponent<CapsuleCollider>()))
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ToggleTopColliderRPC", RpcTarget.All, true, bendState);
			}
			else
			{
				ToggleTopColliderRPC(_active: true, bendState);
			}
			bendStateChangedCollider = false;
		}
	}

	private void TorsoRotationLogic()
	{
		float x = 0f;
		float t = Time.deltaTime * torsoBendingSpeed;
		if (bendState != -1)
		{
			x = torsoBends[bendState];
		}
		torsoBend.localRotation = Quaternion.Slerp(torsoBend.localRotation, Quaternion.Euler(x, 0f, 0f), t);
	}

	private void NeckRotationLogic()
	{
		float x = 0f;
		if (bendState != -1)
		{
			x = neckBends[bendState];
		}
		neckBend.localRotation = Quaternion.Slerp(neckBend.localRotation, Quaternion.Euler(x, 0f, 0f), Time.deltaTime * torsoBendingSpeed);
	}

	private void HeadRotationLogic()
	{
		Quaternion quaternion = ((bendState == -1) ? Quaternion.identity : (Quaternion.identity * Quaternion.Euler(headBends[bendState], 0f, 0f)));
		if (closeEnoughToLook && currentState != State.Leave)
		{
			Vector3 lookTargetWorld = GetLookTargetWorld();
			quaternion = BuildClampedLocalRotation(headBend, torsoBend, lookTargetWorld, 80f, 50f, 50f);
		}
		headBend.localRotation = Quaternion.Slerp(headBend.localRotation, quaternion, Time.deltaTime * headTurnSpeed);
	}

	private void EyeRotationLogic()
	{
		if (!closeEnoughToLook)
		{
			leftEyePivot.localRotation = Quaternion.Slerp(leftEyePivot.localRotation, Quaternion.identity, Time.deltaTime * eyeTurnSpeed);
			rightEyePivot.localRotation = Quaternion.Slerp(rightEyePivot.localRotation, Quaternion.identity, Time.deltaTime * eyeTurnSpeed);
			return;
		}
		Vector3 lookTargetWorld = GetLookTargetWorld();
		Quaternion quaternion = BuildClampedLocalRotation(leftEyePivot, headBend, lookTargetWorld, 25f, 20f, 25f);
		leftEyePivot.localRotation = Quaternion.Slerp(leftEyePivot.localRotation, quaternion, Time.deltaTime * eyeTurnSpeed);
		rightEyePivot.localRotation = Quaternion.Slerp(rightEyePivot.localRotation, quaternion, Time.deltaTime * eyeTurnSpeed);
	}

	private void GeneralRotationLogic()
	{
		if (currentState != State.CoolDown)
		{
			if (currentState == State.StuckAttack)
			{
				if (Vector3.Distance(stuckAttackTarget, enemy.Rigidbody.transform.position) > 0.1f)
				{
					horizontalRotationTarget = Quaternion.LookRotation(stuckAttackTarget - enemy.Rigidbody.transform.position);
					horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
				}
			}
			else if (enemy.NavMeshAgent.AgentVelocity.sqrMagnitude > 0.01f && (!closeEnoughToReachOut || currentState == State.Leave))
			{
				horizontalRotationTarget = Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized);
				horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
			}
			else if (closeEnoughToLook || closeEnoughToReachOut)
			{
				Vector3 vector = GetLookTargetWorld() - base.transform.position;
				vector.y = 0f;
				if (vector.sqrMagnitude > 0.0001f)
				{
					horizontalRotationTarget = Quaternion.LookRotation(vector.normalized);
				}
			}
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(horizontalRotationSpring, horizontalRotationTarget);
	}

	private void ArmRotationLogic()
	{
		rightArmBend.position = rightArmPoint.position;
		leftArmBend.position = leftArmPoint.position;
		if (bendState != -1 && !closeEnoughToReachOut)
		{
			rightWristPivot.localPosition = Vector3.Lerp(rightWristPivot.localPosition, rightBendHandPoses[bendState], Time.deltaTime * 2f);
			leftWristPivot.localPosition = Vector3.Lerp(leftWristPivot.localPosition, leftBendHandPoses[bendState], Time.deltaTime * 2f);
		}
	}

	private void FaceHairDown()
	{
		Vector3 forward = headBend.forward;
		forward.y = 0f;
		forward.Normalize();
		if ((bool)hairTarget1)
		{
			hairTarget1.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}
		if ((bool)hairTarget2)
		{
			hairTarget2.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}
		if ((bool)hairTarget3)
		{
			hairTarget3.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}
		if ((bool)hairTarget4)
		{
			hairTarget4.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}
	}

	private void HandLogic()
	{
		StretchLimbs();
		if (currentState == State.Slap)
		{
			rightWristPivot.localScale = Vector3.Slerp(rightWristPivot.localScale, Vector3.one * maxHandSize, Time.deltaTime * 10f);
			leftWristPivot.localScale = Vector3.Slerp(leftWristPivot.localScale, Vector3.one * maxHandSize, Time.deltaTime * 10f);
		}
		else if (currentState == State.Follow && closeEnoughToReachOut && postStunTimer <= 0f && seesTarget)
		{
			ReachOutHandsToSlap();
		}
		else if (currentState == State.StuckAttack)
		{
			ReachOutHandsToStuckTarget();
		}
		else if (currentState == State.CoolDown)
		{
			ReturnHandsToDefaultPosition(0.1f, _pos: false, _rot: false);
			ShakeHands();
		}
		else
		{
			ReturnHandsToDefaultPosition();
			_rwVel = Vector3.zero;
			_lwVel = Vector3.zero;
		}
	}

	private void StretchLimbs()
	{
		springPoseAnimator.StretchLimbToPoint("Left Arm", leftWristPivot.position);
		springPoseAnimator.StretchLimbToPoint("Right Arm", rightWristPivot.position);
	}

	private void ReachOutHandsToSlap()
	{
		Vector3 handTarget = GetHandTarget(_rightHand: true);
		Vector3 handTarget2 = GetHandTarget(_rightHand: false);
		UpdateHandPositionTo(rightWristPivot, ref _rwVel, _rightHand: true, handTarget);
		UpdateHandPositionTo(leftWristPivot, ref _lwVel, _rightHand: false, handTarget2);
		float num = ((distanceFromPlayer - slapDistance <= 0f) ? 1f : (1f - Mathf.Clamp01((distanceFromPlayer - slapDistance) / (handMoveDistance - slapDistance))));
		float num2 = 1f + (maxHandSize - 1f) * num;
		leftHandAnimator.Play("SlapHand", 0, num);
		rightHandAnimator.Play("SlapHand", 0, num);
		rightWristPivot.localScale = Vector3.Slerp(rightWristPivot.localScale, Vector3.one * num2, Time.deltaTime * 10f);
		leftWristPivot.localScale = Vector3.Slerp(leftWristPivot.localScale, Vector3.one * num2, Time.deltaTime * 10f);
	}

	private void ReachOutHandsToStuckTarget()
	{
		if (!(stuckAttackTarget == Vector3.zero))
		{
			float num = Vector3.Distance(base.transform.position, stuckAttackTarget);
			float num2 = 0.3f;
			Vector3 handTarget = stuckAttackTarget + base.transform.right * num2 - base.transform.forward * 0.6f;
			Vector3 handTarget2 = stuckAttackTarget - base.transform.right * num2 - base.transform.forward * 0.6f;
			UpdateHandPositionTo(rightWristPivot, ref _rwVel, _rightHand: true, handTarget);
			UpdateHandPositionTo(leftWristPivot, ref _lwVel, _rightHand: false, handTarget2);
			float num3 = ((num - slapDistance <= 0f) ? 1f : (1f - Mathf.Clamp01((num - slapDistance) / (handMoveDistance - slapDistance))));
			float num4 = 1f + (maxHandSize - 1f) * num3;
			leftHandAnimator.Play("SlapHand", 0, num3);
			rightHandAnimator.Play("SlapHand", 0, num3);
			rightWristPivot.localScale = Vector3.Slerp(rightWristPivot.localScale, Vector3.one * num4, Time.deltaTime * 10f);
			leftWristPivot.localScale = Vector3.Slerp(leftWristPivot.localScale, Vector3.one * num4, Time.deltaTime * 10f);
		}
	}

	private void UpdateHandPositionTo(Transform _wrist, ref Vector3 _vel, bool _rightHand, Vector3 _handTarget)
	{
		Quaternion quaternion = (_rightHand ? Quaternion.Euler(rightWristSlapRotation) : Quaternion.Euler(leftWristSlapRotation));
		float num = (_rightHand ? Mathf.Abs(_wrist.localPosition.x - rightHandDownPos.localPosition.x) : Mathf.Abs(_wrist.localPosition.x - leftHandDownPos.localPosition.x));
		if (playerTarget.isCrouching && _wrist.position.y - (playerTarget.transform.position.y + 0.2f) > 0.2f && num > 0.25f)
		{
			_handTarget = (_rightHand ? new Vector3(rightHandDownPos.position.x, _handTarget.y, rightHandDownPos.position.z) : new Vector3(leftHandDownPos.position.x, _handTarget.y, leftHandDownPos.position.z));
		}
		Vector3 vector = Vector3.SmoothDamp(_wrist.position, _handTarget, ref _vel, 0.12f, 4f);
		if ((_handTarget - vector).sqrMagnitude <= 0.0049f)
		{
			vector = _handTarget;
		}
		Vector3 vector2 = handArea.position - handArea.localScale / 2f;
		Vector3 vector3 = handArea.position + handArea.localScale / 2f;
		if (playerTarget.isCrouching)
		{
			vector2 = handAreaUnder.position - handAreaUnder.localScale / 2f;
			vector3 = handAreaUnder.position + handAreaUnder.localScale / 2f;
		}
		if (vector.x < vector2.x)
		{
			vector.x = vector2.x;
			_vel.x = 0f;
		}
		else if (vector.x > vector3.x)
		{
			vector.x = vector3.x;
			_vel.x = 0f;
		}
		if (vector.y < vector2.y)
		{
			vector.y = vector2.y;
			_vel.y = 0f;
		}
		else if (vector.y > vector3.y)
		{
			vector.y = vector3.y;
			_vel.y = 0f;
		}
		if (vector.z < vector2.z)
		{
			vector.z = vector2.z;
			_vel.z = 0f;
		}
		else if (vector.z > vector3.z)
		{
			vector.z = vector3.z;
			_vel.z = 0f;
		}
		Vector3 position = base.transform.InverseTransformPoint(vector);
		if (position.z < 0f)
		{
			position.z = 0f;
			vector = base.transform.TransformPoint(position);
			_vel.z = 0f;
		}
		_wrist.position = vector;
		_wrist.localRotation = Quaternion.Slerp(_wrist.localRotation, quaternion, Time.deltaTime * 10f);
	}

	private void ReturnHandsToDefaultPosition(float _speed = 2f, bool _pos = true, bool _rot = true, bool _scale = true, bool _anim = true)
	{
		if (_anim)
		{
			AnimatorStateInfo currentAnimatorStateInfo = leftHandAnimator.GetCurrentAnimatorStateInfo(0);
			AnimatorStateInfo currentAnimatorStateInfo2 = rightHandAnimator.GetCurrentAnimatorStateInfo(0);
			float normalizedTime = Mathf.Lerp(currentAnimatorStateInfo.normalizedTime % 1f, 0f, Time.deltaTime * 6f);
			float normalizedTime2 = Mathf.Lerp(currentAnimatorStateInfo2.normalizedTime % 1f, 0f, Time.deltaTime * 6f);
			leftHandAnimator.Play("SlapHand", 0, normalizedTime);
			leftHandAnimator.speed = 0f;
			rightHandAnimator.Play("SlapHand", 0, normalizedTime2);
			rightHandAnimator.speed = 0f;
		}
		if (_pos)
		{
			if (bendState == -1)
			{
				rightWristPivot.localPosition = Vector3.Lerp(rightWristPivot.localPosition, originalRightWristPosition, Time.deltaTime * _speed);
				leftWristPivot.localPosition = Vector3.Lerp(leftWristPivot.localPosition, originalLeftWristPosition, Time.deltaTime * _speed);
			}
			else
			{
				rightWristPivot.localPosition = Vector3.Lerp(rightWristPivot.localPosition, rightBendHandPoses[bendState], Time.deltaTime * _speed);
				leftWristPivot.localPosition = Vector3.Lerp(leftWristPivot.localPosition, leftBendHandPoses[bendState], Time.deltaTime * _speed);
			}
		}
		if (_rot)
		{
			rightWristPivot.localRotation = Quaternion.Slerp(rightWristPivot.localRotation, Quaternion.Euler(originalRightWristRotation), Time.deltaTime * _speed);
			leftWristPivot.localRotation = Quaternion.Slerp(leftWristPivot.localRotation, Quaternion.Euler(originalLeftWristRotation), Time.deltaTime * _speed);
		}
		if (_scale)
		{
			rightWristPivot.localScale = Vector3.Lerp(rightWristPivot.localScale, Vector3.one, Time.deltaTime * _speed);
			leftWristPivot.localScale = Vector3.Lerp(leftWristPivot.localScale, Vector3.one, Time.deltaTime * _speed);
		}
	}

	private Vector3 GetHandTarget(bool _rightHand)
	{
		float num = ((playerTarget.isCrouching || playerTarget.isCrawling || playerTarget.isTumbling) ? 0.2f : 1.2f);
		if (_rightHand)
		{
			return playerTarget.transform.position + Vector3.up * num - base.transform.forward * 0.6f + base.transform.right * 0.3f;
		}
		return playerTarget.transform.position + Vector3.up * num - base.transform.forward * 0.6f - base.transform.right * 0.3f;
	}

	public void GetAnnoyed(PlayerAvatar _nearbyPlayer)
	{
		if (SemiFunc.EnemySpawnIdlePause() || !SemiFunc.IsMasterClientOrSingleplayer() || annoyedTimer > 0f)
		{
			return;
		}
		if (!SemiFunc.IsMultiplayer())
		{
			if (currentState == State.Leave)
			{
				UpdateState(State.Follow);
			}
		}
		else
		{
			if (!_nearbyPlayer)
			{
				_nearbyPlayer = GetNearestPlayer();
			}
			if (playerTarget != _nearbyPlayer)
			{
				skipTargetTell = true;
				actualPlayerTarget = playerTarget;
				annoyedFollowTimer = 5f;
				annoyed = true;
				UpdatePlayerTarget(_nearbyPlayer);
				UpdateState(State.Follow);
			}
			else if (currentState == State.Leave)
			{
				UpdateState(State.Follow);
			}
		}
		annoyedTimer = 10f;
	}

	public void SkipShockwave()
	{
		if (hurtColliderScript.onImpactPlayerAvatar.isLocal)
		{
			hurtColliderWaveScript.playerLogic = false;
		}
	}

	private void Slap()
	{
		float num = (slapNetValid ? Mathf.Clamp01((float)((PhotonNetwork.Time - slapStartTimeNetwork) / (double)slapDuration)) : Mathf.Clamp01(stateTimerClient / slapDuration));
		if (num >= 0.92f && SemiFunc.IsMasterClientOrSingleplayer())
		{
			Vector3 pos = (rightWristPivot.position + leftWristPivot.position) * 0.5f + base.transform.forward * 0.2f;
			EnableHurtCollider(pos);
		}
		float num2 = slapAnimationCurve.Evaluate(num);
		rightWristPivot.localPosition = rightHandSlapLocation + Vector3.right * num2;
		leftWristPivot.localPosition = leftHandSlapLocation - Vector3.right * num2;
		rightWristPivot.localRotation = Quaternion.Slerp(rightWristPivot.localRotation, Quaternion.Euler(rightWristSlapRotation), Time.deltaTime * 10f);
		leftWristPivot.localRotation = Quaternion.Slerp(leftWristPivot.localRotation, Quaternion.Euler(leftWristSlapRotation), Time.deltaTime * 10f);
	}

	private void SpringLogic()
	{
		hairPivot1.localRotation = SemiFunc.SpringQuaternionGet(hairSpring1, hairTarget1.localRotation);
		hairPivot2.localRotation = SemiFunc.SpringQuaternionGet(hairSpring2, hairTarget2.localRotation);
		hairPivot3.localRotation = SemiFunc.SpringQuaternionGet(hairSpring3, hairTarget3.localRotation);
		hairPivot3.localRotation = Quaternion.Euler(Mathf.Min(hairPivot3.localRotation.eulerAngles.x, 0f), hairPivot3.localRotation.eulerAngles.y, hairPivot3.localRotation.eulerAngles.z);
		hairPivot4.localRotation = SemiFunc.SpringQuaternionGet(hairSpring4, hairTarget4.localRotation);
	}

	private void ResetNavMesh()
	{
		enemy.NavMeshAgent.ResetPath();
		enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
	}

	private void ShakeHands()
	{
		Vector3 localPosition = rightWristPivot.localPosition + Random.insideUnitSphere * 0.01f;
		if (localPosition.x < rightWristPivot.localPosition.x && localPosition.x < rightHandSlapLocation.x)
		{
			localPosition.x = rightWristPivot.localPosition.x + Random.Range(0.005f, 0.01f);
		}
		Vector3 localPosition2 = leftWristPivot.localPosition + Random.insideUnitSphere * 0.01f;
		if (localPosition2.x > leftWristPivot.localPosition.x && localPosition2.x > leftHandSlapLocation.x)
		{
			localPosition2.x = leftWristPivot.localPosition.x - Random.Range(0.005f, 0.01f);
		}
		rightWristPivot.localPosition = localPosition;
		leftWristPivot.localPosition = localPosition2;
	}

	private void AttackNearestPhysObjectOrFollow()
	{
		stuckAttackTarget = Vector3.zero;
		if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			stuckAttackTarget = SemiFunc.EnemyGetNearestPhysObject(enemy);
		}
		if (stuckAttackTarget == Vector3.zero || Vector3.Distance(base.transform.position, stuckAttackTarget) > 5f)
		{
			UpdateState(State.Leave);
			return;
		}
		stuckAttackAttempts = 0;
		UpdateState(State.StuckAttack);
	}

	private void PlayerTargetTell()
	{
		if ((bool)playerTarget && playerTarget.isLocal && !playerTarget.isDisabled)
		{
			anim.playTargetedSound(playerTarget.transform.position);
			PostProcessing.Instance.VignetteOverride(new Color(0.5f, 0f, 0f), 0.3f, 1f, 3f, 1f, 0.5f, base.gameObject);
			PostProcessing.Instance.SaturationOverride(-20f, 3f, 1f, 0.5f, base.gameObject);
			PostProcessing.Instance.ContrastOverride(5f, 3f, 1f, 0.5f, base.gameObject);
			GameDirector.instance.CameraImpact.Shake(1f, 0.1f);
			GameDirector.instance.CameraShake.Shake(1f, 0.25f);
			if (!activeVeinEffect)
			{
				Transform parent = playerTarget.localCamera.transform;
				GameObject gameObject = Object.Instantiate(screenVeinEffect, parent);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				activeVeinEffect = gameObject.GetComponent<EnemyShadowScreenVeinEffect>();
			}
			if ((bool)activeVeinEffect)
			{
				activeVeinEffect.Active();
			}
		}
	}

	private void PlayerTargetStopTell()
	{
		if ((bool)playerTargetPrevious && playerTargetPrevious.isLocal && !playerTargetPrevious.isDisabled)
		{
			anim.PlayUntargetedSound(playerTargetPrevious.transform.position);
			PostProcessing.Instance.VignetteOverride(new Color(0.3f, 0.6f, 0.5f), 0.15f, 1f, 2f, 1f, 0.3f, base.gameObject);
			PostProcessing.Instance.SaturationOverride(10f, 2f, 1f, 0.3f, base.gameObject);
			PostProcessing.Instance.ContrastOverride(-3f, 2f, 1f, 0.3f, base.gameObject);
			GameDirector.instance.CameraImpact.Shake(0.5f, 0.05f);
			GameDirector.instance.CameraShake.Shake(1.5f, 0.1f);
		}
	}

	private Vector3 GetLookTargetWorld()
	{
		if (!playerTarget)
		{
			return base.transform.position + base.transform.forward;
		}
		return PlayerTargetEyePos();
	}

	private Vector3 PlayerTargetEyePos()
	{
		Vector3 position;
		if (SemiFunc.IsMultiplayer() && !playerTarget.isLocal)
		{
			position = playerTarget.playerAvatarVisuals.headLookAtTransform.position;
		}
		else
		{
			position = playerTarget.localCamera.transform.position;
			position.y -= 0.3f;
		}
		return position;
	}

	private Quaternion BuildClampedLocalRotation(Transform _pivot, Transform _parent, Vector3 _targetWorld, float _yawLimit, float _pitchUpLimit, float _pitchDownLimit)
	{
		Vector3 vector = _targetWorld - _pivot.position;
		if (vector.sqrMagnitude <= 0.0001f)
		{
			return Quaternion.identity;
		}
		Quaternion quaternion = Quaternion.LookRotation(vector.normalized, _parent ? _parent.up : Vector3.up);
		Quaternion quaternion2 = Quaternion.Inverse(_parent.rotation) * quaternion;
		Vector3 eulerAngles = (Quaternion.Inverse(Quaternion.identity) * quaternion2).eulerAngles;
		float value = Normalize180(eulerAngles.x);
		float value2 = Normalize180(eulerAngles.y);
		Quaternion quaternion3 = Quaternion.Euler(y: Mathf.Clamp(value2, 0f - _yawLimit, _yawLimit), x: Mathf.Clamp(value, 0f - _pitchUpLimit, _pitchDownLimit), z: 0f);
		return Quaternion.identity * quaternion3;
	}

	private float Normalize180(float angle)
	{
		angle %= 360f;
		if (angle > 180f)
		{
			angle -= 360f;
		}
		if (angle < -180f)
		{
			angle += 360f;
		}
		return angle;
	}

	private float DistanceFromPlayer()
	{
		if (!playerTarget)
		{
			return float.PositiveInfinity;
		}
		Vector3 position = playerTarget.transform.position;
		Vector3 position2 = base.transform.position;
		position.y = 0f;
		position2.y = 0f;
		return Vector3.Distance(position, position2);
	}

	private Collider[] GetCollidingColliders(Transform _collisionChecker, Vector3 _pos)
	{
		return Physics.OverlapBox(_pos, _collisionChecker.lossyScale / 2f, _collisionChecker.rotation, SemiFunc.LayerMaskGetVisionObstruct());
	}

	private Collider[] GetCollidingCollidersCapsule(Vector3 _pos, CapsuleCollider _capsuleReference)
	{
		Transform transform = _capsuleReference.transform;
		Vector3 lossyScale = transform.lossyScale;
		float num;
		float num2;
		Vector3 vector;
		switch (_capsuleReference.direction)
		{
		case 0:
			num = _capsuleReference.radius * Mathf.Max(lossyScale.y, lossyScale.z);
			num2 = _capsuleReference.height * lossyScale.x;
			vector = transform.right;
			break;
		case 1:
			num = _capsuleReference.radius * Mathf.Max(lossyScale.x, lossyScale.z);
			num2 = _capsuleReference.height * lossyScale.y;
			vector = transform.up;
			break;
		case 2:
			num = _capsuleReference.radius * Mathf.Max(lossyScale.x, lossyScale.y);
			num2 = _capsuleReference.height * lossyScale.z;
			vector = transform.forward;
			break;
		default:
			num = _capsuleReference.radius * Mathf.Max(lossyScale.x, lossyScale.z);
			num2 = _capsuleReference.height * lossyScale.y;
			vector = transform.up;
			break;
		}
		float num3 = Mathf.Max(0f, num2 / 2f - num);
		Vector3 point = _pos + vector * num3;
		Vector3 point2 = _pos - vector * num3;
		return Physics.OverlapCapsule(point, point2, num, SemiFunc.LayerMaskGetVisionObstruct());
	}

	private PlayerAvatar GetNearestPlayer()
	{
		PlayerAvatar result = null;
		List<PlayerAvatar> list = SemiFunc.PlayerGetList();
		float num = float.PositiveInfinity;
		foreach (PlayerAvatar item in list)
		{
			float num2 = Vector3.Distance(item.transform.position, base.transform.position);
			if (num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	private bool IsColliding(Transform _collisionChecker, Vector3 _pos)
	{
		Collider[] collidingColliders = GetCollidingColliders(_collisionChecker, _pos);
		for (int i = 0; i < collidingColliders.Length; i++)
		{
			if (!collidingColliders[i].transform.IsChildOf(enemy.EnemyParent.transform))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsCollidingCapsule(Vector3 _pos, CapsuleCollider _capsuleReference)
	{
		Collider[] collidingCollidersCapsule = GetCollidingCollidersCapsule(_pos, _capsuleReference);
		for (int i = 0; i < collidingCollidersCapsule.Length; i++)
		{
			if (!collidingCollidersCapsule[i].transform.IsChildOf(enemy.EnemyParent.transform))
			{
				return true;
			}
		}
		return false;
	}

	private bool HandsInPositionToSlap()
	{
		float num = Vector3.Distance(rightWristPivot.position, GetHandTarget(_rightHand: true));
		float num2 = Vector3.Distance(leftWristPivot.position, GetHandTarget(_rightHand: false));
		if (num < 0.15f)
		{
			return num2 < 0.15f;
		}
		return false;
	}

	private bool HandsInDownPositionToSlap()
	{
		Vector3 vector = handAreaUnderSlapReady.position - handAreaUnderSlapReady.localScale / 2f;
		Vector3 vector2 = handAreaUnderSlapReady.position + handAreaUnderSlapReady.localScale / 2f;
		bool num = rightWristPivot.position.x >= vector.x && rightWristPivot.position.x <= vector2.x && rightWristPivot.position.y >= vector.y && rightWristPivot.position.y <= vector2.y && rightWristPivot.position.z >= vector.z && rightWristPivot.position.z <= vector2.z;
		bool flag = leftWristPivot.position.x >= vector.x && leftWristPivot.position.x <= vector2.x && leftWristPivot.position.y >= vector.y && leftWristPivot.position.y <= vector2.y && leftWristPivot.position.z >= vector.z && leftWristPivot.position.z <= vector2.z;
		return num && flag;
	}

	private bool VisionBlocked()
	{
		if (Time.time - visionBlockedCheckTime > 0.2f && (bool)playerTarget)
		{
			Vector3 position = playerTarget.PlayerVisionTarget.VisionTransform.position;
			visionBlockedCheckTime = Time.time;
			Vector3 direction = position - visionTransform.position;
			visionBlocked = Physics.Raycast(visionTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default") + LayerMask.GetMask("PhysGrabObjectHinge"), QueryTriggerInteraction.Ignore);
		}
		return visionBlocked;
	}

	private static double NetNow()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			return Time.time;
		}
		return PhotonNetwork.Time;
	}
}
