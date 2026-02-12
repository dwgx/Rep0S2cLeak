using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EnemyHeartHugger : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Grow,
		AggroStart,
		Aggro,
		LureStart,
		Lure,
		LureStop,
		ChompGasp,
		Chomp,
		Degrow,
		Stunned,
		AggroGrow,
		AggroDegrow,
		IdleBreaker,
		AggroBreaker
	}

	[Serializable]
	public class SpringLimb
	{
		public SpringQuaternion spring;

		public string name;

		public Transform transform;

		public Transform target;

		[HideInInspector]
		public Quaternion originalQuaternion;

		[HideInInspector]
		public float growAmount;
	}

	[Serializable]
	public class MeshTransform
	{
		public Transform transform;

		[HideInInspector]
		public float eval;

		[HideInInspector]
		public bool soundPlayed;
	}

	[Serializable]
	public class CuteMeshTransform
	{
		public Transform transform;

		[HideInInspector]
		public float eval;

		[HideInInspector]
		public bool soundPlayed;
	}

	public class PlayersInGas
	{
		public int photonViewId;

		public PlayerAvatar playerAvatar;

		public float inGasTime;

		public bool isCaught;

		public float outsideGasTime;

		public Vector3 lastPositionInsideGas;
	}

	private PhotonView photonView;

	public Transform followTarget;

	public Transform headLook;

	private List<PlayersInGas> playersInGas = new List<PlayersInGas>();

	private List<PlayersInGas> playersInGasPrevious = new List<PlayersInGas>();

	private List<PlayerAvatar> playersInGasClient = new List<PlayerAvatar>();

	private Dictionary<PlayerAvatar, float> playersOnCooldown = new Dictionary<PlayerAvatar, float>();

	[Space]
	public List<SpringLimb> springLimbs = new List<SpringLimb>();

	[Space]
	public List<Transform> vineTransforms = new List<Transform>();

	[Space]
	public List<MeshTransform> meshTransforms = new List<MeshTransform>();

	[Space]
	public List<CuteMeshTransform> cuteMeshTransforms = new List<CuteMeshTransform>();

	public State currentState = State.Idle;

	private bool stateStart;

	private float stateTimer;

	private float stateTimerMax;

	private float lureTimerMax;

	private bool isFixedUpdate;

	private PlayerAvatar currentTarget;

	private bool hasTarget;

	private float hugVOTimer;

	public AnimationCurve growAnimationCurve;

	public AnimationCurve popAwayMeshesCurve;

	public AnimationCurve popBackMeshesCurve;

	public AnimationCurve biteCurve;

	public AnimationCurve biteScaleBounceCurve;

	public AnimationCurve idleBreakerCurve;

	[Space(30f)]
	public Sound popAwaySound;

	[Space(5f)]
	public Sound soundIdleLoop;

	public Sound soundStunLoop;

	public Sound soundGrow;

	public Sound soundDegrow;

	public Sound soundChompThrust;

	public Sound soundChompBite;

	public Sound soundGasLoop;

	public Sound soundAggroLoop;

	public Sound soundGasStart;

	public Sound soundGasTell;

	public Sound soundScare;

	public Sound soundScareGlobal;

	public Sound soundMove;

	public Sound soundMoveHead;

	public Sound soundGasStop;

	[Space(5f)]
	public Sound soundIdleBreaker01;

	public Sound soundIdleBreaker02;

	public Sound soundIdleBreaker03;

	[Space(5f)]
	public Sound soundAggroBreaker01;

	public Sound soundAggroBreaker02;

	public Sound soundAggroBreaker03;

	[Space(5f)]
	public Sound soundAggroStart;

	[Space(5f)]
	public Sound soundEnchantedVO;

	public Sound soundEnchantedLoop;

	[Space(5f)]
	public Sound soundHurt;

	public Sound soundDeath;

	private bool growDone = true;

	private float vineRotation;

	private float vineRotationSpeed = 50f;

	private float baseRotation;

	private float overrideSpringValuesTimer;

	private float overrideSpringValuesSpeed = 15f;

	private float overrideSpringValuesDamping = 0.5f;

	private float localGameTime;

	private float canAggroTimer;

	private bool canAggro;

	private Vector3 investigatePosition;

	[Space(30f)]
	public GameObject horrorMesh1;

	public GameObject horrorMesh2;

	public GameObject normalMesh1;

	public GameObject normalMesh2;

	[Space(10f)]
	public GameObject biteMesh;

	public Transform headSegment1TargetTransform;

	public Transform headSegment2TargetTransform;

	public Transform headCenterTransform;

	public GameObject biteHurtCollider;

	[Space(10f)]
	public Transform breatheScaleParent1;

	public Transform breatheScaleParent2;

	public Transform holeScaleParent1;

	public Transform holeScaleParent2;

	[Space(10f)]
	public GameObject gasChecker;

	public Transform mouthTransform;

	[Space(10f)]
	public Rigidbody rb;

	[Space(10f)]
	public Transform baseMeshScale;

	public Transform baseMeshScaleTarget;

	[Space(10f)]
	public Transform cuteMeshRightClosedMouth;

	public Transform cuteMeshLeftClosedMouth;

	public Transform cuteMeshRightOpenMouth;

	public Transform cuteMeshLeftOpenMouth;

	[Space(30f)]
	public Light headLight;

	public ParticleSystem headParticles;

	public ParticleSystem gasParticles;

	public ParticleSystem particlesShatteredDream;

	public ParticleSystem particlesHearts;

	public List<ParticleSystem> particlesDeath;

	private SpringVector3 scaleBreathing;

	private Vector3 scaleBreathingTarget;

	private Vector3 scaleBreathingCurrent;

	private SpringVector3 holeScaleBreathing;

	private Vector3 holeScaleBreathingTarget;

	private Vector3 holeScaleBreathingCurrent;

	private float lookAtActiveTimer;

	[Space(30f)]
	public List<AudioSource> audioSources = new List<AudioSource>();

	private float shootGasTimer;

	internal bool isShootingGas;

	private float gasCheckerEmissionTimer;

	private bool playingLoopSound;

	private float playLoopSoundTimer;

	private Sound currentLoopSound;

	internal bool disabled;

	private int bites;

	private Quaternion prevHeadLookRotation;

	private Quaternion prevRigidBodyRotation;

	private float turnRandomTimer;

	private float turnRandomTimerMax;

	private float turnRandomTimerMin;

	private float moveSoundHeadTimer;

	private float moveSoundRigidBodyTimer;

	private List<LevelPoint> levelPoints = new List<LevelPoint>();

	private bool doAttack;

	private bool doLeave;

	private bool doDespawn;

	private int attacksInARow;

	private int aggroBreakersInARow;

	internal Enemy enemy;

	private int idleBreakerIndex;

	private float overrideActivateHorrorMeshesTimer;

	private bool horrorMeshesActive;

	private SpringVector3 baseScaleSpring = new SpringVector3();

	private float overrideVisualsToEnchantedTimer;

	private bool overrideVisualsToEnchantedTransitioning;

	private bool overrideVisualsToEnchanted;

	private bool overrideVisualsToEnchantedInstantTransition;

	private float particlePlayHeartsTimer;

	private float particlePlayGasTimer;

	private int instanceID;

	private float releaseGrabTimer;

	private void Awake()
	{
		baseScaleSpring = new SpringVector3();
		scaleBreathing = new SpringVector3();
		holeScaleBreathing = new SpringVector3();
		photonView = GetComponent<PhotonView>();
		foreach (SpringLimb springLimb in springLimbs)
		{
			springLimb.originalQuaternion = springLimb.transform.localRotation;
			if (springLimb.name == "head look")
			{
				springLimb.originalQuaternion = Quaternion.identity;
			}
		}
		enemy = GetComponent<Enemy>();
		hasTarget = true;
		if (SemiFunc.IsMultiplayer())
		{
			instanceID = photonView.ViewID;
		}
		else
		{
			instanceID = base.gameObject.GetInstanceID();
		}
		ResetLocalGameTime();
	}

	private void Update()
	{
		localGameTime += Time.deltaTime;
		EnemySystemStateTriggers();
		StateMachine();
		if (stateTimer <= stateTimerMax)
		{
			stateTimer += Time.deltaTime;
		}
		VisualUpdateSprings();
		VisualRotateVines();
		OverrideSpringValuesTick();
		PlayLoopSoundLogic();
		VisualScaleBreathing();
		ShootGasLogic();
		TurnSoundLogic();
		PlayersInGasLogic();
		OverrideVisualsToEnchantedLogic();
		OverrideActivateHorrorMeshesTick();
		OverrideVisualsToEnchantedTick();
		PlayersOnCooldownTick();
		ParticlePlayGasTick();
		ParticlePlayHeartsTick();
		OverrideCanAggroTick();
		InvestigatePositionReset();
		AttacksInARowReset();
		AggroBreakersInARowReset();
		LeaveReset();
		DespawnReset();
		GrowDoneReset();
		LookAtTargetReset();
		ReleaseGrabLogic();
	}

	private void FixedUpdate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			isFixedUpdate = true;
			StateMachine();
			isFixedUpdate = false;
		}
	}

	private void OnEnable()
	{
		headParticles?.Play(withChildren: true);
	}

	private void OnDisable()
	{
		headParticles?.Stop(withChildren: true);
		gasParticles.Stop(withChildren: true);
		particlesShatteredDream.Stop(withChildren: true);
		particlesHearts.Stop(withChildren: true);
	}

	private void StateSpawn()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 3f;
			stateTimer = 0f;
			GrowStart();
			soundGrow.Play(base.transform.position);
		}
		if (!isFixedUpdate)
		{
			enemy.StateStunned.OverrideDisable(0.5f);
			VisualGrowAnimation();
			VisualGrowDance();
			if (growDone)
			{
				StateSet(State.Idle);
			}
		}
		_ = isFixedUpdate;
	}

	private void StateIdle()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = UnityEngine.Random.Range(5f, 15f);
			stateTimer = 0f;
			doAttack = true;
			TurnRandomTimer(5f, 10f);
		}
		if (!isFixedUpdate)
		{
			OverrideCanAggro();
			VisualStateNormal();
			TurnRandomFull();
			if (SemiFunc.EnemySpawnIdlePause())
			{
				return;
			}
			if (SemiFunc.IsMasterClientOrSingleplayer() && stateTimer >= stateTimerMax)
			{
				if (UnityEngine.Random.Range(0, 2) == 0 && SemiFunc.PlayerNearestDistance(base.transform.position) <= 20f)
				{
					int idleBreaker = UnityEngine.Random.Range(0, 2);
					StateSetIdleBreaker(idleBreaker);
				}
				else
				{
					StateSet(State.Roam);
				}
			}
		}
		_ = isFixedUpdate;
	}

	private void StateRoam()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = UnityEngine.Random.Range(5f, 10f);
			stateTimer = 0f;
			doAttack = true;
			TurnRandomTimer(3f, 6f);
		}
		if (!isFixedUpdate)
		{
			VisualStateNormal();
			TurnRandomFull();
			OverrideCanAggro();
			if (stateTimer >= stateTimerMax)
			{
				StateSet(State.Degrow);
			}
		}
		_ = isFixedUpdate;
	}

	private void StateGrow()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 3f;
			stateTimer = 0f;
			GrowStart();
			if (doLeave)
			{
				LevelPointsLeave();
			}
			else if (investigatePosition != Vector3.zero)
			{
				LevelPointsInvestigate();
			}
			else
			{
				LevelPointsRoam();
			}
			soundGrow.Play(base.transform.position);
		}
		if (!isFixedUpdate)
		{
			VisualGrowAnimation();
			VisualGrowDance();
			enemy.StateStunned.OverrideDisable(0.5f);
			if (growDone)
			{
				StateSet(State.Idle);
			}
		}
		_ = isFixedUpdate;
	}

	private void StateAggro()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimer = 0f;
			stateTimerMax = UnityEngine.Random.Range(0.2f, 1f);
		}
		if (!isFixedUpdate)
		{
			OverrideCanAggro();
			VisualStateAggro();
			LookAtTarget();
			if (stateTimer >= stateTimerMax)
			{
				if (aggroBreakersInARow < 1 && UnityEngine.Random.Range(0, 2) == 0)
				{
					aggroBreakersInARow++;
					int idleBreaker = UnityEngine.Random.Range(0, 2);
					StateSetIdleBreaker(idleBreaker, _aggro: true);
					return;
				}
				aggroBreakersInARow = 0;
				if (doAttack && (bool)currentTarget)
				{
					StateSet(State.LureStart);
				}
				if (!doAttack)
				{
					StateSet(State.AggroDegrow);
				}
			}
		}
		_ = isFixedUpdate;
	}

	private void StateLureStart()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 0.8f;
			stateTimer = 0f;
			GameDirector.instance.CameraShake.ShakeDistance(1f, 3f, 8f, base.transform.position, 0.5f);
			GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.5f);
			soundGasTell.Play(headCenterTransform.position);
			doAttack = false;
		}
		if (!isFixedUpdate)
		{
			VisualStateLureStart();
			hasTarget = true;
			LookAtTarget();
			if (stateTimer >= stateTimerMax)
			{
				StateSet(State.Lure);
			}
		}
		_ = isFixedUpdate;
	}

	private void StateLure()
	{
		if (stateStart && !isFixedUpdate)
		{
			attacksInARow++;
			stateStart = false;
			stateTimerMax = 5f;
			stateTimer = 0f;
			lureTimerMax = 0f;
			GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.5f);
			GameDirector.instance.CameraImpact.ShakeDistance(4f, 3f, 8f, base.transform.position, 0.5f);
			soundGasStart.Play(headCenterTransform.position);
		}
		if (!isFixedUpdate)
		{
			lureTimerMax += Time.deltaTime;
			VisualStateLure();
			hasTarget = true;
			LookAtTarget();
			ShootGas();
			if (stateTimer >= stateTimerMax)
			{
				if (playersInGas.Count <= 0)
				{
					StateSet(State.LureStop);
					return;
				}
				stateTimer = 0f;
			}
			if (lureTimerMax > 25f)
			{
				StateSet(State.LureStop);
				return;
			}
		}
		_ = isFixedUpdate;
	}

	private void StateLureStop()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 0.5f;
			stateTimer = 0f;
			GameDirector.instance.CameraShake.ShakeDistance(1f, 3f, 8f, base.transform.position, 0.5f);
			GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.5f);
			soundGasStop.Play(headCenterTransform.position);
		}
		if (!isFixedUpdate)
		{
			VisualStateLureStop();
			LookAtTarget();
			if (stateTimer >= stateTimerMax)
			{
				if (attacksInARow > 3 || UnityEngine.Random.Range(0, 2) == 0)
				{
					LeaveStart();
				}
				else
				{
					StateSet(State.AggroDegrow);
				}
			}
		}
		_ = isFixedUpdate;
	}

	private void StateChompGasp()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 1f;
			stateTimer = 0f;
			GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, base.transform.position, 3f);
			GameDirector.instance.CameraImpact.ShakeDistance(4f, 3f, 8f, base.transform.position, 0.5f);
			soundScare.Play(headCenterTransform.position);
			soundScareGlobal.Play(headCenterTransform.position);
			bites = UnityEngine.Random.Range(3, 6);
			JumpScareAtChompStart();
		}
		if (!isFixedUpdate)
		{
			JumpScareAtChompStartForceLookAtHead();
			VisualStateChompGasp();
			LookAtTarget();
			ShootGas();
			OverrideActivateHorrorMeshes();
			if (stateTimer >= stateTimerMax)
			{
				StateSet(State.Chomp);
			}
		}
		_ = isFixedUpdate;
	}

	private void StateChomp()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 0.5f;
			stateTimer = 0f;
			soundChompThrust.Play(headCenterTransform.position);
		}
		if (!isFixedUpdate)
		{
			VisualStateChomp();
			ShootGas();
			LookAtTarget();
			float t = biteCurve.Evaluate(stateTimer / stateTimerMax);
			headSegment1TargetTransform.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 10f, t), Mathf.Lerp(0f, -60f, t), Mathf.Lerp(0f, 10f, t));
			headSegment2TargetTransform.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 10f, t), Mathf.Lerp(0f, 60f, t), Mathf.Lerp(0f, -10f, t));
			if (stateTimer > stateTimerMax * 0.3f && stateTimer < stateTimerMax * 0.7f)
			{
				if (!biteMesh.activeSelf)
				{
					ActivateBiteMesh(_active: true);
					soundChompBite.Play(headCenterTransform.position);
				}
				float num = biteScaleBounceCurve.Evaluate((stateTimer - stateTimerMax * 0.3f) / (stateTimerMax * 0.7f));
				biteMesh.transform.localScale = new Vector3(num, 1f, num);
			}
			else if (biteMesh.activeSelf)
			{
				ActivateBiteMesh(_active: false);
			}
			VisualBiteAnimation();
			if (stateTimer >= stateTimerMax)
			{
				if (bites <= 0)
				{
					playersInGas.Clear();
					if (attacksInARow > 3 || UnityEngine.Random.Range(0, 3) == 0)
					{
						LeaveStart();
					}
					else
					{
						StateSet(State.Aggro);
					}
				}
				else
				{
					stateTimer = 0f;
					stateStart = true;
					bites--;
				}
			}
		}
		_ = isFixedUpdate;
	}

	private void StateDegrow()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 3f;
			stateTimer = 0f;
			soundDegrow.Play(base.transform.position);
			DegrowStart();
			ResetLocalGameTime();
		}
		if (!isFixedUpdate)
		{
			enemy.StateStunned.OverrideDisable(0.5f);
			VisualDegrowAnimation();
			VisualGrowDance();
			if (growDone)
			{
				if (doDespawn)
				{
					enemy.EnemyParent.Despawn();
					doDespawn = false;
				}
				else
				{
					StateSet(State.Grow);
				}
			}
		}
		_ = isFixedUpdate;
	}

	private void StateStunned()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 0f;
			stateTimer = 0f;
			playersInGas.Clear();
		}
		if (!isFixedUpdate)
		{
			VisualStateStunned();
			if (!enemy.IsStunned())
			{
				if (Physics.Raycast(headCenterTransform.position, Vector3.down, out var hitInfo, 100f, LayerMask.GetMask("Default")))
				{
					base.transform.position = hitInfo.point;
				}
				else
				{
					base.transform.position = enemy.Rigidbody.transform.position;
				}
				StateSet(State.Idle);
			}
		}
		_ = isFixedUpdate;
	}

	private void StateAggroGrow()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 1f;
			stateTimer = 0f;
			GrowStart();
			LevelPointsRoamAggro();
			soundGrow.Play(base.transform.position).pitch = 2f;
			doAttack = true;
		}
		if (!isFixedUpdate)
		{
			enemy.StateStunned.OverrideDisable(0.5f);
			VisualGrowAnimation(3f);
			VisualGrowDance();
			if (growDone)
			{
				StateSet(State.Aggro);
			}
		}
		_ = isFixedUpdate;
	}

	private void StateAggroDegrow()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 1f;
			stateTimer = 0f;
			soundDegrow.Play(base.transform.position).pitch = 2f;
			DegrowStart();
			doAttack = true;
			ResetLocalGameTime();
		}
		if (!isFixedUpdate)
		{
			enemy.StateStunned.OverrideDisable(0.5f);
			VisualDegrowAnimation(5f);
			VisualGrowDance();
			if (growDone)
			{
				StateSet(State.AggroGrow);
			}
		}
		_ = isFixedUpdate;
	}

	private void StateIdleBreaker()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 0f;
			stateTimer = 0f;
			doAttack = true;
			if (idleBreakerIndex == 0)
			{
				AudioSource audioSource = soundIdleBreaker01.Play(headCenterTransform.position);
				stateTimerMax = audioSource.clip.length;
				audioSources.Add(audioSource);
			}
			if (idleBreakerIndex == 1)
			{
				AudioSource audioSource2 = soundIdleBreaker02.Play(headCenterTransform.position);
				stateTimerMax = audioSource2.clip.length;
				audioSources.Add(audioSource2);
			}
			if (idleBreakerIndex == 2)
			{
				AudioSource audioSource3 = soundIdleBreaker03.Play(headCenterTransform.position);
				stateTimerMax = audioSource3.clip.length;
				audioSources.Add(audioSource3);
			}
		}
		if (!isFixedUpdate)
		{
			VisualStateIdleBreaker();
			if (stateTimer >= stateTimerMax)
			{
				StateSet(State.Idle);
			}
		}
		_ = isFixedUpdate;
	}

	private void StateAggroBreaker()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 0f;
			stateTimer = 0f;
			if (idleBreakerIndex == 0)
			{
				AudioSource audioSource = soundAggroBreaker01.Play(headCenterTransform.position);
				stateTimerMax = audioSource.clip.length;
			}
			if (idleBreakerIndex == 1)
			{
				AudioSource audioSource2 = soundAggroBreaker02.Play(headCenterTransform.position);
				stateTimerMax = audioSource2.clip.length;
			}
			if (idleBreakerIndex == 2)
			{
				AudioSource audioSource3 = soundAggroBreaker03.Play(headCenterTransform.position);
				stateTimerMax = audioSource3.clip.length;
			}
		}
		if (!isFixedUpdate)
		{
			VisualStateAggroBreaker();
			if (stateTimer >= stateTimerMax)
			{
				StateSet(State.Aggro);
			}
		}
		_ = isFixedUpdate;
	}

	private void StateAggroStart()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = 1f;
			stateTimer = 0f;
			soundAggroStart.Play(headCenterTransform.position);
			aggroBreakersInARow = 2;
			doAttack = true;
		}
		if (!isFixedUpdate)
		{
			VisualStateAggroBreaker();
			LookAtTarget();
			if (stateTimer >= stateTimerMax)
			{
				StateSet(State.Aggro);
			}
		}
		_ = isFixedUpdate;
	}

	private void VisualStateNormal()
	{
		VisualAnimateBaseMeshScale(15f, 0.4f, 30f, -0.15f, 20f, 0.1f, 35f, 0.15f);
		VisualSineDanceTargets();
	}

	private void VisualStateNormalToEnchanting()
	{
		overrideVisualsToEnchantedTransitioning = VisualPopAwayMeshes();
	}

	private void VisualStateEnchanting()
	{
		hugVOTimer += Time.deltaTime;
		float num = 0.2f;
		float num2 = 0.6f;
		float num3 = 1.2f;
		if (hugVOTimer < num)
		{
			SetMouthState(closed: true);
		}
		else if (hugVOTimer >= num && hugVOTimer < num2)
		{
			SetMouthState(closed: false);
			Vector3 localScale = new Vector3(1f, 1f + Mathf.Sin(localGameTime * 65f) * 0.3f, 1f);
			cuteMeshRightOpenMouth.localScale = localScale;
			cuteMeshLeftOpenMouth.localScale = localScale;
		}
		else if (hugVOTimer >= num2 && hugVOTimer < num3)
		{
			SetMouthState(closed: true);
			Vector3 localScale2 = new Vector3(1f, 1f - Mathf.Sin(localGameTime * 35f) * 0.15f, 1f);
			cuteMeshRightClosedMouth.localScale = localScale2;
			cuteMeshLeftClosedMouth.localScale = localScale2;
		}
		if (hugVOTimer >= num3)
		{
			hugVOTimer = 0f;
		}
		VisualSineDanceTargets();
		void SetMouthState(bool closed)
		{
			bool activeSelf = cuteMeshRightClosedMouth.gameObject.activeSelf;
			if (closed && !activeSelf)
			{
				cuteMeshRightClosedMouth.gameObject.SetActive(value: true);
				cuteMeshLeftClosedMouth.gameObject.SetActive(value: true);
				cuteMeshRightOpenMouth.gameObject.SetActive(value: false);
				cuteMeshLeftOpenMouth.gameObject.SetActive(value: false);
			}
			else if (!closed && activeSelf)
			{
				cuteMeshRightClosedMouth.gameObject.SetActive(value: false);
				cuteMeshLeftClosedMouth.gameObject.SetActive(value: false);
				cuteMeshRightOpenMouth.gameObject.SetActive(value: true);
				cuteMeshLeftOpenMouth.gameObject.SetActive(value: true);
				soundEnchantedVO.Play(cuteMeshRightOpenMouth.position);
			}
		}
	}

	private void VisualStateEnchantingToNormal()
	{
		overrideVisualsToEnchantedTransitioning = VisualPopBackMeshes();
	}

	private void VisualStateChompGasp()
	{
		OverrideActivateHorrorMeshes();
		OverrideSpringValues(50f, 0.3f);
		VisualCatchDance();
	}

	private void VisualStateChomp()
	{
		OverrideActivateHorrorMeshes();
		OverrideSpringValues(50f, 0.3f);
	}

	private void VisualStateLureStart()
	{
		OverrideSpringValues(30f, 0.5f);
		int num = 0;
		foreach (SpringLimb springLimb in springLimbs)
		{
			num++;
			springLimb.target.localRotation = springLimb.originalQuaternion;
			if (springLimb.name == "body")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler((float)num * -1.5f + Mathf.Sin(localGameTime * 24f + (float)num) * 5f, 0f, Mathf.Sin(localGameTime * 16f + (float)num) * 5f);
			}
			if (springLimb.name == "arm" || springLimb.name == "arm base")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, 0f, Mathf.Sin(localGameTime * 16f) * 80f);
			}
			if (springLimb.name == "leaf")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Sin(localGameTime * 30f) * 20f, Mathf.Sin(localGameTime * 30f) * 20f, Mathf.Sin(localGameTime * 30f) * 20f);
			}
			if (springLimb.name == "head")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, Mathf.Sin(localGameTime * 42f) * 30f, 0f);
			}
		}
		vineRotationSpeed = 2000f;
	}

	private void VisualStateLure()
	{
		OverrideSpringValues(15f, 0.5f);
		int num = 0;
		foreach (SpringLimb springLimb in springLimbs)
		{
			springLimb.target.localRotation = springLimb.originalQuaternion;
			num++;
			if (springLimb.name == "body")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler((float)num + Mathf.Sin(localGameTime * 24f + (float)num) * 5f, 0f, Mathf.Sin(localGameTime * 16f + (float)num) * 5f);
			}
			if (springLimb.name == "arm" || springLimb.name == "arm base")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, 0f, Mathf.Sin(localGameTime * 16f) * 80f);
			}
			if (springLimb.name == "leaf")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Sin(localGameTime * 20f) * (float)num * 2f, Mathf.Sin(localGameTime * 20f) * (float)num * 2f, Mathf.Sin(localGameTime * 20f) * (float)num * 2f);
			}
			if (springLimb.name == "head")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, Mathf.Sin(localGameTime * 42f) * 100f, 0f);
			}
			if (springLimb.name == "head look")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, 0f, 0f);
			}
		}
		vineRotationSpeed = 1000f;
	}

	private void VisualStateLureStop()
	{
	}

	private void VisualStateAggro()
	{
		OverrideSpringValues(30f, 0.5f);
		int num = 0;
		foreach (SpringLimb springLimb in springLimbs)
		{
			num++;
			springLimb.target.localRotation = springLimb.originalQuaternion;
			if (springLimb.name == "body")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler((float)num * 0.5f + Mathf.Sin(localGameTime * 2f + (float)num) * 2f, 0f, Mathf.Sin(localGameTime * 5f + (float)num) * 5f);
			}
			if (springLimb.name == "arm" || springLimb.name == "arm base")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, 0f, Mathf.Sin(localGameTime * 5f) * 20f);
			}
			if (springLimb.name == "leaf")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Sin(localGameTime * 30f) * 20f, Mathf.Sin(localGameTime * 30f) * 20f, Mathf.Sin(localGameTime * 30f) * 20f);
			}
			if (springLimb.name == "head")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, Mathf.Sin(localGameTime * 20f) * 10f, 0f);
			}
		}
		vineRotationSpeed = 2000f;
	}

	private void VisualStateStunned()
	{
		VisualAnimateBaseMeshScale(30f, 0.5f, 30f, -0.2f, 20f, 0.1f, 35f, 0.2f);
		OverrideSpringValues(40f, 0.5f);
		int num = 0;
		int num2 = -1;
		int num3 = -1;
		foreach (SpringLimb springLimb in springLimbs)
		{
			num++;
			springLimb.target.localRotation = springLimb.originalQuaternion;
			if (springLimb.name == "body")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0.5f + Mathf.Sin(localGameTime * 20f + (float)num) * 2f - (float)num * 0.5f, Mathf.Sin(localGameTime * 20f) * 5f, Mathf.Sin(localGameTime * 5f + (float)num) * 2f);
			}
			if (springLimb.name == "arm")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, Mathf.Sin(localGameTime * 8f) * 40f * (float)num3, -40f);
			}
			if (springLimb.name == "arm base")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, -80f * (float)num3, Mathf.Sin(localGameTime * 5f) * 20f);
				num3 *= -1;
			}
			if (springLimb.name == "leaf")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Sin(localGameTime * 30f) * 20f, Mathf.Sin(localGameTime * 30f) * 20f, Mathf.Sin(localGameTime * 30f) * 20f);
			}
			if (springLimb.name == "head look")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(-40f, 0f, 0f);
			}
			if (springLimb.name == "head")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(10f, (70f - Mathf.Sin(localGameTime * 25f) * 2f) * (float)num2, 10f * (float)num2);
				num2 *= -1;
			}
		}
		vineRotationSpeed = 1000f;
	}

	private void VisualStateIdleBreaker()
	{
		float num = idleBreakerCurve.Evaluate(stateTimer / stateTimerMax);
		if (num < 0f)
		{
			num = 0f;
		}
		OverrideSpringValues(20f + 20f * num, 0.5f - 0.05f * num);
		num = idleBreakerCurve.Evaluate(stateTimer / stateTimerMax);
		int num2 = 0;
		int num3 = -1;
		foreach (SpringLimb springLimb in springLimbs)
		{
			num2++;
			springLimb.target.localRotation = springLimb.originalQuaternion;
			float num4 = (float)(num2 / 5) * num;
			if (springLimb.name == "body")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler((0f - num4) * 20f, Mathf.Sin(localGameTime * 16f) * 5f * num, Mathf.Sin(localGameTime * 16f + (float)num2) * 2f * num);
			}
			if (springLimb.name == "arm" || springLimb.name == "arm base")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, 0f, Mathf.Sin(localGameTime * 16f) * 20f - 20f * num);
			}
			if (springLimb.name == "head look")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(20f * num, 0f, 0f);
			}
			if (springLimb.name == "leaf")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Sin(localGameTime * 30f) * 20f, Mathf.Sin(localGameTime * 30f) * 20f, Mathf.Sin(localGameTime * 30f) * 20f);
			}
			if (springLimb.name == "head")
			{
				float num5 = num;
				if (num5 < 0f)
				{
					num5 = 0f;
				}
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, num5 * (40f * (float)num3), 0f);
				num3 += 2;
			}
			vineRotationSpeed = 200f + 800f * num;
		}
	}

	private void VisualStateAggroBreaker()
	{
		if (stateTimer > stateTimerMax * 0.3f && stateTimer < stateTimerMax - stateTimerMax * 0.3f)
		{
			OverrideActivateHorrorMeshes();
		}
		float num = idleBreakerCurve.Evaluate(stateTimer / stateTimerMax);
		if (num < 0f)
		{
			num = 0f;
		}
		OverrideSpringValues(20f + 20f * num, 0.5f - 0.05f * num);
		num = idleBreakerCurve.Evaluate(stateTimer / stateTimerMax);
		int num2 = 0;
		int num3 = -1;
		foreach (SpringLimb springLimb in springLimbs)
		{
			num2++;
			springLimb.target.localRotation = springLimb.originalQuaternion;
			float num4 = (float)(num2 / 5) * num;
			if (springLimb.name == "body")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(num4 * 20f, Mathf.Sin(localGameTime * 16f) * 5f * num, Mathf.Sin(localGameTime * 16f + (float)num2) * 2f * num);
			}
			if (springLimb.name == "arm" || springLimb.name == "arm base")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, 0f, Mathf.Sin(localGameTime * 16f) * 20f - 20f * num);
			}
			if (springLimb.name == "head look")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(-20f * num, 0f, 0f);
			}
			if (springLimb.name == "leaf")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Sin(localGameTime * 30f) * 20f, Mathf.Sin(localGameTime * 30f) * 20f, Mathf.Sin(localGameTime * 30f) * 20f);
			}
			if (springLimb.name == "head")
			{
				float num5 = num;
				if (num5 < 0f)
				{
					num5 = 0f;
				}
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, num5 * (40f * (float)num3), 0f);
				num3 += 2;
			}
			vineRotationSpeed = 200f + 800f * num;
		}
	}

	private void VisualSineDanceTargets()
	{
		if (!growDone)
		{
			return;
		}
		int num = 0;
		foreach (SpringLimb springLimb in springLimbs)
		{
			num++;
			if (springLimb.name == "body")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Sin(localGameTime * 2f + (float)num) * 4f, 0f, Mathf.Sin(localGameTime * 1f + (float)num) * 4f);
			}
			if (springLimb.name == "head")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, Mathf.Sin(localGameTime * 2f + (float)num) * 30f, 0f);
			}
			if (springLimb.name == "head look")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, 0f, (0f - Mathf.Sin(localGameTime * 1f + (float)num)) * 5f);
			}
			if (springLimb.name == "arm" || springLimb.name == "arm base")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, Mathf.Sin(localGameTime * 2f + (float)num) * 30f, Mathf.Sin(localGameTime * 2f + (float)num) * 15f);
			}
			if (springLimb.name == "leaf")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, Mathf.Sin(localGameTime * 2f + (float)num) * 10f, Mathf.Sin(localGameTime * 2f + (float)num) * 5f);
			}
			vineRotationSpeed = Mathf.Lerp(vineRotationSpeed, 50f, Time.deltaTime * 0.1f);
		}
	}

	private void VisualGrowDance()
	{
		int num = 0;
		foreach (SpringLimb springLimb in springLimbs)
		{
			num++;
			if (springLimb.name == "body")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Sin(localGameTime * 24f + (float)num) * 5f, 0f, Mathf.Sin(localGameTime * 16f + (float)num) * 5f);
			}
			if (springLimb.name == "leaf")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Sin(localGameTime * 24f + (float)num) * 5f, Mathf.Sin(localGameTime * 16f + (float)num) * 10f, Mathf.Sin(localGameTime * 16f + (float)num) * 5f);
			}
			if (springLimb.name == "head")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, Mathf.Sin(localGameTime * 24f + (float)num) * 10f, 0f);
			}
		}
		vineRotationSpeed = 1000f;
	}

	private void VisualCatchDance()
	{
		int num = 0;
		float num2 = 4f;
		float num3 = 0.5f;
		foreach (SpringLimb springLimb in springLimbs)
		{
			num++;
			if (springLimb.name == "body")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Sin(localGameTime * (24f * num2) + (float)num) * (5f * num3), 0f, Mathf.Sin(localGameTime * (16f * num2) + (float)num) * (5f * num3));
			}
			if (springLimb.name == "leaf")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Sin(localGameTime * (24f * num2) + (float)num) * (5f * num3), Mathf.Sin(localGameTime * (16f * num2) + (float)num) * (10f * num3), Mathf.Sin(localGameTime * (16f * num2) + (float)num) * (5f * num3));
			}
			if (springLimb.name == "head")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(0f, Mathf.Sin(localGameTime * (24f * num2) + (float)num) * (40f * num3), 0f);
			}
		}
		vineRotationSpeed = 1000f;
	}

	private void VisualUpdateSprings()
	{
		float num = 15f;
		float num2 = 0.5f;
		if (!growDone)
		{
			num = 25f;
			num2 = 0.5f;
		}
		float num3 = num;
		float num4 = num2;
		if (overrideSpringValuesTimer > 0f)
		{
			num3 = overrideSpringValuesSpeed;
			num4 = overrideSpringValuesDamping;
		}
		foreach (SpringLimb springLimb in springLimbs)
		{
			if (growDone)
			{
				if (springLimb.name == "leaf" && overrideSpringValuesTimer <= 0f)
				{
					num = 5f;
					num2 = 0.25f;
				}
				else
				{
					num = num3;
					num2 = num4;
				}
			}
			springLimb.spring.speed = num;
			springLimb.spring.damping = num2;
			springLimb.spring.maxAngle = 5f;
			springLimb.spring.clamp = false;
			springLimb.transform.rotation = SemiFunc.SpringQuaternionGet(springLimb.spring, springLimb.target.rotation);
		}
		baseMeshScale.localScale = SemiFunc.SpringVector3Get(baseScaleSpring, baseMeshScaleTarget.localScale);
	}

	private void VisualGrowAnimation(float _growSpeed = 1f)
	{
		GrowSpin();
		SpringLimb springLimb = null;
		bool flag = false;
		foreach (SpringLimb springLimb2 in springLimbs)
		{
			if (!(springLimb2.name == "head look") && (springLimb == null || springLimb.growAmount > 0.2f))
			{
				springLimb2.growAmount += Time.deltaTime * _growSpeed;
				if (springLimb2.growAmount >= 1f)
				{
					springLimb2.growAmount = 1f;
				}
				else
				{
					flag = true;
				}
				Vector3 vector = Vector3.one * (1f * growAnimationCurve.Evaluate(springLimb2.growAmount));
				vector = new Vector3(Mathf.Max(vector.x, 0.01f), Mathf.Max(vector.y, 0.01f), Mathf.Max(vector.z, 0.01f));
				springLimb2.transform.localScale = vector;
				springLimb = springLimb2;
			}
		}
		if (!flag)
		{
			growDone = true;
		}
	}

	private void VisualDegrowAnimation(float _growSpeed = 1f)
	{
		GrowSpin();
		SpringLimb springLimb = null;
		bool flag = false;
		for (int num = springLimbs.Count - 1; num >= 0; num--)
		{
			SpringLimb springLimb2 = springLimbs[num];
			if (!(springLimb2.name == "head look") && (springLimb == null || springLimb.growAmount > 0.2f))
			{
				springLimb2.growAmount += Time.deltaTime * _growSpeed;
				if (springLimb2.growAmount >= 1f)
				{
					springLimb2.growAmount = 1f;
					springLimb2.transform.gameObject.SetActive(value: false);
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					Vector3 localScale = Vector3.one * (1f - 1f * growAnimationCurve.Evaluate(springLimb2.growAmount));
					if (localScale.x < 0.01f)
					{
						localScale.x = 0.01f;
					}
					if (localScale.y < 0.01f)
					{
						localScale.y = 0.01f;
					}
					if (localScale.z < 0.01f)
					{
						localScale.z = 0.01f;
					}
					springLimb2.transform.localScale = localScale;
				}
				springLimb = springLimb2;
			}
		}
		if (!flag)
		{
			growDone = true;
		}
	}

	private void VisualRotateVines()
	{
		vineRotation += vineRotationSpeed * Time.deltaTime;
		baseRotation += vineRotationSpeed * 0.5f * Time.deltaTime;
		vineRotation %= 360f;
		baseRotation %= 360f;
		if (float.IsNaN(vineRotation))
		{
			vineRotation = 0f;
		}
		if (float.IsNaN(baseRotation))
		{
			baseRotation = 0f;
		}
		foreach (Transform vineTransform in vineTransforms)
		{
			vineTransform.localRotation = Quaternion.Euler(0f, vineRotation, 0f);
		}
		baseMeshScale.localRotation = Quaternion.Euler(0f, baseRotation, 0f);
	}

	private void VisualPopAwayMeshesStart()
	{
		meshTransforms.Shuffle();
		foreach (MeshTransform meshTransform in meshTransforms)
		{
			meshTransform.eval = 0f;
			meshTransform.soundPlayed = false;
			meshTransform.transform.localScale = Vector3.one;
			meshTransform.transform.gameObject.SetActive(value: true);
		}
		cuteMeshTransforms.Shuffle();
		foreach (CuteMeshTransform cuteMeshTransform in cuteMeshTransforms)
		{
			cuteMeshTransform.eval = 0f;
			cuteMeshTransform.soundPlayed = false;
			cuteMeshTransform.transform.localScale = Vector3.zero;
			cuteMeshTransform.transform.gameObject.SetActive(value: true);
		}
	}

	private bool VisualPopAwayMeshes()
	{
		float num = 1f;
		bool flag = false;
		MeshTransform meshTransform = null;
		foreach (MeshTransform meshTransform2 in meshTransforms)
		{
			if (meshTransform == null || meshTransform.eval > 0.3f)
			{
				meshTransform2.eval += Time.deltaTime * 10f * num;
				if (meshTransform2.eval > 0.2f && !meshTransform2.soundPlayed)
				{
					popAwaySound.Play(meshTransform2.transform.position);
					meshTransform2.soundPlayed = true;
				}
				if (meshTransform2.eval >= 1f)
				{
					meshTransform2.eval = 1f;
					meshTransform2.transform.gameObject.SetActive(value: false);
					meshTransform2.transform.localScale = Vector3.one;
				}
				else
				{
					flag = true;
				}
				meshTransform2.transform.localScale = Vector3.one * popAwayMeshesCurve.Evaluate(meshTransform2.eval);
				meshTransform = meshTransform2;
			}
		}
		if ((bool)headLight)
		{
			headLight.intensity = Mathf.Lerp(headLight.intensity, 2f, Time.deltaTime * 2f);
		}
		if ((bool)headParticles)
		{
			ParticleSystem.EmissionModule emission = headParticles.emission;
			emission.rateOverTime = Mathf.Lerp(emission.rateOverTime.constant, 20f, Time.deltaTime * 2f);
		}
		bool flag2 = false;
		CuteMeshTransform cuteMeshTransform = null;
		foreach (CuteMeshTransform cuteMeshTransform2 in cuteMeshTransforms)
		{
			if (cuteMeshTransform == null || cuteMeshTransform.eval > 0.3f)
			{
				cuteMeshTransform2.eval += Time.deltaTime * 10f * num;
				if (cuteMeshTransform2.eval > 0.2f && !cuteMeshTransform2.soundPlayed)
				{
					popAwaySound.Play(cuteMeshTransform2.transform.position);
					cuteMeshTransform2.soundPlayed = true;
				}
				if (cuteMeshTransform2.eval >= 1f)
				{
					cuteMeshTransform2.eval = 1f;
				}
				else
				{
					flag2 = true;
				}
				cuteMeshTransform2.transform.localScale = Vector3.one * popBackMeshesCurve.Evaluate(cuteMeshTransform2.eval);
				cuteMeshTransform = cuteMeshTransform2;
			}
		}
		VisualGrowDance();
		if (!flag && !flag2)
		{
			foreach (CuteMeshTransform cuteMeshTransform3 in cuteMeshTransforms)
			{
				cuteMeshTransform3.eval = 0f;
				cuteMeshTransform3.transform.localScale = Vector3.one;
			}
			foreach (MeshTransform meshTransform3 in meshTransforms)
			{
				meshTransform3.eval = 0f;
				meshTransform3.transform.localScale = Vector3.one;
			}
		}
		return flag || flag2;
	}

	private bool VisualPopBackMeshes()
	{
		bool flag = overrideVisualsToEnchantedInstantTransition;
		float num = 1f;
		bool flag2 = true;
		CuteMeshTransform cuteMeshTransform = null;
		foreach (CuteMeshTransform cuteMeshTransform2 in cuteMeshTransforms)
		{
			if (cuteMeshTransform == null || cuteMeshTransform.eval > 0.3f)
			{
				cuteMeshTransform2.eval += Time.deltaTime * 10f * num;
				if (flag)
				{
					cuteMeshTransform2.eval = 1f;
				}
				if (cuteMeshTransform2.eval > 0.2f && !cuteMeshTransform2.soundPlayed)
				{
					popAwaySound.Play(cuteMeshTransform2.transform.position);
					cuteMeshTransform2.soundPlayed = true;
				}
				cuteMeshTransform2.transform.localScale = Vector3.one * popAwayMeshesCurve.Evaluate(cuteMeshTransform2.eval);
				cuteMeshTransform = cuteMeshTransform2;
				if (cuteMeshTransform2.eval >= 1f)
				{
					cuteMeshTransform2.eval = 1f;
					cuteMeshTransform2.transform.gameObject.SetActive(value: false);
					cuteMeshTransform2.transform.localScale = Vector3.one;
				}
				else
				{
					flag2 = false;
				}
			}
		}
		if ((bool)headLight)
		{
			headLight.intensity = Mathf.Lerp(headLight.intensity, 0.2f, Time.deltaTime * 2f);
			if (flag)
			{
				headLight.intensity = 0.2f;
			}
		}
		if ((bool)headParticles)
		{
			ParticleSystem.EmissionModule emission = headParticles.emission;
			emission.rateOverTime = Mathf.Lerp(emission.rateOverTime.constant, 5f, Time.deltaTime * 2f);
			if (flag)
			{
				emission.rateOverTime = 5f;
			}
		}
		if (!flag2)
		{
			return true;
		}
		bool result = false;
		MeshTransform meshTransform = null;
		foreach (MeshTransform meshTransform2 in meshTransforms)
		{
			if (meshTransform == null || meshTransform.eval > 0.3f)
			{
				meshTransform2.eval += Time.deltaTime * 10f * num;
				if (flag)
				{
					meshTransform2.eval = 1f;
				}
				if (meshTransform2.eval > 0.2f && !meshTransform2.soundPlayed)
				{
					popAwaySound.Play(meshTransform2.transform.position);
					meshTransform2.soundPlayed = true;
				}
				if (meshTransform2.eval >= 1f)
				{
					meshTransform2.eval = 1f;
				}
				else
				{
					result = true;
				}
				meshTransform2.transform.localScale = Vector3.one * popBackMeshesCurve.Evaluate(meshTransform2.eval);
				meshTransform = meshTransform2;
			}
		}
		return result;
	}

	private void VisualPopBackMeshesStart()
	{
		meshTransforms.Shuffle();
		cuteMeshTransforms.Shuffle();
		foreach (MeshTransform meshTransform in meshTransforms)
		{
			meshTransform.eval = 0f;
			meshTransform.soundPlayed = false;
			meshTransform.transform.localScale = Vector3.zero;
			meshTransform.transform.gameObject.SetActive(value: true);
		}
		foreach (CuteMeshTransform cuteMeshTransform in cuteMeshTransforms)
		{
			cuteMeshTransform.eval = 0f;
			cuteMeshTransform.soundPlayed = false;
			cuteMeshTransform.transform.localScale = Vector3.one;
			cuteMeshTransform.transform.gameObject.SetActive(value: true);
		}
	}

	private void VisualBiteAnimation()
	{
		float t = biteCurve.Evaluate(stateTimer / stateTimerMax);
		int num = 1;
		foreach (SpringLimb springLimb in springLimbs)
		{
			if (springLimb.name == "body")
			{
				springLimb.target.localRotation = springLimb.originalQuaternion * Quaternion.Euler(Mathf.Lerp(0f, (float)num * 1.5f, t), 0f, 0f);
			}
			num++;
		}
	}

	private void VisualScaleBreathing()
	{
		scaleBreathing.speed = 60f;
		scaleBreathing.damping = 0.3f;
		holeScaleBreathing.speed = 60f;
		holeScaleBreathing.damping = 0.1f;
		float num = 0f;
		foreach (AudioSource audioSource in audioSources)
		{
			if ((bool)audioSource && audioSource.isPlaying)
			{
				float[] array = new float[256];
				audioSource.GetSpectrumData(array, 0, FFTWindow.BlackmanHarris);
				num += array[0];
			}
		}
		if (SemiFunc.FPSImpulse1())
		{
			List<AudioSource> list = new List<AudioSource>();
			foreach (AudioSource audioSource2 in audioSources)
			{
				if (!audioSource2)
				{
					list.Add(audioSource2);
				}
			}
			if (list.Count > 0)
			{
				foreach (AudioSource item in list)
				{
					audioSources.Remove(item);
				}
			}
		}
		num *= 3f;
		if (num > 0.3f)
		{
			num = 0.3f;
		}
		scaleBreathingTarget = new Vector3(1f + num, 1f - num, 1f - num * 2f);
		holeScaleBreathingTarget = new Vector3(1f + num * 2f, 1f + num * 2f, 1f + num * 3f);
		scaleBreathingCurrent = SemiFunc.SpringVector3Get(scaleBreathing, scaleBreathingTarget);
		holeScaleBreathingCurrent = SemiFunc.SpringVector3Get(holeScaleBreathing, holeScaleBreathingTarget);
		breatheScaleParent1.localScale = scaleBreathingCurrent;
		breatheScaleParent2.localScale = scaleBreathingCurrent;
		holeScaleParent1.localScale = holeScaleBreathingCurrent;
		holeScaleParent2.localScale = holeScaleBreathingCurrent;
	}

	private void VisualAnimateBaseMeshScale(float _springSpeed, float _springDamping, float _xSpeed, float _xAmount, float _ySpeed, float _yAmount, float _zSpeed, float _zAmount)
	{
		baseScaleSpring.speed = Mathf.Lerp(baseScaleSpring.speed, _springSpeed, Time.deltaTime * 0.1f);
		baseScaleSpring.damping = Mathf.Lerp(baseScaleSpring.damping, _springDamping, Time.deltaTime * 0.1f);
		baseMeshScaleTarget.localScale = new Vector3(1f + Mathf.Sin(localGameTime * _xSpeed) * _xAmount, 1f + Mathf.Sin(localGameTime * _ySpeed) * _yAmount, 1f + Mathf.Sin(localGameTime * _zSpeed) * _zAmount);
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
		case State.Grow:
			StateGrow();
			break;
		case State.AggroStart:
			StateAggroStart();
			break;
		case State.Aggro:
			StateAggro();
			break;
		case State.LureStart:
			StateLureStart();
			break;
		case State.Lure:
			StateLure();
			break;
		case State.LureStop:
			StateLureStop();
			break;
		case State.ChompGasp:
			StateChompGasp();
			break;
		case State.Chomp:
			StateChomp();
			break;
		case State.Degrow:
			StateDegrow();
			break;
		case State.Stunned:
			StateStunned();
			break;
		case State.AggroGrow:
			StateAggroGrow();
			break;
		case State.AggroDegrow:
			StateAggroDegrow();
			break;
		case State.IdleBreaker:
			StateIdleBreaker();
			break;
		case State.AggroBreaker:
			StateAggroBreaker();
			break;
		}
	}

	public void StateSet(State newState)
	{
		if (currentState != newState && SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("StateSetRPC", RpcTarget.All, (int)newState);
			}
			else
			{
				StateSetRPC((int)newState);
			}
		}
	}

	[PunRPC]
	private void StateSetRPC(int _newState, PhotonMessageInfo info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (currentState != (State)_newState)
			{
				currentState = (State)_newState;
				stateStart = true;
			}
		}
	}

	private void StateSetIdleBreaker(int _idleBreaker, bool _aggro = false)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				StateSetIdleBreakerRPC(_idleBreaker, _aggro);
				return;
			}
			photonView.RPC("StateSetIdleBreakerRPC", RpcTarget.All, _idleBreaker, _aggro);
		}
	}

	[PunRPC]
	private void StateSetIdleBreakerRPC(int _idleBreaker, bool _aggro, PhotonMessageInfo info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			idleBreakerIndex = _idleBreaker;
			if (!_aggro)
			{
				StateSet(State.IdleBreaker);
			}
			else
			{
				StateSet(State.AggroBreaker);
			}
		}
	}

	private void EnemySystemStateTriggers()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (enemy.IsStunned())
			{
				StateSet(State.Stunned);
			}
			if (enemy.CurrentState == EnemyState.Despawn && (currentState == State.Idle || currentState == State.Roam))
			{
				DespawnStart();
			}
			if (SemiFunc.EnemyForceLeave(enemy) && (currentState == State.Idle || currentState == State.Roam))
			{
				LeaveStart();
			}
		}
	}

	private void LookAtTarget()
	{
		hasTarget = true;
		if (hasTarget && (bool)currentTarget)
		{
			Vector3 vector = currentTarget.PlayerVisionTarget.VisionTransform.position + Vector3.down * 0.2f;
			Quaternion localRotation = headLook.localRotation;
			headLook.LookAt(vector);
			Quaternion localRotation2 = headLook.localRotation;
			headLook.localRotation = Quaternion.Slerp(localRotation, localRotation2, Time.deltaTime * 10f);
			Vector3 forward = vector - headLook.position;
			forward.y = 0f;
			Quaternion quaternion = Quaternion.LookRotation(forward);
			if (!(forward.sqrMagnitude < 0.01f))
			{
				followTarget.rotation = Quaternion.Slerp(followTarget.rotation, quaternion, Time.deltaTime * 5f);
				lookAtActiveTimer = 1f;
			}
		}
	}

	private void LookAtTargetReset()
	{
		if (lookAtActiveTimer <= 0f)
		{
			headLook.localRotation = Quaternion.Slerp(headLook.localRotation, Quaternion.Euler(new Vector3(-30f, 0f, 0f)), Time.deltaTime * 5f);
		}
		else
		{
			lookAtActiveTimer -= Time.deltaTime;
		}
	}

	private void GrowStart()
	{
		growDone = false;
		foreach (SpringLimb springLimb in springLimbs)
		{
			if (!(springLimb.name == "head look"))
			{
				springLimb.growAmount = 0f;
				springLimb.transform.localScale = Vector3.zero;
				springLimb.transform.gameObject.SetActive(value: true);
			}
		}
	}

	private void DegrowStart()
	{
		growDone = false;
		foreach (SpringLimb springLimb in springLimbs)
		{
			if (!(springLimb.name == "head look"))
			{
				springLimb.growAmount = 0f;
				springLimb.transform.localScale = Vector3.one;
			}
		}
	}

	private void ActivateHorrorMeshes(bool _activate)
	{
		if (_activate)
		{
			horrorMesh1.SetActive(value: true);
			horrorMesh2.SetActive(value: true);
			normalMesh1.SetActive(value: false);
			normalMesh2.SetActive(value: false);
			headLight.intensity = 1f;
			horrorMeshesActive = true;
		}
		else
		{
			horrorMesh1.SetActive(value: false);
			horrorMesh2.SetActive(value: false);
			normalMesh1.SetActive(value: true);
			normalMesh2.SetActive(value: true);
			headLight.intensity = 0.2f;
			horrorMeshesActive = false;
		}
	}

	private void ActivateBiteMesh(bool _active)
	{
		if (_active)
		{
			GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
			GameDirector.instance.CameraImpact.ShakeDistance(4f, 3f, 8f, base.transform.position, 0.1f);
			biteMesh.SetActive(value: true);
			NudgeSpring("head look", new Vector3(30f, ((double)UnityEngine.Random.value > 0.5) ? (-10f) : 10f, ((double)UnityEngine.Random.value > 0.5) ? (-10f) : 10f));
			horrorMesh1.SetActive(value: false);
			horrorMesh2.SetActive(value: false);
			biteHurtCollider.SetActive(value: true);
		}
		else
		{
			NudgeAllSprings(new Vector3(((double)UnityEngine.Random.value > 0.5) ? (-2f) : 2f, ((double)UnityEngine.Random.value > 0.5) ? (-2f) : 2f, ((double)UnityEngine.Random.value > 0.5) ? (-2f) : 2f));
			biteMesh.SetActive(value: false);
			horrorMesh1.SetActive(value: true);
			horrorMesh2.SetActive(value: true);
		}
	}

	private void NudgeSpring(string _name, Vector3 _nudgeAmount)
	{
		foreach (SpringLimb springLimb in springLimbs)
		{
			if (springLimb.name == _name)
			{
				springLimb.spring.springVelocity = _nudgeAmount;
			}
		}
	}

	public void NudgeAllSprings(Vector3 _nudgeAmount)
	{
		int num = 1;
		foreach (SpringLimb springLimb in springLimbs)
		{
			springLimb.spring.springVelocity = _nudgeAmount * ((float)num / 4f);
			num++;
		}
	}

	private void ShootGasLogic()
	{
		if (shootGasTimer <= 0f && isShootingGas)
		{
			gasParticles.Stop(withChildren: true);
			playersInGas.Clear();
			isShootingGas = false;
		}
		if (shootGasTimer > 0f)
		{
			if (gasCheckerEmissionTimer > 0.1f)
			{
				EnemyHeartHuggerGasChecker component = UnityEngine.Object.Instantiate(gasChecker, mouthTransform.position, Quaternion.identity).GetComponent<EnemyHeartHuggerGasChecker>();
				component.scaleTarget = UnityEngine.Random.Range(1.5f, 2f);
				component.lifeTimeMax = UnityEngine.Random.Range(1f, 2f);
				component.transform.localScale = Vector3.zero;
				component.enemyHeartHugger = this;
				Vector3 forward = mouthTransform.forward;
				float num = 10f;
				forward = Quaternion.Euler(UnityEngine.Random.Range(0f - num, num), UnityEngine.Random.Range(0f - num, num), UnityEngine.Random.Range(0f - num, num)) * forward;
				component.velocity = forward * UnityEngine.Random.Range(2f, 8f);
				gasCheckerEmissionTimer = 0f;
				component.gameObject.SetActive(value: true);
			}
			gasCheckerEmissionTimer += Time.deltaTime;
			isShootingGas = true;
			shootGasTimer -= Time.deltaTime;
		}
	}

	private void ShootGas()
	{
		ParticlePlayGas();
		shootGasTimer = 0.2f;
	}

	private void PlayersInGasLogic()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (playersInGas.Count > 0)
		{
			List<PlayersInGas> list = new List<PlayersInGas>();
			foreach (PlayersInGas playersInGa in playersInGas)
			{
				playersInGa.playerAvatar.upgradeTumbleWingsLogic.tumbleWingPinkTimer = 1f;
				if (playersInGa.inGasTime >= 2f)
				{
					playersInGa.isCaught = true;
				}
				playersInGa.inGasTime += Time.deltaTime;
				float num = Vector3.Distance(playersInGa.lastPositionInsideGas, playersInGa.playerAvatar.transform.position);
				if (playersInGa.outsideGasTime >= 3f || num > 2f)
				{
					list.Add(playersInGa);
				}
				playersInGa.outsideGasTime += Time.deltaTime;
			}
			foreach (PlayersInGas item in list)
			{
				playersOnCooldown.Add(item.playerAvatar, Time.time);
				playersInGas.Remove(item);
			}
		}
		if (!SemiFunc.FPSImpulse5())
		{
			return;
		}
		foreach (PlayersInGas playersInGa2 in playersInGas)
		{
			bool flag = true;
			foreach (PlayersInGas playersInGasPreviou in playersInGasPrevious)
			{
				if (playersInGa2.playerAvatar == playersInGasPreviou.playerAvatar)
				{
					flag = false;
				}
			}
			if (flag)
			{
				if (SemiFunc.IsMultiplayer())
				{
					photonView.RPC("PlayerInGasClientRPC", RpcTarget.All, playersInGa2.playerAvatar.photonView.ViewID, true);
				}
				else
				{
					PlayerInGasClientRPC(playersInGa2.playerAvatar.photonView.ViewID, _add: true);
				}
			}
		}
		foreach (PlayersInGas playersInGasPreviou2 in playersInGasPrevious)
		{
			bool flag2 = true;
			foreach (PlayersInGas playersInGa3 in playersInGas)
			{
				if (playersInGa3.playerAvatar == playersInGasPreviou2.playerAvatar)
				{
					flag2 = false;
				}
			}
			if (flag2)
			{
				if (SemiFunc.IsMultiplayer())
				{
					photonView.RPC("PlayerInGasClientRPC", RpcTarget.All, playersInGasPreviou2.playerAvatar.photonView.ViewID, false);
				}
				else
				{
					PlayerInGasClientRPC(playersInGasPreviou2.playerAvatar.photonView.ViewID, _add: false);
				}
			}
		}
		playersInGasPrevious.Clear();
		playersInGasPrevious.AddRange(playersInGas);
	}

	[PunRPC]
	private void PlayerInGasClientRPC(int _photonViewID, bool _add, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item.photonView.ViewID != _photonViewID)
			{
				continue;
			}
			if (_add)
			{
				if (!playersInGasClient.Contains(item))
				{
					playersInGasClient.Add(item);
				}
			}
			else
			{
				playersInGasClient.Remove(item);
			}
			break;
		}
	}

	private void PlayersOnCooldownTick()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || playersOnCooldown.Count == 0)
		{
			return;
		}
		List<PlayerAvatar> list = new List<PlayerAvatar>();
		foreach (KeyValuePair<PlayerAvatar, float> item in playersOnCooldown)
		{
			if (item.Value + 5f < Time.time)
			{
				list.Add(item.Key);
			}
		}
		foreach (PlayerAvatar item2 in list)
		{
			playersOnCooldown.Remove(item2);
		}
	}

	public void PlayerInGas(PlayerAvatar _player)
	{
		foreach (PlayersInGas playersInGa in this.playersInGas)
		{
			if (playersInGa.playerAvatar == _player)
			{
				playersInGa.outsideGasTime = 0f;
				playersInGa.lastPositionInsideGas = _player.transform.position;
				return;
			}
		}
		PlayersInGas playersInGas = new PlayersInGas();
		playersInGas.playerAvatar = _player;
		playersInGas.outsideGasTime = 0f;
		playersInGas.lastPositionInsideGas = _player.transform.position;
		this.playersInGas.Add(playersInGas);
	}

	public bool PlayerInGasCheck(PlayerAvatar _player)
	{
		foreach (PlayersInGas playersInGa in playersInGas)
		{
			if (playersInGa.playerAvatar == _player)
			{
				return true;
			}
		}
		return false;
	}

	private void TurnRandomTimer(float min, float max)
	{
		turnRandomTimer = UnityEngine.Random.Range(min, max);
		turnRandomTimerMin = min;
		turnRandomTimerMax = max;
	}

	private void TurnRandomFull()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (turnRandomTimer <= 0f)
			{
				float num = ((UnityEngine.Random.value > 0.5f) ? 1f : (-1f));
				float num2 = UnityEngine.Random.Range(35f, 65f) * num;
				base.transform.rotation = Quaternion.Euler(base.transform.rotation.x, base.transform.rotation.y + num2, base.transform.rotation.y);
				turnRandomTimer = UnityEngine.Random.Range(turnRandomTimerMin, turnRandomTimerMax);
			}
			else
			{
				turnRandomTimer -= Time.deltaTime;
			}
		}
	}

	private void LevelPointsRoam()
	{
		levelPoints.Clear();
		List<LevelPoint> list = ((UnityEngine.Random.Range(0, 4) != 0) ? SemiFunc.LevelPointGetWithinDistance(base.transform.position, 10f, 25f) : SemiFunc.LevelPointsGetPlayerDistance(base.transform.position, 0f, 20f));
		List<LevelPoint> list2 = SemiFunc.LevelPointListPurgeObstructed(list, rb.GetComponentInChildren<CapsuleCollider>());
		if (list2.Count > 0)
		{
			levelPoints = list2;
		}
		SetPositionToRandomLevelPoint();
	}

	private void LevelPointsInvestigate()
	{
		levelPoints.Clear();
		List<LevelPoint> list = SemiFunc.LevelPointGetWithinDistance(investigatePosition, 0f, 10f);
		List<LevelPoint> list2 = new List<LevelPoint>();
		if (list != null)
		{
			list2 = SemiFunc.LevelPointListPurgeObstructed(list, rb.GetComponentInChildren<CapsuleCollider>());
		}
		if (list2.Count > 0)
		{
			levelPoints = list2;
			SetPositionToRandomLevelPoint();
		}
		else
		{
			LevelPointsRoam();
		}
	}

	private void LevelPointsLeave()
	{
		levelPoints.Clear();
		List<LevelPoint> list = SemiFunc.LevelPointsGetPlayerDistance(base.transform.position, 30f, 999f);
		if (list.Count == 0)
		{
			LevelPoint levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
			if ((bool)levelPoint)
			{
				list.Add(levelPoint);
			}
		}
		List<LevelPoint> list2 = new List<LevelPoint>();
		if (list.Count > 0)
		{
			list2 = SemiFunc.LevelPointListPurgeObstructed(list, rb.GetComponentInChildren<CapsuleCollider>());
		}
		if (list2.Count > 0)
		{
			levelPoints = list2;
			SetPositionToRandomLevelPoint();
		}
		else
		{
			LevelPointsRoam();
		}
		doLeave = false;
	}

	private void LevelPointsRoamAggro()
	{
		levelPoints.Clear();
		List<LevelPoint> list = SemiFunc.LevelPointListPurgeObstructed(SemiFunc.LevelPointsGetVisiblePointsBehindPlayers(1f, 10f, 0.5f, currentTarget), rb.GetComponentInChildren<CapsuleCollider>());
		if (list.Count > 0)
		{
			levelPoints = list;
		}
		else
		{
			List<LevelPoint> list2 = SemiFunc.LevelPointGetWithinDistance(base.transform.position, 3f, 10f);
			list.Clear();
			list = SemiFunc.LevelPointListPurgeObstructed(list2, rb.GetComponentInChildren<CapsuleCollider>());
			if (list.Count > 0)
			{
				levelPoints = list;
			}
		}
		SetPositionToRandomLevelPoint();
	}

	private void SetPositionToRandomLevelPoint()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && levelPoints.Count > 0)
		{
			Vector3 position = levelPoints[UnityEngine.Random.Range(0, levelPoints.Count)].transform.position;
			base.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
			enemy.EnemyTeleported(position);
		}
	}

	private void GrowSpin()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			float num = 1000f;
			float num2 = stateTimer / stateTimerMax * num;
			base.transform.Rotate(Vector3.up, num2 * Time.deltaTime, Space.Self);
		}
	}

	private void JumpScareAtChompStart()
	{
		foreach (PlayerAvatar item in playersInGasClient)
		{
			if ((bool)item && item.isLocal)
			{
				CameraGlitch.Instance.PlayLong();
				AudioScare.instance.PlayImpact();
				particlesShatteredDream.Play(withChildren: true);
				GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
				GameDirector.instance.CameraImpact.ShakeDistance(4f, 3f, 8f, base.transform.position, 0.1f);
			}
		}
	}

	private void JumpScareAtChompStartForceLookAtHead()
	{
		foreach (PlayerAvatar item in playersInGasClient)
		{
			if ((bool)item && item.isLocal)
			{
				Vector3 vector = item.localCamera.transform.position - headCenterTransform.position;
				float num = Vector3.Dot(Vector3.down, vector.normalized);
				float strengthNoAim = 40f;
				if (num > 0.9f)
				{
					strengthNoAim = 20f;
				}
				CameraAim.Instance.AimTargetSoftSet(headCenterTransform.position, 0.1f, 2f, strengthNoAim, base.gameObject, 100);
				PostProcessing.Instance.VignetteOverride(Color.black, 0.5f, 1f, 1f, 0.5f, 0.1f, base.gameObject);
				CameraZoom.Instance.OverrideZoomSet(40f, 0.1f, 1f, 1f, base.gameObject, 50);
			}
		}
	}

	public bool PlayerIsOnCooldown(PlayerAvatar _player)
	{
		if (playersOnCooldown.ContainsKey(_player) && playersOnCooldown[_player] + 5f > Time.time)
		{
			return true;
		}
		return false;
	}

	public void RemovePlayerFromGas(PlayerAvatar _player)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		List<PlayersInGas> list = new List<PlayersInGas>();
		foreach (PlayersInGas playersInGa in playersInGas)
		{
			if (playersInGa.playerAvatar == _player)
			{
				list.Add(playersInGa);
			}
		}
		foreach (PlayersInGas item in list)
		{
			playersInGas.Remove(item);
		}
	}

	private void ResetLocalGameTime()
	{
		float num = 50f;
		if (!SemiFunc.IsMultiplayer())
		{
			num = 0.0001f;
		}
		localGameTime = Math.Abs((float)instanceID * num);
	}

	[PunRPC]
	private void UpdateCurrentTargetRPC(int _photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item.photonView.ViewID == _photonViewID)
			{
				currentTarget = item;
				hasTarget = true;
				break;
			}
		}
	}

	private void InvestigatePositionReset()
	{
		if (investigatePosition != Vector3.zero && currentState != State.Degrow && currentState != State.Grow)
		{
			investigatePosition = Vector3.zero;
		}
	}

	private void LeaveReset()
	{
		if (doLeave && currentState != State.Degrow && currentState != State.Grow)
		{
			doLeave = false;
		}
	}

	private void DespawnReset()
	{
		if (doDespawn && currentState != State.Degrow)
		{
			doDespawn = false;
		}
	}

	private void AttacksInARowReset()
	{
		if (attacksInARow > 0 && (currentState == State.Spawn || currentState == State.Degrow))
		{
			attacksInARow = 0;
		}
	}

	private void AggroBreakersInARowReset()
	{
		if (aggroBreakersInARow > 0 && (currentState == State.Spawn || currentState == State.Degrow))
		{
			aggroBreakersInARow = 0;
		}
	}

	private void LeaveStart()
	{
		attacksInARow = 0;
		doLeave = true;
		StateSet(State.Degrow);
	}

	private void DespawnStart()
	{
		attacksInARow = 0;
		doDespawn = true;
		StateSet(State.Degrow);
	}

	private void GrowDoneReset()
	{
		if (!growDone && currentState != State.Spawn && currentState != State.Degrow && currentState != State.Grow && currentState != State.AggroDegrow && currentState != State.AggroGrow)
		{
			growDone = true;
		}
	}

	private void ReleaseGrabLogic()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		bool flag = false;
		if ((currentState == State.Spawn || currentState == State.Degrow || currentState == State.Grow || currentState == State.AggroDegrow || currentState == State.AggroGrow) && enemy.Rigidbody.physGrabObject.playerGrabbing.Count > 0)
		{
			flag = true;
		}
		if (flag)
		{
			releaseGrabTimer += Time.deltaTime;
			if (releaseGrabTimer > 0.5f)
			{
				enemy.Rigidbody.GrabRelease(_effects: false, 1f);
				releaseGrabTimer = 0f;
			}
		}
		else
		{
			releaseGrabTimer -= Time.deltaTime;
		}
		releaseGrabTimer = Mathf.Max(releaseGrabTimer, 0f);
	}

	private void PlayLoopSoundLogic()
	{
		bool playing = currentState == State.Idle || currentState == State.Roam;
		soundIdleLoop.PlayLoop(playing, 1f, 3f);
		bool playing2 = currentState == State.Aggro;
		soundAggroLoop.PlayLoop(playing2, 1f, 3f);
		bool flag = currentState == State.Lure || currentState == State.ChompGasp || currentState == State.Chomp;
		if (flag && overrideVisualsToEnchanted)
		{
			flag = false;
		}
		soundGasLoop.PlayLoop(flag, 1f, 3f);
		bool playing3 = currentState == State.Stunned;
		soundStunLoop.PlayLoop(playing3, 1f, 3f);
		bool playing4 = overrideVisualsToEnchanted;
		soundEnchantedLoop.PlayLoop(playing4, 1f, 3f);
	}

	private void TurnSoundLogic()
	{
		if (SemiFunc.FPSImpulse5() && currentState != State.Spawn && currentState != State.Grow && currentState != State.Degrow && currentState != State.AggroGrow && currentState != State.AggroDegrow && currentState != State.Stunned && !overrideVisualsToEnchanted)
		{
			if (Quaternion.Angle(prevHeadLookRotation, headLook.rotation) > 30f && moveSoundHeadTimer <= 0f)
			{
				soundMoveHead.Play(headLook.position);
				prevHeadLookRotation = headLook.rotation;
				moveSoundHeadTimer = 0.2f;
			}
			if (Quaternion.Angle(prevRigidBodyRotation, rb.rotation) > 20f && moveSoundRigidBodyTimer <= 0f)
			{
				moveSoundRigidBodyTimer = 0.5f;
				soundMove.Play(followTarget.position);
				prevRigidBodyRotation = rb.rotation;
			}
			prevHeadLookRotation = headLook.rotation;
			prevRigidBodyRotation = rb.rotation;
		}
		if (moveSoundRigidBodyTimer > 0f)
		{
			moveSoundRigidBodyTimer -= Time.deltaTime;
		}
		if (moveSoundHeadTimer > 0f)
		{
			moveSoundHeadTimer -= Time.deltaTime;
		}
	}

	private void OverrideSpringValues(float _speed, float _damping)
	{
		overrideSpringValuesTimer = 0.1f;
		overrideSpringValuesSpeed = _speed;
		overrideSpringValuesDamping = _damping;
	}

	private void OverrideSpringValuesTick()
	{
		if (overrideSpringValuesTimer > 0f)
		{
			overrideSpringValuesTimer -= Time.deltaTime;
		}
	}

	private void OverrideActivateHorrorMeshes()
	{
		overrideActivateHorrorMeshesTimer = 0.05f;
	}

	private void OverrideActivateHorrorMeshesTick()
	{
		if (overrideActivateHorrorMeshesTimer <= 0f && horrorMeshesActive)
		{
			ActivateHorrorMeshes(_activate: false);
		}
		if (overrideActivateHorrorMeshesTimer > 0f)
		{
			if (!horrorMeshesActive)
			{
				ActivateHorrorMeshes(_activate: true);
			}
			overrideActivateHorrorMeshesTimer -= Time.deltaTime;
		}
	}

	private void OverrideVisualsToEnchantedLogic()
	{
		OverrideVisualToEnchantedForAllCapturedLocalPlayers();
		if (overrideVisualsToEnchantedTimer > 0f)
		{
			if (overrideVisualsToEnchantedTransitioning)
			{
				VisualStateNormalToEnchanting();
			}
			else
			{
				VisualStateEnchanting();
			}
		}
		else if (overrideVisualsToEnchantedTransitioning)
		{
			VisualStateEnchantingToNormal();
		}
	}

	private void OverrideVisualsToEnchanted()
	{
		if (!overrideVisualsToEnchanted)
		{
			overrideVisualsToEnchantedTransitioning = true;
			overrideVisualsToEnchanted = true;
			VisualPopAwayMeshesStart();
			particlesShatteredDream.Play(withChildren: true);
			overrideVisualsToEnchantedInstantTransition = false;
		}
		overrideVisualsToEnchantedTimer = 0.05f;
	}

	private void OverrideVisualsToEnchantedTick()
	{
		if (overrideVisualsToEnchantedTimer <= 0f && overrideVisualsToEnchanted)
		{
			overrideVisualsToEnchanted = false;
			overrideVisualsToEnchantedTransitioning = true;
			VisualPopBackMeshesStart();
		}
		if (overrideVisualsToEnchantedTimer > 0f)
		{
			overrideVisualsToEnchantedTimer -= Time.deltaTime;
			ParticlePlayHearts();
			if (currentState == State.ChompGasp)
			{
				VisualPopBackMeshesStart();
				overrideVisualsToEnchantedInstantTransition = true;
				overrideVisualsToEnchantedTimer = 0f;
				overrideVisualsToEnchantedTransitioning = true;
				overrideVisualsToEnchanted = false;
			}
		}
	}

	private void OverrideVisualToEnchantedForAllCapturedLocalPlayers()
	{
		if (currentState != State.Lure)
		{
			return;
		}
		foreach (PlayerAvatar item in playersInGasClient)
		{
			if ((bool)item && item.isLocal)
			{
				OverrideVisualsToEnchanted();
			}
		}
	}

	private void OverrideCanAggro()
	{
		canAggro = true;
		canAggroTimer = 0.05f;
	}

	private void OverrideCanAggroTick()
	{
		if (canAggroTimer <= 0f)
		{
			canAggro = false;
		}
		if (canAggroTimer > 0f)
		{
			canAggroTimer -= Time.deltaTime;
		}
	}

	private void ParticlePlayHearts()
	{
		if (particlePlayHeartsTimer <= 0f)
		{
			particlesHearts.Play(withChildren: true);
		}
		particlePlayHeartsTimer = 0.2f;
	}

	private void ParticlePlayHeartsTick()
	{
		if (particlePlayHeartsTimer <= 0f && particlesHearts.isPlaying)
		{
			particlesHearts.Stop(withChildren: true);
		}
		if (particlePlayHeartsTimer > 0f)
		{
			particlePlayHeartsTimer -= Time.deltaTime;
		}
	}

	private void ParticlePlayGas()
	{
		if (!(particlePlayHeartsTimer > 0f))
		{
			if (particlePlayGasTimer <= 0f)
			{
				gasParticles.Play(withChildren: true);
			}
			particlePlayGasTimer = 0.2f;
		}
	}

	private void ParticlePlayGasTick()
	{
		if (particlePlayGasTimer <= 0f && gasParticles.isPlaying)
		{
			gasParticles.Stop(withChildren: true);
		}
		if (particlePlayGasTimer > 0f)
		{
			particlePlayGasTimer -= Time.deltaTime;
		}
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			StateSet(State.Spawn);
		}
	}

	public void OnHurt()
	{
		Vector3 nudgeAmount = rb.velocity;
		if (nudgeAmount.magnitude > 5f)
		{
			nudgeAmount = nudgeAmount.normalized * 5f;
		}
		NudgeAllSprings(nudgeAmount);
		soundHurt.Play(headCenterTransform.position);
	}

	public void OnDeath()
	{
		GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(4f, 3f, 8f, base.transform.position, 0.1f);
		foreach (ParticleSystem item in particlesDeath)
		{
			item.gameObject.SetActive(value: true);
		}
		soundDeath.Play(headCenterTransform.position);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnVision()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && canAggro)
		{
			currentTarget = enemy.Vision.onVisionTriggeredPlayer;
			hasTarget = currentTarget;
			if (GameManager.Multiplayer() && (bool)currentTarget)
			{
				photonView.RPC("UpdateCurrentTargetRPC", RpcTarget.All, currentTarget.photonView.ViewID);
			}
			if (currentState != State.Aggro)
			{
				StateSet(State.AggroStart);
			}
		}
	}

	public void OnInvestigate()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || (currentState != State.Idle && currentState != State.Roam))
		{
			return;
		}
		investigatePosition = enemy.StateInvestigate.onInvestigateTriggeredPosition;
		if (Vector3.Distance(base.transform.position, investigatePosition) > 10f)
		{
			StateSet(State.Degrow);
			return;
		}
		if ((investigatePosition - base.transform.position).magnitude > 0.01f)
		{
			base.transform.rotation = Quaternion.LookRotation(investigatePosition - base.transform.position);
			base.transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
		}
		turnRandomTimer = UnityEngine.Random.Range(turnRandomTimerMin, turnRandomTimerMax);
	}
}
