using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemEquippable : MonoBehaviourPunCallbacks
{
	public enum ItemState
	{
		Idle,
		Equipping,
		Equipped,
		Unequipping
	}

	[FormerlySerializedAs("_currentState")]
	[SerializeField]
	private ItemState currentState;

	public Sprite ItemIcon;

	private InventorySpot equippedSpot;

	private int ownerPlayerId = -1;

	internal bool isEquipped;

	internal bool isEquippedPrev;

	internal float wasEquippedTimer;

	internal bool isUnequipping;

	internal bool isEquipping;

	private float isUnequippingTimer;

	private float isEquippingTimer;

	public LayerMask ObstructionLayers;

	public SemiFunc.emojiIcon itemEmojiIcon;

	internal string itemEmoji;

	internal int inventorySpotIndex;

	internal float unequipTimer;

	internal float equipTimer;

	private const float animationDuration = 0.4f;

	private PhysGrabObject physGrabObject;

	private bool stateStart = true;

	private float itemEquipCubeShowTimer;

	private Vector3 teleportPosition;

	private float forceGrabTimer;

	private bool wasForceGrabbed;

	internal PhysGrabber latestOwner;

	private Rigidbody rb => GetComponent<Rigidbody>();

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
	}

	public bool IsEquipped()
	{
		return currentState == ItemState.Equipped;
	}

	private bool CollisionCheck()
	{
		return false;
	}

	public void RequestEquip(int spot, int requestingPlayerId = -1)
	{
		if (!IsEquipped() && currentState != ItemState.Unequipping)
		{
			if (SemiFunc.IsMultiplayer())
			{
				base.photonView.RPC("RPC_RequestEquip", RpcTarget.MasterClient, spot, requestingPlayerId);
			}
			else
			{
				RPC_RequestEquip(spot, -1);
			}
		}
	}

	[PunRPC]
	private void RPC_RequestEquip(int spotIndex, int physGrabberPhotonViewID)
	{
		bool flag = SemiFunc.IsMultiplayer();
		if (currentState == ItemState.Idle)
		{
			if (flag)
			{
				base.photonView.RPC("RPC_UpdateItemState", RpcTarget.All, 2, spotIndex, physGrabberPhotonViewID);
			}
			else
			{
				RPC_UpdateItemState(2, spotIndex, physGrabberPhotonViewID);
			}
		}
	}

	[PunRPC]
	private void RPC_UpdateItemState(int state, int spotIndex, int ownerId)
	{
		bool num = SemiFunc.IsMultiplayer();
		PlayerAvatar playerAvatar = PlayerAvatar.instance;
		if (SemiFunc.IsMultiplayer())
		{
			playerAvatar = PhotonView.Find(ownerId)?.GetComponent<PlayerAvatar>();
		}
		InventorySpot inventorySpot = null;
		if (num)
		{
			if (PhysGrabber.instance.photonView.ViewID == ownerId && spotIndex != -1)
			{
				inventorySpot = Inventory.instance.GetSpotByIndex(spotIndex);
			}
		}
		else if (spotIndex != -1)
		{
			inventorySpot = Inventory.instance.GetSpotByIndex(spotIndex);
		}
		bool flag = false;
		if (inventorySpot == null)
		{
			flag = true;
		}
		if (inventorySpot != null && inventorySpot.IsOccupied())
		{
			flag = true;
		}
		if (Inventory.instance.IsItemEquipped(this))
		{
			flag = true;
		}
		if (state == 2)
		{
			string instanceName = GetComponent<ItemAttributes>().instanceName;
			StatsManager.instance.PlayerInventoryUpdate(playerAvatar.steamID, instanceName, spotIndex);
			currentState = ItemState.Equipped;
			if (!flag)
			{
				equippedSpot = inventorySpot;
				equippedSpot?.EquipItem(this);
			}
			else
			{
				equippedSpot = null;
			}
		}
		else
		{
			equippedSpot?.UnequipItem();
			equippedSpot = null;
		}
		inventorySpotIndex = spotIndex;
		currentState = (ItemState)state;
		ownerPlayerId = ownerId;
		stateStart = true;
		UpdateVisuals();
	}

	private void IsEquippingAndUnequippingTimer()
	{
		if (isEquippingTimer > 0f)
		{
			if (isEquippingTimer <= 0f)
			{
				isEquipping = false;
			}
			isEquippingTimer -= Time.deltaTime;
		}
		if (isUnequippingTimer > 0f)
		{
			if (isUnequippingTimer <= 0f)
			{
				isUnequipping = false;
			}
			isUnequippingTimer -= Time.deltaTime;
		}
	}

	public void RequestUnequip()
	{
		if (IsEquipped())
		{
			currentState = ItemState.Unequipping;
			if (SemiFunc.IsMultiplayer())
			{
				base.photonView.RPC("RPC_StartUnequip", RpcTarget.All, ownerPlayerId);
			}
			else
			{
				RPC_StartUnequip(ownerPlayerId);
			}
		}
	}

	[PunRPC]
	private void RPC_StartUnequip(int requestingPlayerId)
	{
		if (ownerPlayerId == requestingPlayerId && (!SemiFunc.IsMultiplayer() || PhysGrabber.instance.photonView.ViewID == ownerPlayerId))
		{
			PerformUnequip(requestingPlayerId);
		}
	}

	private void PerformUnequip(int requestingPlayerId)
	{
		unequipTimer = 0.4f;
		SetRotation();
		currentState = ItemState.Unequipping;
		physGrabObject.OverrideDeactivateReset();
		if (SemiFunc.IsMultiplayer())
		{
			RayHitTestNew(1f);
			base.photonView.RPC("RPC_CompleteUnequip", RpcTarget.MasterClient, requestingPlayerId, teleportPosition);
		}
		else
		{
			RayHitTestNew(1f);
			RPC_CompleteUnequip(requestingPlayerId, teleportPosition);
		}
	}

	private bool RayHitTestNew(float distance)
	{
		int layerMask = (int)SemiFunc.LayerMaskGetVisionObstruct() & ~LayerMask.GetMask("Ignore Raycast", "CollisionCheck");
		if ((bool)Camera.main)
		{
			if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var hitInfo, distance, layerMask))
			{
				teleportPosition = hitInfo.point;
			}
			else
			{
				teleportPosition = Camera.main.transform.position + Camera.main.transform.forward * distance;
			}
		}
		return CollisionCheck();
	}

	private bool RayHitTest(float distance)
	{
		int layerMask = (int)SemiFunc.LayerMaskGetVisionObstruct() & ~LayerMask.GetMask("Ignore Raycast", "CollisionCheck");
		if ((bool)Camera.main)
		{
			Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var _, distance, layerMask);
		}
		return CollisionCheck();
	}

	private Vector3 GetUnequipPosition()
	{
		return base.transform.position;
	}

	[PunRPC]
	private void RPC_CompleteUnequip(int physGrabberPhotonViewID, Vector3 teleportPos)
	{
		PhysGrabber physGrabber = ((!SemiFunc.IsMultiplayer()) ? PhysGrabber.instance : PhotonView.Find(physGrabberPhotonViewID).GetComponent<PhysGrabber>());
		StatsManager.instance.PlayerInventoryUpdate(physGrabber.playerAvatar.steamID, "", inventorySpotIndex);
		Transform visionTransform = physGrabber.playerAvatar.PlayerVisionTarget.VisionTransform;
		physGrabObject.Teleport(teleportPos, Quaternion.LookRotation(visionTransform.transform.forward, Vector3.up));
		rb.isKinematic = false;
		int num = ((equippedSpot != null) ? equippedSpot.inventorySpotIndex : (-1));
		equippedSpot?.UnequipItem();
		equippedSpot = null;
		ownerPlayerId = -1;
		if (SemiFunc.IsMultiplayer())
		{
			base.photonView.RPC("RPC_UpdateItemState", RpcTarget.All, 3, num, physGrabberPhotonViewID);
		}
		else
		{
			RPC_UpdateItemState(3, num, -1);
		}
	}

	private void UpdateVisuals()
	{
		if (currentState == ItemState.Equipped)
		{
			SetItemActive(isActive: false);
		}
		else if (currentState == ItemState.Idle)
		{
			SetItemActive(isActive: true);
		}
		else if (currentState == ItemState.Unequipping)
		{
			StartCoroutine(AnimateUnequip());
		}
	}

	private void SetItemActive(bool isActive)
	{
	}

	private IEnumerator AnimateUnequip()
	{
		float duration = 0.2f;
		float elapsed = 0f;
		Vector3 originalScale = base.transform.localScale;
		Vector3 targetScale = Vector3.one;
		List<Collider> colliders = new List<Collider>();
		colliders.AddRange(GetComponents<Collider>());
		colliders.AddRange(GetComponentsInChildren<Collider>());
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			physGrabObject.OverrideMass(0.1f, 1f);
			physGrabObject.impactDetector.PlayerHitDisableSet();
		}
		isUnequipping = true;
		isUnequippingTimer = 0.2f;
		Collider _unequipCollider = null;
		bool _hasUnequipCollider = false;
		foreach (Collider item in colliders)
		{
			PhysGrabObjectBoxCollider component = item.GetComponent<PhysGrabObjectBoxCollider>();
			if ((bool)component && component.unEquipCollider)
			{
				item.enabled = true;
				_hasUnequipCollider = true;
				_unequipCollider = item;
				colliders.Remove(item);
				break;
			}
		}
		if (_hasUnequipCollider)
		{
			foreach (Collider item2 in colliders)
			{
				item2.enabled = false;
			}
		}
		else
		{
			foreach (Collider item3 in colliders)
			{
				item3.enabled = true;
			}
		}
		while (elapsed < duration)
		{
			float t = elapsed / duration;
			base.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
			elapsed += Time.deltaTime;
			yield return null;
		}
		if (_hasUnequipCollider)
		{
			_unequipCollider.enabled = false;
			foreach (Collider item4 in colliders)
			{
				item4.enabled = true;
			}
		}
		isUnequipping = false;
		isUnequippingTimer = 0f;
		base.transform.localScale = targetScale;
		wasForceGrabbed = false;
		forceGrabTimer = 1.5f;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			physGrabObject.OverrideMass(0.25f);
			while (physGrabObject.rb.mass < physGrabObject.massOriginal)
			{
				physGrabObject.OverrideMass(physGrabObject.rb.mass + 5f * Time.deltaTime);
				yield return null;
			}
		}
	}

	private void ForceGrab()
	{
		bool flag = PhysGrabber.instance.photonView.ViewID == ownerPlayerId;
		if (!(forceGrabTimer > 0f))
		{
			return;
		}
		if (!SemiFunc.IsMultiplayer() || flag)
		{
			PhysGrabber.instance.OverrideGrab(physGrabObject);
			if (SemiFunc.InputUp(InputKey.Grab))
			{
				forceGrabTimer = 0f;
			}
			else if (PhysGrabber.instance.grabbedPhysGrabObject == physGrabObject)
			{
				wasForceGrabbed = true;
			}
			else if (wasForceGrabbed)
			{
				forceGrabTimer = 0f;
			}
		}
		forceGrabTimer -= Time.deltaTime;
	}

	private IEnumerator AnimateEquip()
	{
		float duration = 0.1f;
		float elapsed = 0f;
		Vector3 originalScale = base.transform.localScale;
		Vector3 targetScale = originalScale * 0.01f;
		List<Collider> list = new List<Collider>();
		list.AddRange(GetComponents<Collider>());
		list.AddRange(GetComponentsInChildren<Collider>());
		isEquipping = true;
		isEquippingTimer = 0.2f;
		Bounds bounds = new Bounds(base.transform.position, Vector3.zero);
		foreach (Collider item in list)
		{
			bounds.Encapsulate(item.bounds);
		}
		SemiFunc.AwakeRigidbodyBox(bounds.center, Quaternion.identity, bounds.size);
		foreach (Collider item2 in list)
		{
			item2.enabled = false;
		}
		while (elapsed < duration)
		{
			float t = elapsed / duration;
			base.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
			elapsed += Time.deltaTime;
			yield return null;
		}
		isEquipping = false;
		isEquippingTimer = 0f;
		base.transform.localScale = targetScale;
	}

	public void ForceUnequip(Vector3 dropPosition, int physGrabberPhotonViewID)
	{
		if (currentState != ItemState.Idle)
		{
			dropPosition += Random.insideUnitSphere * 0.2f;
			if (SemiFunc.IsMultiplayer())
			{
				base.photonView.RPC("RPC_ForceUnequip", RpcTarget.All, dropPosition, physGrabberPhotonViewID);
			}
			else
			{
				RPC_ForceUnequip(dropPosition, physGrabberPhotonViewID);
			}
		}
	}

	[PunRPC]
	private void RPC_ForceUnequip(Vector3 dropPosition, int physGrabberPhotonViewID)
	{
		PlayerAvatar playerAvatar = PlayerAvatar.instance;
		if (SemiFunc.IsMultiplayer())
		{
			playerAvatar = PhotonView.Find(physGrabberPhotonViewID)?.GetComponent<PlayerAvatar>();
		}
		if (currentState != ItemState.Idle)
		{
			ownerPlayerId = -1;
			currentState = ItemState.Unequipping;
			StatsManager.instance.PlayerInventoryUpdate(playerAvatar.steamID, "", inventorySpotIndex);
			if ((bool)equippedSpot)
			{
				equippedSpot.UnequipItem();
				equippedSpot = null;
			}
			UpdateVisuals();
			physGrabObject.OverrideDeactivateReset();
			physGrabObject.Teleport(dropPosition, Quaternion.identity);
			StartCoroutine(AnimateUnequip());
			SetItemActive(isActive: true);
		}
	}

	private void WasEquippedTimer()
	{
		if (isEquippedPrev != isEquipped)
		{
			wasEquippedTimer = 0.5f;
			isEquippedPrev = isEquipped;
		}
		if (wasEquippedTimer > 0f)
		{
			wasEquippedTimer -= Time.deltaTime;
		}
	}

	private void Update()
	{
		if (SemiFunc.RunIsArena())
		{
			return;
		}
		ForceGrab();
		WasEquippedTimer();
		IsEquippingAndUnequippingTimer();
		switch (currentState)
		{
		case ItemState.Idle:
			StateIdle();
			break;
		case ItemState.Equipping:
			StateEquipping();
			break;
		case ItemState.Equipped:
			StateEquipped();
			break;
		case ItemState.Unequipping:
			StateUnequipping();
			break;
		}
		if (unequipTimer > 0f)
		{
			unequipTimer -= Time.deltaTime;
		}
		if (equipTimer > 0f)
		{
			equipTimer -= Time.deltaTime;
		}
		if (itemEquipCubeShowTimer > 0f)
		{
			itemEquipCubeShowTimer -= Time.deltaTime;
			if (itemEquipCubeShowTimer <= 0f)
			{
				Vector3 localScale = base.transform.localScale;
				base.transform.localScale = Vector3.one;
				base.transform.localScale = localScale;
			}
		}
	}

	private void StateIdleStart()
	{
		if (stateStart)
		{
			stateStart = false;
		}
	}

	private void StateIdle()
	{
		if (currentState == ItemState.Idle)
		{
			StateIdleStart();
			isEquipped = false;
		}
	}

	private void StateEquippingStart()
	{
		if (stateStart)
		{
			stateStart = false;
		}
	}

	private void StateEquipping()
	{
		if (currentState == ItemState.Equipping)
		{
			StateEquippingStart();
			currentState = ItemState.Equipped;
			isEquipped = true;
		}
	}

	private void StateEquippedStart()
	{
		if (stateStart)
		{
			stateStart = false;
			AssetManager.instance.soundEquip.Play(physGrabObject.midPoint);
			StartCoroutine(AnimateEquip());
		}
	}

	private void StateEquipped()
	{
		if (currentState != ItemState.Equipped)
		{
			return;
		}
		StateEquippedStart();
		foreach (PhysGrabber item in physGrabObject.playerGrabbing.ToList())
		{
			item.OverrideGrabRelease(base.photonView.ViewID);
		}
		if (!isEquipped)
		{
			equipTimer = 0.5f;
		}
		isEquipped = true;
		if (physGrabObject.transform.localScale.magnitude < 0.1f)
		{
			physGrabObject.OverrideDeactivate();
		}
	}

	private void StateUnequippingStart()
	{
		if (stateStart)
		{
			stateStart = false;
		}
	}

	private void StateUnequipping()
	{
		AssetManager.instance.soundUnequip.Play(physGrabObject.midPoint);
		if (currentState == ItemState.Unequipping)
		{
			currentState = ItemState.Idle;
			isEquipped = false;
		}
	}

	private void SetRotation()
	{
		if ((bool)Camera.main)
		{
			physGrabObject.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
		}
		physGrabObject.rb.rotation = physGrabObject.transform.rotation;
	}

	private void OnDestroy()
	{
		if ((bool)equippedSpot)
		{
			equippedSpot.UnequipItem();
		}
	}
}
