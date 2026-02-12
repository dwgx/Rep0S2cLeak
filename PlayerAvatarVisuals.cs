using UnityEngine;

public class PlayerAvatarVisuals : MonoBehaviour
{
	public bool isMenuAvatar;

	[Space]
	public PlayerAvatar playerAvatar;

	public GameObject meshParent;

	internal Animator animator;

	private bool animSprinting;

	private bool animSliding;

	private bool animSlidingImpulse;

	private bool animJumping;

	private bool animJumpingImpulse;

	private float animJumpTimer;

	private float animJumpedTimer;

	private float animFallingTimer;

	internal bool animInCrawl;

	internal bool animTumbling;

	internal PlayerAvatarTalkAnimation playerAvatarTalkAnimation;

	internal PlayerAvatarRightArm playerAvatarRightArm;

	[Space]
	public Transform headUpTransform;

	public Transform headSideTransform;

	public Transform TTSTransform;

	[Space]
	public Transform bodyTopUpTransform;

	public Transform bodyTopSideTransform;

	[Space]
	public GameObject PhysRiderPoint;

	public PlayerEyes playerEyes;

	private GameObject PhysRiderPointInstance;

	[Space]
	public ParticleSystem[] powerupJumpEffect;

	public ParticleSystem[] tumbleBreakFreeEffect;

	[Space]
	public Transform effectGetIntoTruck;

	private float effectGetIntoTruckTimer;

	[Space]
	public GameObject arenaCrown;

	public Transform leanTransform;

	public SpringQuaternion leanSpring;

	private Vector3 leanSpringTargetPrevious;

	[Space]
	public Transform tiltTransform;

	public SpringQuaternion tiltSpring;

	private bool tiltSprinting;

	private float tiltTimer;

	private Vector3 tiltTarget;

	[Space]
	public SpringQuaternion bodySpring;

	[HideInInspector]
	public Quaternion bodySpringTarget;

	public Transform legTwistTransform;

	public SpringQuaternion legTwistSpring;

	private bool legTwistActive;

	public Transform headLookAtTransform;

	public SpringFloat lookUpSpring;

	public SpringQuaternion lookSideSpring;

	public Transform attachPointJawTop;

	public Transform attachPointJawBottom;

	public Transform attachPointTopHeadMiddle;

	public Transform attachNeck;

	private Vector3 positionLast;

	internal Vector3 visualPosition = Vector3.zero;

	private float visualFollowLerp;

	internal float turnDifference;

	private float turnDifferenceResetTimer;

	internal float turnDirection;

	private float turnPrevious;

	private float headSideSteer;

	internal float upDifference;

	internal float upDirection;

	private float upPrevious;

	private float headTiltOverrideAmount;

	private float headTiltOverrideTimer;

	internal float animationSpeedMultiplier = 1f;

	internal float deltaTime;

	internal Color color;

	internal bool colorSet;

	internal int colorIndex = -1;

	private bool crownSetterWasHere;

	internal bool expressionAvatar;

