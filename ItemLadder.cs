using System.Linq;
using Photon.Pun;
using UnityEngine;

public class ItemLadder : MonoBehaviour
{
	public enum States
	{
		Denied,
		Neutral,
		Grabbed,
		OutOfBattery,
		Flickering
	}

	[Header("Assignables")]
	public Transform topTransformPivot;

	public Transform bottomTransform;

	public Transform plasmaBridgePivot;

	[Space]
	public MeshRenderer grabIcon;

	public MeshRenderer bridge;

	[Space]
	public Transform collisionCheckPivot;

	public Transform collisionCheck;

	public Transform groundCollisionCheck;

	[Space]
	public Transform colliderPivot;

	public Transform endCollider;

	[Space]
	public AnimationCurve animationCurve;

	[Header("Extension variables")]
	public float extendingSpeed = 1f;

	public float extensionBatteryDrain = 5f;

	public int extensionAmount = 2;

	[Header("Physic material")]
	public PhysicMaterial defaultPhysicMaterial;

	public PhysicMaterial littleStickyMaterial;

	[Header("Phys sounds")]
	public PhysAudio OffPhysAudio;

	public PhysAudio OnPhysAudio;

	[Header("Sounds")]
	public Sound grabSound;

	public Sound releaseSound;

	public Sound extendSound;

	public Sound retractSound;

	public Sound bridgeLoop;

	public Sound deniedSound;

	public Sound flickeringSound;

	[HideInInspector]
	public bool denied;

	private States currentState = States.Neutral;

	private States previousState = States.Neutral;

	private States previousPreviousState = States.Neutral;

	private Vector3 colliderPivotOriginalScale;

	private Vector3 collisionCheckPivotOriginalPosition;

	private Vector3 topTransformPivotOriginalPosition;

	private Vector3 plasmaBridgePivotOriginalScale;

	private Vector3 endColliderOriginalPosition;

	private PhotonView photonView;

	private ItemToggle itemToggle;

	private ItemEquippable itemEquippable;

	private ItemBattery itemBattery;

	private PhysGrabObject physGrabObject;

	private Rigidbody rb;

	private PhysGrabObjectImpactDetector impactDetector;

	private RoomVolumeCheck roomVolumeCheck;

	private Vector3 roomVolumeCheckOriginalPosition;

	private Vector3 roomVolumeCheckOriginalScale;

	private bool animate;

	private bool previousToggleState;

	private bool previousEquippedState;

	private int previousExtensionIndex;

	private int extensionIndex;

	private float deniedTime;

	private float animationCurveEval;

	private Material grabMaterial;

	private Color bridgeBaseColor;

	private Vector3 startTopPosition;

	private Vector3 startScale;

	private bool flickering;

	private float shopTimer = 10f;

	private bool shopTimerOn;

	private bool justGrabbed;

	private float flickeringTime = 3f;

	private float flickeringTimer;

	private float staticResetTimer;

	private Vector3 staticPosition;

	private Quaternion staticRotation;

	private float staticLerp;

	private void Start()
	{
		itemToggle = base.gameObject.GetComponent<ItemToggle>();
		itemEquippable = base.gameObject.GetComponent<ItemEquippable>();
		photonView = base.gameObject.GetComponent<PhotonView>();
		itemBattery = base.gameObject.GetComponent<ItemBattery>();
		physGrabObject = base.gameObject.GetComponent<PhysGrabObject>();
		rb = base.gameObject.GetComponent<Rigidbody>();
		impactDetector = base.gameObject.GetComponent<PhysGrabObjectImpactDetector>();
		roomVolumeCheck = base.gameObject.GetComponent<RoomVolumeCheck>();
		roomVolumeCheckOriginalPosition = roomVolumeCheck.CheckPosition;
		roomVolumeCheckOriginalScale = roomVolumeCheck.currentSize;
		previousToggleState = itemToggle.toggleState;
		previousEquippedState = itemEquippable.isEquipped;
		colliderPivotOriginalScale = colliderPivot.localScale;
		colliderPivot.GetComponent<Collider>().material = defaultPhysicMaterial;
		collisionCheckPivotOriginalPosition = collisionCheckPivot.localPosition;
		topTransformPivotOriginalPosition = topTransformPivot.localPosition;
		plasmaBridgePivotOriginalScale = plasmaBridgePivot.localScale;
		bridgeBaseColor = bridge.material.GetColor("_Color");
		endColliderOriginalPosition = endCollider.localPosition;
		collisionCheckPivot.localScale = new Vector3(collisionCheckPivot.localScale.x, collisionCheckPivot.localScale.y, extensionAmount);
	}

