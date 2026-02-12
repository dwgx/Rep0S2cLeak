using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public static PlayerController instance;

	private bool previousCrouchingState;

	private bool previousCrawlingState;

	private bool previousSprintingState;

	private bool previousSlidingState;

	private bool previousMovingState;

	public GameObject playerAvatar;

	public GameObject playerAvatarPrefab;

	public PlayerCollision PlayerCollision;

	[HideInInspector]
	public GameObject physGrabObject;

	[HideInInspector]
	public bool physGrabActive;

	[HideInInspector]
	public Transform physGrabPoint;

	[Space]
	public PlayerCollisionController CollisionController;

	public PlayerCollisionGrounded CollisionGrounded;

	private bool GroundedPrevious;

	public Materials.MaterialTrigger MaterialTrigger;

	[HideInInspector]
	public Rigidbody rb;

	private bool CanLand;

	private float landCooldown;

	[Space]
	public float MoveSpeed = 0.5f;

	public float MoveFriction = 5f;

	[HideInInspector]
	public Vector3 Velocity;

	[HideInInspector]
	public Vector3 VelocityRelative;

	private Vector3 VelocityRelativeNew;

	private bool VelocityIdle;

	private Vector3 VelocityImpulse = Vector3.zero;

	[Space]
	public float SprintSpeed = 1f;

	public float SprintSpeedUpgrades;

	private float SprintSpeedCurrent = 1f;

	[HideInInspector]
	public float SprintSpeedLerp;

	public float SprintAcceleration = 1f;

	private float SprintedTimer;

	private float SprintDrainTimer;

	[Space]
	public float CrouchSpeed = 1f;

	public float CrouchTimeMin = 0.2f;

	private float CrouchActiveTimer;

	private float CrouchInactiveTimer;

	[Space]
	public float SlideTime = 1f;

	public float SlideDecay = 0.1f;

	private float SlideTimer;

	private Vector3 SlideDirection;

	private Vector3 SlideDirectionCurrent;

	internal bool CanSlide;

	[HideInInspector]
	public bool Sliding;

	[Space]
	public float JumpForce = 20f;

	internal int JumpExtra;

	private int JumpExtraCurrent;

	private bool JumpFirst;

	public float CustomGravity = 20f;

	internal bool JumpImpulse;

	private float JumpCooldown;

	private float JumpGroundedBuffer;

	internal List<PhysGrabObject> JumpGroundedObjects = new List<PhysGrabObject>();

	private float JumpInputBuffer;

	private float OverrideJumpCooldownAmount;

	private float OverrideJumpCooldownCurrent;

	private float OverrideJumpCooldownTimer;

	public float StepUpForce = 2f;

	public bool DebugNoTumble;

	public bool DebugDisableOvercharge;

	[Space]
	public bool DebugEnergy;

	public float EnergyStart = 100f;

	[HideInInspector]
	public float EnergyCurrent;

	public float EnergySprintDrain = 1f;

	private float sprintRechargeTimer;

	private float sprintRechargeTime = 1f;

	private float sprintRechargeAmount = 2f;

	[Space(15f)]
	public CameraAim cameraAim;

	public GameObject cameraGameObject;

	public GameObject cameraGameObjectLocal;

	public Transform VisionTarget;

	[HideInInspector]
	public bool CanInteract;

	[HideInInspector]
	public bool moving;

	private float movingResetTimer;

	[HideInInspector]
	public bool sprinting;

	[HideInInspector]
	public bool Crouching;

	[HideInInspector]
	public bool Crawling;

	[HideInInspector]
	public Vector3 InputDirection;

	[HideInInspector]
	public AudioSource AudioSource;

	private Vector3 positionPrevious;

	private Vector3 MoveForceDirection = Vector3.zero;

	private float MoveForceAmount;

	private float MoveForceTimer;

	internal float InputDisableTimer;

	private float MoveMultiplier = 1f;

	private float MoveMultiplierTimer;

	internal string playerName;

	internal string playerSteamID;

	private float overrideSpeedTimer;

	internal float overrideSpeedMultiplier = 1f;

	private float overrideLookSpeedTimer;

	internal float overrideLookSpeedTarget = 1f;

	private float overrideLookSpeedTimeIn = 15f;

	private float overrideLookSpeedTimeOut = 0.3f;

	private float overrideLookSpeedLerp;

	private float overrideLookSpeedProgress;

	private float overrideVoicePitchTimer;

	internal float overrideVoicePitchMultiplier = 1f;

	private float overrideTimeScaleTimer;

	internal float overrideTimeScaleMultiplier = 1f;

	private Vector3 originalVelocity;

	private Vector3 originalAngularVelocity;

	public PlayerAvatar playerAvatarScript;

	[Space]
	public Collider col;

	public PhysicMaterial PhysicMaterialMove;

	public PhysicMaterial PhysicMaterialIdle;

	internal float antiGravityTimer;

	internal float featherTimer;

	internal float deathSeenTimer;

	internal float tumbleInputDisableTimer;

	private float kinematicTimer;

	private bool toggleSprint;

	private bool toggleCrouch;

	private float rbOriginalMass;

	private float rbOriginalDrag;

	private float playerOriginalMoveSpeed;

	private float playerOriginalCustomGravity;

	internal float playerOriginalSprintSpeed;

	private float playerOriginalCrouchSpeed;

	internal bool debugSlow;

	private void Awake()
	{
		instance = this;
	}

	public void PlayerSetName(string _playerName, string _steamID)
	{
		playerName = _playerName;
		playerSteamID = _steamID;
	}

	public void MoveForce(Vector3 direction, float amount, float time)
	{
		MoveForceDirection = direction.normalized;
		MoveForceAmount = amount;
		MoveForceTimer = time;
	}

	public void InputDisable(float time)
	{
		InputDisableTimer = time;
	}

	public void MoveMult(float multiplier, float time)
	{
		MoveMultiplier = multiplier;
		MoveMultiplierTimer = time;
	}

	public void CrouchDisable(float time)
	{
		CrouchInactiveTimer = Mathf.Max(time, CrouchInactiveTimer);
	}

	private void OnCollisionEnter(Collision other)
	{
		if (GameManager.instance.gameMode != 0 && !PhotonNetwork.IsMasterClient && other.gameObject.CompareTag("Phys Grab Object"))
		{
			playerAvatarScript.photonView.RPC("ResetPhysPusher", RpcTarget.MasterClient);
		}
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		rbOriginalMass = rb.mass;
		rbOriginalDrag = rb.drag;
		AudioSource = GetComponent<AudioSource>();
		positionPrevious = base.transform.position;
		Inventory component = GetComponent<Inventory>();
		if (SemiFunc.RunIsArena())
		{
			component.enabled = false;
		}
		if (GameManager.instance.gameMode == 0)
		{
			Object.Instantiate(playerAvatarPrefab, base.transform.position, Quaternion.identity);
		}
		StartCoroutine(LateStart());
		if ((bool)DebugCommandHandler.instance && DebugCommandHandler.instance.infiniteEnergy)
		{
			if (SemiFunc.IsMainMenu())
			{
				DebugCommandHandler.instance.infiniteEnergy = false;
			}
			else
			{
				DebugEnergy = true;
			}
		}
	}

	private IEnumerator LateStart()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.2f);
		string key = SemiFunc.PlayerGetSteamID(playerAvatarScript);
		if (StatsManager.instance.playerUpgradeStamina.ContainsKey(key))
		{
			EnergyStart += (float)StatsManager.instance.playerUpgradeStamina[key] * 10f;
			SprintSpeed += StatsManager.instance.playerUpgradeSpeed[key];
			SprintSpeedUpgrades += StatsManager.instance.playerUpgradeSpeed[key];
			JumpExtra = StatsManager.instance.playerUpgradeExtraJump[key];
		}
		EnergyCurrent = EnergyStart;
		playerOriginalMoveSpeed = MoveSpeed;
		playerOriginalSprintSpeed = SprintSpeed;
		playerOriginalCrouchSpeed = CrouchSpeed;
		playerOriginalCustomGravity = CustomGravity;
		if (SemiFunc.MenuLevel())
		{
			rb.isKinematic = true;
			base.gameObject.SetActive(value: false);
		}
	}

	public void ChangeState()
	{
		playerAvatarScript.UpdateState(Crouching, sprinting, Crawling, Sliding, moving);
	}

	public void ForceImpulse(Vector3 force)
	{
		VelocityImpulse += base.transform.InverseTransformDirection(force);
	}

	public void AntiGravity(float _timer)
	{
		antiGravityTimer = _timer;
	}

	public void Feather(float _timer)
	{
		featherTimer = _timer;
	}

	public void Kinematic(float _timer)
	{
		kinematicTimer = _timer;
		rb.isKinematic = true;
		rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
	}

	public void SetCrawl()
	{
		Crouching = true;
		Crawling = true;
		CrouchActiveTimer = CrouchTimeMin;
		sprinting = false;
		Sliding = false;
		moving = false;
		PlayerCollisionStand.instance.SetBlocked();
		CameraCrouchPosition.instance.Lerp = 1f;
		CameraCrouchPosition.instance.Active = true;
		CameraCrouchPosition.instance.ActivePrev = true;
		ChangeState();
	}

	public void OverrideSpeed(float _speedMulti, float _time = 0.1f)
	{
		overrideSpeedTimer = _time;
		overrideSpeedMultiplier = _speedMulti;
	}

	private void OverrideSpeedTick()
	{
		if (overrideSpeedTimer > 0f)
		{
			overrideSpeedTimer -= Time.fixedDeltaTime;
			if (overrideSpeedTimer <= 0f)
			{
				overrideSpeedMultiplier = 1f;
				MoveSpeed = playerOriginalMoveSpeed;
				SprintSpeed = playerOriginalSprintSpeed;
				CrouchSpeed = playerOriginalCrouchSpeed;
			}
		}
	}

	private void OverrideSpeedLogic()
	{
		if (!(overrideSpeedTimer <= 0f))
		{
			MoveSpeed = playerOriginalMoveSpeed * overrideSpeedMultiplier;
			SprintSpeed = playerOriginalSprintSpeed * overrideSpeedMultiplier;
			CrouchSpeed = playerOriginalCrouchSpeed * overrideSpeedMultiplier;
		}
	}

	public void OverrideAnimationSpeed(float _animSpeedMulti, float _timeIn, float _timeOut, float _time = 0.1f)
	{
		playerAvatarScript.OverrideAnimationSpeed(_animSpeedMulti, _timeIn, _timeOut, _time);
	}

	public void OverrideTimeScale(float _timeScaleMulti, float _time = 0.1f)
	{
		overrideTimeScaleTimer = _time;
		overrideTimeScaleMultiplier = _timeScaleMulti;
	}

	private void OverrideTimeScaleTick()
	{
		if (overrideTimeScaleTimer > 0f)
		{
			overrideTimeScaleTimer -= Time.fixedDeltaTime;
			if (overrideTimeScaleTimer <= 0f)
			{
				overrideTimeScaleMultiplier = 1f;
				rb.mass = rbOriginalMass;
				rb.drag = rbOriginalDrag;
				CustomGravity = playerOriginalCustomGravity;
				MoveSpeed = playerOriginalMoveSpeed;
				SprintSpeed = playerOriginalSprintSpeed;
				CrouchSpeed = playerOriginalCrouchSpeed;
				rb.useGravity = true;
			}
		}
	}

	private void OverrideTimeScaleLogic()
	{
		if (!(overrideTimeScaleTimer <= 0f))
		{
			float t = overrideSpeedMultiplier;
			float y = rb.velocity.y;
			rb.velocity = Vector3.Lerp(Vector3.zero, rb.velocity, t);
			rb.velocity = new Vector3(rb.velocity.x, y, rb.velocity.z);
			rb.angularVelocity = Vector3.Lerp(Vector3.zero, rb.angularVelocity, t);
			rb.mass = Mathf.Lerp(0.01f, rbOriginalMass, t);
			rb.drag = Mathf.Lerp((1f + overrideSpeedMultiplier) * 10f, rbOriginalDrag, t);
			CustomGravity = Mathf.Lerp(0.1f, playerOriginalCustomGravity, t);
			MoveSpeed = Mathf.Lerp(0.1f, playerOriginalMoveSpeed, t);
			SprintSpeed = Mathf.Lerp(0.1f, playerOriginalSprintSpeed, t);
			CrouchSpeed = Mathf.Lerp(0.1f, playerOriginalCrouchSpeed, t);
			rb.useGravity = false;
		}
	}

	public void OverrideLookSpeed(float _lookSpeedTarget, float timeIn, float timeOut, float _time = 0.1f)
	{
		overrideLookSpeedTimer = _time;
		overrideLookSpeedTarget = _lookSpeedTarget;
		overrideLookSpeedTimeIn = timeIn;
		overrideLookSpeedTimeOut = timeOut;
	}

	private void OverrideLookSpeedTick()
	{
		if (overrideLookSpeedTimer > 0f)
		{
			overrideLookSpeedTimer -= Time.fixedDeltaTime;
		}
	}

	private void OverrideLookSpeedLogic()
	{
		if (overrideLookSpeedTimer <= 0f && overrideLookSpeedProgress <= 0f)
		{
			return;
		}
		float smooth;
		if (overrideLookSpeedTimer > 0f)
		{
			overrideLookSpeedProgress += Time.fixedDeltaTime / overrideLookSpeedTimeIn;
			overrideLookSpeedProgress = Mathf.Clamp01(overrideLookSpeedProgress);
			overrideLookSpeedLerp = Mathf.SmoothStep(0f, 1f, overrideLookSpeedProgress);
			smooth = Mathf.Lerp(cameraAim.aimSmoothOriginal, overrideLookSpeedTarget, overrideLookSpeedLerp);
		}
		else
		{
			overrideLookSpeedProgress -= Time.fixedDeltaTime / overrideLookSpeedTimeOut;
			overrideLookSpeedProgress = Mathf.Clamp01(overrideLookSpeedProgress);
			overrideLookSpeedLerp = Mathf.SmoothStep(0f, 1f, overrideLookSpeedProgress);
			smooth = Mathf.Lerp(cameraAim.aimSmoothOriginal, overrideLookSpeedTarget, overrideLookSpeedLerp);
			if (overrideLookSpeedProgress <= 0f)
			{
				smooth = cameraAim.aimSmoothOriginal;
			}
		}
		cameraAim.OverrideAimSmooth(smooth, 0.1f);
	}

	public void OverrideVoicePitch(float _voicePitchMulti, float _timeIn, float _timeOut, float _time = 0.1f)
	{
		if ((bool)playerAvatarScript.voiceChat)
		{
			playerAvatarScript.voiceChat.OverridePitch(_voicePitchMulti, _timeIn, _timeOut, _time);
		}
	}

	private void OverrideVoicePitchTick()
	{
		if (overrideVoicePitchTimer > 0f)
		{
			overrideVoicePitchTimer -= Time.fixedDeltaTime;
			if (overrideVoicePitchTimer <= 0f)
			{
				overrideVoicePitchMultiplier = 1f;
			}
		}
	}

	public void OverrideJumpCooldown(float _cooldown)
	{
		OverrideJumpCooldownAmount = _cooldown;
		OverrideJumpCooldownTimer = 0.1f;
	}

	private void FixedUpdate()
	{
		if (GameDirector.instance.currentState != GameDirector.gameState.Main)
		{
			return;
		}
		OverrideSpeedTick();
		OverrideTimeScaleTick();
		OverrideLookSpeedTick();
		OverrideVoicePitchTick();
		if (kinematicTimer > 0f)
		{
			VelocityImpulse = Vector3.zero;
			rb.isKinematic = true;
			kinematicTimer -= Time.fixedDeltaTime;
			if (kinematicTimer <= 0f)
			{
				rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				rb.isKinematic = false;
			}
			return;
		}
		if (playerAvatarScript.isTumbling)
		{
			base.transform.position = playerAvatarScript.tumble.transform.position + Vector3.down * 0.3f;
		}
		if (Crawling != previousCrawlingState)
		{
			ChangeState();
			previousCrawlingState = Crawling;
		}
		if (Crouching != previousCrouchingState)
		{
			ChangeState();
			previousCrouchingState = Crouching;
		}
		if (sprinting != previousSprintingState)
		{
			ChangeState();
			previousSprintingState = sprinting;
		}
		if (Sliding != previousSlidingState)
		{
			ChangeState();
			previousSlidingState = Sliding;
		}
		if (moving != previousMovingState)
		{
			ChangeState();
			previousMovingState = moving;
		}
		base.transform.rotation = Quaternion.Euler(0f, cameraGameObject.transform.localRotation.eulerAngles.y, 0f);
		if ((SemiFunc.InputHold(InputKey.Sprint) || toggleSprint) && !playerAvatarScript.isTumbling && !Crouching && EnergyCurrent >= 1f)
		{
			if (rb.velocity.magnitude > 0.01f)
			{
				CanSlide = true;
				TutorialDirector.instance.playerSprinted = true;
				sprinting = true;
				SprintedTimer = 0.5f;
				SprintDrainTimer = 0.2f;
			}
		}
		else
		{
			if (SprintedTimer > 0f)
			{
				SprintedTimer -= Time.fixedDeltaTime;
				if (SprintedTimer <= 0f)
				{
					CanSlide = false;
					SprintedTimer = 0f;
				}
			}
			SprintSpeedLerp = 0f;
			sprinting = false;
		}
		if (SprintDrainTimer > 0f && !DebugEnergy)
		{
			float energySprintDrain = EnergySprintDrain;
			energySprintDrain += SprintSpeedUpgrades;
			EnergyCurrent -= energySprintDrain * Time.fixedDeltaTime;
			EnergyCurrent = Mathf.Max(0f, EnergyCurrent);
			if (EnergyCurrent <= 0f)
			{
				toggleSprint = false;
			}
			SprintDrainTimer -= Time.fixedDeltaTime;
		}
		if ((Crouching && PlayerCollisionStand.instance.CheckBlocked()) || playerAvatarScript.isTumbling)
		{
			TutorialDirector.instance.playerCrawled = true;
			Crawling = true;
		}
		else
		{
			Crawling = false;
		}
		if (playerAvatarScript.isTumbling || (CollisionController.Grounded && (SemiFunc.InputHold(InputKey.Crouch) || toggleCrouch)))
		{
			if (CrouchInactiveTimer <= 0f)
			{
				if (!Crouching)
				{
					CrouchActiveTimer = CrouchTimeMin;
				}
				TutorialDirector.instance.playerCrouched = true;
				Crouching = true;
				sprinting = false;
			}
		}
		else if (Crouching && CrouchActiveTimer <= 0f && !Crawling)
		{
			Crawling = false;
			Crouching = false;
			CrouchInactiveTimer = CrouchTimeMin;
		}
		if (CrouchActiveTimer > 0f)
		{
			CrouchActiveTimer -= Time.fixedDeltaTime;
		}
		if (CrouchInactiveTimer > 0f)
		{
			CrouchInactiveTimer -= Time.fixedDeltaTime;
		}
		if (sprinting || Crouching)
		{
			CanInteract = false;
		}
		else
		{
			CanInteract = true;
		}
		Vector3 zero = Vector3.zero;
		if (MoveForceTimer > 0f)
		{
			InputDirection = MoveForceDirection;
			MoveForceTimer -= Time.fixedDeltaTime;
			rb.velocity = MoveForceDirection * MoveForceAmount;
		}
		else if (InputDisableTimer <= 0f)
		{
			InputDirection = new Vector3(SemiFunc.InputMovementX(), 0f, SemiFunc.InputMovementY()).normalized;
			if (GameDirector.instance.DisableInput || playerAvatarScript.isTumbling)
			{
				InputDirection = Vector3.zero;
			}
			if (InputDirection.magnitude <= 0.1f)
			{
				SprintSpeedLerp = 0f;
			}
			if (MoveMultiplierTimer > 0f)
			{
				InputDirection *= MoveMultiplier;
			}
			if (sprinting)
			{
				SprintSpeedCurrent = Mathf.Lerp(MoveSpeed, SprintSpeed, SprintSpeedLerp);
				SprintSpeedLerp += SprintAcceleration * Time.fixedDeltaTime;
				SprintSpeedLerp = Mathf.Clamp01(SprintSpeedLerp);
				zero += InputDirection * SprintSpeedCurrent;
				SlideDirection = InputDirection * SprintSpeedCurrent;
				SlideDirectionCurrent = SlideDirection;
				Sliding = false;
			}
			else if (Crouching)
			{
				if (CanSlide)
				{
					playerAvatarScript.Slide();
					if (!DebugEnergy)
					{
						EnergyCurrent -= 5f;
					}
					EnergyCurrent = Mathf.Max(0f, EnergyCurrent);
					CanSlide = false;
					Sliding = true;
					SlideTimer = SlideTime;
				}
				if (SlideTimer > 0f)
				{
					zero += SlideDirectionCurrent;
					SlideDirectionCurrent -= SlideDirection * SlideDecay * Time.fixedDeltaTime;
					SlideTimer -= Time.fixedDeltaTime;
					if (SlideTimer <= 0f)
					{
						Sliding = false;
					}
				}
				if (debugSlow)
				{
					InputDirection *= 0.2f;
				}
				zero += InputDirection * CrouchSpeed;
			}
			else
			{
				if (debugSlow)
				{
					InputDirection *= 0.1f;
				}
				zero += InputDirection * MoveSpeed;
				Sliding = false;
			}
		}
		else
		{
			InputDirection = Vector3.zero;
		}
		if (InputDisableTimer > 0f)
		{
			InputDisableTimer -= Time.fixedDeltaTime;
		}
		if (MoveMultiplierTimer > 0f)
		{
			MoveMultiplierTimer -= Time.fixedDeltaTime;
		}
		if (antiGravityTimer > 0f)
		{
			if (rb.useGravity)
			{
				rb.drag = 2f;
				rb.useGravity = false;
			}
			antiGravityTimer -= Time.fixedDeltaTime;
		}
		else if (!rb.useGravity)
		{
			rb.drag = 0f;
			rb.useGravity = true;
		}
		zero += VelocityImpulse;
		VelocityRelativeNew += VelocityImpulse;
		VelocityImpulse = Vector3.zero;
		if (VelocityIdle)
		{
			VelocityRelativeNew = zero;
		}
		else
		{
			VelocityRelativeNew = Vector3.Lerp(VelocityRelativeNew, zero, MoveFriction * Time.fixedDeltaTime);
		}
		Vector3 vector = base.transform.InverseTransformDirection(rb.velocity);
		if (VelocityRelativeNew.magnitude > 0.1f)
		{
			VelocityIdle = false;
			col.material = PhysicMaterialMove;
			VelocityRelative = Vector3.Lerp(VelocityRelative, VelocityRelativeNew, MoveFriction * Time.fixedDeltaTime);
			VelocityRelative.y = vector.y;
			rb.AddRelativeForce(VelocityRelative - vector, ForceMode.Impulse);
			Velocity = base.transform.InverseTransformDirection(VelocityRelative - vector);
		}
		else
		{
			VelocityIdle = true;
			col.material = PhysicMaterialIdle;
			VelocityRelative = Vector3.zero;
			Velocity = rb.velocity;
		}
		if (!CollisionController.Grounded && !JumpImpulse && featherTimer <= 0f)
		{
			if (rb.useGravity)
			{
				rb.AddForce(new Vector3(0f, (0f - CustomGravity) * Time.fixedDeltaTime, 0f), ForceMode.Impulse);
			}
			else
			{
				rb.AddForce(new Vector3(0f, (0f - CustomGravity * 0.1f) * Time.fixedDeltaTime, 0f), ForceMode.Impulse);
			}
		}
		if (JumpImpulse)
		{
			bool flag = false;
			foreach (PhysGrabObject jumpGroundedObject in JumpGroundedObjects)
			{
				foreach (PhysGrabber item in jumpGroundedObject.playerGrabbing)
				{
					if (item.playerAvatar == playerAvatarScript)
					{
						flag = true;
						item.ReleaseObject(jumpGroundedObject.photonView.ViewID, 1f);
						break;
					}
				}
			}
			Vector3 vector2 = new Vector3(0f, rb.velocity.y, 0f);
			float num = JumpForce;
			if (flag)
			{
				num = JumpForce * 0.5f;
			}
			rb.AddForce(Vector3.up * num - vector2, ForceMode.Impulse);
			JumpCooldown = 0.1f;
			JumpImpulse = false;
			CollisionGrounded.Grounded = false;
			JumpGroundedBuffer = 0f;
			CollisionController.GroundedDisableTimer = 0.1f;
			CollisionController.fallDistance = 0f;
		}
		if (VelocityRelativeNew.magnitude > 0.1f)
		{
			movingResetTimer = 0.1f;
			moving = true;
		}
		else if (movingResetTimer > 0f)
		{
			movingResetTimer -= Time.fixedDeltaTime;
			if (movingResetTimer <= 0f)
			{
				sprinting = false;
				moving = false;
			}
		}
		if (featherTimer > 0f)
		{
			if (rb.useGravity)
			{
				rb.useGravity = false;
			}
			if (antiGravityTimer <= 0f)
			{
				rb.AddForce(new Vector3(0f, -15f, 0f), ForceMode.Force);
			}
			featherTimer -= Time.fixedDeltaTime;
			if (featherTimer <= 0f)
			{
				rb.useGravity = true;
			}
		}
		OverrideTimeScaleLogic();
		OverrideSpeedLogic();
		OverrideLookSpeedLogic();
		positionPrevious = base.transform.position;
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated || SemiFunc.MenuLevel())
		{
			return;
		}
		if (deathSeenTimer > 0f)
		{
			deathSeenTimer -= Time.deltaTime;
		}
		if (CollisionController.Grounded)
		{
			if (InputManager.instance.InputToggleGet(InputKey.Crouch))
			{
				if (SemiFunc.InputDown(InputKey.Crouch))
				{
					toggleCrouch = !toggleCrouch;
					if (toggleCrouch)
					{
						toggleSprint = false;
					}
				}
			}
			else
			{
				toggleCrouch = false;
			}
		}
		if (!playerAvatarScript.isTumbling)
		{
			if (InputManager.instance.InputToggleGet(InputKey.Sprint))
			{
				if (SemiFunc.InputDown(InputKey.Sprint))
				{
					toggleSprint = !toggleSprint;
					if (toggleSprint)
					{
						toggleCrouch = false;
					}
				}
			}
			else
			{
				toggleSprint = false;
			}
		}
		if (sprinting)
		{
			sprintRechargeTimer = sprintRechargeTime;
			if (SemiFunc.RunIsArena())
			{
				sprintRechargeTimer *= 0.5f;
			}
		}
		else if (sprintRechargeTimer > 0f)
		{
			sprintRechargeTimer -= Time.deltaTime;
		}
		else if (EnergyCurrent < EnergyStart)
		{
			float num = sprintRechargeAmount;
			if (SemiFunc.RunIsArena() && !playerAvatarScript.isTumbling)
			{
				num *= 5f;
			}
			if (playerAvatarScript.physGrabber.grabState == PhysGrabber.GrabState.Climb)
			{
				num = 0f;
			}
			EnergyCurrent += num * Time.deltaTime;
			if (EnergyCurrent > EnergyStart)
			{
				EnergyCurrent = EnergyStart;
			}
		}
		if (!JumpImpulse)
		{
			if (SemiFunc.InputDown(InputKey.Jump) && !playerAvatarScript.isTumbling && InputDisableTimer <= 0f && OverrideJumpCooldownCurrent <= 0f)
			{
				JumpInputBuffer = 0.25f;
				if (OverrideJumpCooldownTimer > 0f)
				{
					OverrideJumpCooldownCurrent = OverrideJumpCooldownAmount;
				}
			}
			if (OverrideJumpCooldownTimer > 0f)
			{
				OverrideJumpCooldownTimer -= Time.deltaTime;
			}
			if (OverrideJumpCooldownCurrent > 0f)
			{
				OverrideJumpCooldownCurrent -= Time.deltaTime;
			}
			if (CollisionGrounded.Grounded)
			{
				JumpFirst = true;
				JumpExtraCurrent = JumpExtra;
				JumpGroundedBuffer = 0.25f;
			}
			else if (JumpGroundedBuffer > 0f)
			{
				JumpGroundedBuffer -= Time.deltaTime;
				if (JumpGroundedBuffer <= 0f)
				{
					JumpFirst = false;
				}
			}
			if (JumpInputBuffer > 0f)
			{
				JumpInputBuffer -= Time.deltaTime;
			}
			if (JumpCooldown > 0f)
			{
				JumpCooldown -= Time.deltaTime;
			}
			if (JumpInputBuffer > 0f && (JumpGroundedBuffer > 0f || (!JumpFirst && JumpExtraCurrent > 0)) && JumpCooldown <= 0f)
			{
				if (JumpFirst)
				{
					JumpFirst = false;
					playerAvatarScript.Jump(_powerupEffect: false);
				}
				else
				{
					JumpExtraCurrent--;
					playerAvatarScript.Jump(_powerupEffect: true);
				}
				CameraJump.instance.Jump();
				TutorialDirector.instance.playerJumped = true;
				JumpImpulse = true;
				JumpInputBuffer = 0f;
			}
			if ((JumpGroundedBuffer <= 0f || !CollisionGrounded.onPhysObject) && JumpGroundedObjects.Count > 0)
			{
				JumpGroundedObjects.Clear();
			}
		}
		if (landCooldown > 0f)
		{
			landCooldown -= Time.deltaTime;
		}
		if (rb.velocity.y < -4f || ((bool)playerAvatarScript.tumble && playerAvatarScript.tumble.physGrabObject.rbVelocity.y < -4f))
		{
			CanLand = true;
		}
		if (GroundedPrevious != CollisionController.Grounded)
		{
			if (CollisionController.Grounded && CanLand)
			{
				if (!SemiFunc.MenuLevel() && landCooldown <= 0f)
				{
					landCooldown = 1f;
					CameraJump.instance.Land();
					playerAvatarScript.Land();
				}
				CanLand = false;
			}
			GroundedPrevious = CollisionController.Grounded;
		}
		if (tumbleInputDisableTimer > 0f)
		{
			tumbleInputDisableTimer -= Time.deltaTime;
		}
		if (playerAvatarScript.isTumbling)
		{
			col.enabled = false;
			rb.isKinematic = true;
			bool flag = false;
			if (playerAvatarScript.tumble.notMovingTimer > 0.5f && (Mathf.Abs(SemiFunc.InputMovementX()) > 0f || Mathf.Abs(SemiFunc.InputMovementY()) > 0f))
			{
				flag = true;
			}
			if ((SemiFunc.InputDown(InputKey.Jump) || SemiFunc.InputDown(InputKey.Tumble) || flag) && tumbleInputDisableTimer <= 0f && !playerAvatarScript.tumble.tumbleOverride && InputDisableTimer <= 0f)
			{
				playerAvatarScript.tumble.TumbleRequest(_isTumbling: false, _playerInput: true);
			}
		}
		else
		{
			col.enabled = true;
			rb.isKinematic = false;
			if (SemiFunc.InputDown(InputKey.Tumble) && tumbleInputDisableTimer <= 0f && InputDisableTimer <= 0f)
			{
				TutorialDirector.instance.playerTumbled = true;
				playerAvatarScript.tumble.TumbleRequest(_isTumbling: true, _playerInput: true);
			}
		}
	}

	public void Revive(Vector3 _rotation)
	{
		base.transform.rotation = Quaternion.Euler(0f, _rotation.y, 0f);
		InputDisable(0.5f);
		Kinematic(0.2f);
		SetCrawl();
		CollisionController.ResetFalling();
		VelocityIdle = true;
		col.material = PhysicMaterialIdle;
		VelocityRelative = Vector3.zero;
		Velocity = Vector3.zero;
		EnergyCurrent = EnergyStart;
	}
}
