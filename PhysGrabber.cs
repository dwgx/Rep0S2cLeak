using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PhysGrabber : MonoBehaviour, IPunObservable
{
	private enum ColorState
	{
		Orange,
		Green,
		Purple,
		Blue
	}

	public enum GrabState
	{
		None,
		Phys,
		Heal,
		Static,
		Climb
	}

	private Camera playerCamera;

	[HideInInspector]
	public float grabRange = 4f;

	[HideInInspector]
	public float grabReleaseDistance = 8f;

	public static PhysGrabber instance;

	[Space]
	[HideInInspector]
	public float minDistanceFromPlayer = 1f;

	[HideInInspector]
	public float maxDistanceFromPlayer = 2.5f;

	private float minDistanceFromPlayerOriginal = 1f;

	[Space]
	public PhysGrabBeam physGrabBeamComponent;

	public GameObject physGrabBeam;

	private PhysGrabBeam physGrabBeamScript;

	public Transform physGrabPoint;

	public Transform physGrabPointPuller;

	public Transform physGrabPointPlane;

	public Transform climbStickTransform;

	public Transform climbTargetPositionTransform;

	private GameObject physGrabPointVisual1;

	private GameObject physGrabPointVisual2;

	internal Vector3 grabbedcObjectPrevCamRelForward;

	internal Vector3 grabbedObjectPrevCamRelUp;

	internal PhysGrabObject grabbedPhysGrabObject;

	internal int grabbedPhysGrabObjectColliderID;

	internal Collider grabbedPhysGrabObjectCollider;

	internal StaticGrabObject grabbedStaticGrabObject;

	internal Rigidbody grabbedObject;

	[HideInInspector]
	public Transform grabbedObjectTransform;

	[HideInInspector]
	public float physGrabPointPullerDampen = 80f;

	[HideInInspector]
	public float springConstant = 0.9f;

	[HideInInspector]
	public float dampingConstant = 0.5f;

	[HideInInspector]
	public float forceConstant = 4f;

	[HideInInspector]
	public float forceMax = 4f;

	internal bool physGrabBeamActive;

	[HideInInspector]
	public PhotonView photonView;

	[HideInInspector]
	public bool isLocal;

	[HideInInspector]
	public bool grabbed;

	internal float grabDisableTimer;

	[HideInInspector]
	public Vector3 physGrabPointPosition;

	[HideInInspector]
	public Vector3 physGrabPointPullerPosition;

	[HideInInspector]
	public PlayerAvatar playerAvatar;

	[HideInInspector]
	public Vector3 localGrabPosition;

	[HideInInspector]
	public Vector3 cameraRelativeGrabbedForward;

	[HideInInspector]
	public Vector3 cameraRelativeGrabbedUp;

	[HideInInspector]
	public Vector3 cameraRelativeGrabbedRight;

	private Transform physGrabPointVisualRotate;

	[HideInInspector]
	public Transform physGrabPointVisualGrid;

	[HideInInspector]
	public GameObject physGrabPointVisualGridObject;

	private List<GameObject> physGrabPointVisualGridObjects = new List<GameObject>();

	private float overrideMinimumGrabDistance;

	private float overrideMinimumGrabDistanceTimer;

	internal Vector3 currentGrabForce = Vector3.zero;

	internal Vector3 currentTorqueForce = Vector3.zero;

	private int prevColorState = -1;

	[HideInInspector]
	public int colorState;

	private float colorStateOverrideTimer;

	[Space]
	public LayerMask maskLayers;

	internal bool healing;

	internal ItemAttributes currentlyLookingAtItemAttributes;

	internal PhysGrabObject currentlyLookingAtPhysGrabObject;

	internal StaticGrabObject currentlyLookingAtStaticGrabObject;

	[Space]
	public Material physGrabBeamMaterial;

	public Material physGrabBeamMaterialBatteryCharge;

	[HideInInspector]
	public float physGrabForcesDisabledTimer;

	[HideInInspector]
	public float initialPressTimer;

	private bool overrideGrabRelease;

	private float overrideGrabTimer;

	private PhysGrabObject overrideGrabTarget;

	private float overrideBeamColorTimer;

	private Color overrideBeamColor = Color.white;

	private Color currentBeamColor = Color.white;

	private float overrideOverchargeDisableTimer;

	private float physGrabBeamAlpha = 1f;

	private float physGrabBeamAlphaChangeTo = 1f;

	private float physGramBeamAlphaTimer;

	private float physGrabBeamAlphaChangeProgress;

	private float physGrabBeamAlphaOriginal;

	private float overrideGrabDistance;

	private float overrideGrabDistanceTimer;

	private float overrideDisableRotationControlsTimer;

	private bool overrideDisableRotationControls;

	private LayerMask mask;

	private float grabCheckTimer;

	internal float pullerDistance;

	[Space]
	public Transform grabberAudioTransform;

	public Sound startSound;

	public Sound loopSound;

	public Sound stopSound;

	private float physRotatingTimer;

	internal Quaternion physRotation;

	private Quaternion physRotationBase;

	[HideInInspector]
	public Vector3 mouseTurningVelocity;

	[HideInInspector]
	public float grabStrength = 1f;

	[HideInInspector]
	public float throwStrength;

	internal bool debugStickyGrabber;

	[HideInInspector]
	public float stopRotationTimer;

	[HideInInspector]
	public Quaternion nextPhysRotation;

	[HideInInspector]
	public bool isRotating;

	private float isRotatingTimer;

	internal bool isPushing;

	internal bool isPulling;

	private float isPushingTimer;

	private float isPullingTimer;

	private float prevPullerDistance;

	internal bool prevGrabbed;

	private bool toggleGrab;

	private float toggleGrabTimer;

	private float overrideGrabPointTimer;

	private Transform overrideGrabPointTransform;

	internal byte physGrabBeamOverCharge;

	internal float physGrabBeamOverChargeFloat;

	private float physGrabBeamOverChargeAmount;

	private float physGrabBeamOverChargeTimer;

	private float physGrabBeamOverchargeDecreaseCooldown;

	private float physGrabBeamOverchargeInitialBoostCooldown;

	private float test;

	private Vector3 grabClimbPos = Vector3.zero;

	private Collider grabClimbCollider;

	private Vector3 grabClimbColliderPosition;

	private bool grabStateStart;

	private Vector3 wallClimbPosition;

	private float climbStrengthEase;

	private SpringQuaternion climbSpring;

	[HideInInspector]
	public GrabState grabState;

	private void Start()
	{
		minDistanceFromPlayerOriginal = minDistanceFromPlayer;
		StartCoroutine(LateStart());
		physGrabBeamScript = physGrabBeam.GetComponent<PhysGrabBeam>();
		physRotation = Quaternion.identity;
		physRotationBase = Quaternion.identity;
		mask = (int)SemiFunc.LayerMaskGetVisionObstruct() - LayerMask.GetMask("Player");
		playerAvatar = GetComponent<PlayerAvatar>();
		photonView = GetComponent<PhotonView>();
		climbSpring = new SpringQuaternion();
		climbSpring.damping = 0.5f;
		climbSpring.speed = 20f;
		if (GameManager.instance.gameMode == 0 || photonView.IsMine)
		{
			isLocal = true;
			instance = this;
		}
		foreach (Transform item in physGrabPoint)
		{
			if (item.name == "Visual1")
			{
				physGrabPointVisual1 = item.gameObject;
				foreach (Transform item2 in item)
				{
					if (item2.name == "Visual2")
					{
						physGrabPointVisual2 = item2.gameObject;
					}
				}
			}
			if (item.name == "Rotate")
			{
				physGrabPointVisualRotate = item;
				item.GetComponent<PhysGrabPointRotate>().physGrabber = this;
			}
			if (!(item.name == "Grid"))
			{
				continue;
			}
			physGrabPointVisualGrid = item;
			foreach (Transform item3 in item)
			{
				physGrabPointVisualGridObject = item3.gameObject;
				physGrabPointVisualGridObject.SetActive(value: false);
			}
		}
		physGrabPoint.SetParent(base.transform.parent, worldPositionStays: true);
		PhysGrabPointDeactivate();
		physGrabPointPuller.gameObject.SetActive(value: false);
		physGrabBeam.transform.SetParent(base.transform.parent, worldPositionStays: false);
		physGrabBeam.transform.position = Vector3.zero;
		physGrabBeam.transform.rotation = Quaternion.identity;
		prevGrabbed = grabbed;
		grabbed = false;
		physGrabBeamAlphaOriginal = physGrabBeam.GetComponent<LineRenderer>().material.color.a;
		SoundSetup(startSound);
		SoundSetup(loopSound);
		SoundSetup(stopSound);
		if (isLocal)
		{
			playerCamera = Camera.main;
			PlayerController.instance.physGrabPoint = physGrabPoint;
			physGrabPointPlane.SetParent(base.transform.parent, worldPositionStays: false);
			physGrabPointPlane.position = Vector3.zero;
			physGrabPointPlane.rotation = Quaternion.identity;
			physGrabPointPlane.SetParent(CameraAim.Instance.transform, worldPositionStays: false);
			physGrabPointPlane.localPosition = Vector3.zero;
			physGrabPointPlane.localRotation = Quaternion.identity;
		}
	}

	private void GrabStateNone()
	{
		if (grabStateStart)
		{
			grabStateStart = false;
		}
	}

	private void GrabStatePhys()
	{
		if (grabStateStart)
		{
			grabStateStart = false;
		}
	}

	private void GrabStateHeal()
	{
		if (grabStateStart)
		{
			grabStateStart = false;
		}
	}

	private void GrabStateStatic()
	{
		if (grabStateStart)
		{
			grabStateStart = false;
		}
	}

	private void GrabStateClimb()
	{
		if (grabStateStart)
		{
			PhysGrabBeamActivate();
			Vector3 position = playerAvatar.localCamera.transform.position;
			climbStickTransform.transform.LookAt(position + (position - physGrabPointPullerPosition), Vector3.up);
			climbSpring.lastRotation = climbStickTransform.rotation;
			float value = Vector3.Distance(position, physGrabPointPosition);
			value = Mathf.Clamp(value, 10f, 20f);
			climbStickTransform.localScale = new Vector3(1f, 1f, value);
			grabStateStart = false;
			if (isLocal && !PlayerController.instance.DebugEnergy)
			{
				float num = Mathf.Round(PlayerController.instance.EnergyStart / 20f);
				for (float num2 = PlayerController.instance.playerAvatarScript.upgradeTumbleClimb - 1f; num2 > 0f; num2 -= 1f)
				{
					num *= 0.95f;
				}
				PlayerController.instance.EnergyCurrent -= num;
				PlayerController.instance.EnergyCurrent = Mathf.Max(PlayerController.instance.EnergyCurrent, 0f);
			}
			wallClimbPosition = playerAvatar.tumble.rb.position;
			climbSpring.damping = 1f;
			climbSpring.speed = 50f;
			climbStrengthEase = 0f;
		}
		if (isLocal && (!grabClimbCollider || !grabClimbCollider.gameObject.activeInHierarchy || grabClimbCollider.transform.position != grabClimbColliderPosition))
		{
			ReleaseObject(-1, 0.5f);
			return;
		}
		colorState = 3;
		colorStateOverrideTimer = 0.1f;
		physGrabPointPosition = grabClimbPos;
		physGrabPoint.position = grabClimbPos;
		climbStickTransform.position = grabClimbPos;
		Vector3 position2 = playerAvatar.localCamera.transform.position;
		Quaternion rotation = climbStickTransform.rotation;
		climbStickTransform.transform.LookAt(position2 + (position2 - physGrabPointPullerPosition), Vector3.up);
		Quaternion rotation2 = climbStickTransform.rotation;
		float num3 = Quaternion.Angle(rotation, rotation2);
		climbStickTransform.rotation = Quaternion.Lerp(rotation, rotation2, num3 * Time.deltaTime);
		climbStickTransform.rotation = SemiFunc.SpringQuaternionGet(climbSpring, climbStickTransform.rotation);
		climbTargetPositionTransform.position = climbStickTransform.position + climbStickTransform.forward * 2f;
		wallClimbPosition = climbTargetPositionTransform.position;
		if (!isLocal)
		{
			return;
		}
		if (!PlayerController.instance.DebugEnergy)
		{
			float num4 = Mathf.Round(PlayerController.instance.EnergyStart / 6f);
			for (float num5 = PlayerController.instance.playerAvatarScript.upgradeTumbleClimb - 1f; num5 > 0f; num5 -= 1f)
			{
				num4 *= 0.95f;
			}
			PlayerController.instance.EnergyCurrent -= num4 * Time.deltaTime;
			if (PlayerController.instance.EnergyCurrent < 0f)
			{
				ReleaseObject(-1, 0.5f);
				PlayerController.instance.tumbleInputDisableTimer = 3f;
				PlayerController.instance.EnergyCurrent = 0f;
			}
		}
		Aim.instance.SetState(Aim.State.Climb);
	}

	private void StateMachine()
	{
		switch (grabState)
		{
		case GrabState.None:
			GrabStateNone();
			break;
		case GrabState.Phys:
			GrabStatePhys();
			break;
		case GrabState.Heal:
			GrabStateHeal();
			break;
		case GrabState.Static:
			GrabStateStatic();
			break;
		case GrabState.Climb:
			GrabStateClimb();
			break;
		}
	}

	private void OnDestroy()
	{
		Object.Destroy(physGrabBeam);
	}

	private void ClimbPhysLogic()
	{
		if (grabState != GrabState.Climb || !playerAvatar.isTumbling)
		{
			return;
		}
		Rigidbody rb = playerAvatar.tumble.rb;
		if ((bool)rb)
		{
			Vector3 position = rb.position;
			Vector3 targetPosition = wallClimbPosition;
			float num = 14f;
			float num2 = 2f;
			for (float num3 = PlayerController.instance.playerAvatarScript.upgradeTumbleClimb - 1f; num3 > 0f; num3 -= 1f)
			{
				num += num2;
				num2 *= 0.8f;
			}
			climbStrengthEase = Mathf.Lerp(climbStrengthEase, num, Time.fixedDeltaTime * 2f);
			float num4 = 0.3f;
			float num5 = Vector3.Distance(position, physGrabPointPosition);
			num4 += num5 / 20f;
			Vector3 vector = SemiFunc.PhysFollowPositionWithDamping(position, targetPosition, rb.velocity, climbStrengthEase, num4);
			rb.AddForce(vector * (climbStrengthEase * Time.fixedDeltaTime), ForceMode.Impulse);
			playerAvatar.tumble.OverrideLookAtCamera(0.5f, 5f, 10f);
		}
	}

	public void PhysGrabOverCharge(float _amount, float _multiplier = 1f)
	{
		if (!(overrideOverchargeDisableTimer > 0f) && !PlayerController.instance.DebugDisableOvercharge && SemiFunc.MoonLevel() >= 2)
		{
			float num = 1f;
			if (SemiFunc.MoonLevel() == 3)
			{
				num = 1.25f;
			}
			else if (SemiFunc.MoonLevel() >= 4)
			{
				num = 1.5f;
			}
			if (physGrabBeamOverChargeAmount == 0f && physGrabBeamOverchargeInitialBoostCooldown <= 0f)
			{
				physGrabBeamOverchargeInitialBoostCooldown = 1f;
				physGrabBeamOverChargeFloat += 0.1f * _multiplier * num;
			}
			physGrabBeamOverChargeAmount = _amount * num;
			physGrabBeamOverChargeTimer = 0.2f;
			physGrabBeamOverchargeDecreaseCooldown = 1.5f;
		}
	}

	private void PhysGrabOverChargeImpact()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("PhysGrabOverChargeImpactRPC", RpcTarget.All);
		}
		else
		{
			PhysGrabOverChargeImpactRPC();
		}
	}

	[PunRPC]
	public void PhysGrabOverChargeImpactRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			physGrabBeam.GetComponent<PhysGrabBeam>().OverChargeLaunchPlayer();
		}
	}

	private void PhysGrabOverChargeLogic()
	{
		if (overrideOverchargeDisableTimer > 0f)
		{
			return;
		}
		if (physGrabBeamOverchargeInitialBoostCooldown > 0f)
		{
			physGrabBeamOverchargeInitialBoostCooldown -= Time.deltaTime;
		}
		if (physGrabBeamOverChargeFloat > 1f)
		{
			physGrabBeamOverChargeFloat = 1f;
		}
		if (!isLocal)
		{
			return;
		}
		if (!grabbed)
		{
			currentGrabForce = Vector3.zero;
			currentTorqueForce = Vector3.zero;
			physGrabBeamOverChargeAmount = 0f;
			physGrabBeamOverChargeTimer = 0f;
		}
		if (physGrabBeamOverChargeTimer <= 0f)
		{
			physGrabBeamOverChargeAmount = 0f;
		}
		if (physGrabBeamOverChargeTimer > 0f)
		{
			physGrabBeamOverChargeFloat += physGrabBeamOverChargeAmount * Time.deltaTime;
			if (physGrabBeamOverChargeFloat >= 1f)
			{
				physGrabBeamOverChargeFloat = 1f;
				OverrideGrabRelease(-1);
				PhysGrabOverChargeImpact();
				physGrabBeamOverChargeTimer = 0f;
			}
			physGrabBeamOverCharge = (byte)(physGrabBeamOverChargeFloat * 200f);
			physGrabBeamOverChargeTimer -= Time.deltaTime;
		}
		else if (physGrabBeamOverChargeFloat > 0f)
		{
			if (physGrabBeamOverchargeDecreaseCooldown > 0f)
			{
				physGrabBeamOverchargeDecreaseCooldown -= Time.deltaTime;
				return;
			}
			physGrabBeamOverChargeFloat -= 0.1f * Time.deltaTime;
			physGrabBeamOverCharge = (byte)(physGrabBeamOverChargeFloat * 200f);
		}
		else if (physGrabBeamOverCharge > 0)
		{
			physGrabBeamOverCharge = 0;
		}
	}

	public void OverrideGrabDistance(float dist)
	{
		prevPullerDistance = pullerDistance;
		pullerDistance = dist;
		overrideGrabDistance = dist;
		overrideGrabDistanceTimer = 0.1f;
	}

	public void OverrideMinimumGrabDistance(float dist)
	{
		overrideMinimumGrabDistance = dist;
		overrideMinimumGrabDistanceTimer = 0.1f;
	}

	private void OverrideGrabDistanceTick()
	{
		if (overrideGrabDistanceTimer > 0f)
		{
			overrideGrabDistanceTimer -= Time.deltaTime;
		}
		else if (overrideGrabDistanceTimer != -123f)
		{
			overrideGrabDistance = 0f;
			overrideGrabDistanceTimer = -123f;
		}
		if (overrideMinimumGrabDistanceTimer > 0f)
		{
			overrideMinimumGrabDistanceTimer -= Time.deltaTime;
		}
		else if (overrideMinimumGrabDistanceTimer != -123f)
		{
			overrideMinimumGrabDistance = 0f;
			minDistanceFromPlayer = minDistanceFromPlayerOriginal;
			overrideMinimumGrabDistanceTimer = -123f;
		}
	}

	private IEnumerator LateStart()
	{
		while (!playerAvatar)
		{
			yield return new WaitForSeconds(0.2f);
		}
		string _steamID = SemiFunc.PlayerGetSteamID(playerAvatar);
		yield return new WaitForSeconds(0.2f);
		while (!StatsManager.instance.playerUpgradeStrength.ContainsKey(_steamID))
		{
			yield return new WaitForSeconds(0.2f);
		}
		if (!SemiFunc.MenuLevel())
		{
			grabStrength += (float)StatsManager.instance.playerUpgradeStrength[_steamID] * 0.2f;
			throwStrength += (float)StatsManager.instance.playerUpgradeThrow[_steamID] * 0.3f;
			grabRange += (float)StatsManager.instance.playerUpgradeRange[_steamID] * 1f;
		}
	}

	public void SoundSetup(Sound _sound)
	{
		if (!SemiFunc.IsMultiplayer() || photonView.IsMine)
		{
			_sound.SpatialBlend = 0f;
			return;
		}
		_sound.Volume *= 0.5f;
		_sound.VolumeRandom *= 0.5f;
		_sound.SpatialBlend = 1f;
	}

	public void OverrideDisableRotationControls()
	{
		overrideDisableRotationControls = true;
		overrideDisableRotationControlsTimer = 0.1f;
	}

	private void OverrideDisableRotationControlsTick()
	{
		if (overrideDisableRotationControlsTimer > 0f)
		{
			overrideDisableRotationControlsTimer -= Time.fixedDeltaTime;
			if (overrideDisableRotationControlsTimer <= 0f)
			{
				overrideDisableRotationControls = false;
			}
		}
	}

	public void OverrideGrab(PhysGrabObject target, float _grabTime = 0.1f, bool _grabRelease = false)
	{
		if (grabbed && grabbedPhysGrabObject != target)
		{
			ReleaseObject(-1);
		}
		overrideGrabTarget = target;
		overrideGrabTimer = _grabTime;
		overrideGrabRelease = _grabRelease;
	}

	public void OverrideBeamColor(Color _color, float _time = 0.1f)
	{
		overrideBeamColor = _color;
		overrideBeamColorTimer = _time;
	}

	public void OverrideOverchargeDisable(float _disableTimer = 0.1f)
	{
		overrideOverchargeDisableTimer = _disableTimer;
	}

	public void OverrideGrabPoint(Transform grabPoint)
	{
		overrideGrabPointTransform = grabPoint;
		overrideGrabPointTimer = 0.1f;
	}

	public void OverrideGrabRelease(int releaseObjectViewID, float _disableTimer = 0.5f)
	{
		if (!SemiFunc.IsMultiplayer())
		{
			ReleaseObject(releaseObjectViewID, _disableTimer);
			return;
		}
		photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, _disableTimer, releaseObjectViewID);
	}

	private void OverrideGrabTick()
	{
		if (overrideGrabTimer <= 0f)
		{
			if (overrideGrabRelease && (!overrideGrabTarget || grabbedPhysGrabObject == overrideGrabTarget))
			{
				OverrideGrabRelease(-1);
			}
			overrideGrabRelease = false;
			overrideGrabTarget = null;
			overrideGrabPointTimer = 0f;
		}
		if (overrideGrabTimer > 0f)
		{
			overrideGrabTimer -= Time.deltaTime;
			if ((bool)overrideGrabTarget && grabbedPhysGrabObject != overrideGrabTarget)
			{
				ForceGrabPhysObject(overrideGrabTarget);
			}
		}
	}

	private void OverrideBeamColorTick()
	{
		if (overrideBeamColorTimer <= 0f && currentBeamColor == overrideBeamColor)
		{
			prevColorState = -1;
			ColorStates();
		}
		if (overrideBeamColorTimer > 0f)
		{
			if (currentBeamColor != overrideBeamColor)
			{
				ColorStateSetColor(overrideBeamColor, overrideBeamColor);
			}
			overrideBeamColorTimer -= Time.fixedDeltaTime;
		}
	}

	private void OverrideOverchargeDisableTick()
	{
		if (overrideOverchargeDisableTimer > 0f)
		{
			OverchargeResetValues();
			overrideOverchargeDisableTimer -= Time.fixedDeltaTime;
		}
	}

	public void GrabberHeal()
	{
		if (!healing)
		{
			photonView.RPC("HealStart", RpcTarget.All);
		}
	}

	private void ColorStateSetColor(Color mainColor, Color emissionColor)
	{
		currentBeamColor = mainColor;
		Material material = physGrabBeam.GetComponent<LineRenderer>().material;
		Material material2 = physGrabPointVisual1.GetComponent<MeshRenderer>().material;
		Material material3 = physGrabPointVisual2.GetComponent<MeshRenderer>().material;
		Material material4 = physGrabPointVisualRotate.GetComponent<MeshRenderer>().material;
		Light grabberLight = playerAvatar.playerAvatarVisuals.playerAvatarRightArm.grabberLight;
		Material material5 = playerAvatar.playerAvatarVisuals.playerAvatarRightArm.grabberOrbSpheres[0].GetComponent<MeshRenderer>().material;
		Material material6 = playerAvatar.playerAvatarVisuals.playerAvatarRightArm.grabberOrbSpheres[1].GetComponent<MeshRenderer>().material;
		if ((bool)material)
		{
			material.color = mainColor;
		}
		if ((bool)material)
		{
			material.SetColor("_EmissionColor", emissionColor);
		}
		if ((bool)material2)
		{
			material2.color = mainColor;
		}
		if ((bool)material2)
		{
			material2.SetColor("_EmissionColor", emissionColor);
		}
		if ((bool)material3)
		{
			material3.color = mainColor;
		}
		if ((bool)material3)
		{
			material3.SetColor("_EmissionColor", emissionColor);
		}
		if ((bool)material4)
		{
			material4.color = mainColor;
		}
		if ((bool)material4)
		{
			material4.SetColor("_EmissionColor", emissionColor);
		}
		if ((bool)grabberLight)
		{
			grabberLight.color = mainColor;
		}
		if ((bool)material5)
		{
			material5.color = mainColor;
		}
		if ((bool)material5)
		{
			material5.SetColor("_EmissionColor", emissionColor);
		}
		if ((bool)material6)
		{
			material6.color = mainColor;
		}
		if ((bool)material6)
		{
			material6.SetColor("_EmissionColor", emissionColor);
		}
	}

	public void OverrideColorToGreen(float time = 0.1f)
	{
		colorState = 1;
		colorStateOverrideTimer = time;
	}

	public void OverrideColorToPurple(float time = 0.1f)
	{
		colorState = 2;
		colorStateOverrideTimer = time;
	}

	private void ColorStates()
	{
		if (prevColorState != colorState)
		{
			prevColorState = colorState;
			Color color = new Color(1f, 0.1856f, 0f, 0.15f);
			Color color2 = new Color(1f, 0.1856f, 0f, 1f);
			if (colorState == 0)
			{
				color = (VideoGreenScreen.instance ? new Color(1f, 0.1856f, 0f, 1f) : new Color(1f, 0.1856f, 0f, 0.15f));
				color2 = new Color(1f, 0.1856f, 0f, 1f);
				ColorStateSetColor(color, color2);
			}
			else if (colorState == 1)
			{
				color = (VideoGreenScreen.instance ? new Color(0f, 1f, 0f, 1f) : new Color(0f, 1f, 0f, 0.15f));
				color2 = new Color(0f, 1f, 0f, 1f);
				ColorStateSetColor(color, color2);
			}
			else if (colorState == 2)
			{
				color = (VideoGreenScreen.instance ? new Color(1f, 0f, 1f, 1f) : new Color(1f, 0f, 1f, 0.15f));
				color2 = new Color(1f, 0f, 1f, 1f);
				ColorStateSetColor(color, color2);
			}
			else if (colorState == 3)
			{
				color = (VideoGreenScreen.instance ? new Color(0f, 0.5f, 1f, 1f) : new Color(0f, 0.5f, 1f, 0.15f));
				color2 = new Color(0f, 0.5f, 1f, 1f);
				ColorStateSetColor(color, color2);
			}
		}
	}

	private void ColorStateTick()
	{
		if (colorStateOverrideTimer > 0f)
		{
			colorStateOverrideTimer -= Time.fixedDeltaTime;
		}
		else
		{
			colorState = 0;
		}
	}

	[PunRPC]
	private void HealStart()
	{
		physGrabBeam.GetComponent<LineRenderer>().material = physGrabBeamMaterialBatteryCharge;
		physGrabPointVisual1.GetComponent<MeshRenderer>().material = physGrabBeamMaterialBatteryCharge;
		physGrabPointVisual2.GetComponent<MeshRenderer>().material = physGrabBeamMaterialBatteryCharge;
		physGrabBeam.GetComponent<PhysGrabBeam>().scrollSpeed = new Vector2(-5f, 0f);
		physGrabBeam.GetComponent<PhysGrabBeam>().lineMaterial = physGrabBeam.GetComponent<LineRenderer>().material;
		healing = true;
	}

	private void ResetBeam()
	{
		if (healing)
		{
			physGrabBeam.GetComponent<LineRenderer>().material = physGrabBeamMaterial;
			physGrabPointVisual1.GetComponent<MeshRenderer>().material = physGrabBeamMaterial;
			physGrabPointVisual2.GetComponent<MeshRenderer>().material = physGrabBeamMaterial;
			physGrabBeam.GetComponent<PhysGrabBeam>().scrollSpeed = physGrabBeam.GetComponent<PhysGrabBeam>().originalScrollSpeed;
			physGrabBeam.GetComponent<PhysGrabBeam>().lineMaterial = physGrabBeam.GetComponent<LineRenderer>().material;
			healing = false;
		}
	}

	public void ChangeBeamAlpha(float alpha)
	{
		if (physGramBeamAlphaTimer == -123f)
		{
			physGrabBeamAlpha = physGrabBeamAlphaOriginal;
		}
		physGrabBeamAlphaChangeTo = alpha;
		physGramBeamAlphaTimer = 0.1f;
		physGrabBeamAlphaChangeProgress = 0f;
	}

	private void TickerBeamAlphaChange()
	{
		if (physGramBeamAlphaTimer > 0f)
		{
			physGrabBeamAlpha = Mathf.Lerp(physGrabBeamAlpha, physGrabBeamAlphaChangeTo, physGrabBeamAlphaChangeProgress);
			if (physGrabBeamAlphaChangeProgress < 1f)
			{
				physGrabBeamAlphaChangeProgress += 4f * Time.deltaTime;
				Material material = physGrabBeam.GetComponent<LineRenderer>().material;
				material.SetColor("_Color", new Color(material.color.r, material.color.g, material.color.b, physGrabBeamAlpha));
				Material material2 = physGrabPointVisual1.GetComponent<MeshRenderer>().material;
				Material material3 = physGrabPointVisual2.GetComponent<MeshRenderer>().material;
				material2.SetColor("_Color", new Color(material2.color.r, material2.color.g, material2.color.b, physGrabBeamAlpha));
				material3.SetColor("_Color", new Color(material3.color.r, material3.color.g, material3.color.b, physGrabBeamAlpha));
			}
		}
		else if (physGramBeamAlphaTimer != -123f)
		{
			physGrabBeamAlphaChangeProgress = 0f;
			Material material4 = physGrabBeam.GetComponent<LineRenderer>().material;
			material4.SetColor("_Color", new Color(material4.color.r, material4.color.g, material4.color.b, physGrabBeamAlphaOriginal));
			Material material5 = physGrabPointVisual1.GetComponent<MeshRenderer>().material;
			Material material6 = physGrabPointVisual2.GetComponent<MeshRenderer>().material;
			material5.SetColor("_Color", new Color(material5.color.r, material5.color.g, material5.color.b, physGrabBeamAlphaOriginal));
			material6.SetColor("_Color", new Color(material6.color.r, material6.color.g, material6.color.b, physGrabBeamAlphaOriginal));
			physGramBeamAlphaTimer = -123f;
		}
		if (physGramBeamAlphaTimer > 0f)
		{
			physGramBeamAlphaTimer -= Time.deltaTime;
		}
	}

	public Quaternion GetRotationInput()
	{
		Quaternion quaternion = Quaternion.AngleAxis(mouseTurningVelocity.y, Vector3.right);
		Quaternion quaternion2 = Quaternion.AngleAxis(0f - mouseTurningVelocity.x, Vector3.up);
		Quaternion quaternion3 = Quaternion.AngleAxis(mouseTurningVelocity.z, Vector3.forward);
		return quaternion2 * quaternion * quaternion3;
	}

	private void ObjectTurning()
	{
		if (!grabbedPhysGrabObject)
		{
			return;
		}
		if (!grabbed)
		{
			mouseTurningVelocity = Vector3.zero;
			physGrabPointVisualGrid.gameObject.SetActive(value: false);
			isRotating = false;
			return;
		}
		float num = Mathf.Max(grabbedPhysGrabObject.rb.mass, 1f);
		if ((bool)physGrabPointVisualGrid && (bool)grabbedPhysGrabObject)
		{
			physGrabPointVisualGrid.position = grabbedPhysGrabObject.midPoint;
		}
		if (mouseTurningVelocity.magnitude > 0.01f)
		{
			if (isRotating)
			{
				float t = 1f * Time.deltaTime;
				mouseTurningVelocity = Vector3.Lerp(mouseTurningVelocity, Vector3.zero, t);
			}
			else
			{
				float t2 = 10f * Time.deltaTime;
				mouseTurningVelocity = Vector3.Lerp(mouseTurningVelocity, Vector3.zero, t2);
			}
		}
		else
		{
			mouseTurningVelocity = Vector3.zero;
		}
		cameraRelativeGrabbedForward = cameraRelativeGrabbedForward.normalized;
		cameraRelativeGrabbedUp = cameraRelativeGrabbedUp.normalized;
		bool flag = false;
		if (isLocal && PlayerController.instance.InputDisableTimer <= 0f && SemiFunc.InputHold(InputKey.Rotate))
		{
			flag = true;
		}
		if (!flag)
		{
			test = 0f;
		}
		if (flag)
		{
			float num2 = Mathf.Lerp(0.2f, 2.5f, GameplayManager.instance.aimSensitivity / 100f);
			float value = Input.GetAxis("Mouse X") * num2;
			float value2 = Input.GetAxis("Mouse Y") * num2;
			value = Mathf.Clamp(value, -5f, 5f);
			value2 = Mathf.Clamp(value2, -5f, 5f);
			if (value > test)
			{
				test = value;
			}
			if (value2 > test)
			{
				test = value2;
			}
			float num3 = Mathf.Lerp(0.1f, 1f, num * 0.05f);
			value *= num3;
			value2 *= num3;
			Vector3 vector = new Vector3(value, value2, 0f) * 15f * grabStrength;
			if (grabbedPhysGrabObject.impactDetector.isCart)
			{
				vector *= 0.25f;
			}
			if (vector.magnitude != 0f)
			{
				mouseTurningVelocity += vector * 15f * Time.deltaTime;
			}
			mouseTurningVelocity = Vector3.ClampMagnitude(mouseTurningVelocity, 70f);
			if (isLocal)
			{
				isRotatingTimer = 0.1f;
			}
		}
		if (isRotating)
		{
			physGrabPointVisualGrid.gameObject.SetActive(value: true);
			Transform transform = playerAvatar.localCamera.transform;
			if (physRotatingTimer <= 0f)
			{
				physRotatingTimer = 0.25f;
				cameraRelativeGrabbedForward = transform.InverseTransformDirection(grabbedObjectTransform.forward);
				cameraRelativeGrabbedUp = transform.InverseTransformDirection(grabbedObjectTransform.up);
				physGrabPointVisualGrid.rotation = grabbedObjectTransform.rotation;
			}
			physRotatingTimer = 0.25f;
			float num4 = Mathf.Clamp(1f / num, 0f, 0.5f);
			if (num4 != 0f)
			{
				grabbedPhysGrabObject.OverrideAngularDrag(40f * num4);
			}
			Quaternion quaternion = Quaternion.AngleAxis(mouseTurningVelocity.y, transform.right);
			Quaternion quaternion2 = Quaternion.AngleAxis(0f - mouseTurningVelocity.x, transform.up);
			Quaternion quaternion3 = Quaternion.AngleAxis(mouseTurningVelocity.z, transform.forward);
			Quaternion quaternion4 = quaternion2 * quaternion * quaternion3;
			quaternion4 = Quaternion.Slerp(Quaternion.identity, quaternion4, Time.deltaTime * 20f);
			float value3 = 1f / num;
			value3 = Mathf.Clamp(value3, 0f, 1f);
			if (num < grabbedPhysGrabObject.massOriginal && grabbedPhysGrabObject.roomVolumeCheck.currentSize.magnitude > 1f)
			{
				value3 = 0.25f;
			}
			quaternion4 = Quaternion.Slerp(Quaternion.identity, quaternion4, value3);
			physGrabPointVisualGrid.rotation = quaternion4 * physGrabPointVisualGrid.rotation;
			Quaternion rotation = grabbedObjectTransform.rotation;
			Quaternion rotation2 = physGrabPointVisualGrid.rotation;
			float num5 = Quaternion.Angle(rotation, rotation2);
			float num6 = 45f;
			if (num5 > num6)
			{
				float t3 = num6 / num5;
				rotation2 = Quaternion.Slerp(rotation, rotation2, t3);
			}
			physGrabPointVisualGrid.rotation = rotation2;
			cameraRelativeGrabbedForward = transform.InverseTransformDirection(grabbedObjectTransform.forward);
			cameraRelativeGrabbedUp = transform.InverseTransformDirection(grabbedObjectTransform.up);
			foreach (PhysGrabber item in grabbedPhysGrabObject.playerGrabbing)
			{
				Transform transform2 = item.playerAvatar.localCamera.transform;
				item.cameraRelativeGrabbedForward = transform2.InverseTransformDirection(physGrabPointVisualGrid.forward);
				item.cameraRelativeGrabbedUp = transform2.InverseTransformDirection(physGrabPointVisualGrid.up);
			}
			float t4 = 2f * Time.deltaTime;
			physGrabPointVisualGrid.transform.rotation = Quaternion.Slerp(physGrabPointVisualGrid.transform.rotation, grabbedObjectTransform.rotation, t4);
		}
		else
		{
			physGrabPointVisualGrid.gameObject.SetActive(value: false);
		}
	}

	private void OverrideGrabPointTimer()
	{
		if (overrideGrabPointTimer > 0f)
		{
			overrideGrabPointTimer -= Time.fixedDeltaTime;
		}
		else
		{
			overrideGrabPointTransform = null;
		}
	}

	private void FixedUpdate()
	{
		OverrideGrabPointTimer();
		OverrideDisableRotationControlsTick();
		OverrideDisableRotationControlsTick();
		PhysGrabInTumbleStateLookAtCameraDirection();
		ClimbPhysLogic();
		if (isLocal && !overrideDisableRotationControls)
		{
			if (isRotatingTimer > 0f)
			{
				SemiFunc.CameraOverrideStopAim();
				if (!isRotating && (bool)grabbedObjectTransform)
				{
					mouseTurningVelocity = Vector3.zero;
				}
				isRotating = true;
			}
			else
			{
				isRotating = false;
			}
		}
		if (stopRotationTimer > 0f)
		{
			stopRotationTimer -= Time.fixedDeltaTime;
		}
		ColorStateTick();
	}

	private void PushingPullingChecker()
	{
		if (overrideGrabDistanceTimer > 0f)
		{
			pullerDistance = overrideGrabDistance;
			prevPullerDistance = pullerDistance;
		}
		if (overrideMinimumGrabDistanceTimer > 0f && minDistanceFromPlayer < overrideMinimumGrabDistance)
		{
			minDistanceFromPlayer = overrideMinimumGrabDistance;
		}
		if (!grabbed)
		{
			isPushing = false;
			isPulling = false;
			isPushingTimer = 0f;
			isPullingTimer = 0f;
			prevPullerDistance = pullerDistance;
			return;
		}
		if (initialPressTimer > 0f)
		{
			prevPullerDistance = pullerDistance;
			isPushingTimer = 0f;
		}
		if (InputManager.instance.KeyPullAndPush() > 0f)
		{
			isPushingTimer = 0.1f;
		}
		if (InputManager.instance.KeyPullAndPush() < 0f)
		{
			isPullingTimer = 0.1f;
		}
		if (isPushingTimer > 0f)
		{
			TutorialDirector.instance.playerPushedAndPulled = true;
			isPushing = true;
			isPushingTimer -= Time.deltaTime;
		}
		else
		{
			isPushing = false;
		}
		if (isPullingTimer > 0f)
		{
			TutorialDirector.instance.playerPushedAndPulled = true;
			isPulling = true;
			isPullingTimer -= Time.deltaTime;
		}
		else
		{
			isPulling = false;
		}
		prevPullerDistance = pullerDistance;
		if (overrideGrabDistanceTimer > 0f)
		{
			pullerDistance = overrideGrabDistance;
			prevPullerDistance = pullerDistance;
		}
	}

	public void OverridePullDistanceIncrement(float distSpeed)
	{
		physGrabPointPlane.position += playerCamera.transform.forward * distSpeed;
	}

	private void OverchargeResetValues()
	{
		physGrabBeamOverCharge = 0;
		physGrabBeamOverChargeFloat = 0f;
		physGrabBeamOverChargeAmount = 0f;
		physGrabBeamOverChargeTimer = 0f;
	}

	private void OnDisable()
	{
		OverchargeResetValues();
	}

	private void OnEnable()
	{
		OverchargeResetValues();
	}

	private void Update()
	{
		StateMachine();
		OverrideGrabTick();
		OverrideOverchargeDisableTick();
		OverrideBeamColorTick();
		if (isRotatingTimer > 0f)
		{
			isRotatingTimer -= Time.deltaTime;
		}
		PushingPullingChecker();
		ColorStates();
		ObjectTurning();
		PhysGrabOverChargeLogic();
		if ((bool)grabbedObjectTransform && grabbedObjectTransform.name == playerAvatar.healthGrab.name)
		{
			OverrideColorToGreen();
		}
		OverrideGrabDistanceTick();
		TickerBeamAlphaChange();
		if (initialPressTimer > 0f)
		{
			initialPressTimer -= Time.deltaTime;
		}
		if (physRotatingTimer > 0f)
		{
			physRotatingTimer -= Time.deltaTime;
		}
		if (physGrabForcesDisabledTimer > 0f)
		{
			physGrabForcesDisabledTimer -= Time.deltaTime;
		}
		if (grabbed && (bool)grabbedObjectTransform)
		{
			if (!overrideGrabPointTransform)
			{
				physGrabPoint.position = grabbedObjectTransform.TransformPoint(localGrabPosition);
			}
			else
			{
				physGrabPoint.position = overrideGrabPointTransform.position;
			}
		}
		if (isLocal)
		{
			if ((bool)grabbedPhysGrabObject)
			{
				_ = grabbedPhysGrabObject.isMelee;
			}
			else
				_ = 0;
			if (PlayerController.instance.InputDisableTimer <= 0f && !SemiFunc.InputHold(InputKey.Rotate))
			{
				if (InputManager.instance.KeyPullAndPush() > 0f && Vector3.Distance(physGrabPointPuller.position, playerCamera.transform.position) < grabRange)
				{
					physGrabPointPlane.position += playerCamera.transform.forward * 0.2f;
				}
				if (InputManager.instance.KeyPullAndPush() < 0f && Vector3.Distance(physGrabPointPuller.position, playerCamera.transform.position) > minDistanceFromPlayer)
				{
					physGrabPointPlane.position -= playerCamera.transform.forward * 0.2f;
				}
			}
			if (overrideGrabDistanceTimer < 0f)
			{
				pullerDistance = Vector3.Distance(physGrabPointPuller.position, playerCamera.transform.position);
			}
			if (overrideGrabDistance > 0f)
			{
				Transform visionTransform = playerAvatar.PlayerVisionTarget.VisionTransform;
				physGrabPointPlane.position = visionTransform.position + visionTransform.forward * overrideGrabDistance;
			}
			else
			{
				if (pullerDistance < minDistanceFromPlayer)
				{
					physGrabPointPuller.position = playerCamera.transform.position + playerCamera.transform.forward * minDistanceFromPlayer;
				}
				if (pullerDistance > maxDistanceFromPlayer)
				{
					physGrabPointPuller.position = playerCamera.transform.position + playerCamera.transform.forward * maxDistanceFromPlayer;
				}
			}
		}
		else if (overrideGrabDistanceTimer <= 0f)
		{
			pullerDistance = Vector3.Distance(physGrabPointPuller.position, playerAvatar.localCamera.transform.position);
		}
		grabberAudioTransform.position = physGrabBeamComponent.PhysGrabPointOrigin.position;
		loopSound.PlayLoop(physGrabBeamScript.lineRenderer.enabled, 10f, 10f);
		ShowValue();
		if (!isLocal)
		{
			return;
		}
		bool flag = SemiFunc.InputHold(InputKey.Grab);
		bool flag2 = flag || toggleGrab;
		if (debugStickyGrabber && !SemiFunc.InputHold(InputKey.Rotate))
		{
			flag2 = true;
		}
		if (InputManager.instance.InputToggleGet(InputKey.Grab))
		{
			if (PlayerController.instance.InputDisableTimer <= 0f && SemiFunc.InputDown(InputKey.Grab))
			{
				toggleGrab = !toggleGrab;
				if (toggleGrab)
				{
					toggleGrabTimer = 0.1f;
				}
			}
		}
		else if (toggleGrab && (flag || !GameplayManager.instance.itemUnequipAutoHold))
		{
			toggleGrab = false;
		}
		if (toggleGrabTimer > 0f)
		{
			toggleGrabTimer -= Time.deltaTime;
		}
		else if (!grabbed && toggleGrab)
		{
			toggleGrab = false;
		}
		if (overrideGrabTimer > 0f && (bool)overrideGrabTarget && (flag || toggleGrab))
		{
			overrideGrabTarget = null;
		}
		if (overrideGrabTimer > 0f)
		{
			flag2 = true;
		}
		else if (PlayerController.instance.InputDisableTimer > 0f && !toggleGrab)
		{
			flag2 = false;
		}
		if (grabState == GrabState.Climb && (!playerAvatar.isTumbling || !TumbleUI.instance.canExit))
		{
			flag2 = false;
		}
		bool flag3 = false;
		if (flag2 && !grabbed)
		{
			if (grabDisableTimer <= 0f)
			{
				flag3 = true;
			}
		}
		else if (!flag2 && grabbed)
		{
			ReleaseObject(-1);
		}
		if (LevelGenerator.Instance.Generated && PlayerController.instance.InputDisableTimer <= 0f)
		{
			if (grabCheckTimer <= 0f || flag3)
			{
				grabCheckTimer = 0.02f;
				RayCheck(flag3);
			}
			else
			{
				grabCheckTimer -= Time.deltaTime;
			}
		}
		PhysGrabLogic();
		if (grabDisableTimer > 0f)
		{
			grabDisableTimer -= Time.deltaTime;
		}
	}

	private void PhysGrabLogic()
	{
		grabReleaseDistance = Mathf.Max(grabRange * 2f, 10f);
		if (!grabbed)
		{
			return;
		}
		if (grabState != GrabState.Climb)
		{
			if (physRotatingTimer > 0f)
			{
				Aim.instance.SetState(Aim.State.Rotate);
			}
			else
			{
				Aim.instance.SetState(Aim.State.Grab);
			}
		}
		if (Vector3.Distance(physGrabPoint.position, playerCamera.transform.position) > grabReleaseDistance)
		{
			ReleaseObject(-1);
			return;
		}
		if ((bool)grabbedPhysGrabObject)
		{
			if (!grabbedPhysGrabObject.enabled || grabbedPhysGrabObject.dead || !grabbedPhysGrabObjectCollider || !grabbedPhysGrabObjectCollider.enabled)
			{
				ReleaseObject(-1);
				return;
			}
		}
		else if ((bool)grabbedStaticGrabObject)
		{
			if (!grabbedStaticGrabObject.isActiveAndEnabled || grabbedStaticGrabObject.dead)
			{
				ReleaseObject(-1);
				return;
			}
		}
		else if (grabState != GrabState.Climb)
		{
			ReleaseObject(-1);
			return;
		}
		physGrabPointPullerPosition = physGrabPointPuller.position;
		PhysGrabStarted();
		PhysGrabBeamActivate();
	}

	private void PhysGrabBeamActivate()
	{
		if (GameManager.instance.gameMode == 0)
		{
			if (!physGrabBeamActive)
			{
				physGrabBeamScript.lineRenderer.enabled = true;
				physGrabForcesDisabledTimer = 0f;
				physGrabBeamComponent.physGrabPointPullerSmoothPosition = physGrabPoint.position;
				physGrabBeamActive = true;
				PhysGrabStartEffects();
			}
		}
		else if (!physGrabBeamActive)
		{
			photonView.RPC("PhysGrabBeamActivateRPC", RpcTarget.All);
			physGrabBeamActive = true;
		}
	}

	public void ShowValue()
	{
		if ((!isLocal && (!SpectateCamera.instance || !SpectateCamera.instance.CheckState(SpectateCamera.State.Normal) || !(SpectateCamera.instance.player == playerAvatar) || !physGrabPoint.gameObject.activeSelf)) || !grabbed || !grabbedPhysGrabObject)
		{
			return;
		}
		ValuableObject component = grabbedPhysGrabObject.GetComponent<ValuableObject>();
		if ((bool)component)
		{
			WorldSpaceUIValue.instance.Show(grabbedPhysGrabObject, (int)component.dollarValueCurrent, _cost: false, Vector3.zero);
		}
		else if (SemiFunc.RunIsShop())
		{
			ItemAttributes component2 = grabbedPhysGrabObject.GetComponent<ItemAttributes>();
			if ((bool)component2)
			{
				WorldSpaceUIValue.instance.Show(grabbedPhysGrabObject, component2.value, _cost: true, component2.costOffset);
			}
		}
	}

	private void PhysGrabStartEffects()
	{
		startSound.Play(loopSound.Source.transform.position);
		if (!GameManager.Multiplayer() || photonView.IsMine)
		{
			GameDirector.instance.CameraImpact.Shake(0.5f, 0.1f);
		}
	}

	private void PhysGrabEndEffects()
	{
		stopSound.Play(loopSound.Source.transform.position);
		if (!GameManager.Multiplayer() || photonView.IsMine)
		{
			GameDirector.instance.CameraImpact.Shake(0.5f, 0.1f);
		}
	}

	[PunRPC]
	private void PhysGrabBeamActivateRPC()
	{
		PhysGrabStartEffects();
		initialPressTimer = 0.1f;
		physGrabForcesDisabledTimer = 0f;
		physGrabBeamScript.lineRenderer.enabled = true;
		physGrabBeamComponent.physGrabPointPullerSmoothPosition = physGrabPoint.position;
		physGrabBeamActive = true;
		physGrabPointVisualRotate.GetComponent<PhysGrabPointRotate>().animationEval = 0f;
		PhysGrabPointActivate();
	}

	private void PhysGrabPointDeactivate()
	{
		physGrabPointVisualGrid.parent = physGrabPoint;
		physGrabPointVisualRotate.localScale = Vector3.zero;
		physGrabPointVisualRotate.GetComponent<PhysGrabPointRotate>().animationEval = 0f;
		GridObjectsRemove();
		physGrabPoint.gameObject.SetActive(value: false);
	}

	private void PhysGrabPointActivate()
	{
		physGrabPointVisualRotate.localScale = Vector3.zero;
		PhysGrabPointRotate component = physGrabPointVisualRotate.GetComponent<PhysGrabPointRotate>();
		if ((bool)component)
		{
			component.animationEval = 0f;
			component.rotationActiveTimer = 0f;
		}
		physGrabPointVisualGrid.localPosition = Vector3.zero;
		physGrabPointVisualGrid.parent = base.transform.parent;
		physGrabPointVisualGrid.localScale = Vector3.one;
		if ((bool)grabbedObjectTransform)
		{
			grabbedPhysGrabObject = grabbedObjectTransform.GetComponent<PhysGrabObject>();
			if ((bool)grabbedPhysGrabObject)
			{
				physGrabPointVisualGrid.localRotation = grabbedPhysGrabObject.rb.rotation;
			}
			if ((bool)grabbedPhysGrabObject)
			{
				GridObjectsInstantiate();
			}
		}
		if (grabState == GrabState.Climb)
		{
			physGrabPoint.localScale = Vector3.one * 2f;
		}
		else
		{
			physGrabPoint.localScale = Vector3.one;
		}
		physGrabPointVisualGrid.gameObject.SetActive(value: false);
		physGrabPoint.gameObject.SetActive(value: true);
	}

	public void OverridePhysGrabForcesDisable(float _time)
	{
		physGrabForcesDisabledTimer = _time;
	}

	[PunRPC]
	private void PhysGrabBeamDeactivateRPC()
	{
		physGrabForcesDisabledTimer = 0f;
		ResetBeam();
		physGrabBeamScript.lineRenderer.enabled = false;
		PhysGrabPointDeactivate();
		physGrabBeamActive = false;
		PhysGrabEndEffects();
		physRotation = Quaternion.identity;
	}

	private void PhysGrabBeamDeactivate()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			PhysGrabBeamDeactivateRPC();
		}
		else
		{
			photonView.RPC("PhysGrabBeamDeactivateRPC", RpcTarget.All);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterAndOwnerOnlyRPC(info, info.photonView))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(physGrabPointPullerPosition);
				stream.SendNext(physGrabPointPlane.position);
				stream.SendNext(mouseTurningVelocity);
				stream.SendNext(isRotating);
				stream.SendNext(colorState);
				stream.SendNext(physGrabBeamOverCharge);
			}
			else
			{
				physGrabPointPullerPosition = (Vector3)stream.ReceiveNext();
				physGrabPointPuller.position = physGrabPointPullerPosition;
				physGrabPointPlane.position = (Vector3)stream.ReceiveNext();
				mouseTurningVelocity = (Vector3)stream.ReceiveNext();
				isRotating = (bool)stream.ReceiveNext();
				colorState = (int)stream.ReceiveNext();
				physGrabBeamOverCharge = (byte)stream.ReceiveNext();
			}
		}
	}

	private void PhysGrabStarted()
	{
		if ((bool)grabbedPhysGrabObject)
		{
			grabbedPhysGrabObject.GrabStarted(this);
		}
		else if ((bool)grabbedStaticGrabObject)
		{
			grabbedStaticGrabObject.GrabStarted(this);
		}
		else if (grabState != GrabState.Climb)
		{
			ReleaseObject(-1);
		}
	}

	private void PhysGrabEnded()
	{
		if ((bool)grabbedPhysGrabObject)
		{
			grabbedPhysGrabObject.GrabEnded(this);
		}
		else if ((bool)grabbedStaticGrabObject)
		{
			grabbedStaticGrabObject.GrabEnded(this);
		}
		else if (grabState == GrabState.Climb && SemiFunc.IsMultiplayer())
		{
			photonView.RPC("GrabClimbEndedRPC", RpcTarget.MasterClient);
		}
	}

	private void StartGrabbingPhysObject(RaycastHit hit, PhysGrabObject _physGrabObject)
	{
		grabbedPhysGrabObject = _physGrabObject;
		if ((bool)grabbedPhysGrabObject && grabbedPhysGrabObject.rb.IsSleeping())
		{
			grabbedPhysGrabObject.OverrideIndestructible(0.5f);
			grabbedPhysGrabObject.OverrideBreakEffects(0.5f);
		}
		grabbedObjectTransform = hit.transform;
		if ((bool)grabbedPhysGrabObject)
		{
			PhysGrabObjectCollider component = hit.collider.GetComponent<PhysGrabObjectCollider>();
			grabbedPhysGrabObjectCollider = hit.collider;
			grabbedPhysGrabObjectColliderID = component.colliderID;
			if ((bool)grabbedStaticGrabObject)
			{
				grabbedStaticGrabObject.GrabEnded(this);
				grabbedStaticGrabObject = null;
			}
		}
		else
		{
			grabbedPhysGrabObject = null;
			grabbedPhysGrabObjectCollider = null;
			grabbedPhysGrabObjectColliderID = 0;
			grabbedStaticGrabObject = grabbedObjectTransform.GetComponent<StaticGrabObject>();
			if (!grabbedStaticGrabObject)
			{
				StaticGrabObject[] componentsInParent = grabbedObjectTransform.GetComponentsInParent<StaticGrabObject>();
				foreach (StaticGrabObject staticGrabObject in componentsInParent)
				{
					if (staticGrabObject.colliderTransform == hit.collider.transform)
					{
						grabbedStaticGrabObject = staticGrabObject;
					}
				}
			}
			if (!grabbedStaticGrabObject || !grabbedStaticGrabObject.enabled)
			{
				return;
			}
		}
		PhysGrabPointActivate();
		physGrabPointPuller.gameObject.SetActive(value: true);
		grabbedObject = hit.rigidbody;
		Vector3 vector = hit.point;
		if ((bool)grabbedPhysGrabObject && (bool)grabbedPhysGrabObject.roomVolumeCheck && grabbedPhysGrabObject.roomVolumeCheck.currentSize.magnitude < 0.5f)
		{
			vector = hit.collider.bounds.center;
		}
		float num = Vector3.Distance(playerCamera.transform.position, vector);
		Vector3 position = playerCamera.transform.position + playerCamera.transform.forward * num;
		physGrabPointPlane.position = position;
		physGrabPointPuller.position = position;
		if (physRotatingTimer <= 0f && (bool)Camera.main)
		{
			cameraRelativeGrabbedForward = Camera.main.transform.InverseTransformDirection(grabbedObjectTransform.forward);
			cameraRelativeGrabbedUp = Camera.main.transform.InverseTransformDirection(grabbedObjectTransform.up);
			cameraRelativeGrabbedRight = Camera.main.transform.InverseTransformDirection(grabbedObjectTransform.right);
		}
		if (GameManager.instance.gameMode == 0)
		{
			physGrabPoint.position = vector;
			if (!grabbedPhysGrabObject || !grabbedPhysGrabObject.forceGrabPoint || !grabbedPhysGrabObject.forceGrabPoint.gameObject.activeSelf)
			{
				localGrabPosition = grabbedObjectTransform.InverseTransformPoint(vector);
			}
			else
			{
				vector = grabbedPhysGrabObject.forceGrabPoint.position;
				num = 1f;
				position = playerCamera.transform.position + playerCamera.transform.forward * num - playerCamera.transform.up * 0.3f;
				physGrabPoint.position = vector;
				physGrabPointPlane.position = position;
				physGrabPointPuller.position = position;
				localGrabPosition = grabbedObjectTransform.InverseTransformPoint(vector);
			}
		}
		else if ((bool)grabbedPhysGrabObject)
		{
			if ((bool)grabbedPhysGrabObject.forceGrabPoint && grabbedPhysGrabObject.forceGrabPoint.gameObject.activeSelf)
			{
				vector = grabbedPhysGrabObject.forceGrabPoint.position;
				Quaternion quaternion = Quaternion.Euler(45f, 0f, 0f);
				cameraRelativeGrabbedForward = quaternion * Vector3.forward;
				cameraRelativeGrabbedUp = quaternion * Vector3.up;
				cameraRelativeGrabbedRight = quaternion * Vector3.right;
				num = 1f;
				position = playerCamera.transform.position + playerCamera.transform.forward * num - playerCamera.transform.up * 0.3f;
				if (!overrideGrabPointTransform)
				{
					physGrabPoint.position = vector;
				}
				else
				{
					physGrabPoint.position = overrideGrabPointTransform.position;
				}
				physGrabPointPlane.position = position;
				physGrabPointPuller.position = position;
			}
			grabbedPhysGrabObject.GrabLink(photonView.ViewID, grabbedPhysGrabObjectColliderID, vector, cameraRelativeGrabbedForward, cameraRelativeGrabbedUp);
		}
		else if ((bool)grabbedStaticGrabObject)
		{
			grabbedStaticGrabObject.GrabLink(photonView.ViewID, vector);
		}
		if (isLocal)
		{
			PlayerController.instance.physGrabObject = grabbedObjectTransform.gameObject;
			PlayerController.instance.physGrabActive = true;
		}
		initialPressTimer = 0.1f;
		prevGrabbed = grabbed;
		grabbed = true;
		playerAvatar.tumble.OverrideLookAtCamera(0.5f);
	}

	private void RayCheck(bool _grab)
	{
		if (playerAvatar.isDisabled || playerAvatar.deadSet)
		{
			return;
		}
		float maxDistance = 10f;
		if (_grab)
		{
			grabDisableTimer = 0.1f;
		}
		Vector3 forward = playerCamera.transform.forward;
		bool isTumbling = playerAvatar.isTumbling;
		PhysGrabObject physGrabObject = null;
		if (isTumbling)
		{
			physGrabObject = playerAvatar.tumble.physGrabObject;
		}
		if (!_grab)
		{
			RaycastHit[] array = Physics.SphereCastAll(playerCamera.transform.position, 1f, forward, maxDistance, mask, QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit = array[i];
				ValuableObject component = raycastHit.transform.GetComponent<ValuableObject>();
				if (!component || (component.discovered && !component.discoveredReminder))
				{
					continue;
				}
				Vector3 vector = raycastHit.point;
				Collider[] array2 = Physics.OverlapSphere(vector, 0.01f, mask);
				bool flag = false;
				Collider[] array3 = array2;
				for (int j = 0; j < array3.Length; j++)
				{
					if (array3[j].transform.GetComponentInParent<ValuableObject>() != component)
					{
						flag = true;
						break;
					}
				}
				if (flag && (bool)component.physGrabObject)
				{
					vector = Vector3.MoveTowards(raycastHit.point, component.physGrabObject.centerPoint, 0.1f);
				}
				if (!component.discovered)
				{
					Vector3 direction = playerCamera.transform.position - vector;
					RaycastHit[] array4 = Physics.SphereCastAll(vector, 0.01f, direction, direction.magnitude, mask, QueryTriggerInteraction.Collide);
					bool flag2 = true;
					RaycastHit[] array5 = array4;
					for (int j = 0; j < array5.Length; j++)
					{
						RaycastHit raycastHit2 = array5[j];
						if (!raycastHit2.transform.CompareTag("Player") && raycastHit2.transform != raycastHit.transform)
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						component.Discover(ValuableDiscoverGraphic.State.Discover);
					}
				}
				else
				{
					if (!component.discoveredReminder)
					{
						continue;
					}
					Vector3 direction2 = playerCamera.transform.position - vector;
					RaycastHit[] array6 = Physics.RaycastAll(vector, direction2, direction2.magnitude, mask, QueryTriggerInteraction.Collide);
					bool flag3 = true;
					RaycastHit[] array5 = array6;
					foreach (RaycastHit raycastHit3 in array5)
					{
						if (raycastHit3.collider.transform.CompareTag("Wall"))
						{
							flag3 = false;
							break;
						}
					}
					if (flag3)
					{
						component.discoveredReminder = false;
						component.Discover(ValuableDiscoverGraphic.State.Reminder);
					}
				}
			}
		}
		if (grabState != GrabState.Climb && Physics.Raycast(playerCamera.transform.position, forward, out var hitInfo, maxDistance, mask, QueryTriggerInteraction.Ignore) && hitInfo.collider.CompareTag("Phys Grab Object"))
		{
			PhysGrabObject physGrabObject2 = hitInfo.transform.GetComponent<PhysGrabObject>();
			if ((isTumbling && physGrabObject2 == physGrabObject) || hitInfo.distance > grabRange)
			{
				return;
			}
			if (_grab)
			{
				if ((bool)physGrabObject2 && physGrabObject2.grabDisableTimer > 0f)
				{
					return;
				}
				StartGrabbingPhysObject(hitInfo, physGrabObject2);
			}
			if (!grabbed)
			{
				bool flag4 = false;
				if (!physGrabObject2)
				{
					physGrabObject2 = hitInfo.transform.GetComponentInParent<PhysGrabObject>();
				}
				if ((bool)physGrabObject2)
				{
					currentlyLookingAtPhysGrabObject = physGrabObject2;
					flag4 = true;
				}
				StaticGrabObject staticGrabObject = hitInfo.transform.GetComponent<StaticGrabObject>();
				if (!staticGrabObject)
				{
					staticGrabObject = hitInfo.transform.GetComponentInParent<StaticGrabObject>();
				}
				if ((bool)staticGrabObject && staticGrabObject.enabled)
				{
					currentlyLookingAtStaticGrabObject = staticGrabObject;
					flag4 = true;
				}
				ItemAttributes component2 = hitInfo.transform.GetComponent<ItemAttributes>();
				if ((bool)component2)
				{
					currentlyLookingAtItemAttributes = component2;
					component2.ShowInfo();
				}
				if (flag4)
				{
					Aim.instance.SetState(Aim.State.Grabbable);
				}
			}
		}
		if (!(PlayerController.instance.playerAvatarScript.upgradeTumbleClimb > 0f) || !(PlayerController.instance.EnergyCurrent >= 0f) || !TumbleUI.instance.canExit || grabbed)
		{
			return;
		}
		float maxDistance2 = 3f + grabRange;
		if (grabState != GrabState.Climb && Physics.Raycast(playerCamera.transform.position, forward, out var hitInfo2, maxDistance2, mask, QueryTriggerInteraction.Ignore) && isTumbling && grabState != GrabState.Climb && hitInfo2.collider.gameObject.layer == LayerMask.NameToLayer("Default"))
		{
			Aim.instance.SetState(Aim.State.Climbable);
			if (_grab)
			{
				grabClimbCollider = hitInfo2.collider;
				grabClimbColliderPosition = hitInfo2.collider.transform.position;
				grabClimbPos = hitInfo2.point;
				physGrabPoint.position = hitInfo2.point;
				physGrabPointPlane.position = hitInfo2.point;
				physGrabPointPuller.position = hitInfo2.point;
				physGrabPointPullerPosition = hitInfo2.point;
				physGrabPointPosition = hitInfo2.point;
				localGrabPosition = hitInfo2.point;
				initialPressTimer = 0.1f;
				prevGrabbed = grabbed;
				grabbed = true;
				physGrabPointPuller.gameObject.SetActive(value: true);
				GrabLinkClimb(hitInfo2.point);
				GrabStateSet(GrabState.Climb);
				PhysGrabPointActivate();
			}
		}
	}

	private void ForceGrabPhysObject(PhysGrabObject _physObject)
	{
		Vector3 vector = _physObject.midPoint - playerCamera.transform.position;
		RaycastHit[] array = Physics.RaycastAll(playerCamera.transform.position, vector.normalized, vector.magnitude, LayerMask.GetMask("PhysGrabObject"), QueryTriggerInteraction.Ignore);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit hit = array[i];
			if ((bool)_physObject && !(hit.collider?.GetComponentInParent<PhysGrabObject>() != _physObject))
			{
				if (grabbed)
				{
					ReleaseObject(-1);
				}
				StartGrabbingPhysObject(hit, _physObject);
				if (grabbed)
				{
					toggleGrab = true;
				}
				break;
			}
		}
	}

	private void GrabLinkClimb(Vector3 point)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("GrabLinkClimbRPC", RpcTarget.All, point);
		}
		else
		{
			GrabLinkClimbRPC(point);
		}
	}

	[PunRPC]
	private void GrabLinkClimbRPC(Vector3 point)
	{
		physGrabPoint.position = point;
		physGrabPointPosition = point;
		physGrabPointPlane.position = point;
		physGrabPointPuller.position = point;
		physGrabPointPullerPosition = point;
		grabClimbPos = point;
		localGrabPosition = point;
		grabbedObjectTransform = null;
		grabbedPhysGrabObjectColliderID = -1;
		grabbedPhysGrabObjectCollider = null;
		prevGrabbed = grabbed;
		grabbed = true;
	}

	[PunRPC]
	private void GrabClimbEndedRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			prevGrabbed = grabbed;
			grabbed = false;
		}
	}

	public void ReleaseObject(int _releaseObjectViewID, float _disableTimer = 0.1f)
	{
		if (!grabbed)
		{
			if (_releaseObjectViewID == -1)
			{
				return;
			}
			PhotonView photonView = PhotonView.Find(_releaseObjectViewID);
			if (!photonView)
			{
				return;
			}
			PhysGrabObject component = photonView.GetComponent<PhysGrabObject>();
			if ((bool)component)
			{
				component.photonView.RPC("GrabEndedRPC", RpcTarget.MasterClient, this.photonView.ViewID);
				return;
			}
			StaticGrabObject component2 = photonView.GetComponent<StaticGrabObject>();
			if ((bool)component2)
			{
				component2.photonView.RPC("GrabEndedRPC", RpcTarget.MasterClient, this.photonView.ViewID);
			}
			return;
		}
		overrideGrabTarget = null;
		if ((bool)physGrabPoint)
		{
			PhysGrabEnded();
			physGrabPoint.SetParent(base.transform.parent, worldPositionStays: true);
			grabbedObject = null;
			grabbedObjectTransform = null;
			grabbedPhysGrabObject = null;
			grabbedStaticGrabObject = null;
			prevGrabbed = grabbed;
			grabbed = false;
			GrabStateSet(GrabState.None);
			physGrabBeamScript.lineRenderer.enabled = false;
			if (isLocal)
			{
				PlayerController.instance.physGrabObject = null;
				PlayerController.instance.physGrabActive = false;
			}
			if ((bool)physGrabPoint)
			{
				PhysGrabPointDeactivate();
			}
			if ((bool)physGrabPointPuller)
			{
				physGrabPointPuller.gameObject.SetActive(value: false);
			}
			PhysGrabBeamDeactivate();
			grabDisableTimer = _disableTimer;
		}
	}

	[PunRPC]
	public void ReleaseObjectRPC(bool physGrabEnded, float _disableTimer, int _releaseObjectViewID)
	{
		if (isLocal)
		{
			if (!physGrabEnded && (bool)grabbedStaticGrabObject)
			{
				grabbedStaticGrabObject.GrabEnded(this);
				grabbedStaticGrabObject = null;
			}
			ReleaseObject(_releaseObjectViewID);
			grabDisableTimer = _disableTimer;
		}
	}

	private void GridObjectsInstantiate()
	{
		PhysGrabObject physGrabObject = grabbedPhysGrabObject;
		if (physGrabObject.GetComponent<PhysGrabObjectImpactDetector>().isCart)
		{
			return;
		}
		Quaternion rotation = grabbedPhysGrabObject.rb.rotation;
		grabbedPhysGrabObject.rb.rotation = Quaternion.identity;
		Collider[] componentsInChildren = physGrabObject.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (!collider.isTrigger && collider.gameObject.activeSelf && !(collider is MeshCollider))
			{
				GameObject gameObject = Object.Instantiate(physGrabPointVisualGridObject);
				gameObject.SetActive(value: true);
				SetGridObjectScale(gameObject.transform, collider);
				Quaternion rotation2 = grabbedObjectTransform.rotation;
				physGrabPointVisualGrid.rotation = Quaternion.identity;
				grabbedObjectTransform.rotation = Quaternion.identity;
				physGrabPointVisualGrid.localRotation = Quaternion.identity;
				Vector3 position = grabbedPhysGrabObject.transform.position;
				physGrabPointVisualGrid.position = grabbedPhysGrabObject.transform.TransformPoint(grabbedPhysGrabObject.midPointOffset);
				grabbedPhysGrabObject.transform.position = Vector3.zero;
				gameObject.transform.position = collider.bounds.center;
				gameObject.transform.rotation = collider.transform.rotation;
				gameObject.transform.SetParent(physGrabPointVisualGrid);
				physGrabPointVisualGridObjects.Add(gameObject);
				grabbedObjectTransform.rotation = rotation2;
				grabbedPhysGrabObject.transform.position = position;
			}
		}
		grabbedPhysGrabObject.rb.rotation = rotation;
	}

	private void SetGridObjectScale(Transform _itemEquipCubeTransform, Collider _collider)
	{
		Quaternion rotation = _collider.transform.rotation;
		_collider.transform.rotation = Quaternion.identity;
		if (_collider is BoxCollider boxCollider)
		{
			_itemEquipCubeTransform.localScale = Vector3.Scale(boxCollider.size, _collider.transform.lossyScale);
		}
		else if (_collider is SphereCollider { radius: var radius })
		{
			float num = radius * Mathf.Max(_collider.transform.lossyScale.x, _collider.transform.lossyScale.y, _collider.transform.lossyScale.z) * 2f;
			_itemEquipCubeTransform.localScale = new Vector3(num, num, num);
		}
		else if (_collider is CapsuleCollider capsuleCollider)
		{
			float num2 = capsuleCollider.radius * Mathf.Max(_collider.transform.lossyScale.x, _collider.transform.lossyScale.z) * 2f;
			float y = capsuleCollider.height * _collider.transform.lossyScale.y;
			_itemEquipCubeTransform.localScale = new Vector3(num2, y, num2);
		}
		else
		{
			_itemEquipCubeTransform.localScale = _collider.bounds.size;
		}
		_collider.transform.rotation = rotation;
	}

	public void GrabStateSet(GrabState _state)
	{
		if (isLocal && _state != grabState)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("GrabStateSetRPC", RpcTarget.All, (int)_state);
			}
			else
			{
				GrabStateSetRPC((int)_state);
			}
		}
	}

	[PunRPC]
	private void GrabStateSetRPC(int _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			grabState = (GrabState)_state;
			grabStateStart = true;
		}
	}

	private void GridObjectsRemove()
	{
		foreach (GameObject physGrabPointVisualGridObject in physGrabPointVisualGridObjects)
		{
			Object.Destroy(physGrabPointVisualGridObject);
		}
		physGrabPointVisualGridObjects.Clear();
	}

	private void PhysGrabInTumbleStateLookAtCameraDirection()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && grabbed && grabState != GrabState.Climb && playerAvatar.isTumbling)
		{
			playerAvatar.tumble.OverrideLookAtCamera(0.5f);
		}
	}
}