	private void Update()
	{
		if (physGrabObject.grabbed)
		{
			colliderPivot.GetComponent<Collider>().material = defaultPhysicMaterial;
		}
		else if (physGrabObject.playerGrabbing.Count > 0)
		{
			colliderPivot.GetComponent<Collider>().material = littleStickyMaterial;
		}
		bridgeLoop.PlayLoop(extensionIndex != 0, 1f, 1f);
		if (animate && !itemEquippable.isEquipped && !IsMinSize())
		{
			AnimateExtension();
		}
		else if (animate)
		{
			AnimateRetraction();
		}
		if (flickering)
		{
			FlickerBridge();
		}
		if (IndexChanged())
		{
			ScaleCollider();
			OffsetEndCollider();
			IncrementCollisionCheck();
			ChangePhysSound();
			UpdateRoomVolumeCheck();
			if (extensionIndex == 0)
			{
				retractSound.Play(physGrabObject.centerPoint);
				Retract();
				if (!itemEquippable.isEquipped)
				{
					StartAnimation();
				}
				flickering = false;
			}
			else
			{
				StartAnimation();
				extendSound.Play(physGrabObject.centerPoint);
				animationCurveEval = 0f;
				itemBattery.batteryLife -= extensionBatteryDrain;
			}
		}
		if (StateChanged())
		{
			if (currentState == States.Denied)
			{
				bridge.material.SetColor("_Color", new Color(1f, 0.8f, 0.2f) * 3f);
				grabIcon.material.SetColor("_EmissionColor", Color.green);
				grabIcon.material.mainTextureOffset = new Vector2(0.5f, 0f);
				deniedSound.Play(physGrabObject.centerPoint);
			}
			else if (currentState == States.Neutral)
			{
				if (previousPreviousState != States.Denied)
				{
					grabIcon.material.SetColor("_EmissionColor", Color.red);
					grabIcon.material.mainTextureOffset = new Vector2(0f, 0f);
				}
				bridge.material.SetColor("_Color", bridgeBaseColor);
			}
			else if (currentState == States.Grabbed)
			{
				grabIcon.material.SetColor("_EmissionColor", Color.green);
				grabIcon.material.mainTextureOffset = new Vector2(0.5f, 0f);
				bridge.material.SetColor("_Color", bridgeBaseColor);
				justGrabbed = true;
			}
			else if (currentState == States.OutOfBattery)
			{
				flickering = false;
				bridge.material.SetColor("_Color", Color.red * 0f);
				grabIcon.material.SetColor("_EmissionColor", Color.red);
				grabIcon.material.mainTextureOffset = new Vector2(0f, 0f);
				itemBattery.batteryLife = 0f;
			}
			else if (currentState == States.Flickering)
			{
				flickering = true;
				flickeringSound.Play(physGrabObject.centerPoint);
			}
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (EPressed())
		{
			if (!IsColliding() && !animate && !itemEquippable.isEquipped)
			{
				Extend();
			}
			else if (extensionIndex != 0 && !animate)
			{
				deniedTime = Time.time;
				if (!flickering)
				{
					StateChange(States.Denied);
				}
			}
		}
		if (currentState == States.Denied && Time.time - deniedTime >= 0.25f)
		{
			if (physGrabObject.grabbed)
			{
				StateChange(States.Grabbed);
			}
			else
			{
				StateChange(States.Neutral);
			}
		}
		if (EquipPressed() && extensionIndex != 0)
		{
			SetExtensionIndex(0);
		}
		if (itemBattery.batteryLife <= 0f && extensionIndex > 0 && !flickering)
		{
			StateChange(States.Flickering);
			flickering = true;
		}
		if (flickering)
		{
			flickeringTimer += Time.deltaTime;
			if (flickeringTimer >= flickeringTime)
			{
				flickeringTimer = 0f;
				GrabRelease();
				StateChange(States.OutOfBattery);
				SetExtensionIndex(0);
				flickering = false;
			}
		}
		if (!SemiFunc.RunIsShop())
		{
			return;
		}
		if (extensionIndex > 0)
		{
			bool flag = false;
			foreach (RoomVolume currentRoom in roomVolumeCheck.CurrentRooms)
			{
				if (currentRoom.Extraction)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				SetExtensionIndex(0);
				GrabRelease();
				shopTimerOn = false;
				shopTimer = 10f;
			}
		}
		if (shopTimerOn)
		{
			shopTimer -= Time.deltaTime;
			if (shopTimer <= 0f)
			{
				SetExtensionIndex(0);
				GrabRelease();
				shopTimerOn = false;
				shopTimer = 10f;
			}
		}
		void ChangePhysSound()
		{
			if (extensionIndex == 0)
			{
				impactDetector.ChangePhysAudio(OffPhysAudio);
			}
			else
			{
				impactDetector.ChangePhysAudio(OnPhysAudio);
			}
		}
		void OffsetEndCollider()
		{
			Vector3 localPosition = endCollider.localPosition;
			localPosition.z += extensionAmount;
			endCollider.localPosition = localPosition;
		}
		void UpdateRoomVolumeCheck()
		{
			int num = extensionAmount * extensionIndex;
			roomVolumeCheck.currentSize = new Vector3(roomVolumeCheckOriginalScale.x, roomVolumeCheckOriginalScale.y, roomVolumeCheckOriginalScale.z + (float)num * 0.165f);
			roomVolumeCheck.CheckPosition = new Vector3(roomVolumeCheckOriginalPosition.x, roomVolumeCheckOriginalPosition.y, roomVolumeCheckOriginalPosition.z + (float)num * 0.085f);
		}
	}

	private void FixedUpdate()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() && extensionIndex > 0 && physGrabObject.playerGrabbing.Count == 0)
		{
			physGrabObject.OverrideKinematic();
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		float value = 2f + 0.5f * (float)extensionIndex;
		physGrabObject.OverrideMass(value);
		if (extensionIndex > 0 && physGrabObject.playerGrabbing.Count == 0)
		{
			physGrabObject.OverrideZeroGravity();
			Vector3 vector = SemiFunc.PhysFollowPosition(base.transform.position, staticPosition, physGrabObject.rb.velocity, 5f);
			physGrabObject.rb.AddForce(Vector3.Lerp(Vector3.zero, vector, staticLerp), ForceMode.Impulse);
			Vector3 vector2 = SemiFunc.PhysFollowRotation(base.transform, staticRotation, physGrabObject.rb, 5f);
			physGrabObject.rb.AddTorque(Vector3.Lerp(Vector3.zero, vector2, staticLerp), ForceMode.Impulse);
			staticLerp += Time.fixedDeltaTime;
			if (staticLerp >= 1f && (Vector3.Distance(base.transform.position, staticPosition) > 0.1f || Quaternion.Angle(base.transform.rotation, staticRotation) > 1f))
			{
				staticResetTimer += Time.fixedDeltaTime;
				if (staticResetTimer > 3f)
				{
					staticResetTimer = 0f;
					staticPosition = base.transform.position;
					staticRotation = Quaternion.Euler(base.transform.eulerAngles.x, base.transform.eulerAngles.y, 0f);
				}
			}
			else
			{
				staticResetTimer = 0f;
			}
		}
		else
		{
			staticPosition = base.transform.position;
			staticRotation = Quaternion.Euler(base.transform.eulerAngles.x, base.transform.eulerAngles.y, 0f);
			staticLerp = 0f;
		}
		if (!physGrabObject.grabbed)
		{
			return;
		}
		physGrabObject.OverrideExtraTorqueStrengthDisable();
		physGrabObject.OverrideTorqueStrength(Mathf.Min(4f + (float)extensionIndex * 0.15f, 2f));
		physGrabObject.OverrideGrabStrength(Mathf.Min(1f + 0.5f * (float)extensionIndex, 3f));
		if (justGrabbed && !IsCollidingWithGround())
		{
			justGrabbed = false;
			if (physGrabObject.playerGrabbing.Count == 1)
			{
				Quaternion turnX = Quaternion.Euler(0f, 0f, 0f);
				Quaternion turnY = Quaternion.Euler(0f, 0f, 0f);
				Quaternion identity = Quaternion.identity;
				physGrabObject.TurnXYZ(turnX, turnY, identity);
			}
		}
	}