	private void Start()
	{
		playerAvatarRightArm = GetComponentInChildren<PlayerAvatarRightArm>();
		playerAvatarTalkAnimation = GetComponentInChildren<PlayerAvatarTalkAnimation>();
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
		if (!isMenuAvatar && (!GameManager.Multiplayer() || ((bool)this.playerAvatar && this.playerAvatar.photonView.IsMine)))
		{
			animator.enabled = false;
			meshParent.SetActive(value: false);
		}
		if (!SemiFunc.IsMultiplayer() || SemiFunc.RunIsArena())
		{
			return;
		}
		PlayerAvatar playerAvatar = SessionManager.instance.CrownedPlayerGet();
		if (!isMenuAvatar)
		{
			if (playerAvatar == this.playerAvatar)
			{
				arenaCrown.SetActive(value: true);
			}
		}
		else if (playerAvatar == PlayerAvatar.instance)
		{
			arenaCrown.SetActive(value: true);
		}
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (SemiFunc.FPSImpulse5() && !crownSetterWasHere && (bool)PlayerCrownSet.instance && PlayerCrownSet.instance.crownOwnerFetched)
		{
			if ((bool)playerAvatar && PlayerCrownSet.instance.crownOwnerSteamID == playerAvatar.steamID)
			{
				arenaCrown.SetActive(value: true);
			}
			crownSetterWasHere = true;
		}
		deltaTime = Time.deltaTime * animationSpeedMultiplier;
		deltaTime = Mathf.Max(deltaTime, 0f);
		if (isMenuAvatar)
		{
			MenuAvatarGetColorsFromRealAvatar();
		}
		if (!isMenuAvatar && playerAvatar.isDisabled)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		if (!isMenuAvatar)
		{
			if (!GameManager.Multiplayer() || playerAvatar.photonView.IsMine)
			{
				if ((bool)playerAvatar)
				{
					base.transform.position = playerAvatar.transform.position;
					base.transform.rotation = playerAvatar.transform.rotation;
				}
			}
			else
			{
				if (playerAvatar.isTumbling && (bool)playerAvatar.tumble)
				{
					visualFollowLerp = 0f;
					visualPosition = playerAvatar.tumble.followPosition.position;
					bodySpringTarget = playerAvatar.tumble.followPosition.rotation;
					playerAvatar.clientPosition = visualPosition;
					playerAvatar.clientPositionCurrent = visualPosition;
				}
				else if (!playerAvatar.clientPhysRiding || !PhysRiderPointInstance)
				{
					float num = Mathf.Lerp(0f, 25f, visualFollowLerp);
					visualFollowLerp = Mathf.Clamp01(visualFollowLerp + 2f * deltaTime);
					visualPosition = Vector3.Lerp(visualPosition, playerAvatar.clientPositionCurrent, num * deltaTime);
				}
				else if ((bool)PhysRiderPointInstance)
				{
					float num2 = Mathf.Lerp(0f, 25f, visualFollowLerp);
					visualFollowLerp = Mathf.Clamp01(visualFollowLerp + 2f * deltaTime);
					visualPosition = Vector3.Lerp(visualPosition, PhysRiderPointInstance.transform.position, num2 * deltaTime);
					playerAvatar.clientPosition = visualPosition;
					playerAvatar.clientPositionCurrent = visualPosition;
				}
				if (!playerAvatar.isTumbling)
				{
					if (animSliding)
					{
						if (animSlidingImpulse && playerAvatar.rbVelocity.magnitude > 0.1f)
						{
							bodySpringTarget = Quaternion.LookRotation(base.transform.TransformDirection(playerAvatar.rbVelocity).normalized, Vector3.up);
						}
					}
					else
					{
						bodySpringTarget = playerAvatar.clientRotationCurrent;
					}
					base.transform.rotation = SemiFunc.SpringQuaternionGet(bodySpring, bodySpringTarget, deltaTime);
				}
				else if (playerAvatar.tumble.tumbleSetTimer <= 0f)
				{
					bodySpring.lastRotation = bodySpringTarget;
					base.transform.rotation = bodySpringTarget;
				}
				base.transform.position = visualPosition;
				if (playerAvatar.playerHealth.hurtFreeze)
				{
					animator.speed = 0f;
					return;
				}
				if (turnDifferenceResetTimer > 0f)
				{
					turnDifferenceResetTimer -= Time.deltaTime;
					if (turnDifferenceResetTimer <= 0f)
					{
						turnDifference = 0f;
					}
				}
				float num3 = Quaternion.Angle(Quaternion.Euler(0f, turnPrevious, 0f), Quaternion.Euler(0f, bodySpringTarget.eulerAngles.y, 0f));
				if (num3 != 0f)
				{
					turnDifferenceResetTimer = 0.2f;
					turnDifference = num3;
				}
				float num4 = turnPrevious - bodySpringTarget.eulerAngles.y;
				if (Mathf.Abs(num4) < 180f && num4 != 0f)
				{
					turnDirection = Mathf.Sign(num4);
				}
				if (playerAvatar.isTumbling)
				{
					turnDifference = 0f;
				}
				turnPrevious = bodySpringTarget.eulerAngles.y;
			}
		}
		if (!isMenuAvatar && (!GameManager.Multiplayer() || playerAvatar.photonView.IsMine))
		{
			return;
		}
		if ((bool)playerEyes && playerEyes.lookAtActive && GameDirector.instance.currentState == GameDirector.gameState.Main && (bool)playerAvatar && (bool)playerAvatar.PlayerVisionTarget && (bool)playerAvatar.PlayerVisionTarget.VisionTransform)
		{
			Vector3 vector = playerAvatar.PlayerVisionTarget.VisionTransform.position;
			Vector3 forward = playerAvatar.localCamera.transform.forward;
			if ((bool)playerAvatar.tumble && playerAvatar.tumble.isTumbling)
			{
				forward = playerAvatar.tumble.transform.forward;
			}
			if (isMenuAvatar)
			{
				vector = base.transform.position + Vector3.up * 1.5f;
				forward = base.transform.forward;
			}
			Vector3 direction = playerEyes.lookAt.position - vector;
			direction = SemiFunc.ClampDirection(direction, forward, 40f);
			headLookAtTransform.rotation = Quaternion.Slerp(headLookAtTransform.rotation, Quaternion.LookRotation(direction), deltaTime * 15f);
		}
		else
		{
			headLookAtTransform.localRotation = Quaternion.Slerp(headLookAtTransform.localRotation, Quaternion.identity, deltaTime * 15f);
		}
		float num5 = 0f;
		if (!playerAvatar.isTumbling && !isMenuAvatar)
		{
			num5 = playerAvatar.localCamera.transform.eulerAngles.x;
		}
		if (headTiltOverrideTimer > 0f)
		{
			num5 += headTiltOverrideAmount;
			headTiltOverrideAmount = 0f;
		}
		if (num5 > 90f)
		{
			num5 -= 360f;
		}
		if (playerAvatar.isCrawling)
		{
			num5 *= 0.4f;
		}
		else if (playerAvatar.isCrouching)
		{
			num5 *= 0.75f;
		}
		float num6 = headLookAtTransform.localEulerAngles.x;
		if (num6 > 90f)
		{
			num6 -= 360f;
		}
		if (isMenuAvatar)
		{
			num6 *= 1.25f;
		}
		num5 += num6;
		float num7 = SemiFunc.SpringFloatGet(_targetFloat: playerAvatar.isCrouching ? Mathf.Clamp(num5, -40f, 40f) : ((!playerAvatar.isCrouching) ? Mathf.Clamp(num5, -75f, 85f) : Mathf.Clamp(num5, -60f, 65f)), _attributes: lookUpSpring, _deltaTime: deltaTime);
		headUpTransform.localRotation = Quaternion.Euler(num7 * 0.5f, 0f, 0f);
		bodyTopUpTransform.localRotation = Quaternion.Euler(num7 * 0.25f, 0f, 0f);
		upDifference = Quaternion.Angle(Quaternion.Euler(upPrevious, 0f, 0f), Quaternion.Euler(headUpTransform.eulerAngles.x, 0f, 0f));
		float f = upPrevious - headUpTransform.eulerAngles.x;
		if (Mathf.Abs(f) < 180f)
		{
			upDirection = Mathf.Sign(f);
		}
		upPrevious = headUpTransform.eulerAngles.x;
		headTiltOverrideTimer -= deltaTime;
		float value = 0f;
		if (turnDifference > 1f)
		{
			value = turnDifference * 5f * (0f - turnDirection);
		}
		value = Mathf.Clamp(value, -100f, 100f);
		headSideSteer = Mathf.Lerp(headSideSteer, value, 20f * deltaTime);
		Quaternion quaternion = Quaternion.Euler(0f, headLookAtTransform.localRotation.eulerAngles.y + headSideSteer, 0f);
		quaternion = Quaternion.Slerp(Quaternion.identity, quaternion, 0.5f);
		Quaternion localRotation = SemiFunc.SpringQuaternionGet(lookSideSpring, quaternion, deltaTime);
		headSideTransform.localRotation = localRotation;
		bodyTopSideTransform.localRotation = Quaternion.Slerp(Quaternion.identity, localRotation, 0.5f);
		Vector3 zero = Vector3.zero;
		if (isMenuAvatar)
		{
			if ((bool)PlayerAvatarMenu.instance && (bool)PlayerAvatarMenu.instance.rb && Mathf.Abs(PlayerAvatarMenu.instance.rb.angularVelocity.magnitude) > 1f)
			{
				zero.z = PlayerAvatarMenu.instance.rb.angularVelocity.y * 0.01f;
			}
		}
		else if (playerAvatar.rbVelocity.magnitude > 0.1f)
		{
			Vector3 vector2 = base.transform.TransformDirection(playerAvatar.rbVelocity);
			if (Vector3.Dot(vector2.normalized, base.transform.forward) < -0.5f)
			{
				zero.x = -3f;
			}
			if (Vector3.Dot(vector2.normalized, base.transform.forward) > 0.5f)
			{
				zero.x = 3f;
			}
			if (Vector3.Dot(vector2.normalized, base.transform.right) > 0.5f)
			{
				zero.z = -3f;
			}
			if (Vector3.Dot(vector2.normalized, base.transform.right) < -0.5f)
			{
				zero.z = 3f;
			}
		}
		if (tiltSprinting != animSprinting)
		{
			if (tiltSprinting)
			{
				tiltTimer = 0.25f;
				tiltTarget = leanSpringTargetPrevious * 2f;
			}
			else
			{
				tiltTimer = 0.25f;
				tiltTarget = zero * 3f;
			}
			tiltSprinting = animSprinting;
		}
		leanTransform.localRotation = SemiFunc.SpringQuaternionGet(leanSpring, Quaternion.Euler(zero), deltaTime);
		tiltTransform.localRotation = SemiFunc.SpringQuaternionGet(tiltSpring, Quaternion.Euler(tiltTarget), deltaTime);
		if (tiltTimer > 0f)
		{
			tiltTimer -= deltaTime;
			if (tiltTimer <= 0f)
			{
				tiltTarget = Vector3.zero;
			}
		}
		leanSpringTargetPrevious = zero;
		bool flag = false;
		float speed = 15f;
		float damping = 0.5f;
		Vector3 vector3 = Vector3.zero;
		if (isMenuAvatar)
		{
			if ((bool)PlayerAvatarMenu.instance && (bool)PlayerAvatarMenu.instance.rb && Mathf.Abs(PlayerAvatarMenu.instance.rb.angularVelocity.magnitude) > 1f)
			{
				flag = true;
				speed = 10f;
				damping = 0.7f;
				Vector3 vector4 = Quaternion.Euler(0f, (0f - PlayerAvatarMenu.instance.rb.angularVelocity.y) * 0.1f, 0f) * Vector3.forward;
				vector4.y = 0f;
				vector3 = vector4;
			}
		}
		else if (playerAvatar.isMoving && !animJumping && playerAvatar.rbVelocity.magnitude > 0.1f)
		{
			flag = true;
			speed = 10f;
			damping = 0.7f;
			Vector3 normalized = playerAvatar.rbVelocity.normalized;
			normalized.y = 0f;
			vector3 = normalized;
		}
		if (legTwistActive != flag)
		{
			legTwistActive = flag;
			legTwistSpring.speed = speed;
			legTwistSpring.damping = damping;
		}
		else
		{
			legTwistSpring.speed = Mathf.Lerp(legTwistSpring.speed, speed, deltaTime * 5f);
			legTwistSpring.damping = Mathf.Lerp(legTwistSpring.damping, damping, deltaTime * 5f);
		}
		Quaternion targetRotation = Quaternion.identity;
		if (vector3 != Vector3.zero)
		{
			targetRotation = Quaternion.LookRotation(vector3, Vector3.up);
		}
		legTwistTransform.localRotation = SemiFunc.SpringQuaternionGet(legTwistSpring, targetRotation, deltaTime);
		AnimationLogic();
	}

