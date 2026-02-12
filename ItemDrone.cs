using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ItemDrone : MonoBehaviour
{
	public GameObject teleportParticles;

	private ItemAttributes itemAttributes;

	internal SemiFunc.emojiIcon emojiIcon;

	public Texture droneIcon;

	internal ColorPresets colorPreset;

	public BatteryDrainPresets batteryDrainPreset;

	internal float batteryDrainRate = 0.1f;

	internal Color droneColor;

	internal Color batteryColor;

	internal Color beamColor;

	private float checkTimer;

	private Transform magnetTarget;

	internal PhysGrabObject magnetTargetPhysGrabObject;

	internal Rigidbody magnetTargetRigidbody;

	internal bool magnetActive;

	public PlayerTumble playerTumbleTarget;

	private Rigidbody rb;

	private bool attachPointFound;

	private Vector3 attachPoint;

	private float springConstant = 50f;

	private float dampingCoefficient = 5f;

	private float newAttachPointTimer;

	internal bool itemActivated;

	private PhotonView photonView;

	private Vector3 rayHitPosition;

	private Vector3 animatedRayHitPosition;

	private LineBetweenTwoPoints lineBetweenTwoPoints;

	public Transform lineStartPoint;

	private float rayTimer;

	private Transform prevMagnetTarget;

	private Transform droneTransform;

	private List<Transform> dronePyramidTransforms = new List<Transform>();

	private List<Transform> droneTriangleTransforms = new List<Transform>();

	private float lerpAnimationProgress;

	private bool hasBattery = true;

	private ItemBattery itemBattery;

	private float onNoBatteryTimer;

	private bool animationOpen;

	private Transform onSwitchTransform;

	public ItemDroneSounds itemDroneSounds;

	internal Sound soundDroneLoop = new Sound();

	internal Sound soundDroneBeamLoop = new Sound();

	public PhysicMaterial physicMaterialSlippery;

	public bool targetValuables;

	public bool targetPlayers;

	public bool targetEnemies;

	public bool targetNonValuables;

	internal Vector3 connectionPoint;

	internal Transform lastPlayerToTouch;

	private PhysGrabObject physGrabObject;

	private float randomNudgeTimer;

	private Collider droneCollider;

	private PhysicMaterial physicMaterialOriginal;

	private ItemToggle itemToggle;

	internal PlayerAvatar playerAvatarTarget;

	private bool targetIsPlayer;

	internal bool targetIsLocalPlayer;

	private ItemEquippable itemEquippable;

	private Camera cameraMain;

	internal PlayerAvatar droneOwner;

	private float teleportSpotTimer;

	private bool hadTarget;

	private bool targetIsEnemy;

	private bool togglePrevious;

	private bool fullReset;

	private bool fullInit;

	private EnemyParent enemyTarget;

	private bool magnetActivePrev;

	private ITargetingCondition customTargetingCondition;

	private void Start()
	{
		foreach (Transform item2 in base.transform)
		{
			if (item2.name == "Particles")
			{
				teleportParticles = item2.gameObject;
				break;
			}
		}
		customTargetingCondition = GetComponent<ITargetingCondition>();
		droneCollider = GetComponentInChildren<Collider>();
		cameraMain = Camera.main;
		itemEquippable = GetComponent<ItemEquippable>();
		itemEquippable.itemEmoji = emojiIcon.ToString();
		itemToggle = GetComponent<ItemToggle>();
		rb = GetComponent<Rigidbody>();
		photonView = GetComponent<PhotonView>();
		lineBetweenTwoPoints = GetComponent<LineBetweenTwoPoints>();
		itemBattery = GetComponent<ItemBattery>();
		if (!itemBattery)
		{
			hasBattery = false;
		}
		physGrabObject = GetComponent<PhysGrabObject>();
		itemAttributes = GetComponent<ItemAttributes>();
		emojiIcon = itemAttributes.emojiIcon;
		colorPreset = itemAttributes.colorPreset;
		droneColor = colorPreset.GetColorMain();
		batteryColor = colorPreset.GetColorLight();
		beamColor = colorPreset.GetColorDark();
		batteryDrainRate = batteryDrainPreset.GetBatteryDrainRate();
		itemBattery.batteryDrainRate = batteryDrainRate;
		itemBattery.batteryColor = batteryColor;
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		int num = 0;
		if (num < componentsInChildren.Length)
		{
			Collider collider = componentsInChildren[num];
			physicMaterialOriginal = collider.material;
		}
		Sound.CopySound(itemDroneSounds.DroneLoop, soundDroneLoop);
		Sound.CopySound(itemDroneSounds.DroneBeamLoop, soundDroneBeamLoop);
		ItemLight componentInChildren = GetComponentInChildren<ItemLight>();
		if ((bool)componentInChildren)
		{
			componentInChildren.itemLight.color = droneColor;
		}
		AudioSource component = GetComponent<AudioSource>();
		soundDroneLoop.Source = component;
		soundDroneBeamLoop.Source = component;
		foreach (Transform item3 in base.transform)
		{
			if (item3.name == "Drone Icon")
			{
				onSwitchTransform = item3;
				onSwitchTransform.GetComponent<Renderer>().material.SetTexture("_EmissionMap", droneIcon);
				onSwitchTransform.GetComponent<Renderer>().material.SetColor("_EmissionColor", droneColor);
			}
			if (!(item3.name == "Drone"))
			{
				continue;
			}
			droneTransform = item3;
			foreach (Transform item4 in item3)
			{
				if (item4.name.Contains("Drone Triangle"))
				{
					foreach (Transform item5 in item4)
					{
						droneTriangleTransforms.Add(item5);
					}
				}
				if (!item4.name.Contains("Drone Pyramid"))
				{
					continue;
				}
				foreach (Transform item6 in item4)
				{
					dronePyramidTransforms.Add(item6);
					item6.GetComponent<Renderer>().material.SetColor("_EmissionColor", droneColor);
				}
			}
		}
		droneTransform.GetComponent<Renderer>().material.SetColor("_EmissionColor", droneColor);
		physGrabObject.clientNonKinematic = true;
	}

	private void AnimateDrone()
	{
		if (!itemActivated)
		{
			return;
		}
		lerpAnimationProgress += Time.deltaTime * 10f;
		if (lerpAnimationProgress > 1f)
		{
			lerpAnimationProgress = 1f;
			animationOpen = true;
		}
		float num = 15f;
		if (magnetActive)
		{
			num = 60f;
		}
		foreach (Transform dronePyramidTransform in dronePyramidTransforms)
		{
			float num2 = -33f;
			if (lerpAnimationProgress != 1f)
			{
				dronePyramidTransform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(0f, num2, lerpAnimationProgress), 0f);
				continue;
			}
			float num3 = Mathf.Sin(Time.time * num) * 5f;
			dronePyramidTransform.localRotation = Quaternion.Euler(0f, num2 + num3, 0f);
		}
		foreach (Transform droneTriangleTransform in droneTriangleTransforms)
		{
			float num4 = 45f;
			if (lerpAnimationProgress != 1f)
			{
				droneTriangleTransform.localRotation = Quaternion.Euler(Mathf.Lerp(0f, num4, lerpAnimationProgress), 0f, 0f);
				continue;
			}
			float num5 = Mathf.Sin(Time.time * num / 3f) * 10f;
			droneTriangleTransform.localRotation = Quaternion.Euler(num4 + num5, 0f, 0f);
		}
	}

	private bool TargetFindPlayer()
	{
		if (itemBattery.batteryLife <= 0f)
		{
			return false;
		}
		float num = 10000f;
		Collider[] array = Physics.OverlapSphere(base.transform.position, 1f, LayerMask.GetMask("Player"));
		foreach (Collider collider in array)
		{
			PlayerAvatar playerAvatar = collider.GetComponentInParent<PlayerAvatar>();
			if (!playerAvatar)
			{
				PlayerController componentInParent = collider.GetComponentInParent<PlayerController>();
				if ((bool)componentInParent)
				{
					playerAvatar = componentInParent.playerAvatarScript;
				}
			}
			if (!playerAvatar || (customTargetingCondition != null && !customTargetingCondition.CustomTargetingCondition(playerAvatar.gameObject)))
			{
				continue;
			}
			float num2 = Vector3.Distance(base.transform.position, playerAvatar.PlayerVisionTarget.VisionTransform.position);
			if (num2 < num)
			{
				num = num2;
				playerAvatarTarget = playerAvatar;
				targetIsPlayer = true;
				if (playerAvatarTarget.isLocal)
				{
					targetIsLocalPlayer = true;
				}
			}
		}
		if ((bool)playerAvatarTarget)
		{
			Transform visionTransform = playerAvatarTarget.PlayerVisionTarget.VisionTransform;
			Vector3 newAttachPoint = visionTransform.position;
			if (playerAvatarTarget.isTumbling && (bool)playerAvatarTarget.transform)
			{
				playerTumbleTarget = playerAvatarTarget.tumble;
				visionTransform = playerTumbleTarget.transform;
				newAttachPoint = playerTumbleTarget.physGrabObject.centerPoint;
			}
			targetIsLocalPlayer = playerAvatarTarget.isLocal;
			targetIsPlayer = true;
			NewRayHitPoint(newAttachPoint, playerAvatarTarget.GetComponent<PhotonView>().ViewID, -1, visionTransform);
			attachPoint = rayHitPosition;
			ActivateMagnet();
			return true;
		}
		return false;
	}

	private void GetPlayerTumbleTarget()
	{
		if ((bool)magnetTarget)
		{
			if ((bool)playerTumbleTarget && !playerTumbleTarget.playerAvatar.isTumbling)
			{
				playerAvatarTarget = playerTumbleTarget.playerAvatar;
				targetIsLocalPlayer = playerAvatarTarget.isLocal;
				targetIsPlayer = true;
				ActivateMagnet();
				playerTumbleTarget = null;
				Transform visionTransform = playerAvatarTarget.PlayerVisionTarget.VisionTransform;
				Vector3 position = visionTransform.position;
				NewRayHitPoint(position, playerAvatarTarget.GetComponent<PhotonView>().ViewID, -1, visionTransform);
			}
			if ((bool)playerAvatarTarget && playerAvatarTarget.isTumbling)
			{
				playerTumbleTarget = playerAvatarTarget.tumble;
				targetIsLocalPlayer = false;
				targetIsPlayer = false;
				magnetTarget = playerTumbleTarget.transform;
				magnetTargetPhysGrabObject = playerTumbleTarget.physGrabObject;
				playerAvatarTarget = null;
				attachPoint = rayHitPosition;
				ActivateMagnet();
			}
		}
	}

	private void FullReset()
	{
		hadTarget = false;
		magnetTarget = null;
		magnetActivePrev = true;
		magnetActive = false;
		magnetTargetPhysGrabObject = null;
		magnetTargetRigidbody = null;
		DeactivateMagnet();
		playerTumbleTarget = null;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
		attachPoint = Vector3.zero;
		attachPointFound = false;
		rayHitPosition = Vector3.zero;
		animatedRayHitPosition = Vector3.zero;
	}

	private void ToggleOnFullInit()
	{
		if (!fullInit && itemActivated && !togglePrevious)
		{
			fullReset = false;
			fullInit = true;
			togglePrevious = true;
		}
	}

	private void ToggleOffFullReset()
	{
		if (!fullReset && !itemActivated && togglePrevious)
		{
			fullReset = true;
			fullInit = false;
			togglePrevious = false;
		}
	}

	private void ToggleOffIfLostTarget()
	{
	}

	private void ToggleOffIfEnemyTargetIsDead()
	{
		if (!magnetTarget || !targetIsEnemy)
		{
			return;
		}
		if ((bool)enemyTarget)
		{
			if (!enemyTarget.Spawned && itemToggle.toggleState)
			{
				ForceTurnOff();
				enemyTarget = null;
			}
		}
		else if (itemToggle.toggleState)
		{
			ForceTurnOff();
			enemyTarget = null;
		}
	}

	private void ToggleOffIfPlayerTargetIsDead()
	{
		if (SemiFunc.FPSImpulse5() && targetIsPlayer)
		{
			if ((bool)playerAvatarTarget && playerAvatarTarget.isDisabled && itemToggle.toggleState)
			{
				ButtonToggleSet(toggle: false);
				playerAvatarTarget = null;
			}
			if ((bool)playerTumbleTarget && playerTumbleTarget.playerAvatar.isDisabled && itemToggle.toggleState)
			{
				ButtonToggleSet(toggle: false);
				playerTumbleTarget = null;
			}
		}
	}

	private void ForceTurnOff()
	{
		itemBattery.BatteryToggle(toggle: false);
		ButtonToggleSet(toggle: false);
		itemToggle.ToggleItem(toggle: false);
		hadTarget = false;
		itemActivated = false;
	}

	private void Update()
	{
		if (itemEquippable.isEquipped)
		{
			return;
		}
		if (magnetActivePrev != magnetActive)
		{
			BatteryToggle(magnetActive);
			magnetActivePrev = magnetActive;
		}
		if (hadTarget && !magnetActive)
		{
			ForceTurnOff();
		}
		else
		{
			if (!SemiFunc.RunIsLevel() && !SemiFunc.RunIsLobby() && !SemiFunc.RunIsShop() && !SemiFunc.RunIsArena() && !SemiFunc.RunIsTutorial())
			{
				return;
			}
			soundDroneLoop.PlayLoop(itemActivated, 2f, 2f);
			AnimateDrone();
			if (!itemActivated)
			{
				onNoBatteryTimer = 0f;
			}
			if (itemActivated)
			{
				physGrabObject.impactDetector.canHurtLogic = false;
			}
			else
			{
				physGrabObject.impactDetector.canHurtLogic = true;
			}
			if (itemActivated && magnetActive && (bool)magnetTarget && !itemEquippable.isEquipped)
			{
				if (rayHitPosition != Vector3.zero && !targetIsPlayer)
				{
					bool flag = false;
					if ((bool)playerTumbleTarget && playerTumbleTarget.playerAvatar.isLocal)
					{
						flag = true;
					}
					if (!flag)
					{
						animatedRayHitPosition = Vector3.Lerp(animatedRayHitPosition, rayHitPosition, Time.deltaTime * 10f);
						lineBetweenTwoPoints.DrawLine(lineStartPoint.position, magnetTarget.TransformPoint(animatedRayHitPosition));
						connectionPoint = magnetTarget.TransformPoint(animatedRayHitPosition);
					}
					else
					{
						Vector3 vector = new Vector3(0f, -0.5f, 0f);
						Vector3 point = cameraMain.transform.position + vector;
						lineBetweenTwoPoints.DrawLine(lineStartPoint.position, point);
						connectionPoint = point;
					}
				}
				else
				{
					animatedRayHitPosition = Vector3.Lerp(animatedRayHitPosition, rayHitPosition, Time.deltaTime * 10f);
					if (!targetIsPlayer)
					{
						lineBetweenTwoPoints.DrawLine(lineStartPoint.position, magnetTargetPhysGrabObject.midPoint);
						connectionPoint = magnetTargetPhysGrabObject.midPoint;
					}
					if (targetIsPlayer)
					{
						Vector3 vector2 = new Vector3(0f, -0.5f, 0f);
						if (playerAvatarTarget.isTumbling)
						{
							vector2 = Vector3.zero;
						}
						if (!targetIsLocalPlayer)
						{
							lineBetweenTwoPoints.DrawLine(lineStartPoint.position, magnetTarget.position + vector2);
							connectionPoint = magnetTarget.position + vector2;
						}
						else
						{
							Vector3 point2 = cameraMain.transform.position + vector2;
							lineBetweenTwoPoints.DrawLine(lineStartPoint.position, point2);
							connectionPoint = point2;
						}
					}
				}
			}
			if (!itemActivated && !magnetActive && animationOpen)
			{
				lerpAnimationProgress += Time.deltaTime * 10f;
				if (lerpAnimationProgress > 1f)
				{
					lerpAnimationProgress = 1f;
					animationOpen = false;
				}
				foreach (Transform dronePyramidTransform in dronePyramidTransforms)
				{
					float num = 0f;
					dronePyramidTransform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(-33f, num, lerpAnimationProgress), 0f);
				}
				foreach (Transform droneTriangleTransform in droneTriangleTransforms)
				{
					float num2 = 0f;
					droneTriangleTransform.localRotation = Quaternion.Euler(Mathf.Lerp(45f, num2, lerpAnimationProgress), 0f, 0f);
				}
			}
			if (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient)
			{
				return;
			}
			GetPlayerTumbleTarget();
			if (itemToggle.toggleState != itemActivated)
			{
				ButtonToggle();
			}
			if (physGrabObject.playerGrabbing.Count == 1)
			{
				lastPlayerToTouch = physGrabObject.playerGrabbing[0].transform;
			}
			if (!itemActivated)
			{
				return;
			}
			springConstant = 40f;
			dampingCoefficient = 10f;
			if (!magnetActive)
			{
				checkTimer += Time.deltaTime;
				if (checkTimer > 0.5f)
				{
					bool flag2 = false;
					playerTumbleTarget = null;
					playerAvatarTarget = null;
					targetIsPlayer = false;
					targetIsEnemy = false;
					targetIsLocalPlayer = false;
					if (targetPlayers)
					{
						flag2 = TargetFindPlayer();
					}
					if (targetValuables || targetNonValuables || targetEnemies)
					{
						if (flag2)
						{
							SphereCheck(0.5f);
						}
						else
						{
							flag2 = SphereCheck(1f);
						}
					}
					if (flag2)
					{
						hadTarget = true;
						ActivateMagnet();
					}
					checkTimer = 0f;
				}
			}
			else if (!attachPointFound)
			{
				if (!targetIsPlayer)
				{
					if (rayTimer <= 0f)
					{
						FindBeamAttachPosition();
						rayTimer = 0.5f;
					}
					else
					{
						rayTimer -= Time.deltaTime;
					}
				}
			}
			else if (rb.velocity.magnitude > 0.2f)
			{
				newAttachPointTimer += Time.deltaTime;
				if (newAttachPointTimer > 0.5f)
				{
					attachPointFound = false;
					newAttachPointTimer = 0f;
					rayTimer = 0f;
				}
			}
			if (itemActivated && hasBattery && itemBattery.batteryLife <= 0f)
			{
				if (!itemBattery.batteryActive)
				{
					itemBattery.BatteryToggle(toggle: true);
				}
				onNoBatteryTimer += Time.deltaTime;
				if (onNoBatteryTimer >= 1.5f)
				{
					ForceTurnOff();
					onNoBatteryTimer = 0f;
				}
			}
		}
	}

	public void ButtonToggleSet(bool toggle)
	{
		if (!SemiFunc.IsMultiplayer())
		{
			ButtonToggleRPC(toggle);
		}
		else if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("ButtonToggleRPC", RpcTarget.All, toggle);
		}
	}

	private void FixedUpdate()
	{
		if (!itemActivated)
		{
			return;
		}
		if ((bool)magnetTarget)
		{
			ItemEquippable componentInParent = magnetTarget.GetComponentInParent<ItemEquippable>();
			if ((bool)componentInParent && componentInParent.isEquipped && magnetActive)
			{
				ForceTurnOff();
				DeactivateMagnet();
			}
		}
		if (itemEquippable.isEquipped)
		{
			if (magnetActive)
			{
				ForceTurnOff();
				DeactivateMagnet();
			}
		}
		else
		{
			if ((!SemiFunc.RunIsLevel() && !SemiFunc.RunIsLobby() && !SemiFunc.RunIsShop() && !SemiFunc.RunIsArena() && !SemiFunc.RunIsTutorial()) || (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient) || !itemActivated || !magnetActive)
			{
				return;
			}
			if (!magnetTarget)
			{
				DeactivateMagnet();
				return;
			}
			if (Vector3.Distance(base.transform.position, magnetTarget.position) > 4f)
			{
				FindTeleportSpot();
			}
			Collider collider = null;
			if ((bool)magnetTarget)
			{
				collider = magnetTarget.GetComponent<Collider>();
			}
			if (!playerTumbleTarget && (!magnetTarget || !magnetTarget.gameObject.activeSelf || !magnetTarget.gameObject.activeInHierarchy || ((bool)collider && !collider.enabled)))
			{
				DeactivateMagnet();
				return;
			}
			physGrabObject.OverrideMaterial(physicMaterialSlippery);
			if (randomNudgeTimer <= 0f)
			{
				if (Vector3.Distance(base.transform.position, magnetTarget.transform.position) > 1.5f)
				{
					Vector3 lhs = base.transform.position - magnetTarget.transform.position;
					Vector3[] array = new Vector3[4]
					{
						Vector3.up,
						Vector3.down,
						Vector3.left,
						Vector3.right
					};
					Vector3 rhs = array[Random.Range(0, array.Length)];
					Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
					if (normalized != Vector3.zero)
					{
						rb.AddForce(normalized * 1f, ForceMode.Impulse);
						rb.AddTorque(normalized * 10f, ForceMode.Impulse);
					}
				}
				randomNudgeTimer = 0.5f;
			}
			else
			{
				randomNudgeTimer -= Time.fixedDeltaTime;
			}
			if (attachPointFound)
			{
				Vector3 vector = magnetTarget.TransformPoint(attachPoint) - base.transform.position;
				Vector3 vector2 = springConstant * vector;
				Vector3 velocity = rb.velocity;
				Vector3 vector3 = (0f - dampingCoefficient) * velocity;
				Vector3 vector4 = vector2 + vector3;
				vector4 = Vector3.ClampMagnitude(vector4, 20f);
				rb.AddForce(vector4);
				if (!magnetTarget.gameObject.activeSelf)
				{
					DeactivateMagnet();
				}
				SemiFunc.PhysLookAtPositionWithForce(rb, base.transform, magnetTarget.TransformPoint(rayHitPosition), 1f);
			}
			else
			{
				Vector3 vector5 = magnetTarget.position - base.transform.position;
				if (vector5.magnitude > 0.8f)
				{
					rb.AddForce(vector5.normalized * 3f);
				}
				else
				{
					Vector3 force = -rb.velocity * 0.9f;
					rb.AddForce(force);
				}
				if (!magnetTarget.gameObject.activeSelf)
				{
					DeactivateMagnet();
				}
				SemiFunc.PhysLookAtPositionWithForce(rb, base.transform, magnetTarget.position, 1f);
			}
		}
	}

	[PunRPC]
	public void TeleportEffectRPC(Vector3 startPosition, Vector3 endPosition)
	{
		itemDroneSounds.DroneRetract.Pitch = 3f;
		itemDroneSounds.DroneRetract.Play(startPosition);
		itemDroneSounds.DroneRetract.Pitch = 4f;
		itemDroneSounds.DroneRetract.Play(endPosition);
		Object.Instantiate(teleportParticles, startPosition, Quaternion.identity);
		Object.Instantiate(teleportParticles, endPosition, Quaternion.identity);
	}

	private void TeleportEffect(Vector3 startPosition, Vector3 endPosition)
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("TeleportEffectRPC", RpcTarget.All, startPosition, endPosition);
			}
		}
		else
		{
			TeleportEffectRPC(startPosition, endPosition);
		}
	}

	private void FindTeleportSpot()
	{
		if (!magnetActive || !magnetTarget)
		{
			return;
		}
		if (teleportSpotTimer <= 0f)
		{
			ItemEquippable componentInParent = magnetTarget.GetComponentInParent<ItemEquippable>();
			if ((bool)componentInParent && componentInParent.isEquipped)
			{
				return;
			}
			Vector3 vector = magnetTarget.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
			for (int i = 0; i < 10; i++)
			{
				vector = magnetTarget.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
				float num = Vector3.Distance(vector, magnetTarget.position);
				float maxDistance = Mathf.Max(0f, num - 0.2f);
				RaycastHit[] array = Physics.RaycastAll(vector, magnetTarget.position - vector, maxDistance, SemiFunc.LayerMaskGetVisionObstruct());
				bool flag = false;
				RaycastHit[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					RaycastHit raycastHit = array2[j];
					if (raycastHit.transform.GetComponentInParent<Rigidbody>() != magnetTarget.GetComponentInParent<Rigidbody>() && raycastHit.transform != base.transform)
					{
						flag = true;
						break;
					}
				}
				if (!flag && Physics.OverlapBox(vector, new Vector3(0.2f, 0.2f, 0.2f), base.transform.rotation, SemiFunc.LayerMaskGetVisionObstruct()).Length == 0)
				{
					TeleportEffect(base.transform.position, vector);
					if (SemiFunc.IsMultiplayer())
					{
						physGrabObject.photonTransformView.Teleport(vector, base.transform.rotation);
					}
					else
					{
						base.transform.position = vector;
					}
					break;
				}
			}
			teleportSpotTimer = 0.2f;
		}
		else
		{
			teleportSpotTimer -= Time.deltaTime;
		}
	}

	private void DeactivateMagnet()
	{
		if (magnetActive)
		{
			attachPointFound = false;
			playerAvatarTarget = null;
			targetIsPlayer = false;
			targetIsLocalPlayer = false;
			targetIsEnemy = false;
			playerTumbleTarget = null;
			magnetTargetPhysGrabObject = null;
			magnetTargetRigidbody = null;
			MagnetActiveToggle(toggleBool: false);
		}
	}

	private void ActivateMagnet()
	{
		if (!magnetActive)
		{
			MagnetActiveToggle(toggleBool: true);
		}
	}

	private void BatteryToggle(bool activated)
	{
		if (hasBattery)
		{
			itemBattery.batteryActive = activated;
		}
	}

	private void ButtonToggleLogic(bool activated)
	{
		FullReset();
		MagnetActiveToggle(activated);
		droneOwner = SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID);
		lerpAnimationProgress = 0f;
		if (activated)
		{
			onSwitchTransform.GetComponent<Renderer>().material.SetColor("_EmissionColor", droneColor);
			itemDroneSounds.DroneStart.Play(base.transform.position);
		}
		else
		{
			if (magnetActive)
			{
				DeactivateMagnet();
			}
			soundDroneLoop.PlayLoop(playing: false, 2f, 2f);
			itemDroneSounds.DroneEnd.Play(base.transform.position);
		}
		itemActivated = activated;
	}

	public void ButtonToggle()
	{
		itemActivated = !itemActivated;
		if (GameManager.instance.gameMode == 0)
		{
			ButtonToggleLogic(itemActivated);
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("ButtonToggleRPC", RpcTarget.All, itemActivated);
		}
	}

	[PunRPC]
	private void ButtonToggleRPC(bool activated)
	{
		ButtonToggleLogic(activated);
	}

	private Transform GetHighestParentWithRigidbody(Transform child)
	{
		if (GetComponent<Rigidbody>() != null && child.GetComponent<PhotonView>() != null)
		{
			return child;
		}
		Transform transform = child;
		while (transform.parent != null)
		{
			if (transform.parent.GetComponent<Rigidbody>() != null && transform.parent.GetComponent<PhotonView>() != null)
			{
				return transform.parent;
			}
			transform = transform.parent;
		}
		return null;
	}

	private void MagnetActiveToggleLogic(bool activated)
	{
		magnetActive = activated;
		lerpAnimationProgress = 0f;
		if (!activated)
		{
			itemDroneSounds.DroneRetract.Play(base.transform.position);
			rayHitPosition = Vector3.zero;
		}
		else
		{
			itemDroneSounds.DroneDeploy.Play(base.transform.position);
		}
	}

	public void MagnetActiveToggle(bool toggleBool)
	{
		if (GameManager.instance.gameMode == 0)
		{
			MagnetActiveToggleLogic(toggleBool);
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("MagnetActiveToggleRPC", RpcTarget.All, toggleBool);
		}
	}

	[PunRPC]
	private void MagnetActiveToggleRPC(bool activated)
	{
		MagnetActiveToggleLogic(activated);
	}

	private void NewRayHitPointLogic(Vector3 newRayHitPosition, int photonViewId, int colliderID, Transform newMagnetTarget)
	{
		if ((bool)newMagnetTarget)
		{
			magnetTargetPhysGrabObject = newMagnetTarget.GetComponent<PhysGrabObject>();
			if (colliderID != -1)
			{
				magnetTarget = newMagnetTarget.GetComponent<PhysGrabObject>().FindColliderFromID(colliderID);
				targetIsPlayer = false;
				targetIsLocalPlayer = false;
			}
			else
			{
				magnetTarget = newMagnetTarget;
			}
			animatedRayHitPosition = rayHitPosition;
			rayHitPosition = magnetTarget.InverseTransformPoint(newRayHitPosition);
			magnetTargetRigidbody = GetHighestParentWithRigidbody(magnetTarget).GetComponent<Rigidbody>();
			PlayerTumble component = magnetTargetRigidbody.GetComponent<PlayerTumble>();
			if ((bool)component)
			{
				if (component.isTumbling)
				{
					playerTumbleTarget = component;
				}
				else
				{
					DeactivateMagnet();
				}
			}
		}
		else
		{
			magnetTargetPhysGrabObject = PhotonView.Find(photonViewId).gameObject.GetComponent<PhysGrabObject>();
			if (colliderID != -1)
			{
				magnetTarget = PhotonView.Find(photonViewId).gameObject.GetComponent<PhysGrabObject>().FindColliderFromID(colliderID);
				targetIsPlayer = false;
				targetIsLocalPlayer = false;
			}
			else
			{
				targetIsPlayer = true;
				PlayerAvatar component2 = PhotonView.Find(photonViewId).GetComponent<PlayerAvatar>();
				playerAvatarTarget = component2;
				magnetTarget = playerAvatarTarget.PlayerVisionTarget.VisionTransform;
				targetIsLocalPlayer = playerAvatarTarget.isLocal;
			}
			animatedRayHitPosition = rayHitPosition;
			rayHitPosition = magnetTarget.InverseTransformPoint(newRayHitPosition);
			magnetTargetRigidbody = GetHighestParentWithRigidbody(magnetTarget).GetComponent<Rigidbody>();
		}
	}

	private void NewRayHitPoint(Vector3 newAttachPoint, int photonViewId, int colliderID, Transform newMagnetTarget)
	{
		if (!GameManager.Multiplayer())
		{
			NewRayHitPointLogic(newAttachPoint, photonViewId, colliderID, newMagnetTarget);
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("NewRayHitPointRPC", RpcTarget.All, newAttachPoint, photonViewId, colliderID);
		}
	}

	[PunRPC]
	private void NewRayHitPointRPC(Vector3 newAttachPoint, int photonViewId, int colliderID)
	{
		NewRayHitPointLogic(newAttachPoint, photonViewId, colliderID, null);
	}

	private bool SphereCheck(float _radius)
	{
		bool flag = false;
		if (itemBattery.batteryLife <= 0f)
		{
			return false;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, _radius);
		Transform transform = null;
		PhysGrabObject physGrabObject = null;
		Rigidbody rigidbody = null;
		Transform transform2 = null;
		int colliderID = -1;
		bool flag2 = false;
		EnemyParent enemyParent = null;
		PlayerTumble playerTumble = null;
		float num = 10000f;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			Transform highestParentWithRigidbody = GetHighestParentWithRigidbody(collider.transform);
			PhysGrabObjectCollider component = collider.GetComponent<PhysGrabObjectCollider>();
			PhysGrabObject physGrabObject2 = null;
			bool flag3 = false;
			bool flag4 = false;
			EnemyParent enemyParent2 = null;
			PlayerTumble playerTumble2 = null;
			playerTumble2 = collider.GetComponentInParent<PlayerTumble>();
			if ((bool)highestParentWithRigidbody)
			{
				PhysGrabObjectImpactDetector component2 = highestParentWithRigidbody.GetComponent<PhysGrabObjectImpactDetector>();
				physGrabObject2 = highestParentWithRigidbody.GetComponent<PhysGrabObject>();
				if ((bool)component2)
				{
					if (component2.isValuable)
					{
						flag4 = true;
					}
					if (component2.isEnemy)
					{
						flag3 = true;
						enemyParent2 = component2.GetComponentInParent<EnemyParent>();
					}
				}
			}
			bool flag5 = true;
			if (customTargetingCondition != null && (bool)highestParentWithRigidbody)
			{
				flag5 = customTargetingCondition.CustomTargetingCondition(highestParentWithRigidbody.gameObject);
			}
			bool flag6 = targetValuables && flag4;
			if (!flag6)
			{
				flag6 = targetEnemies && flag3;
			}
			if (!flag6)
			{
				flag6 = targetNonValuables && !flag4;
			}
			if (!((bool)component && highestParentWithRigidbody != base.transform && (bool)highestParentWithRigidbody && flag6 && flag5) || !highestParentWithRigidbody.gameObject.activeSelf)
			{
				continue;
			}
			float num2 = Vector3.Distance(base.transform.position, physGrabObject2.centerPoint);
			if (num2 < num)
			{
				bool flag7 = false;
				if (Physics.Raycast(base.transform.position, physGrabObject2.centerPoint - base.transform.position, out var hitInfo, (physGrabObject2.centerPoint - base.transform.position).magnitude, LayerMask.GetMask("Default")) && hitInfo.collider.transform != collider.transform && hitInfo.collider.transform != base.transform)
				{
					flag7 = true;
				}
				if (!flag7)
				{
					num = num2;
					transform = collider.transform;
					physGrabObject = physGrabObject2;
					rigidbody = highestParentWithRigidbody.GetComponent<Rigidbody>();
					transform2 = highestParentWithRigidbody;
					colliderID = component.colliderID;
					flag2 = flag3;
					enemyParent = enemyParent2;
					playerTumble = playerTumble2;
					flag = true;
				}
			}
		}
		if (flag)
		{
			playerTumbleTarget = playerTumble;
			playerAvatarTarget = null;
			targetIsPlayer = false;
			targetIsEnemy = false;
			targetIsLocalPlayer = false;
			magnetTarget = transform;
			magnetTargetPhysGrabObject = physGrabObject;
			magnetTargetRigidbody = rigidbody;
			NewRayHitPoint(transform.position, transform2.GetComponent<PhotonView>().ViewID, colliderID, transform2);
			attachPoint = rayHitPosition;
			targetIsEnemy = flag2;
			enemyTarget = enemyParent;
		}
		return flag;
	}

	private void FindBeamAttachPosition()
	{
		if (!magnetTarget)
		{
			return;
		}
		for (int i = 0; i < 6; i++)
		{
			float num = 0.5f;
			Vector3 vector = new Vector3(Random.Range(0f - num, num), Random.Range(0f - num, num), Random.Range(0f - num, num));
			if (Physics.Raycast(base.transform.position, magnetTarget.position - base.transform.position + vector, out var hitInfo, 1f, SemiFunc.LayerMaskGetPhysGrabObject()))
			{
				Transform highestParentWithRigidbody = GetHighestParentWithRigidbody(hitInfo.collider.transform);
				PhysGrabObjectCollider component = hitInfo.collider.transform.GetComponent<PhysGrabObjectCollider>();
				if ((bool)component && highestParentWithRigidbody == magnetTargetPhysGrabObject.transform)
				{
					Vector3 normalized = (base.transform.position - hitInfo.point).normalized;
					NewRayHitPoint(hitInfo.point, highestParentWithRigidbody.GetComponent<PhotonView>().ViewID, component.colliderID, highestParentWithRigidbody);
					Vector3 position = hitInfo.point + normalized * 0.5f;
					attachPoint = magnetTarget.InverseTransformPoint(position);
					attachPointFound = true;
				}
			}
		}
	}

	public void SetTumbleTarget(PlayerTumble tumble)
	{
		if (!SemiFunc.IsMultiplayer())
		{
			SetTumbleTargetRPC(tumble.photonView.ViewID);
		}
		else if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("SetTumbleTargetRPC", RpcTarget.All, tumble.photonView.ViewID);
		}
	}

	[PunRPC]
	public void SetTumbleTargetRPC(int _photonViewID)
	{
		PhotonView photonView = PhotonView.Find(_photonViewID);
		if ((bool)photonView)
		{
			PlayerTumble component = photonView.GetComponent<PlayerTumble>();
			if ((bool)component)
			{
				playerTumbleTarget = component;
			}
		}
	}
}