	public void GrabRelease()
	{
		bool flag = false;
		foreach (PhysGrabber item in physGrabObject.playerGrabbing.ToList())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				item.ReleaseObject(photonView.ViewID);
			}
			else
			{
				item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, 0.1f, photonView.ViewID);
			}
			flag = true;
		}
		if (flag)
		{
			if (GameManager.instance.gameMode == 0)
			{
				GrabReleaseRPC();
			}
			else
			{
				photonView.RPC("GrabReleaseRPC", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	private void GrabReleaseRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
			physGrabObject.grabDisableTimer = 1f;
		}
	}

	private void StartAnimation()
	{
		startTopPosition = topTransformPivot.localPosition;
		startScale = plasmaBridgePivot.localScale;
		if (extensionIndex > 0)
		{
			animationCurveEval = 0f;
		}
		else
		{
			animationCurveEval = 1f;
		}
		animate = true;
	}

	private void AnimateExtension()
	{
		animationCurveEval += Time.deltaTime * extendingSpeed;
		float t = animationCurve.Evaluate(animationCurveEval);
		Vector3 vector = startTopPosition;
		vector.z = (float)(extensionIndex * extensionAmount) + 1f;
		topTransformPivot.localPosition = Vector3.Lerp(startTopPosition, vector, t);
		Vector3 vector2 = startScale;
		vector2.z = extensionIndex * extensionAmount;
		plasmaBridgePivot.localScale = Vector3.Lerp(startScale, vector2, t);
		if (animationCurveEval >= 1f)
		{
			animate = false;
		}
	}

	private void AnimateRetraction()
	{
		animationCurveEval -= Time.deltaTime * 3f;
		float t = animationCurve.Evaluate(animationCurveEval);
		Vector3 vector = topTransformPivotOriginalPosition;
		topTransformPivot.localPosition = Vector3.Lerp(vector, startTopPosition, t);
		Vector3 vector2 = plasmaBridgePivotOriginalScale;
		plasmaBridgePivot.localScale = Vector3.Lerp(vector2, startScale, t);
		if (animationCurveEval <= 0f)
		{
			animate = false;
			plasmaBridgePivot.localScale = plasmaBridgePivotOriginalScale;
		}
	}

	[PunRPC]
	public void SetExtensionIndexRPC(int _index, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			extensionIndex = _index;
		}
	}

	private void SetExtensionIndex(int _index)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && extensionIndex != _index)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				SetExtensionIndexRPC(_index);
				return;
			}
			photonView.RPC("SetExtensionIndexRPC", RpcTarget.All, _index);
		}
	}

	public void StateChange(States _state)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState != _state)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("StateChangeRPC", RpcTarget.All, _state);
			}
			else
			{
				StateChangeRPC(_state);
			}
		}
	}

	[PunRPC]
	public void StateChangeRPC(States _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
		}
	}

	private void Retract()
	{
		colliderPivot.localScale = colliderPivotOriginalScale;
		endCollider.localPosition = endColliderOriginalPosition;
		ResetCollisionCheckPosition();
		if (!itemEquippable.isEquipped)
		{
			StartAnimation();
			return;
		}
		topTransformPivot.localPosition = topTransformPivotOriginalPosition;
		plasmaBridgePivot.localScale = plasmaBridgePivotOriginalScale;
	}

	private void Extend()
	{
		if (flickering || itemBattery.batteryLife >= extensionBatteryDrain)
		{
			SetExtensionIndex(extensionIndex + 1);
		}
	}

	private bool IsMinSize()
	{
		return extensionIndex == 0;
	}

	private Collider[] GetCollidingColliders()
	{
		return Physics.OverlapBox(collisionCheck.position, collisionCheck.lossyScale / 2f, collisionCheck.rotation);
	}

	private Collider[] GetCollidingCollidersGround()
	{
		return Physics.OverlapBox(groundCollisionCheck.position, groundCollisionCheck.lossyScale / 2f, groundCollisionCheck.rotation);
	}

	private bool IsColliding()
	{
		Collider[] collidingColliders = GetCollidingColliders();
		foreach (Collider collider in collidingColliders)
		{
			if (collider.gameObject.layer != LayerMask.NameToLayer("RoomVolume") && !collider.transform.IsChildOf(base.transform))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsCollidingWithGround()
	{
		Collider[] collidingCollidersGround = GetCollidingCollidersGround();
		foreach (Collider collider in collidingCollidersGround)
		{
			if (collider.gameObject.layer != LayerMask.NameToLayer("RoomVolume") && !collider.transform.IsChildOf(base.transform))
			{
				return true;
			}
		}
		return false;
	}

	private bool EPressed()
	{
		if (itemToggle.toggleState != previousToggleState)
		{
			previousToggleState = itemToggle.toggleState;
			return true;
		}
		return false;
	}

	private bool EquipPressed()
	{
		if (itemEquippable.isEquipped != previousEquippedState)
		{
			previousEquippedState = itemEquippable.isEquipped;
			return true;
		}
		return false;
	}

	private bool StateChanged()
	{
		if (previousState != currentState)
		{
			previousPreviousState = previousState;
			previousState = currentState;
			return true;
		}
		return false;
	}

	private void ResetCollisionCheckPosition()
	{
		collisionCheckPivot.localPosition = collisionCheckPivotOriginalPosition;
	}

	private void ScaleCollider()
	{
		Vector3 localScale = colliderPivot.localScale;
		localScale.z += extensionAmount;
		colliderPivot.localScale = localScale;
	}

	private void IncrementCollisionCheck()
	{
		Vector3 localPosition = collisionCheckPivot.localPosition;
		localPosition.z += extensionAmount;
		collisionCheckPivot.localPosition = localPosition;
	}

	private bool IndexChanged()
	{
		if (previousExtensionIndex != extensionIndex)
		{
			previousExtensionIndex = extensionIndex;
			return true;
		}
		return false;
	}

	public void OnGrab()
	{
		if (currentState != States.Grabbed && !flickering)
		{
			StateChange(States.Grabbed);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.RunIsShop())
		{
			shopTimerOn = false;
			shopTimer = 10f;
		}
	}

	public void OnRelease()
	{
		if (physGrabObject.playerGrabbing.Count == 0 && !flickering && currentState != States.OutOfBattery)
		{
			StateChange(States.Neutral);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.RunIsShop() && extensionIndex > 0)
		{
			shopTimerOn = true;
			shopTimer = 10f;
		}
		justGrabbed = false;
	}

	public void FlickerBridge()
	{
		float num = 0.7f + Mathf.PingPong(Time.time, 0.1f);
		bridge.material.SetColor("_Color", Color.red * num * 3f);
	}
}
