using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class EnemyOogly : MonoBehaviourPunCallbacks, IPunObservable
{
	public enum State
	{
		Spawn,
		AppearAtLevelPoint,
		Leave,
		FindCeiling,
		CeilingRoam,
		PlayerSpotted,
		Dive,
		WrestlePlayer,
		Stunned,
		TeleportOut,
		TeleportIn,
		Despawn
	}

	[Serializable]
	public class SpringTransform
	{
		public string name;

		public Transform transform;

		public Vector3 originalLocalPosition;

		public Vector3 overridePosition;

		public float overrideTimer;

		public SpringVector3 spring;

		public SpringTransform(string _name, Transform _transform)
		{
			name = _name;
			transform = _transform;
			if (transform != null)
			{
				originalLocalPosition = transform.localPosition;
			}
			spring = new SpringVector3();
			spring.damping = 0.3f;
			spring.speed = 8f;
			overrideTimer = 0f;
		}
	}

	[Serializable]
	public class Vector3Springs
	{
		public List<SpringTransform> springTransforms = new List<SpringTransform>();

		public void AddTransform(string _name, Transform _transform)
		{
			if (!(_transform == null) && GetSpringTransform(_transform) == null)
			{
				SpringTransform item = new SpringTransform(_name, _transform);
				springTransforms.Add(item);
			}
		}

		public void RemoveTransform(Transform _transform)
		{
			if (!(_transform == null))
			{
				SpringTransform springTransform = GetSpringTransform(_transform);
				if (springTransform != null)
				{
					springTransforms.Remove(springTransform);
				}
			}
		}

		public SpringTransform GetSpringTransform(Transform _transform)
		{
			for (int i = 0; i < springTransforms.Count; i++)
			{
				if (springTransforms[i].transform == _transform)
				{
					return springTransforms[i];
				}
			}
			return null;
		}

		public SpringTransform GetSpringTransformByName(string _name)
		{
			for (int i = 0; i < springTransforms.Count; i++)
			{
				if (springTransforms[i].name == _name)
				{
					return springTransforms[i];
				}
			}
			return null;
		}

		public void SetOverridePosition(Transform _transform, Vector3 _position, float _duration)
		{
			SpringTransform springTransform = GetSpringTransform(_transform);
			if (springTransform != null)
			{
				springTransform.overridePosition = _position;
				springTransform.overrideTimer = _duration;
			}
		}

		public void SetOverridePositionByName(string _name, Vector3 _position, float _duration)
		{
			SpringTransform springTransformByName = GetSpringTransformByName(_name);
			if (springTransformByName != null)
			{
				springTransformByName.overridePosition = _position;
				springTransformByName.overrideTimer = _duration;
			}
		}

		public void SetOverridePositionAll(Vector3 _position, float _duration)
		{
			for (int i = 0; i < springTransforms.Count; i++)
			{
				springTransforms[i].overridePosition = _position;
				springTransforms[i].overrideTimer = _duration;
			}
		}

		public void Update()
		{
			for (int i = 0; i < springTransforms.Count; i++)
			{
				SpringTransform springTransform = springTransforms[i];
				if (!(springTransform.transform == null))
				{
					if (springTransform.overrideTimer > 0f)
					{
						springTransform.overrideTimer -= Time.deltaTime;
					}
					Vector3 targetPosition = springTransform.originalLocalPosition;
					if (springTransform.overrideTimer > 0f)
					{
						targetPosition = springTransform.overridePosition;
					}
					springTransform.transform.localPosition = SemiFunc.SpringVector3Get(springTransform.spring, targetPosition);
				}
			}
		}

		public void ResetAll()
		{
			for (int i = 0; i < springTransforms.Count; i++)
			{
				springTransforms[i].overrideTimer = 0f;
				if (springTransforms[i].transform != null)
				{
					springTransforms[i].transform.localPosition = springTransforms[i].originalLocalPosition;
				}
			}
		}

		public void GetOriginalPositions()
		{
			for (int i = 0; i < springTransforms.Count; i++)
			{
				if (springTransforms[i].transform != null)
				{
					springTransforms[i].originalLocalPosition = springTransforms[i].transform.localPosition;
				}
			}
		}

		public void ResetOriginalPositions()
		{
			for (int i = 0; i < springTransforms.Count; i++)
			{
				if (springTransforms[i].transform != null)
				{
					springTransforms[i].originalLocalPosition = springTransforms[i].transform.localPosition;
				}
			}
		}
	}

	private static readonly int emissionColorID = Shader.PropertyToID("_EmissionColor");

	private static readonly int fresnelAmountID = Shader.PropertyToID("_FresnelAmount");

	private static readonly int MainTex = Shader.PropertyToID("_MainTex");

	public Transform visionTransform;

	public Transform visualTransform;

	public Transform teleportTransform;

	public Light visionSpotlight;

	public Light visionPointLight;

	public float pointLightMaxIntensity = 5f;

	public AnimationCurve animationCurveSpotlightIntro;

	public AnimationCurve animationCurveSpotlightOutro;

	public float spotlightIntroDuration = 0.3f;

	public float spotlightOutroDuration = 0.3f;

	public float spotlightMaxIntensity = 8f;

	public AnimationCurve animationCurveTeleportOut;

	public AnimationCurve animationCurveTeleportIn;

	[Space]
	public State currentState;

	private bool stateStart;

	private float stateTimer;

	private float stateTimerMax;

	[Space]
	private PlayerAvatar targetPlayer;

	private PlayerAvatar previousTargetPlayer;

	private PlayerAvatar grabbedPlayer;

	private bool hasTarget;

	private PlayerAvatar lastAttackedPlayer;

	private float attackCooldownTimer;

	private float attackCooldownDuration;

	private int attacksRemaining;

	private int attacksMax;

	private bool visionPrevious;

	private float visionPreviousTime;

	[Space]
	private Vector3 investigatePosition;

	private bool doLeave;

	private bool didLeave;

	[Space]
	private List<Vector3> ceilingRoamPoints = new List<Vector3>();

	private LevelPoint currentLevelPoint;

	private Vector3 currentLevelPointPosition;

	private Vector3 ceilingPoint;

	private List<Vector3> roamPoints = new List<Vector3>();

	private Vector3 currentRoamTarget;

	private int currentRoamIndex;

	private Vector3 teleportDestination;

	private State teleportReturnState;

	private List<Vector3> debugFailedRoamPoints = new List<Vector3>();

	private List<string> debugFailReasons = new List<string>();

	private float stuckTimer;

	private Vector3 lastStuckPos;

	private int findCeilingAttempts;

	private bool justSpawned;

	private bool firstSpawn = true;

	private Vector3 lastTeleportOutPosition = Vector3.zero;

	private bool hasPlayedTeleportInEffects;

	[Space]
	public Rigidbody rb;

	public Transform followTarget;

	public Vector3 followTargetStartPosition;

	private Vector3 roamDestination;

	private float roamReachDistance = 1.5f;

	public EnemyRigidbody enemyRigidbody;

	[Header("Flight System")]
	public float moveForce = 1f;

	public float turnSmoothness = 2f;

	public float minFlightHeight = 5f;

	public float maxFlightHeight = 25f;

	public float preferredHeight = 15f;

	public BotSystemFlight flightSystem;

	public BotSystemSpringPoseAnimator handPose;

	public BotSystemSpringPoseAnimator tailPose;

	public BotPhysicsController physicsController;

	public Renderer tailMeshRenderer;

	public TrailRenderer tailTrailRenderer;

	private float tailScrollTimer;

	private float tailScrollSpeed = 2f;

	private Vector2 tailScrollOffset;

	private Material tailMaterial;

	public GameObject grabbedPlayerTrailObject;

	public TrailRenderer grabbedPlayerTrailRenderer;

	public Renderer grabbedPlayerTrailMeshRenderer;

	private float grabbedPlayerTrailScrollTimer;

	private float grabbedPlayerTrailScrollSpeed = 2f;

	private Vector2 grabbedPlayerTrailScrollOffset;

	private Material grabbedPlayerTrailMaterial;

	[Header("Environment")]
	public LayerMask environmentMask = -1;

	public float ceilingClearance = 0.6f;

	public float roamSphereRadius = 0.45f;

	public float minCeilingHeightDiff = 2.25f;

	[Header("Audio")]
	public Sound audioIdleLoop;

	public Sound audioWrestleLoop;

	public Sound audioOoglyLoop;

	public Sound audioFastFlight;

	public Sound audioSeePlayer;

	public Sound audioSeePlayerGlobal;

	public Sound audioStartCharge;

	public Sound audioGrabPlayer;

	public Sound audioDropPlayer;

	public Sound audioDeath;

	public Sound audioIdleBreaker;

	public Sound audioHurt;

	public Sound audioTeleportOut;

	public Sound audioTeleportIn;

	public Sound audioSpotlightOn;

	public Sound audioSpotlightOff;

	public Sound audioHitPlayer;

	public List<Sound> voList;

	public AudioSource mainAudioSource;

	private float[] audioSourceSpectrum = new float[1024];

	private Vector3 previousPosition;

	private float fakeVelocity;

	private float audioIdleLoopTimer;

	private float audioWrestleLoopTimer;

	private float audioOoglyLoopTimer;

	private float audioFastFlightLoopTimer;

	private float randomVOCooldown;

	private float teleportLoopTimer;

	private float spotlightOnTimer;

	private float spotlightCurrentIntensity;

	private float spotlightFadeTimer;

	private bool spotlightFadingIn;

	private bool playerInRange;

	[Header("Spotlight Visual")]
	public List<Renderer> emissionRenderers = new List<Renderer>();

	public List<ParticleSystem> particlesSpotlight = new List<ParticleSystem>();

	private List<Color> originalEmissionColors = new List<Color>();

	private List<Material> emissionMaterials = new List<Material>();

	private Color originalSpotlightColor;

	[Header("Evil Eyes Override")]
	public ParticleSystem particlesEvilEyes;

	public List<ParticleSystem> particlesSpotlightEvil = new List<ParticleSystem>();

	private float evilEyesTimer;

	private bool isEvilMode;

	[Header("Fresnel Animation")]
	public List<Renderer> fresnelRenderers = new List<Renderer>();

	private List<Material> fresnelMaterials = new List<Material>();

	private float fresnelOverrideTimer;

	private float fresnelCurrentValue = 0.031f;

	private float fresnelDefaultValue = 0.031f;

	private float fresnelAnimationSpeed = 3f;

	[Header("Jaw Animation")]
	public Transform jawTransform;

	private Quaternion jawStartRotation;

	private Quaternion jawTargetRotation;

	private SpringQuaternion jawSpring;

	private float jawOpen;

	private float talkVolume;

	[Header("Follow Target Spring")]
	private SpringQuaternion followTargetRotationSpring;

	public float followTargetSpringDamping = 0.3f;

	public float followTargetSpringSpeed = 8f;

	[Header("Head Spring")]
	public Transform headTransform;

	public Transform headLookTargetTransform;

	private SpringQuaternion headSpring;

	private Quaternion headStartRotation;

	public float headSpringDamping = 0.3f;

	public float headSpringSpeed = 8f;

	public float headLookDownAngle = 45f;

	private float headLookDownTimer;

	private float headLookAtTargetTimer;

	private float headStunnedLookDownTimer;

	private Vector3 headTwitchOffset;

	private float headTwitchUpdateTimer;

	private float headSpringOverrideDamping;

	private float headSpringOverrideSpeed;

	private float headSpringOverrideTimer;

	private bool headSpringOverrideActive;

	private Vector3 investigatePointLookAtPosition;

	private float lookAtInvestigatePointTimer;

	private float headLookAtPositionTimer;

	private Quaternion headLookAtPositionTarget = Quaternion.identity;

	private Quaternion headWorldTargetRotation = Quaternion.identity;

	[Header("Vector3 Springs")]
	public Vector3Springs vector3Springs = new Vector3Springs();

	[Header("Hoover Hands")]
	private float hooverHandsTimer;

	private float hooverHandsWidth = 0.2f;

	private float hooverHandsHeight = 0.1f;

	private float hooverHandsSpeed = 1f;

	[Header("Eye Animation")]
	public List<Transform> eyes = new List<Transform>();

	private List<SpringVector3> eyeScaleSprings = new List<SpringVector3>();

	[Header("Effects")]
	public List<ParticleSystem> particlesDeath;

	public List<ParticleSystem> particlesTeleportOut;

	public List<ParticleSystem> particlesTeleportIn;

	public List<ParticleSystem> particlesTeleportLoop;

	public List<ParticleSystem> particlesTeleportEnd;

	public List<ParticleSystem> particlesHitPlayer;

	[Header("Combat")]
	public HurtCollider hurtCollider;

	private float hurtColliderTimer;

	private float wrestleOutOfRangeTimer;

	private float wrestleLineOfSightLostTimer;

	private int leaveCyclesRemaining;

	private bool isAnimatedDespawn;

	internal Enemy enemy;

	private void Start()
	{
		environmentMask = LayerMask.GetMask("Default");
		followTargetStartPosition = followTarget.localPosition;
		enemy = GetComponent<Enemy>();
		tailMaterial = tailMeshRenderer.material;
		grabbedPlayerTrailMaterial = grabbedPlayerTrailMeshRenderer.material;
		hurtCollider.gameObject.SetActive(value: false);
		InitializeFlightSystem();
		InitializeJawAnimation();
		InitializeHeadSpring();
		InitializeEmissionMaterials();
		InitializeFresnelMaterials();
		vector3Springs.GetOriginalPositions();
		randomVOCooldown = UnityEngine.Random.Range(1f, 1f);
		currentState = State.Spawn;
		stateStart = true;
		foreach (ParticleSystem item in particlesTeleportLoop)
		{
			item.Stop(withChildren: true);
		}
	}

	private void TurnOffAllLoopingParticles()
	{
		foreach (ParticleSystem item in particlesSpotlight)
		{
			item.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
		}
		particlesEvilEyes.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
		foreach (ParticleSystem item2 in particlesSpotlightEvil)
		{
			item2.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
		}
		foreach (ParticleSystem item3 in particlesTeleportLoop)
		{
			item3.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		TurnOffAllLoopingParticles();
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		followTarget.position = rb.position;
		followTarget.rotation = rb.rotation;
		if (currentState != State.Stunned)
		{
			enemyRigidbody.DeactivateFollowTargetPhysics(0.2f);
			enemyRigidbody.DeactivateCustomGravity(0.2f);
			enemyRigidbody.physGrabObject.OverrideZeroGravity(0.2f);
		}
		StateMachine();
		UpdateFakeVelocity();
		CodeAnimatedTalk();
		PlayLoopSoundLogic();
		UpdateVisionSpotlight();
		UpdateTeleportParticles();
		UpdateHeadSpring();
		UpdateHooverHands();
		UpdateHurtColliderTimer();
		UpdateEvilEyesTimer();
		UpdateFresnel();
		UpdateTailScroll();
		UpdateGrabbedPlayerTrailScroll();
		vector3Springs.Update();
		if (stateTimer <= stateTimerMax)
		{
			stateTimer += Time.deltaTime;
		}
		if (attackCooldownTimer > 0f)
		{
			attackCooldownTimer -= Time.deltaTime;
		}
		if (lookAtInvestigatePointTimer > 0f)
		{
			lookAtInvestigatePointTimer -= Time.deltaTime;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (enemy.IsStunned())
			{
				StateSet(State.Stunned);
			}
			else if (enemy.CurrentState == EnemyState.Despawn && (currentState == State.CeilingRoam || currentState == State.FindCeiling))
			{
				AnimatedDespawn();
			}
		}
	}

	private void FixedUpdate()
	{
		if (currentState == State.WrestlePlayer && grabbedPlayer != null && !grabbedPlayer.isDisabled)
		{
			WrestlePlayerPhysics();
		}
	}

	private void LateUpdate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && headTransform != null)
		{
			SetHeadTargetRotation();
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(headWorldTargetRotation);
		}
		else
		{
			headWorldTargetRotation = (Quaternion)stream.ReceiveNext();
		}
	}

	private void CreateRoamPoints(Vector3 startPoint)
	{
		float num = 2f;
		float num2 = 16f;
		float num3 = 16f;
		int num4 = 200;
		int num5 = 100;
		roamPoints.Clear();
		debugFailedRoamPoints.Clear();
		debugFailReasons.Clear();
		int num6 = 0;
		while (roamPoints.Count < num5 && num6 < num4)
		{
			num6++;
			float num7 = UnityEngine.Random.Range(0f - num2, num2);
			float num8 = UnityEngine.Random.Range(0f - num3, num3);
			Vector3 vector = new Vector3(startPoint.x + num7, startPoint.y, startPoint.z + num8);
			if (!Physics.SphereCast(vector + Vector3.down * 0.1f, roamSphereRadius, Vector3.up, out var hitInfo, 6f, environmentMask, QueryTriggerInteraction.Ignore))
			{
				debugFailedRoamPoints.Add(vector);
				debugFailReasons.Add("noCeilingAbove");
				continue;
			}
			Vector3 vector2 = hitInfo.point + hitInfo.normal * ceilingClearance;
			float num9 = 8f;
			if (vector2.y - startPoint.y > num9)
			{
				vector2.y = startPoint.y + num9;
			}
			if (Physics.CheckSphere(vector2, roamSphereRadius, environmentMask, QueryTriggerInteraction.Ignore))
			{
				debugFailedRoamPoints.Add(vector2);
				debugFailReasons.Add("overlap");
				continue;
			}
			if (Physics.Linecast(startPoint, vector2, environmentMask, QueryTriggerInteraction.Ignore))
			{
				debugFailedRoamPoints.Add(vector2);
				debugFailReasons.Add("blocked");
				continue;
			}
			bool flag = true;
			for (int i = 0; i < roamPoints.Count; i++)
			{
				if (Vector3.Distance(vector2, roamPoints[i]) < num)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				debugFailedRoamPoints.Add(vector2);
				debugFailReasons.Add("tooClose");
			}
			else
			{
				roamPoints.Add(vector2);
			}
		}
	}

	private bool FindHighestCeilingPoint()
	{
		int num = 8;
		float maxDistance = 20f;
		float num2 = 0.25f;
		Vector3 vector = currentLevelPointPosition + Vector3.up * num2;
		bool flag = false;
		Vector3 vector2 = Vector3.zero;
		float num3 = float.NegativeInfinity;
		for (int i = 0; i < num; i++)
		{
			float f = 360f / (float)num * (float)i * (MathF.PI / 180f);
			Vector3 vector3 = new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f)) * 0.5f;
			if (Physics.SphereCast(vector + vector3, roamSphereRadius, Vector3.up, out var hitInfo, maxDistance, environmentMask, QueryTriggerInteraction.Ignore))
			{
				Vector3 vector4 = hitInfo.point + hitInfo.normal * ceilingClearance;
				if (!Physics.CheckSphere(vector4, roamSphereRadius, environmentMask, QueryTriggerInteraction.Ignore) && vector4.y > num3)
				{
					num3 = vector4.y;
					vector2 = vector4;
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		if (num3 - currentLevelPointPosition.y < minCeilingHeightDiff)
		{
			return false;
		}
		ceilingPoint = vector2;
		return true;
	}

	public void FindLevelPointAndCreateCeilingRoamPoints()
	{
		List<LevelPoint> list = new List<LevelPoint>();
		if (SemiFunc.EnemyDirectorEndingHeadToTruck())
		{
			list = SemiFunc.LevelPointsGetPlayerDistance(rb.position, 10f, 999f, _startRoomOnly: true);
		}
		else if (doLeave)
		{
			list = SemiFunc.LevelPointsGetPlayerDistance(rb.position, 30f, 999f);
			doLeave = false;
			didLeave = true;
		}
		else if (firstSpawn && (bool)enemy.EnemyParent.firstSpawnPoint)
		{
			list = SemiFunc.LevelPointGetWithinDistance(enemy.EnemyParent.firstSpawnPoint.transform.position, 0f, 10f);
		}
		else if (investigatePosition != Vector3.zero)
		{
			LevelPointsInvestigate();
			list = SemiFunc.LevelPointGetWithinDistance(investigatePosition, 0f, 5f);
			investigatePosition = Vector3.zero;
		}
		else
		{
			float num = 16f;
			PlayerAvatar playerAvatar = SemiFunc.PlayerGetNearestPlayerAvatarWithinRange(num, rb.position);
			if ((bool)playerAvatar)
			{
				list = SemiFunc.LevelPointGetWithinDistance(playerAvatar.transform.position, 6f, num);
			}
		}
		if (list == null || list.Count == 0)
		{
			list = SemiFunc.LevelPointGetWithinDistance(rb.position, 8f, 22f);
			if (list == null || list.Count == 0)
			{
				list = SemiFunc.LevelPointsGetAll().ToList();
			}
		}
		foreach (LevelPoint item in list.ToList())
		{
			foreach (PlayerAvatar item2 in SemiFunc.PlayerGetList())
			{
				if (Vector3.Distance(item2.transform.position, item.transform.position) < 8f)
				{
					list.Remove(item);
				}
			}
		}
		int num2 = 5;
		for (int i = 0; i < num2; i++)
		{
			if (list.Count == 0)
			{
				break;
			}
			int index = UnityEngine.Random.Range(0, list.Count);
			if (firstSpawn && list.Contains(enemy.EnemyParent.firstSpawnPoint))
			{
				index = list.IndexOf(enemy.EnemyParent.firstSpawnPoint);
				firstSpawn = false;
			}
			LevelPoint levelPoint = list[index];
			list.RemoveAt(index);
			currentLevelPoint = levelPoint;
			currentLevelPointPosition = levelPoint.transform.position;
			if (FindHighestCeilingPoint())
			{
				CreateRoamPoints(ceilingPoint);
				ceilingRoamPoints = new List<Vector3>(roamPoints);
				firstSpawn = false;
				return;
			}
		}
		HandleFallbackBehavior();
	}

	private void HandleFallbackBehavior()
	{
		firstSpawn = false;
	}

	private void PhysicsRoamMovementBehavior()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !(physicsController == null) && !(roamDestination == Vector3.zero))
		{
			Vector3 normalized = (roamDestination - rb.position).normalized;
			float num = Vector3.Distance(rb.position, roamDestination);
			float num2 = Mathf.Clamp01((Vector3.Dot(rb.transform.forward, normalized) + 1f) / 2f);
			float value = Vector3.Dot(rb.transform.forward, normalized);
			float num3 = 10f;
			float spring = Mathf.Lerp(3f, num3, Mathf.Clamp01(value));
			float magnitude = rb.velocity.magnitude;
			float num4 = 2.5f;
			float num5 = Mathf.Clamp01(magnitude / num4);
			float num6 = Mathf.Clamp01(num / 10f);
			Vector3 forward = rb.transform.forward;
			float speed = moveForce * num2 * num6 * (1f - num5 * 0.95f);
			physicsController.PhysMoveTowards(forward, speed, 0.2f);
			physicsController.PhysRotateTowards(normalized, spring, 0.001f, 0.2f);
			physicsController.SetDragOverride("roam", 2.5f, 0.2f);
			if (rb.velocity.magnitude > 0.1f)
			{
				LookAtVelocityDirection(_moving: true);
			}
			else
			{
				FloatAround();
			}
		}
	}

	private void LookAtVelocityDirection(bool _moving)
	{
		if (!(followTarget == null) && !(rb == null))
		{
			Vector3 normalized = rb.velocity.normalized;
			if (_moving && normalized.sqrMagnitude > float.Epsilon)
			{
				Quaternion quaternion = Quaternion.LookRotation(normalized, Vector3.up);
				followTarget.rotation = Quaternion.Slerp(followTarget.rotation, quaternion, Time.deltaTime * 2f);
			}
		}
	}

	private void FloatAround()
	{
		if (!(followTarget == null))
		{
			followTarget.localPosition = followTargetStartPosition + Vector3.up * Mathf.Sin(Time.time * 0.4f) * 0.3f;
			followTarget.localPosition += Vector3.right * Mathf.Sin(Time.time * 0.3f) * 0.2f;
			followTarget.localPosition += Vector3.forward * Mathf.Sin(Time.time * 0.25f) * 0.4f;
		}
	}

	private void RandomDirectionalNudge(float _force)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !(rb == null))
		{
			Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
			insideUnitSphere.y = Mathf.Abs(insideUnitSphere.y) * 0.5f;
			rb.AddForce(insideUnitSphere * _force, ForceMode.Impulse);
		}
	}

	private bool HasReachedDestination(Vector3 destination)
	{
		return Vector3.Distance(rb.position, destination) < roamReachDistance;
	}

	private bool VisionBlocked()
	{
		if (Time.time - visionPreviousTime > 0.2f && (bool)targetPlayer)
		{
			visionPreviousTime = Time.time;
			Vector3 direction = targetPlayer.transform.position - visionTransform.position;
			visionPrevious = Physics.Raycast(visionTransform.position, direction, direction.magnitude, environmentMask, QueryTriggerInteraction.Ignore);
		}
		return visionPrevious;
	}

	private void ActivateHurtCollider()
	{
		hurtColliderTimer = 0.1f;
		if (hurtCollider != null && !hurtCollider.gameObject.activeSelf)
		{
			hurtCollider.gameObject.SetActive(value: true);
		}
	}

	private void UpdateHurtColliderTimer()
	{
		if (hurtColliderTimer <= 0f && hurtCollider.gameObject.activeSelf)
		{
			hurtCollider.gameObject.SetActive(value: false);
		}
		if (hurtColliderTimer > 0f)
		{
			hurtColliderTimer -= Time.deltaTime;
		}
	}

	private void UpdateEvilEyesTimer()
	{
		if (evilEyesTimer <= 0f)
		{
			isEvilMode = false;
		}
		if (evilEyesTimer > 0f)
		{
			evilEyesTimer -= Time.deltaTime;
			if ((bool)targetPlayer && targetPlayer.isLocal)
			{
				CameraAim.Instance.AimTargetSet(visionTransform.position, 0.1f, 5f, base.gameObject, 90);
			}
		}
	}

	private void InitializeFresnelMaterials()
	{
		fresnelMaterials.Clear();
		foreach (Renderer fresnelRenderer in fresnelRenderers)
		{
			if (fresnelRenderer != null && fresnelRenderer.material != null)
			{
				Material material = fresnelRenderer.material;
				fresnelMaterials.Add(material);
			}
		}
	}

	private void OverrideTurnOffFresnel(float duration)
	{
		fresnelOverrideTimer = duration;
	}

	private void UpdateFresnel()
	{
		_ = fresnelOverrideTimer;
		_ = 0f;
		if (fresnelOverrideTimer > 0f)
		{
			fresnelOverrideTimer -= Time.deltaTime;
		}
		float num = ((fresnelOverrideTimer > 0f) ? 0f : fresnelDefaultValue);
		fresnelCurrentValue = Mathf.Lerp(fresnelCurrentValue, num, Time.deltaTime * fresnelAnimationSpeed);
		foreach (Material fresnelMaterial in fresnelMaterials)
		{
			fresnelMaterial.SetFloat(fresnelAmountID, fresnelCurrentValue);
			fresnelMaterial.SetFloat(enemy.Health.materialHurtAmount, enemy.Health.hurtCurve.Evaluate(enemy.Health.hurtLerp));
		}
	}

	private void DecideAttackCount()
	{
		if (UnityEngine.Random.Range(0, 5) == 0)
		{
			attacksMax = 3;
		}
		else
		{
			attacksMax = 1;
		}
		attacksRemaining = attacksMax;
	}

	private bool CheckSinglePlayerNearby()
	{
		int num = 0;
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item != null && !item.isDisabled && Vector3.Distance(rb.position, item.transform.position) < 30f)
			{
				num++;
				if (num > 1)
				{
					return false;
				}
			}
		}
		return num == 1;
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			DecideAttackCount();
			if (SemiFunc.EnemySpawn(enemy))
			{
				StateSet(State.Spawn);
			}
		}
	}

	public void OnVision()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !hasTarget && currentState == State.CeilingRoam)
		{
			PlayerAvatar onVisionTriggeredPlayer = enemy.Vision.onVisionTriggeredPlayer;
			if (!(onVisionTriggeredPlayer == lastAttackedPlayer) || !(attackCooldownTimer > 0f))
			{
				SetTarget(onVisionTriggeredPlayer);
				StateSet(State.PlayerSpotted);
			}
		}
	}

	private void SetTarget(PlayerAvatar newTarget)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && newTarget != previousTargetPlayer)
		{
			previousTargetPlayer = newTarget;
			targetPlayer = newTarget;
			hasTarget = newTarget != null;
			if (GameManager.Multiplayer() && newTarget != null)
			{
				base.photonView.RPC("SetTargetRPC", RpcTarget.All, newTarget.photonView.ViewID);
			}
			else if (GameManager.Multiplayer() && newTarget == null)
			{
				base.photonView.RPC("SetTargetRPC", RpcTarget.All, -1);
			}
		}
	}

	[PunRPC]
	private void SetTargetRPC(int photonId, PhotonMessageInfo info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(info))
		{
			return;
		}
		if (photonId == -1)
		{
			targetPlayer = null;
			previousTargetPlayer = null;
			hasTarget = false;
			return;
		}
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item.photonView.ViewID == photonId)
			{
				targetPlayer = item;
				previousTargetPlayer = item;
				hasTarget = true;
				break;
			}
		}
	}

	private void FindPlayerToAttack()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		List<PlayerAvatar> list = SemiFunc.PlayerGetAllPlayerAvatarWithinRangeAndVision(20f, visionTransform.position, enemyRigidbody.physGrabObject);
		if (list.Count > 0)
		{
			PlayerAvatar playerAvatar = list[0];
			float num = Vector3.Distance(rb.position, playerAvatar.transform.position);
			for (int i = 1; i < list.Count; i++)
			{
				float num2 = Vector3.Distance(rb.position, list[i].transform.position);
				if (num2 < num)
				{
					num = num2;
					playerAvatar = list[i];
				}
			}
			SetTarget(playerAvatar);
			StateSet(State.PlayerSpotted);
		}
		else
		{
			StateSet(State.FindCeiling);
		}
	}

	public void OnHurt()
	{
		audioHurt.Play(rb.position);
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.CeilingRoam)
		{
			FindPlayerToAttack();
		}
	}

	public void StunnedEnd()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.StateStunned.OverrideDisable(0.2f);
			FindPlayerToAttack();
		}
	}

	public void OnInvestigate()
	{
		Vector3 onInvestigateTriggeredPosition = enemy.StateInvestigate.onInvestigateTriggeredPosition;
		float num = Vector3.Distance(rb.position, onInvestigateTriggeredPosition);
		if (num < 20f && onInvestigateTriggeredPosition.y < visionTransform.position.y)
		{
			investigatePointLookAtPosition = onInvestigateTriggeredPosition;
			lookAtInvestigatePointTimer = 4f;
		}
		if (currentState == State.CeilingRoam && SemiFunc.PlayerGetAllPlayerAvatarWithinRange(16f, rb.position).Count == 0)
		{
			investigatePosition = onInvestigateTriggeredPosition;
			if (num >= 20f)
			{
				StateSet(State.FindCeiling);
			}
		}
	}

	public void OnHitPlayer()
	{
		if (SemiFunc.IsMultiplayer())
		{
			base.photonView.RPC("OnHitPlayerRPC", RpcTarget.All);
		}
		else
		{
			OnHitPlayerRPC();
		}
	}

	[PunRPC]
	private void OnHitPlayerRPC()
	{
		if (!SemiFunc.Photosensitivity())
		{
			foreach (ParticleSystem item in particlesHitPlayer)
			{
				if (item != null)
				{
					item.Play(withChildren: true);
				}
			}
		}
		audioHitPlayer.Play(rb.position);
	}

	private void StateMachine()
	{
		switch (currentState)
		{
		case State.Spawn:
			StateSpawn();
			break;
		case State.AppearAtLevelPoint:
			StateAppearAtLevelPoint();
			break;
		case State.Leave:
			StateLeave();
			break;
		case State.FindCeiling:
			StateFindCeiling();
			break;
		case State.CeilingRoam:
			StateCeilingRoam();
			break;
		case State.PlayerSpotted:
			StatePlayerSpotted();
			break;
		case State.Dive:
			StateDive();
			break;
		case State.WrestlePlayer:
			StateWrestlePlayer();
			break;
		case State.Stunned:
			StateStunned();
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

	public void StateSet(State newState)
	{
		if (currentState != newState && SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				base.photonView.RPC("StateSetRPC", RpcTarget.All, (int)newState);
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
				stateStart = true;
				currentState = (State)_newState;
			}
		}
	}

	private void StateSpawn()
	{
		if (stateStart)
		{
			stateTimerMax = 2f;
			stateTimer = 0f;
			StateStartResets();
			stateStart = false;
			teleportTransform.localScale = Vector3.zero;
			justSpawned = true;
			hasPlayedTeleportInEffects = false;
			lastTeleportOutPosition = Vector3.zero;
			StateSet(State.FindCeiling);
		}
	}

	private void StateAppearAtLevelPoint()
	{
		if (stateStart)
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				stateTimerMax = UnityEngine.Random.Range(1f, 3f);
			}
			stateTimer = 0f;
			StateStartResets();
			LevelPointsIdleRoam();
			if (currentLevelPointPosition != Vector3.zero)
			{
				rb.position = currentLevelPointPosition + Vector3.up * 0.5f;
				roamDestination = currentLevelPointPosition;
			}
			stateStart = false;
		}
		PlayIdleLoop();
		FloatAround();
		if (SemiFunc.IsMasterClientOrSingleplayer() && stateTimer >= stateTimerMax)
		{
			StateSet(State.FindCeiling);
		}
	}

	private void StateFindCeiling()
	{
		if (stateStart)
		{
			stateTimerMax = 0.001f;
			stateTimer = 0f;
			stateStart = false;
			StateStartResets();
			findCeilingAttempts++;
			FindLevelPointAndCreateCeilingRoamPoints();
		}
		PlayIdleLoop();
		enemy.StateStunned.OverrideDisable(0.2f);
		if (ceilingRoamPoints.Count > 0)
		{
			findCeilingAttempts = 0;
			if (!(ceilingPoint != Vector3.zero) || !(enemy != null))
			{
				return;
			}
			if (followTarget != null)
			{
				followTarget.localPosition = followTargetStartPosition;
				followTarget.rotation = Quaternion.identity;
			}
			teleportDestination = ceilingPoint - Vector3.up * 2f;
			teleportReturnState = State.CeilingRoam;
			if (justSpawned)
			{
				justSpawned = false;
				if (SemiFunc.IsMasterClientOrSingleplayer())
				{
					enemy.EnemyTeleported(teleportDestination);
				}
				StateSet(State.TeleportIn);
			}
			else
			{
				StateSet(State.TeleportOut);
			}
		}
		else if (stateTimer >= stateTimerMax)
		{
			if (findCeilingAttempts >= 3)
			{
				AnimatedDespawn();
				return;
			}
			stateStart = true;
			stateTimer = 0f;
		}
	}

	private void StateCeilingRoam()
	{
		if (stateStart)
		{
			StateStartResets();
			hasTarget = false;
			targetPlayer = null;
			previousTargetPlayer = null;
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				SetTarget(null);
			}
			if (didLeave)
			{
				didLeave = false;
				stateTimerMax = UnityEngine.Random.Range(60f, 120f);
			}
			else if (SemiFunc.EnemyDirectorEndingHeadToTruck() && !currentLevelPoint.inStartRoom)
			{
				stateTimerMax = UnityEngine.Random.Range(1f, 3f);
			}
			else if (!SemiFunc.EnemyDirectorEndingHeadToPlayers() || (bool)SemiFunc.PlayerGetNearestPlayerAvatarWithinRange(30f, rb.position))
			{
				stateTimerMax = UnityEngine.Random.Range(60f, 120f);
			}
			else
			{
				stateTimerMax = UnityEngine.Random.Range(10f, 30f);
			}
			stateTimer = 0f;
			currentRoamIndex = 0;
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				if (ceilingRoamPoints.Count == 0)
				{
					StateSet(State.FindCeiling);
					return;
				}
				currentRoamTarget = ceilingRoamPoints[currentRoamIndex];
				roamDestination = currentRoamTarget;
			}
			stuckTimer = 0f;
			lastStuckPos = rb.position;
			stateStart = false;
			playerInRange = true;
		}
		if (SemiFunc.EnemyForceLeave(enemy) || SemiFunc.KeyAxel(KeyCode.Delete))
		{
			StateSet(State.Leave);
		}
		PlayIdleLoop();
		HooverHandsOverride(0.05f, 0.1f, 0.5f);
		ActivateTailScroll();
		if (SemiFunc.EnemySpawnIdlePause())
		{
			stateTimer = 0f;
			return;
		}
		if (SemiFunc.FPSImpulse1())
		{
			PlayerAvatar playerAvatar = SemiFunc.PlayerGetNearestPlayerAvatarWithinRange(20f, rb.position);
			playerInRange = playerAvatar != null;
		}
		if (stateTimer >= 2f && playerInRange)
		{
			SpotlightOn();
			ActivateTailScroll();
			if (lookAtInvestigatePointTimer <= 0f)
			{
				HeadLookDown();
			}
		}
		if (!playerInRange)
		{
			stateTimerMax -= Time.deltaTime;
		}
		LookAtInvestigates();
		PhysicsRoamMovementBehavior();
		RandomVO();
		if (Vector3.Distance(rb.position, lastStuckPos) < 1f)
		{
			stuckTimer += Time.deltaTime;
			if (stuckTimer >= 2f)
			{
				stateTimer = stateTimerMax;
			}
		}
		else
		{
			stuckTimer = 0f;
			lastStuckPos = rb.position;
		}
		if (HasReachedDestination(currentRoamTarget))
		{
			currentRoamIndex++;
			if (currentRoamIndex >= ceilingRoamPoints.Count)
			{
				currentRoamIndex = 0;
			}
			currentRoamTarget = ceilingRoamPoints[currentRoamIndex];
			roamDestination = currentRoamTarget;
		}
		if (stateTimer >= stateTimerMax)
		{
			ceilingRoamPoints.Clear();
			StateSet(State.FindCeiling);
		}
	}

	private void StatePlayerSpotted()
	{
		if (stateStart)
		{
			stateTimerMax = 0.6f;
			stateTimer = 0f;
			StateStartResets();
			if (attacksRemaining == attacksMax)
			{
				DecideAttackCount();
			}
			audioSeePlayer.Play(rb.position);
			audioSeePlayerGlobal.Play(rb.position);
			roamDestination = rb.position;
			stateStart = false;
		}
		PlayOoglyLoop();
		OverrideEvilEyes(0.1f);
		handPose.SetPoseByName("spotted");
		HeadSpringOverride(0.8f, 35f, 0.1f);
		ActivateTailScroll();
		HeadLookAtTarget();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!targetPlayer)
			{
				Vector3 normalized = (targetPlayer.transform.position - rb.position).normalized;
				physicsController.PhysRotateTowards(normalized, 80f, 10f, 0.2f);
			}
			Vector3 normalized2 = (roamDestination - rb.position).normalized;
			float speed = Vector3.Distance(rb.position, roamDestination);
			physicsController.PhysMoveTowards(normalized2, speed, 0.2f);
			if (!targetPlayer || targetPlayer.isDisabled)
			{
				SetTarget(null);
				hasTarget = false;
				StateSet(State.CeilingRoam);
			}
			else if (stateTimer >= stateTimerMax)
			{
				StateSet(State.Dive);
			}
		}
	}

	private void StateDive()
	{
		if (stateStart)
		{
			stateTimerMax = 5f;
			stateTimer = 0f;
			stuckTimer = 0f;
			lastStuckPos = rb.position;
			StateStartResets();
			handPose.SetPoseByName("attack");
			audioStartCharge.Play(rb.position);
			stateStart = false;
		}
		PlayOoglyLoop();
		PlayFastFlightLoop();
		PlayWrestleLoop();
		HeadLookAtTarget();
		HeadSpringOverride(0.8f, 35f, 0.1f);
		ActivateTailScroll();
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (!targetPlayer)
		{
			StateSet(State.FindCeiling);
			return;
		}
		Vector3 normalized = (targetPlayer.transform.position - rb.position).normalized;
		float num = Vector3.Distance(rb.position, targetPlayer.transform.position);
		physicsController.PhysMoveTowards(normalized, moveForce * 0.5f, 0.2f);
		physicsController.PhysRotateTowards(normalized, 200f, 15f, 0.2f);
		LookAtVelocityDirection(_moving: true);
		if (Vector3.Distance(rb.position, lastStuckPos) < 1f)
		{
			stuckTimer += Time.deltaTime;
			if (stuckTimer >= 2f)
			{
				StateSet(State.FindCeiling);
				return;
			}
		}
		else
		{
			stuckTimer = 0f;
			lastStuckPos = rb.position;
		}
		if (num < 2f)
		{
			StateSet(State.WrestlePlayer);
		}
		else if (stateTimer >= stateTimerMax)
		{
			StateSet(State.FindCeiling);
		}
	}

	private void StateWrestlePlayer()
	{
		if (stateStart)
		{
			stateTimerMax = UnityEngine.Random.Range(4f, 6f);
			stateTimer = 0f;
			StateStartResets();
			stateStart = false;
			grabbedPlayer = targetPlayer;
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				grabbedPlayer.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
			}
			lastAttackedPlayer = grabbedPlayer;
			attackCooldownDuration = UnityEngine.Random.Range(1.5f, 4f);
			attackCooldownTimer = attackCooldownDuration;
			wrestleOutOfRangeTimer = 0f;
			wrestleLineOfSightLostTimer = 0f;
			audioGrabPlayer.Play(rb.position);
		}
		ActivateHurtCollider();
		ActivateTailScroll();
		ActivateGrabbedPlayerTrailScroll();
		PlayWrestleLoop();
		HeadLookAtTarget();
		OverrideTurnOffFresnel(0.1f);
		HeadSpringOverride(0.8f, 35f, 0.1f);
		handPose.SetPoseByName("attack");
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		grabbedPlayer.tumble.TumbleOverrideTime(1.5f);
		enemy.EnemyParent.SpawnedTimerPause(1f);
		if (grabbedPlayer.isDisabled)
		{
			grabbedPlayer = null;
			attacksRemaining--;
			if (attacksRemaining <= 0)
			{
				StateSet(State.Leave);
			}
			else
			{
				StateSet(State.FindCeiling);
			}
			return;
		}
		if ((bool)grabbedPlayer)
		{
			Vector3 normalized = (grabbedPlayer.transform.position - rb.position).normalized;
			physicsController.PhysRotateTowards(normalized, 100f, 1f, 0.2f);
			Vector3 vector = grabbedPlayer.transform.position - rb.position;
			float magnitude = vector.magnitude;
			Vector3 normalized2 = vector.normalized;
			float speed = moveForce * Mathf.Clamp01(magnitude / 3f);
			physicsController.PhysMoveTowards(normalized2, speed, 0.2f);
			if (magnitude > 2f)
			{
				wrestleOutOfRangeTimer += Time.deltaTime;
			}
			else
			{
				wrestleOutOfRangeTimer = 0f;
			}
			Vector3 vector2 = grabbedPlayer.tumble.physGrabObject.centerPoint - visionTransform.position;
			float magnitude2 = vector2.magnitude;
			if (Physics.Raycast(visionTransform.position, vector2.normalized, magnitude2, environmentMask, QueryTriggerInteraction.Ignore))
			{
				wrestleLineOfSightLostTimer += Time.deltaTime;
			}
			else
			{
				wrestleLineOfSightLostTimer = 0f;
			}
			if (wrestleOutOfRangeTimer >= 1.5f || wrestleLineOfSightLostTimer >= 1.5f)
			{
				grabbedPlayer = null;
				attacksRemaining--;
				if (attacksRemaining <= 0)
				{
					StateSet(State.Leave);
				}
				else
				{
					StateSet(State.FindCeiling);
				}
				return;
			}
		}
		if ((bool)grabbedPlayer)
		{
			grabbedPlayer.tumble.TumbleOverrideTime(1f);
			grabbedPlayer.FallDamageResetSet(0.1f);
		}
		if (stateTimer >= stateTimerMax)
		{
			grabbedPlayer = null;
			attacksRemaining--;
			if (attacksRemaining <= 0)
			{
				StateSet(State.Leave);
			}
			else
			{
				StateSet(State.FindCeiling);
			}
		}
	}

	private void WrestlePlayerPhysics()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !(grabbedPlayer == null) && !(rb == null))
		{
			Vector3 position = grabbedPlayer.transform.position;
			_ = (position - rb.position).normalized;
			rb.AddForce(Vector3.up * 2f, ForceMode.Force);
			float num = 5f;
			Vector3 normalized = (rb.position - position).normalized;
			num = 2f;
			float num2 = Vector3.Distance(rb.position, position) / 3f;
			num *= num2;
			grabbedPlayer.tumble.rb.AddForce(normalized * num, ForceMode.Impulse);
			grabbedPlayer.tumble.physGrabObject.OverrideZeroGravity();
			grabbedPlayer.tumble.OverrideEnemyHurt(0.1f);
		}
	}

	private void StateLeave()
	{
		if (stateStart)
		{
			StateStartResets();
			stateStart = false;
			doLeave = true;
			leaveCyclesRemaining = UnityEngine.Random.Range(2, 5);
			DecideAttackCount();
			SemiFunc.EnemyLeaveStart(enemy);
			StateSet(State.FindCeiling);
		}
	}

	private void StateStunned()
	{
		if (stateStart)
		{
			stateTimerMax = 0f;
			stateTimer = 0f;
			stateStart = false;
			StateStartResets();
			followTarget.localPosition = followTargetStartPosition;
			followTarget.rotation = Quaternion.identity;
		}
		handPose.SetPoseByName("stunned");
		tailPose.SetPoseByName("stunned");
		HeadSpringOverride(0.5f, 30f, 0.1f);
		PlayIdleLoop();
		enemy.Vision.DisableVision(1f);
		HooverHandsOverride(0.2f, 0.2f, 5f);
		headStunnedLookDownTimer = 0.1f;
		vector3Springs.SetOverridePositionByName("fullbody", new Vector3(0f, 0.5f, 0f), 0.1f);
		if (!enemy.IsStunned())
		{
			StunnedEnd();
		}
	}

	private void StateTeleportOut()
	{
		if (stateStart)
		{
			stateTimerMax = 1f;
			stateTimer = 0f;
			StateStartResets();
			hasPlayedTeleportInEffects = false;
			lastTeleportOutPosition = Vector3.zero;
			foreach (ParticleSystem item in particlesTeleportEnd)
			{
				if ((bool)item)
				{
					item.Play(withChildren: true);
				}
			}
			foreach (ParticleSystem item2 in particlesTeleportOut)
			{
				if ((bool)item2)
				{
					item2.Play(withChildren: true);
				}
			}
			audioTeleportOut.Play(rb.position);
			stateStart = false;
		}
		enemy.StateStunned.OverrideDisable(0.2f);
		ActivateTeleportLoop();
		float num = stateTimer / stateTimerMax;
		float num2 = num * num * num;
		if (animationCurveTeleportOut != null)
		{
			float num3 = 1f - animationCurveTeleportOut.Evaluate(num);
			teleportTransform.localScale = Vector3.one * num3;
		}
		else
		{
			float num4 = 1f - num;
			teleportTransform.localScale = Vector3.one * num4;
		}
		float num5 = 1440f * num2;
		teleportTransform.localRotation = Quaternion.Euler(num5 * 0.5f, num5, num5 * 0.3f);
		FloatAround();
		if (!(stateTimer >= stateTimerMax))
		{
			return;
		}
		teleportTransform.localRotation = Quaternion.identity;
		teleportTransform.localScale = Vector3.zero;
		followTarget.localPosition = followTargetStartPosition;
		followTarget.rotation = Quaternion.identity;
		if (isAnimatedDespawn)
		{
			StateSet(State.Despawn);
		}
		else if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (teleportDestination != Vector3.zero)
			{
				enemy.EnemyTeleported(teleportDestination);
			}
			lastTeleportOutPosition = rb.position;
			StateSet(State.TeleportIn);
		}
	}

	private void StateTeleportIn()
	{
		if (stateStart)
		{
			stateTimerMax = 1f;
			stateTimer = 0f;
			StateStartResets();
			teleportTransform.localScale = Vector3.zero;
			tailTrailRenderer.Clear();
			grabbedPlayerTrailRenderer.Clear();
			tailTrailRenderer.enabled = false;
			grabbedPlayerTrailRenderer.enabled = false;
			foreach (ParticleSystem item in particlesTeleportEnd)
			{
				if ((bool)item)
				{
					item.Play(withChildren: true);
				}
			}
			hasPlayedTeleportInEffects = false;
			stateStart = false;
		}
		if (!hasPlayedTeleportInEffects)
		{
			bool flag = false;
			if (lastTeleportOutPosition == Vector3.zero)
			{
				flag = true;
			}
			else if (Vector3.Distance(rb.position, lastTeleportOutPosition) >= 1.5f)
			{
				flag = true;
			}
			if (flag)
			{
				foreach (ParticleSystem item2 in particlesTeleportIn)
				{
					if ((bool)item2)
					{
						item2.Play(withChildren: true);
					}
				}
				audioTeleportIn.Play(rb.position);
				hasPlayedTeleportInEffects = true;
				lastTeleportOutPosition = Vector3.zero;
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
		enemy.StateStunned.OverrideDisable(0.2f);
		ActivateTeleportLoop();
		float num = stateTimer / stateTimerMax;
		float num2 = num * num * num;
		if (animationCurveTeleportIn != null)
		{
			float num3 = 1f - animationCurveTeleportIn.Evaluate(1f - num);
			teleportTransform.localScale = Vector3.one * num3;
		}
		else
		{
			float num4 = num;
			teleportTransform.localScale = Vector3.one * num4;
		}
		float num5 = 1440f * num2;
		teleportTransform.localRotation = Quaternion.Euler(num5 * 0.5f, num5, num5 * 0.3f);
		FloatAround();
		if (!(stateTimer >= stateTimerMax))
		{
			return;
		}
		teleportTransform.localRotation = Quaternion.identity;
		teleportTransform.localScale = Vector3.one;
		followTarget.localPosition = followTargetStartPosition;
		followTarget.rotation = Quaternion.identity;
		foreach (ParticleSystem item3 in particlesTeleportEnd)
		{
			if ((bool)item3)
			{
				item3.Play(withChildren: true);
			}
		}
		StateSet(teleportReturnState);
	}

	private void StateStartResets()
	{
		grabbedPlayer = null;
		investigatePosition = Vector3.zero;
		wrestleOutOfRangeTimer = 0f;
		wrestleLineOfSightLostTimer = 0f;
		hurtColliderTimer = 0f;
		if (hurtCollider.gameObject.activeSelf)
		{
			hurtCollider.gameObject.SetActive(value: false);
		}
		stuckTimer = 0f;
		visionPrevious = false;
		visionPreviousTime = 0f;
		playerInRange = false;
		roamDestination = Vector3.zero;
		lastStuckPos = Vector3.zero;
		debugFailedRoamPoints.Clear();
		debugFailReasons.Clear();
		teleportTransform.localRotation = Quaternion.identity;
		teleportTransform.localScale = Vector3.one;
		headLookAtPositionTimer = 0f;
		headLookAtTargetTimer = 0f;
		headLookDownTimer = 0f;
		headStunnedLookDownTimer = 0f;
		grabbedPlayerTrailRenderer.emitting = false;
		tailTrailRenderer.emitting = false;
		grabbedPlayerTrailScrollTimer = 0f;
		tailScrollTimer = 0f;
		lookAtInvestigatePointTimer = 0f;
	}

	private void DespawnResets()
	{
		currentState = State.Spawn;
		stateStart = true;
		stateTimer = 0f;
		stateTimerMax = 0f;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			SetTarget(null);
		}
		grabbedPlayer = null;
		hasTarget = false;
		lastAttackedPlayer = null;
		attackCooldownTimer = 0f;
		attackCooldownDuration = 0f;
		attacksRemaining = 0;
		attacksMax = 0;
		visionPrevious = false;
		visionPreviousTime = 0f;
		investigatePosition = Vector3.zero;
		doLeave = false;
		didLeave = false;
		ceilingRoamPoints.Clear();
		currentLevelPointPosition = Vector3.zero;
		ceilingPoint = Vector3.zero;
		roamPoints.Clear();
		currentRoamTarget = Vector3.zero;
		currentRoamIndex = 0;
		teleportDestination = Vector3.zero;
		teleportReturnState = State.FindCeiling;
		stuckTimer = 0f;
		lastStuckPos = Vector3.zero;
		findCeilingAttempts = 0;
		roamDestination = Vector3.zero;
		wrestleOutOfRangeTimer = 0f;
		wrestleLineOfSightLostTimer = 0f;
		leaveCyclesRemaining = 0;
		isAnimatedDespawn = false;
		audioIdleLoopTimer = 0f;
		audioWrestleLoopTimer = 0f;
		audioOoglyLoopTimer = 0f;
		audioFastFlightLoopTimer = 0f;
		randomVOCooldown = 0f;
		teleportLoopTimer = 0f;
		spotlightOnTimer = 0f;
		spotlightCurrentIntensity = 0f;
		spotlightFadeTimer = 0f;
		spotlightFadingIn = false;
		playerInRange = false;
		evilEyesTimer = 0f;
		isEvilMode = false;
		headLookDownTimer = 0f;
		headLookAtTargetTimer = 0f;
		headStunnedLookDownTimer = 0f;
		headTwitchOffset = Vector3.zero;
		headTwitchUpdateTimer = 0f;
		headSpringOverrideDamping = 0f;
		headSpringOverrideSpeed = 0f;
		headSpringOverrideTimer = 0f;
		hooverHandsTimer = 0f;
		hurtColliderTimer = 0f;
		if (hurtCollider.gameObject.activeSelf)
		{
			hurtCollider.gameObject.SetActive(value: false);
		}
		debugFailedRoamPoints.Clear();
		debugFailReasons.Clear();
	}

	private void AnimatedDespawn()
	{
		if (teleportTransform.localScale.x > 0.1f)
		{
			isAnimatedDespawn = true;
			teleportDestination = rb.position;
			teleportReturnState = State.Despawn;
			StateSet(State.TeleportOut);
		}
		else
		{
			StateSet(State.Despawn);
		}
	}

	private void StateDespawn()
	{
		if (stateStart)
		{
			StateStartResets();
			stateStart = false;
			StopAllParticles();
			DespawnResets();
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				enemy.EnemyParent.Despawn();
			}
		}
	}

	private void LevelPointsIdleRoam()
	{
		List<LevelPoint> list = SemiFunc.LevelPointGetWithinDistance(rb.position, 3f, 16f);
		if (list.Count > 0)
		{
			LevelPoint levelPoint = list[UnityEngine.Random.Range(0, list.Count)];
			currentLevelPointPosition = levelPoint.transform.position;
		}
	}

	private void LevelPointsInvestigate()
	{
		List<LevelPoint> list = SemiFunc.LevelPointGetWithinDistance(investigatePosition, 0f, 10f);
		if (list.Count > 0)
		{
			LevelPoint levelPoint = list[UnityEngine.Random.Range(0, list.Count)];
			currentLevelPointPosition = levelPoint.transform.position;
		}
		else
		{
			LevelPointsIdleRoam();
		}
	}

	private void TeleportToLevelPoint()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !(enemy == null) && !(currentLevelPointPosition == Vector3.zero))
		{
			teleportDestination = currentLevelPointPosition + Vector3.up * 1f;
			teleportReturnState = State.FindCeiling;
			StateSet(State.TeleportOut);
		}
	}

	private void InitializeFlightSystem()
	{
		FlightSettings flightSettings = new FlightSettings
		{
			moveForce = moveForce,
			turnSmoothness = turnSmoothness,
			maxSpeed = 20f,
			minFlightHeight = minFlightHeight,
			maxFlightHeight = maxFlightHeight,
			preferredHeight = preferredHeight,
			stabilizationForce = 1000f,
			groundAvoidanceDistance = 8f,
			stabilizationRange = 6f,
			positionDamping = 0.5f,
			rotationDamping = 0.5f,
			positionSpeed = 20f,
			rotationSpeed = 40f
		};
		flightSystem.SetFlightSettings(flightSettings);
		flightSystem.Initialize();
	}

	private void InitializeHeadSpring()
	{
		headStartRotation = headTransform.localRotation;
		headWorldTargetRotation = headStartRotation;
		headSpring = new SpringQuaternion();
		headSpring.damping = headSpringDamping;
		headSpring.speed = headSpringSpeed;
		headWorldTargetRotation = headTransform.rotation;
	}

	private void HeadLookDown()
	{
		headLookDownTimer = 0.1f;
	}

	private void HeadLookAtTarget()
	{
		headLookAtTargetTimer = 0.2f;
	}

	private void HeadSpringOverride(float damping, float speed, float duration)
	{
		headSpringOverrideDamping = damping;
		headSpringOverrideSpeed = speed;
		headSpringOverrideTimer = duration;
	}

	private void LookAtInvestigates()
	{
		if (lookAtInvestigatePointTimer > 0f)
		{
			headLookAtPositionTimer = 0.1f;
		}
	}

	private void SetHeadTargetRotation()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || headTransform == null)
		{
			return;
		}
		Quaternion quaternion = headStartRotation;
		if (headStunnedLookDownTimer > 0f)
		{
			if (headTwitchUpdateTimer <= 0f)
			{
				float num = 10f;
				float x = UnityEngine.Random.Range(0f - num, num);
				float y = UnityEngine.Random.Range(0f - num, num);
				float z = UnityEngine.Random.Range(0f - num, num);
				headTwitchOffset = new Vector3(x, y, z);
				headTwitchUpdateTimer = UnityEngine.Random.Range(0.05f, 0.15f);
			}
			Quaternion quaternion2 = Quaternion.Euler(45f, 0f, 0f);
			Quaternion quaternion3 = Quaternion.Euler(headTwitchOffset);
			quaternion = headStartRotation * quaternion2 * quaternion3;
		}
		else if (headLookDownTimer > 0f)
		{
			Quaternion quaternion4 = Quaternion.LookRotation(Vector3.down, Vector3.forward);
			Quaternion quaternion5 = ((headTransform.parent != null) ? (Quaternion.Inverse(headTransform.parent.rotation) * quaternion4) : quaternion4);
			if (headLookTargetTransform != null)
			{
				Vector3 normalized = (headLookTargetTransform.position - headTransform.position).normalized;
				Vector3 forward = ((headTransform.parent != null) ? headTransform.parent.InverseTransformDirection(normalized) : normalized);
				if (forward.sqrMagnitude > float.Epsilon)
				{
					Quaternion quaternion6 = Quaternion.LookRotation(forward);
					Vector3 eulerAngles = quaternion5.eulerAngles;
					Vector3 eulerAngles2 = quaternion6.eulerAngles;
					quaternion = Quaternion.Euler(eulerAngles.x, eulerAngles2.y, eulerAngles2.z);
				}
			}
			else
			{
				quaternion = quaternion5;
			}
		}
		else if (headLookAtPositionTimer > 0f && investigatePointLookAtPosition != Vector3.zero)
		{
			Vector3 vector = investigatePointLookAtPosition - headTransform.position;
			vector.Normalize();
			if (Vector3.Angle(visualTransform.forward, vector) <= 120f)
			{
				Vector3 forward2 = ((headTransform.parent != null) ? headTransform.parent.InverseTransformDirection(vector) : vector);
				if (forward2.sqrMagnitude > float.Epsilon)
				{
					quaternion = Quaternion.LookRotation(forward2);
				}
			}
		}
		else if (headLookAtTargetTimer > 0f)
		{
			Transform transform = null;
			if (targetPlayer != null)
			{
				transform = targetPlayer.PlayerVisionTarget.VisionTransform;
			}
			else if (headLookTargetTransform != null)
			{
				transform = headLookTargetTransform;
			}
			if (transform != null)
			{
				Vector3 vector2 = transform.position - headTransform.position;
				vector2.Normalize();
				if (Vector3.Angle(visualTransform.forward, vector2) <= 120f)
				{
					Vector3 forward3 = ((headTransform.parent != null) ? headTransform.parent.InverseTransformDirection(vector2) : vector2);
					if (forward3.sqrMagnitude > float.Epsilon)
					{
						quaternion = Quaternion.LookRotation(forward3);
					}
				}
			}
		}
		if (headLookAtPositionTimer > 0f && investigatePointLookAtPosition != Vector3.zero)
		{
			headLookAtPositionTarget = Quaternion.Slerp(headLookAtPositionTarget, quaternion, 0.5f * Time.deltaTime);
			quaternion = headLookAtPositionTarget;
		}
		else
		{
			headLookAtPositionTarget = quaternion;
		}
		Quaternion quaternion7 = ((headTransform.parent != null) ? headTransform.parent.rotation : Quaternion.identity);
		headWorldTargetRotation = quaternion7 * quaternion;
	}

	private void UpdateHeadSpring()
	{
		if (headLookDownTimer > 0f)
		{
			headLookDownTimer -= Time.deltaTime;
		}
		if (headLookAtTargetTimer > 0f)
		{
			headLookAtTargetTimer -= Time.deltaTime;
		}
		if (headLookAtPositionTimer > 0f)
		{
			headLookAtPositionTimer -= Time.deltaTime;
		}
		if (headStunnedLookDownTimer > 0f)
		{
			headStunnedLookDownTimer -= Time.deltaTime;
		}
		if (headTwitchUpdateTimer > 0f)
		{
			headTwitchUpdateTimer -= Time.deltaTime;
		}
		if (headSpringOverrideTimer <= 0f)
		{
			headSpring.damping = headSpringDamping;
			headSpring.speed = headSpringSpeed;
		}
		if (headSpringOverrideTimer > 0f)
		{
			headSpringOverrideTimer -= Time.deltaTime;
			headSpring.damping = headSpringOverrideDamping;
			headSpring.speed = headSpringOverrideSpeed;
		}
		Quaternion targetRotation = ((headTransform.parent != null) ? Quaternion.Inverse(headTransform.parent.rotation) : Quaternion.identity) * headWorldTargetRotation;
		headTransform.localRotation = SemiFunc.SpringQuaternionGet(headSpring, targetRotation);
	}

	private void HooverHandsOverride(float _width, float _height, float _speed)
	{
		hooverHandsWidth = _width;
		hooverHandsHeight = _height;
		hooverHandsSpeed = _speed;
		hooverHandsTimer = 0.1f;
	}

	private void UpdateHooverHands()
	{
		if (!(hooverHandsTimer <= 0f))
		{
			if (hooverHandsTimer > 0f)
			{
				hooverHandsTimer -= Time.deltaTime;
			}
			float num = Time.time * hooverHandsSpeed;
			float f = num * MathF.PI * 2f;
			float f2 = num * MathF.PI * 2f + MathF.PI;
			float num2 = Mathf.PerlinNoise(num * 0.5f, 0f) * 0.5f;
			float num3 = Mathf.PerlinNoise(num * 0.5f, 100f) * 0.5f;
			float x = Mathf.Cos(f) * hooverHandsWidth + num2 * hooverHandsWidth;
			float y = Mathf.Sin(f) * hooverHandsHeight + num2 * hooverHandsHeight;
			float x2 = Mathf.Cos(f2) * hooverHandsWidth + num3 * hooverHandsWidth;
			float y2 = Mathf.Sin(f2) * hooverHandsHeight + num3 * hooverHandsHeight;
			Vector3 vector = new Vector3(x, y, 0f);
			Vector3 vector2 = new Vector3(x2, y2, 0f);
			SpringTransform springTransformByName = vector3Springs.GetSpringTransformByName("hand1");
			SpringTransform springTransformByName2 = vector3Springs.GetSpringTransformByName("hand2");
			vector3Springs.SetOverridePositionByName("hand1", springTransformByName.originalLocalPosition + vector, 0.1f);
			vector3Springs.SetOverridePositionByName("hand2", springTransformByName2.originalLocalPosition + vector2, 0.1f);
		}
	}

	private void InitializeJawAnimation()
	{
		if (jawTransform != null)
		{
			jawStartRotation = jawTransform.localRotation;
			jawTargetRotation = jawStartRotation;
			jawSpring = new SpringQuaternion();
			jawSpring.damping = 0.6f;
			jawSpring.speed = 30f;
		}
		eyeScaleSprings.Clear();
		foreach (Transform eye in eyes)
		{
			if (eye != null)
			{
				SpringVector3 springVector = new SpringVector3();
				springVector.damping = 0.05f;
				springVector.speed = 36f;
				eyeScaleSprings.Add(springVector);
			}
		}
	}

	private void CodeAnimatedTalk()
	{
		if (SemiFunc.FPSImpulse30())
		{
			mainAudioSource.GetSpectrumData(audioSourceSpectrum, 0, FFTWindow.Hamming);
			float num = audioSourceSpectrum[0] * 100000f;
			if (num > 60f)
			{
				num = 60f;
			}
			talkVolume = num;
			jawSpring.springVelocity += UnityEngine.Random.insideUnitSphere * 0.5f;
			if (talkVolume > 5f)
			{
				for (int i = 0; i < eyeScaleSprings.Count; i++)
				{
					eyeScaleSprings[i].springVelocity += UnityEngine.Random.insideUnitSphere * (talkVolume * 0.25f);
				}
			}
		}
		float x = talkVolume;
		jawTargetRotation = jawStartRotation * Quaternion.Euler(x, 0f, 0f);
		jawTransform.localRotation = SemiFunc.SpringQuaternionGet(jawSpring, jawTargetRotation);
		for (int j = 0; j < eyes.Count && j < eyeScaleSprings.Count; j++)
		{
			if (eyes[j] != null)
			{
				eyes[j].localScale = SemiFunc.SpringVector3Get(eyeScaleSprings[j], Vector3.one);
			}
		}
		if (talkVolume > 0f)
		{
			talkVolume = Mathf.Lerp(talkVolume, 0f, Time.deltaTime * 5f);
		}
	}

	private void InitializeEmissionMaterials()
	{
		emissionMaterials.Clear();
		originalEmissionColors.Clear();
		foreach (Renderer emissionRenderer in emissionRenderers)
		{
			Material material = emissionRenderer.material;
			emissionMaterials.Add(material);
			if (material.HasProperty(emissionColorID))
			{
				Color color = material.GetColor(emissionColorID);
				originalEmissionColors.Add(color);
			}
			else
			{
				originalEmissionColors.Add(Color.white);
			}
		}
		if (visionSpotlight != null)
		{
			originalSpotlightColor = visionSpotlight.color;
		}
	}

	private void SpotlightOn()
	{
		spotlightOnTimer = 0.1f;
	}

	public void OverrideEvilEyes(float duration)
	{
		evilEyesTimer = duration;
		isEvilMode = true;
		SpotlightOn();
		HeadLookAtTarget();
		HeadSpringOverride(0.8f, 35f, 0.1f);
	}

	private void UpdateVisionSpotlight()
	{
		if (spotlightOnTimer > 0f)
		{
			spotlightOnTimer -= Time.deltaTime;
		}
		bool flag = spotlightOnTimer > 0f;
		if (flag && spotlightFadeTimer < spotlightIntroDuration)
		{
			if (!spotlightFadingIn)
			{
				spotlightFadingIn = true;
				spotlightFadeTimer = 0f;
				audioSpotlightOn.Play(rb.position);
			}
			spotlightFadeTimer += Time.deltaTime;
			float num = Mathf.Clamp01(spotlightFadeTimer / spotlightIntroDuration);
			if (animationCurveSpotlightIntro != null)
			{
				spotlightCurrentIntensity = animationCurveSpotlightIntro.Evaluate(num) * spotlightMaxIntensity;
			}
			else
			{
				spotlightCurrentIntensity = num * spotlightMaxIntensity;
			}
			if (spotlightFadeTimer >= spotlightIntroDuration)
			{
				spotlightCurrentIntensity = spotlightMaxIntensity;
			}
		}
		else if (!flag && spotlightCurrentIntensity > 0f)
		{
			if (spotlightFadingIn)
			{
				spotlightFadingIn = false;
				spotlightFadeTimer = 0f;
				audioSpotlightOff.Play(rb.position);
			}
			spotlightFadeTimer += Time.deltaTime;
			float time = Mathf.Clamp01(spotlightFadeTimer / spotlightOutroDuration);
			spotlightCurrentIntensity = (1f - animationCurveSpotlightOutro.Evaluate(time)) * spotlightMaxIntensity;
		}
		visionSpotlight.intensity = spotlightCurrentIntensity;
		if (isEvilMode)
		{
			visionSpotlight.color = Color.red;
		}
		else
		{
			visionSpotlight.color = originalSpotlightColor;
		}
		visionSpotlight.transform.position = visionTransform.position;
		visionSpotlight.transform.rotation = visionTransform.rotation;
		visionSpotlight.range = enemy.Vision.VisionDistance;
		float spotAngle = Mathf.Acos(enemy.Vision.VisionDotStanding) * 57.29578f * 2f;
		visionSpotlight.spotAngle = spotAngle;
		float num2 = spotlightCurrentIntensity / spotlightMaxIntensity;
		visionPointLight.intensity = num2 * pointLightMaxIntensity;
		if (isEvilMode)
		{
			visionPointLight.color = Color.red;
		}
		else
		{
			visionPointLight.color = originalSpotlightColor;
		}
		if (spotlightCurrentIntensity < spotlightMaxIntensity)
		{
			enemy.Vision.DisableVision(0.1f);
		}
		UpdateEmissionColors();
		UpdateSpotlightParticles(flag);
	}

	private void UpdateEmissionColors()
	{
		float num = spotlightCurrentIntensity / spotlightMaxIntensity;
		for (int i = 0; i < emissionMaterials.Count && i < originalEmissionColors.Count; i++)
		{
			Color value = (isEvilMode ? Color.red : originalEmissionColors[i]) * num;
			emissionMaterials[i].SetColor(emissionColorID, value);
			emissionMaterials[i].SetFloat(enemy.Health.materialHurtAmount, enemy.Health.hurtCurve.Evaluate(enemy.Health.hurtLerp));
		}
	}

	private void UpdateSpotlightParticles(bool shouldBeOn)
	{
		if (isEvilMode && shouldBeOn)
		{
			foreach (ParticleSystem item in particlesSpotlight)
			{
				if (item.isPlaying)
				{
					item.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
				}
			}
			foreach (ParticleSystem item2 in particlesSpotlightEvil)
			{
				if (!item2.isPlaying)
				{
					item2.Play(withChildren: true);
				}
			}
			if (!particlesEvilEyes.isPlaying)
			{
				particlesEvilEyes.Play(withChildren: true);
			}
			return;
		}
		foreach (ParticleSystem item3 in particlesSpotlightEvil)
		{
			if (item3.isPlaying)
			{
				item3.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
			}
		}
		if (particlesEvilEyes.isPlaying)
		{
			particlesEvilEyes.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
		}
		foreach (ParticleSystem item4 in particlesSpotlight)
		{
			if (shouldBeOn && !item4.isPlaying)
			{
				item4.Play(withChildren: true);
			}
			else if (!shouldBeOn && item4.isPlaying)
			{
				item4.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
			}
		}
	}

	private void ActivateTeleportLoop()
	{
		teleportLoopTimer = 0.1f;
	}

	private void UpdateTeleportParticles()
	{
		if (teleportLoopTimer <= 0f)
		{
			foreach (ParticleSystem item in particlesTeleportLoop)
			{
				if (item.isPlaying)
				{
					item.Stop(withChildren: true);
				}
			}
		}
		if (!(teleportLoopTimer > 0f))
		{
			return;
		}
		teleportLoopTimer -= Time.deltaTime;
		foreach (ParticleSystem item2 in particlesTeleportLoop)
		{
			if (!item2.isPlaying)
			{
				item2.Play(withChildren: true);
			}
		}
	}

	private void UpdateFakeVelocity()
	{
		if (SemiFunc.FPSImpulse30())
		{
			fakeVelocity = Vector3.Distance(rb.position, previousPosition);
			previousPosition = rb.position;
		}
	}

	private void PlayIdleLoop()
	{
		audioIdleLoopTimer = 0.1f;
	}

	private void PlayWrestleLoop()
	{
		audioWrestleLoopTimer = 0.1f;
	}

	private void PlayOoglyLoop()
	{
		audioOoglyLoopTimer = 0.1f;
	}

	private void PlayFastFlightLoop()
	{
		audioFastFlightLoopTimer = 0.1f;
	}

	private void PlayLoopSoundLogic()
	{
		bool playing = audioIdleLoopTimer > 0f;
		audioIdleLoop.PlayLoop(playing, 1f, 3f);
		bool playing2 = audioWrestleLoopTimer > 0f;
		audioWrestleLoop.PlayLoop(playing2, 1f, 3f);
		bool playing3 = audioOoglyLoopTimer > 0f;
		audioOoglyLoop.PlayLoop(playing3, 1f, 3f);
		bool playing4 = audioFastFlightLoopTimer > 0f;
		float t = Mathf.Clamp01(fakeVelocity / 5f);
		float pitchMultiplier = Mathf.Lerp(0.8f, 1.2f, t);
		audioFastFlight.PlayLoop(playing4, 1f, 3f, pitchMultiplier);
		if (audioFastFlightLoopTimer > 0f)
		{
			audioFastFlightLoopTimer -= Time.deltaTime;
		}
		if (audioIdleLoopTimer > 0f)
		{
			audioIdleLoopTimer -= Time.deltaTime;
		}
		if (audioWrestleLoopTimer > 0f)
		{
			audioWrestleLoopTimer -= Time.deltaTime;
		}
		if (audioOoglyLoopTimer > 0f)
		{
			audioOoglyLoopTimer -= Time.deltaTime;
		}
	}

	private void RandomVO()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (randomVOCooldown <= 0f)
		{
			int num = UnityEngine.Random.Range(0, voList.Count);
			if (SemiFunc.IsMultiplayer())
			{
				base.photonView.RPC("PlayVORPC", RpcTarget.All, num);
			}
			else
			{
				PlayVORPC(num);
			}
			randomVOCooldown = UnityEngine.Random.Range(2f, 12f);
		}
		if (randomVOCooldown > 0f)
		{
			randomVOCooldown -= Time.deltaTime;
		}
	}

	[PunRPC]
	private void PlayVORPC(int voIndex, PhotonMessageInfo info = default(PhotonMessageInfo))
	{
		if ((!SemiFunc.IsMultiplayer() || SemiFunc.MasterOnlyRPC(info)) && voIndex >= 0 && voIndex < voList.Count && voList[voIndex] != null)
		{
			voList[voIndex].Play(rb.position);
		}
	}

	private void StopAllParticles()
	{
		foreach (ParticleSystem item in particlesSpotlight)
		{
			item.Stop(withChildren: true);
		}
		particlesEvilEyes.Stop(withChildren: true);
		foreach (ParticleSystem item2 in particlesSpotlightEvil)
		{
			item2.Stop(withChildren: true);
		}
		foreach (ParticleSystem item3 in particlesTeleportOut)
		{
			item3.Stop(withChildren: true);
		}
		foreach (ParticleSystem item4 in particlesTeleportIn)
		{
			item4.Stop(withChildren: true);
		}
		foreach (ParticleSystem item5 in particlesTeleportLoop)
		{
			item5.Stop(withChildren: true);
		}
		foreach (ParticleSystem item6 in particlesTeleportEnd)
		{
			item6.Stop(withChildren: true);
		}
		foreach (ParticleSystem item7 in particlesHitPlayer)
		{
			item7.Stop(withChildren: true);
		}
	}

	public void OnDeath()
	{
		GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, rb.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(4f, 3f, 8f, rb.position, 0.1f);
		foreach (ParticleSystem item in particlesDeath)
		{
			item.Play(withChildren: true);
		}
		audioDeath.Play(rb.position);
		StopAllParticles();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
			StateSet(State.Despawn);
		}
	}

	private void ActivateTailScroll()
	{
		tailScrollTimer = 0.1f;
	}

	private void UpdateTailScroll()
	{
		if (tailScrollTimer <= 0f)
		{
			tailTrailRenderer.emitting = false;
		}
		if (tailScrollTimer > 0f)
		{
			tailScrollTimer -= Time.deltaTime;
			if (currentState == State.Dive || currentState == State.WrestlePlayer)
			{
				tailScrollTimer -= Time.deltaTime * 10f;
			}
			tailScrollOffset.x -= Time.deltaTime * tailScrollSpeed * 0.5f;
			tailScrollOffset.y += Time.deltaTime * (tailScrollSpeed * 0.08f);
			tailMaterial.SetTextureOffset(MainTex, tailScrollOffset);
			tailTrailRenderer.enabled = true;
			tailTrailRenderer.emitting = true;
		}
	}

	private void ActivateGrabbedPlayerTrailScroll()
	{
		grabbedPlayerTrailScrollTimer = 0.1f;
	}

	private void UpdateGrabbedPlayerTrailScroll()
	{
		if (grabbedPlayerTrailScrollTimer <= 0f)
		{
			grabbedPlayerTrailRenderer.emitting = false;
		}
		if (grabbedPlayerTrailScrollTimer > 0f)
		{
			grabbedPlayerTrailScrollTimer -= Time.deltaTime;
			bool num = currentState == State.WrestlePlayer;
			if (num)
			{
				grabbedPlayerTrailScrollTimer -= Time.deltaTime * 10f;
			}
			if (num && (bool)grabbedPlayer && grabbedPlayerTrailObject != null)
			{
				Vector3 centerPoint = grabbedPlayer.tumble.physGrabObject.centerPoint;
				grabbedPlayerTrailObject.transform.position = centerPoint;
			}
			grabbedPlayerTrailScrollOffset.x -= Time.deltaTime * grabbedPlayerTrailScrollSpeed * 0.5f;
			grabbedPlayerTrailScrollOffset.y += Time.deltaTime * (grabbedPlayerTrailScrollSpeed * 0.08f);
			grabbedPlayerTrailMaterial.SetTextureOffset(MainTex, grabbedPlayerTrailScrollOffset);
			grabbedPlayerTrailRenderer.enabled = true;
			grabbedPlayerTrailRenderer.emitting = true;
		}
	}
}