	public void HeadTiltOverride(float _amount)
	{
		headTiltOverrideAmount += _amount;
		headTiltOverrideTimer = 0.1f;
	}

	public void HeadTiltImpulse(float _amount)
	{
		lookUpSpring.springVelocity += _amount;
	}

	private void MenuAvatarGetColorsFromRealAvatar()
	{
		if (isMenuAvatar && !playerAvatar)
		{
			playerAvatar = PlayerAvatar.instance;
		}
		if ((bool)playerAvatar && playerAvatar.playerAvatarVisuals.color != color)
		{
			SetColor(-1, playerAvatar.playerAvatarVisuals.color);
		}
	}

	private void OnDestroy()
	{
		Object.Destroy(PhysRiderPointInstance);
	}

	private void AnimationLogic()
	{
		if (isMenuAvatar && !expressionAvatar && (bool)PlayerAvatarMenu.instance && (bool)PlayerAvatarMenu.instance.rb)
		{
			if (Mathf.Abs(PlayerAvatarMenu.instance.rb.angularVelocity.magnitude) > 1f)
			{
				animator.SetBool("Turning", value: true);
			}
			else
			{
				animator.SetBool("Turning", value: false);
			}
		}
		else
		{
			if (isMenuAvatar)
			{
				return;
			}
			bool flag = false;
			if (playerAvatar.isTumbling)
			{
				if (!animSprinting && !animTumbling)
				{
					animator.SetTrigger("TumblingImpulse");
					animTumbling = true;
				}
				if ((playerAvatar.tumble.physGrabObject.rbVelocity.magnitude > 1f && !playerAvatar.tumble.physGrabObject.impactDetector.inCart) || playerAvatar.tumble.physGrabObject.rbAngularVelocity.magnitude > 1f)
				{
					animator.SetBool("TumblingMove", value: true);
					flag = true;
				}
				else
				{
					animator.SetBool("TumblingMove", value: false);
				}
				animator.SetBool("Tumbling", value: true);
			}
			else
			{
				animator.SetBool("Tumbling", value: false);
				animator.SetBool("TumblingMove", value: false);
				animTumbling = false;
			}
			if (playerAvatarRightArm.poseNew == playerAvatarRightArm.grabberPose)
			{
				animator.SetBool("Grabbing", value: true);
			}
			else
			{
				animator.SetBool("Grabbing", value: false);
			}
			if (playerAvatar.isCrouching || playerAvatar.isTumbling)
			{
				animator.SetBool("Crouching", value: true);
			}
			else
			{
				animator.SetBool("Crouching", value: false);
			}
			if (playerAvatar.isCrawling || playerAvatar.isTumbling)
			{
				animator.SetBool("Crawling", value: true);
			}
			else
			{
				animator.SetBool("Crawling", value: false);
			}
			if (animator.GetCurrentAnimatorStateInfo(0).IsName("Crouch to Crawl") || animator.GetCurrentAnimatorStateInfo(0).IsName("Crawl") || animator.GetCurrentAnimatorStateInfo(0).IsName("Crawl Move") || animator.GetCurrentAnimatorStateInfo(0).IsName("Slide"))
			{
				animInCrawl = true;
			}
			else
			{
				animInCrawl = false;
			}
			if (playerAvatar.isMoving && !animJumping)
			{
				animator.SetBool("Moving", value: true);
			}
			else
			{
				animator.SetBool("Moving", value: false);
			}
			if (!playerAvatar.isMoving && !animJumping && Mathf.Abs(turnDifference) > 0.5f)
			{
				animator.SetBool("Turning", value: true);
			}
			else
			{
				animator.SetBool("Turning", value: false);
			}
			if (playerAvatar.isSprinting && !animJumping && !animTumbling)
			{
				if (!animSprinting && !animSliding)
				{
					animator.SetTrigger("SprintingImpulse");
					animSprinting = true;
				}
				animator.SetBool("Sprinting", value: true);
			}
			else
			{
				animator.SetBool("Sprinting", value: false);
				animSprinting = false;
			}
			animSlidingImpulse = false;
			if (playerAvatar.isSliding && !animJumping && !animTumbling)
			{
				if (!animSliding)
				{
					animSlidingImpulse = true;
					animator.SetTrigger("SlidingImpulse");
				}
				animator.SetBool("Sliding", value: true);
				animSliding = true;
			}
			else
			{
				animator.SetBool("Sliding", value: false);
				animSliding = false;
			}
			if (animJumping)
			{
				if (animJumpingImpulse)
				{
					animJumpTimer = 0.2f;
					animJumpingImpulse = false;
					animator.SetTrigger("JumpingImpulse");
					animator.SetBool("Jumping", value: true);
					animator.SetBool("Falling", value: false);
				}
				else if (playerAvatar.rbVelocityRaw.y < -0.5f && animJumpTimer <= 0f)
				{
					animator.SetBool("Falling", value: true);
				}
				if (playerAvatar.isGrounded && animJumpTimer <= 0f)
				{
					animJumpedTimer = 0.5f;
					animJumping = false;
				}
				animJumpTimer -= deltaTime;
			}
			else
			{
				animator.SetBool("Jumping", value: false);
				animator.SetBool("Falling", value: false);
			}
			if (animJumpedTimer > 0f)
			{
				animJumpedTimer -= deltaTime;
			}
			if (!playerAvatar.isGrounded)
			{
				animFallingTimer += deltaTime;
			}
			else
			{
				animFallingTimer = 0f;
			}
			if (!playerAvatar.isCrawling && !animJumping && !animSliding && !animTumbling && animFallingTimer > 0.25f && animJumpedTimer <= 0f)
			{
				animJumpTimer = 0.2f;
				animJumping = true;
				animJumpingImpulse = false;
				animator.SetTrigger("FallingImpulse");
				animator.SetBool("Jumping", value: true);
				animator.SetBool("Falling", value: true);
			}
			if (flag)
			{
				float value = Mathf.Max(playerAvatar.tumble.physGrabObject.rbVelocity.magnitude, playerAvatar.tumble.physGrabObject.rbAngularVelocity.magnitude) * 0.5f;
				value = Mathf.Clamp(value, 0.5f, 1.25f);
				animator.speed = value * animationSpeedMultiplier;
				playerAvatar.tumble.TumbleMoveSoundSet(flag, value);
			}
			else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Sprint"))
			{
				float num = 1f + (float)StatsManager.instance.playerUpgradeSpeed[playerAvatar.steamID] * 0.1f;
				animator.speed = num * animationSpeedMultiplier;
			}
			else if (playerAvatar.isMoving && playerAvatar.mapToolController.Active)
			{
				animator.speed = 0.5f * animationSpeedMultiplier;
			}
			else
			{
				animator.speed = 1f * animationSpeedMultiplier;
			}
		}
	}

