using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class ItemUpgrade : MonoBehaviour
{
	public UnityEvent upgradeEvent;

	public bool isPlayerUpgrade;

	public ColorPresets colorPreset;

	internal Color beamColor;

	private float checkTimer;

	private Transform magnetTarget;

	internal PhysGrabObject magnetTargetPhysGrabObject;

	internal Rigidbody magnetTargetRigidbody;

	internal bool magnetActive;

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

	private PhysGrabObjectImpactDetector impactDetector;

	private ItemAttributes itemAttributes;

	private Transform particleEffects;

	private Transform onSwitchTransform;

	internal Vector3 connectionPoint;

	internal Transform lastPlayerToTouch;

	private PhysGrabObject physGrabObject;

	private PhysicMaterial physicMaterialOriginal;

	private ItemToggle itemToggle;

	internal PlayerAvatar playerAvatarTarget;

	private bool targetIsPlayer;

	internal bool targetIsLocalPlayer;

	private Camera cameraMain;

	private bool upgradeDone;

	private bool pushedOrPulled;

	private ITargetingCondition customTargetingCondition;

	private void Start()
	{
		itemAttributes = GetComponent<ItemAttributes>();
		impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		customTargetingCondition = GetComponent<ITargetingCondition>();
		particleEffects = base.transform.Find("Particle Effects");
		cameraMain = Camera.main;
		itemToggle = GetComponent<ItemToggle>();
		rb = GetComponent<Rigidbody>();
		photonView = GetComponent<PhotonView>();
		lineBetweenTwoPoints = GetComponent<LineBetweenTwoPoints>();
		physGrabObject = GetComponent<PhysGrabObject>();
		beamColor = colorPreset.GetColorDark();
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		int num = 0;
		if (num < componentsInChildren.Length)
		{
			Collider collider = componentsInChildren[num];
			physicMaterialOriginal = collider.material;
		}
		if (SemiFunc.RunIsShop())
		{
			itemToggle.enabled = false;
		}
		physGrabObject.clientNonKinematic = true;
	}

	private void Update()
	{
		if (physGrabObject.playerGrabbing.Count > 0)
		{
			bool flag = false;
			foreach (PhysGrabber item in physGrabObject.playerGrabbing)
			{
				if (item.isRotating)
				{
					flag = true;
				}
			}
			float dist = 0.5f;
			if (physGrabObject.grabbed)
			{
				if (physGrabObject.grabbedLocal && !pushedOrPulled)
				{
					PhysGrabber.instance.OverrideGrabDistance(dist);
				}
				if (PhysGrabber.instance.isPulling || PhysGrabber.instance.isPushing)
				{
					pushedOrPulled = true;
				}
			}
			else
			{
				pushedOrPulled = false;
			}
			if (!flag && !pushedOrPulled)
			{
				Quaternion turnX = Quaternion.Euler(45f, 0f, 0f);
				Quaternion turnY = Quaternion.Euler(45f, 180f, 0f);
				Quaternion identity = Quaternion.identity;
				physGrabObject.TurnXYZ(turnX, turnY, identity);
			}
		}
		else
		{
			pushedOrPulled = false;
		}
		TargetingLogic();
	}

	public void PlayerUpgrade()
	{
		if (!upgradeDone && isPlayerUpgrade && itemToggle.toggleState)
		{
			Transform transform = base.transform.Find("Mesh");
			if ((bool)transform)
			{
				transform.GetComponent<MeshRenderer>().enabled = false;
			}
			upgradeEvent.Invoke();
			particleEffects.parent = null;
			particleEffects.gameObject.SetActive(value: true);
			PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID);
			if (playerAvatar.isLocal)
			{
				StatsUI.instance.Fetch();
				StatsUI.instance.ShowStats();
				CameraGlitch.Instance.PlayUpgrade();
			}
			else
			{
				GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, base.transform.position, 0.2f);
			}
			if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
			{
				playerAvatar.playerHealth.MaterialEffectOverride(PlayerHealth.Effect.Upgrade);
			}
			StatsManager.instance.itemsPurchased[itemAttributes.item.name] = Mathf.Max(StatsManager.instance.itemsPurchased[itemAttributes.item.name] - 1, 0);
			impactDetector.DestroyObject(effects: false);
			upgradeDone = true;
		}
	}

	private void TargetingLogic()
	{
		if (isPlayerUpgrade)
		{
			return;
		}
		if (magnetActive && !physGrabObject.grabbed)
		{
			DeactivateMagnet();
		}
		if (itemActivated && magnetActive)
		{
			if ((bool)magnetTarget && !magnetTarget.gameObject.activeSelf)
			{
				magnetActive = false;
				magnetTarget = null;
			}
			if ((bool)magnetTarget)
			{
				if (rayHitPosition != Vector3.zero && !targetIsPlayer)
				{
					animatedRayHitPosition = Vector3.Lerp(animatedRayHitPosition, rayHitPosition, Time.deltaTime * 10f);
					lineBetweenTwoPoints.DrawLine(lineStartPoint.position, magnetTarget.TransformPoint(animatedRayHitPosition));
					connectionPoint = magnetTarget.TransformPoint(animatedRayHitPosition);
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
						Vector3 vector = new Vector3(0f, -0.5f, 0f);
						if (!targetIsLocalPlayer)
						{
							lineBetweenTwoPoints.DrawLine(lineStartPoint.position, magnetTarget.position + vector);
							connectionPoint = magnetTarget.position + vector;
						}
						else
						{
							Vector3 point = cameraMain.transform.position + vector;
							lineBetweenTwoPoints.DrawLine(lineStartPoint.position, point);
							connectionPoint = point;
						}
					}
				}
			}
		}
		if (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
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
		if (physGrabObject.grabbed)
		{
			checkTimer += Time.deltaTime;
			if (checkTimer > 0.5f)
			{
				if (SphereCheck())
				{
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
	}

	private void MagnetLogic()
	{
		if (isPlayerUpgrade || (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient) || !itemActivated || !magnetActive)
		{
			return;
		}
		if (!magnetTarget)
		{
			DeactivateMagnet();
			return;
		}
		if (attachPointFound)
		{
			Vector3 vector = magnetTarget.TransformPoint(attachPoint) - base.transform.position;
			Vector3 vector2 = springConstant * vector;
			Vector3 velocity = rb.velocity;
			Vector3 vector3 = (0f - dampingCoefficient) * velocity;
			Vector3.ClampMagnitude(vector2 + vector3, 20f);
		}
		else
		{
			_ = (magnetTarget.position - base.transform.position).magnitude;
		}
		if (targetIsPlayer)
		{
			if (Vector3.Distance(base.transform.position, magnetTarget.position) > 1.8f)
			{
				DeactivateMagnet();
			}
		}
		else if (Vector3.Distance(base.transform.position, magnetTarget.position) > 1f)
		{
			DeactivateMagnet();
		}
		if ((bool)magnetTargetPhysGrabObject)
		{
			magnetTargetPhysGrabObject.OverrideZeroGravity();
			magnetTargetPhysGrabObject.OverrideMass(0.1f);
			magnetTargetPhysGrabObject.OverrideMaterial(SemiFunc.PhysicMaterialSticky());
			magnetTargetRigidbody.AddForce((base.transform.position - magnetTarget.position).normalized * 1f, ForceMode.Force);
		}
	}

	private void FixedUpdate()
	{
		MagnetLogic();
	}

	private void DeactivateMagnet()
	{
		attachPointFound = false;
		MagnetActiveToggle(toggleBool: false);
	}

	private void ActivateMagnet()
	{
		MagnetActiveToggle(toggleBool: true);
	}

	private void ButtonToggleLogic(bool activated)
	{
		if (!activated && magnetActive)
		{
			DeactivateMagnet();
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
		targetIsPlayer = false;
		targetIsLocalPlayer = false;
		if ((bool)newMagnetTarget)
		{
			magnetTargetPhysGrabObject = newMagnetTarget.GetComponent<PhysGrabObject>();
			if (colliderID != -1)
			{
				magnetTarget = newMagnetTarget.GetComponent<PhysGrabObject>().FindColliderFromID(colliderID);
			}
			else
			{
				magnetTarget = newMagnetTarget;
			}
			animatedRayHitPosition = rayHitPosition;
			rayHitPosition = magnetTarget.InverseTransformPoint(newRayHitPosition);
			magnetTargetRigidbody = GetHighestParentWithRigidbody(magnetTarget).GetComponent<Rigidbody>();
			return;
		}
		magnetTargetPhysGrabObject = PhotonView.Find(photonViewId).gameObject.GetComponent<PhysGrabObject>();
		if (colliderID != -1)
		{
			magnetTarget = PhotonView.Find(photonViewId).gameObject.GetComponent<PhysGrabObject>().FindColliderFromID(colliderID);
		}
		else
		{
			targetIsPlayer = true;
			magnetTarget = PhotonView.Find(photonViewId).GetComponent<PlayerAvatar>().PlayerVisionTarget.VisionTransform;
			if (PhotonView.Find(photonViewId).GetComponent<PlayerAvatar>().isLocal)
			{
				targetIsLocalPlayer = true;
			}
			playerAvatarTarget = PhotonView.Find(photonViewId).GetComponent<PlayerAvatar>();
		}
		animatedRayHitPosition = rayHitPosition;
		rayHitPosition = magnetTarget.InverseTransformPoint(newRayHitPosition);
		magnetTargetRigidbody = GetHighestParentWithRigidbody(magnetTarget).GetComponent<Rigidbody>();
	}

	private void NewRayHitPoint(Vector3 newAttachPoint, int photonViewId, int colliderID, Transform newMagnetTarget)
	{
		if (GameManager.instance.gameMode == 0)
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

	private bool SphereCheck()
	{
		bool result = false;
		Collider[] array = Physics.OverlapSphere(base.transform.position, 0.75f);
		float num = 10000f;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			Transform highestParentWithRigidbody = GetHighestParentWithRigidbody(collider.transform);
			PhysGrabObjectCollider component = collider.GetComponent<PhysGrabObjectCollider>();
			bool flag = false;
			if (highestParentWithRigidbody != null)
			{
				PhysGrabObjectImpactDetector component2 = highestParentWithRigidbody.GetComponent<PhysGrabObjectImpactDetector>();
				if (component2 != null && component2.isValuable)
				{
					flag = true;
				}
			}
			bool flag2 = true;
			if (customTargetingCondition != null && highestParentWithRigidbody != null)
			{
				flag2 = customTargetingCondition.CustomTargetingCondition(highestParentWithRigidbody.gameObject);
			}
			bool flag3 = false;
			if (!flag3)
			{
				flag3 = !flag;
			}
			if (!(component != null && highestParentWithRigidbody != base.transform && highestParentWithRigidbody != null && flag3 && flag2))
			{
				continue;
			}
			float num2 = Vector3.Distance(base.transform.position, collider.transform.position);
			if (num2 < num)
			{
				bool flag4 = false;
				if (Physics.Raycast(base.transform.position, collider.transform.position - base.transform.position, out var hitInfo, 1f, SemiFunc.LayerMaskGetVisionObstruct()) && hitInfo.collider.transform != collider.transform && hitInfo.collider.transform != base.transform)
				{
					flag4 = true;
				}
				if (!flag4)
				{
					num = num2;
					magnetTarget = collider.transform;
					magnetTargetPhysGrabObject = highestParentWithRigidbody.GetComponent<PhysGrabObject>();
					magnetTargetRigidbody = highestParentWithRigidbody.GetComponent<Rigidbody>();
					Vector3 position = collider.transform.position;
					NewRayHitPoint(position, highestParentWithRigidbody.GetComponent<PhotonView>().ViewID, component.colliderID, highestParentWithRigidbody);
					attachPoint = rayHitPosition;
					result = true;
				}
			}
		}
		return result;
	}

	private void FindBeamAttachPosition()
	{
		for (int i = 0; i < 6; i++)
		{
			float num = 0.5f;
			Vector3 vector = new Vector3(Random.Range(0f - num, num), Random.Range(0f - num, num), Random.Range(0f - num, num));
			if (Physics.Raycast(base.transform.position, magnetTarget.position - base.transform.position + vector, out var hitInfo, 1f, SemiFunc.LayerMaskGetPhysGrabObject()))
			{
				Transform highestParentWithRigidbody = GetHighestParentWithRigidbody(hitInfo.collider.transform);
				PhysGrabObjectCollider component = hitInfo.collider.transform.GetComponent<PhysGrabObjectCollider>();
				if (component != null && highestParentWithRigidbody == magnetTargetPhysGrabObject.transform)
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
}