	public void JumpImpulse()
	{
		if (!playerAvatar.isCrawling && !animTumbling)
		{
			animJumpingImpulse = true;
			animJumping = true;
		}
	}

	public void PhysRidingCheck()
	{
		bool flag = PhysRiderPointInstance != null;
		if (flag && PhysRiderPointInstance.transform.parent != playerAvatar.clientPhysRidingTransform)
		{
			Object.Destroy(PhysRiderPointInstance);
			flag = false;
		}
		if (!flag)
		{
			PhysRiderPointInstance = Object.Instantiate(PhysRiderPoint, Vector3.zero, Quaternion.identity, playerAvatar.clientPhysRidingTransform);
		}
		PhysRiderPointInstance.transform.localPosition = playerAvatar.clientPhysRidingPosition;
	}

	public void SetColor(int _colorIndex, Color _setColor = default(Color))
	{
		bool flag = false;
		Color value;
		if (_colorIndex != -1)
		{
			value = AssetManager.instance.playerColors[_colorIndex];
		}
		else
		{
			value = _setColor;
			flag = true;
		}
		colorIndex = _colorIndex;
		int nameID = Shader.PropertyToID("_AlbedoColor");
		color = value;
		if (!flag)
		{
			playerAvatar.playerHealth.bodyMaterial.SetColor(nameID, value);
		}
		else
		{
			PlayerHealth componentInParent = GetComponentInParent<PlayerHealth>();
			if ((bool)componentInParent)
			{
				componentInParent.bodyMaterial.SetColor(nameID, value);
			}
		}
		if (SemiFunc.RunIsLobbyMenu() && (bool)MenuPageLobby.instance)
		{
			foreach (MenuPlayerListed menuPlayerListed in MenuPageLobby.instance.menuPlayerListedList)
			{
				if (menuPlayerListed.playerAvatar == playerAvatar)
				{
					menuPlayerListed.playerHead.SetColor(value);
					break;
				}
			}
		}
		colorSet = true;
	}

	public void Revive()
	{
		bodySpringTarget = playerAvatar.clientRotationCurrent;
		bodySpring.lastRotation = bodySpringTarget;
		turnPrevious = bodySpringTarget.eulerAngles.y;
		playerAvatar.isCrawling = true;
		playerAvatar.isCrouching = true;
		playerAvatar.isTumbling = false;
		playerAvatar.isMoving = false;
		playerAvatar.isSprinting = false;
		visualFollowLerp = 1f;
		animator.Play("Crawl");
		animInCrawl = true;
		animator.SetBool("Crouching", value: true);
		animator.SetBool("Crawling", value: true);
		animator.SetBool("Moving", value: false);
		animator.SetBool("Sprinting", value: false);
		animator.SetBool("Sliding", value: false);
		animator.SetBool("Jumping", value: false);
		animator.SetBool("Falling", value: false);
		animator.SetBool("Turning", value: false);
		animator.SetBool("Tumbling", value: false);
	}

	public void PowerupJumpEffect()
	{
		ParticleSystem[] array = powerupJumpEffect;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	public void TumbleBreakFreeEffect()
	{
		ParticleSystem[] array = tumbleBreakFreeEffect;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	public void FootstepLight()
	{
		playerAvatar.Footstep(Materials.SoundType.Light);
	}

	public void FootstepMedium()
	{
		if (!isMenuAvatar)
		{
			playerAvatar.Footstep(Materials.SoundType.Medium);
		}
	}

	public void FootstepHeavy()
	{
		playerAvatar.Footstep(Materials.SoundType.Heavy);
	}

	public void StandToCrouch()
	{
		if ((bool)playerAvatar)
		{
			playerAvatar.StandToCrouch();
		}
	}

	public void CrouchToStand()
	{
		if ((bool)playerAvatar)
		{
			playerAvatar.CrouchToStand();
		}
	}

	public void CrouchToCrawl()
	{
		playerAvatar.CrouchToCrawl();
	}

	public void CrawlToCrouch()
	{
		playerAvatar.CrawlToCrouch();
	}
}
