using System.Collections;
using Photon.Pun;
using UnityEngine;

public class ItemAttributes : MonoBehaviour
{
	private PhotonView photonView;

	public Item item;

	public Vector3 costOffset;

	internal SemiFunc.emojiIcon emojiIcon;

	internal ColorPresets colorPreset;

	internal int value;

	internal RoomVolumeCheck roomVolumeCheck;

	private float inStartRoomCheckTimer;

	private bool inStartRoom;

	private ItemEquippable itemEquippable;

	private Transform itemVolume;

	internal bool shopItem;

	private PhysGrabObject physGrabObject;

	internal bool disableUI;

	internal string itemName;

	internal string instanceName;

	internal float showInfoTimer;

	internal bool hasIcon;

	public Sprite icon;

	private SemiFunc.itemType itemType;

	private ItemToggle itemToggle;

	private float isHeldTimer;

	private string itemTag = "";

	private string promptName = "";

	private string itemAssetName = "";

	private float itemValueMin;

	private float itemValueMax;

	private void OnValidate()
	{
		if (!SemiFunc.OnValidateCheck())
		{
			_ = base.enabled;
		}
	}

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		if ((bool)item)
		{
			colorPreset = item.colorPreset;
		}
		else
		{
			colorPreset = null;
		}
		if ((bool)item)
		{
			emojiIcon = item.emojiIcon;
		}
		else
		{
			emojiIcon = SemiFunc.emojiIcon.drone_heal;
		}
	}

	private void Start()
	{
		itemName = item.itemName;
		instanceName = null;
		physGrabObject = GetComponent<PhysGrabObject>();
		itemToggle = GetComponent<ItemToggle>();
		itemType = item.itemType;
		itemAssetName = item.name;
		itemValueMin = item.value.valueMin;
		itemValueMax = item.value.valueMax;
		if (SemiFunc.RunIsShop())
		{
			shopItem = true;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			ItemVolume componentInChildren = GetComponentInChildren<ItemVolume>();
			if ((bool)componentInChildren)
			{
				itemVolume = componentInChildren.transform;
			}
			if ((bool)itemVolume)
			{
				Vector3 vector = itemVolume.position - base.transform.position;
				base.transform.position -= vector;
				Rigidbody component = GetComponent<Rigidbody>();
				component.position -= vector;
				if (SemiFunc.IsMultiplayer())
				{
					GetComponent<PhotonTransformView>().Teleport(component.position, base.transform.rotation);
				}
				Object.Destroy(itemVolume.gameObject);
			}
		}
		if (!shopItem)
		{
			ItemManager.instance.AddSpawnedItem(this);
		}
		base.transform.parent = LevelGenerator.Instance.ItemParent.transform;
		GetValue();
		roomVolumeCheck = GetComponent<RoomVolumeCheck>();
		itemEquippable = GetComponent<ItemEquippable>();
		if ((bool)itemEquippable)
		{
			itemEquippable.itemEmojiIcon = emojiIcon;
			itemEquippable.itemEmoji = emojiIcon.ToString();
		}
		StartCoroutine(GenerateIcon());
		StartCoroutine(LateStart());
	}

	private IEnumerator GenerateIcon()
	{
		yield return null;
		if ((bool)itemEquippable && !icon)
		{
			BatteryVisualLogic componentInChildren = GetComponentInChildren<BatteryVisualLogic>(includeInactive: true);
			if ((bool)componentInChildren)
			{
				componentInChildren.gameObject.SetActive(value: false);
			}
			SemiIconMaker componentInChildren2 = GetComponentInChildren<SemiIconMaker>();
			if ((bool)componentInChildren2)
			{
				icon = componentInChildren2.CreateIconFromRenderTexture();
			}
			else
			{
				Debug.LogWarning("No IconMaker found in " + base.gameObject.name + ", add SemiIconMaker prefab and align the camera to make a proper icon... or make a custom icon and assign it in the Item Attributes!");
			}
			if ((bool)componentInChildren)
			{
				componentInChildren.gameObject.SetActive(value: true);
			}
		}
		hasIcon = true;
	}

	private IEnumerator LateStart()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		photonView = GetComponent<PhotonView>();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			StatsManager.instance.ItemFetchName(itemAssetName, this, photonView.ViewID);
		}
	}

	public void GetValue()
	{
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			float num = Random.Range(itemValueMin, itemValueMax) * ShopManager.instance.itemValueMultiplier;
			if (num < 1000f)
			{
				num = 1000f;
			}
			num = Mathf.Ceil(num / 1000f);
			if (itemType == SemiFunc.itemType.item_upgrade)
			{
				num = ShopManager.instance.UpgradeValueGet(num, item);
			}
			else if (itemType == SemiFunc.itemType.healthPack)
			{
				num = ShopManager.instance.HealthPackValueGet(num);
			}
			else if (itemType == SemiFunc.itemType.power_crystal)
			{
				num = ShopManager.instance.CrystalValueGet(num);
			}
			value = (int)num;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("GetValueRPC", RpcTarget.Others, value);
			}
		}
	}

	[PunRPC]
	public void GetValueRPC(int _value)
	{
		value = _value;
	}

	public void DisableUI(bool _disable)
	{
		if (GameManager.Multiplayer())
		{
			photonView.RPC("DisableUIRPC", RpcTarget.All, _disable);
		}
		else
		{
			DisableUIRPC(_disable);
		}
	}

	[PunRPC]
	public void DisableUIRPC(bool _disable)
	{
		disableUI = _disable;
	}

	private void ShopInTruckLogic()
	{
		if ((!SemiFunc.RunIsShop() && !RunManager.instance.levelIsShop) || !SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (inStartRoomCheckTimer > 0f)
		{
			inStartRoomCheckTimer -= Time.deltaTime;
			return;
		}
		bool flag = false;
		foreach (RoomVolume currentRoom in roomVolumeCheck.CurrentRooms)
		{
			if (currentRoom.Extraction)
			{
				flag = true;
			}
		}
		if (!flag && inStartRoom)
		{
			ShopManager.instance.ShoppingListItemRemove(this);
			inStartRoom = false;
		}
		inStartRoomCheckTimer = 0.5f;
		if (flag && !inStartRoom)
		{
			ShopManager.instance.ShoppingListItemAdd(this);
			inStartRoom = true;
		}
	}

	private void Update()
	{
		if (showInfoTimer > 0f && physGrabObject.grabbedLocal)
		{
			itemTag = "";
			promptName = "";
			showInfoTimer = 0f;
		}
		if (showInfoTimer > 0f)
		{
			if (!PhysGrabber.instance.grabbed)
			{
				ShowingInfo();
				showInfoTimer -= Time.fixedDeltaTime;
			}
			else
			{
				showInfoTimer = 0f;
			}
		}
		ShopInTruckLogic();
		if (physGrabObject.playerGrabbing.Count > 0 && !disableUI && PhysGrabber.instance.grabbedPhysGrabObject == physGrabObject && PhysGrabber.instance.grabbed)
		{
			ShowingInfo();
		}
		if (isHeldTimer > 0f)
		{
			isHeldTimer -= Time.deltaTime;
		}
		if (isHeldTimer <= 0f && physGrabObject.grabbedLocal)
		{
			isHeldTimer = 0.2f;
		}
		if (isHeldTimer > 0f)
		{
			if (SemiFunc.RunIsShop() && !PhysGrabber.instance.grabbed && PhysGrabber.instance.currentlyLookingAtItemAttributes == this)
			{
				WorldSpaceUIValue.instance.Show(physGrabObject, value, _cost: true, costOffset);
			}
			SemiFunc.UIItemInfoText(this, promptName);
		}
		else if (itemTag != "")
		{
			itemTag = "";
			promptName = "";
		}
	}

	public void ShowingInfo()
	{
		if (isHeldTimer < 0f)
		{
			return;
		}
		bool grabbedLocal = physGrabObject.grabbedLocal;
		if (!grabbedLocal && !SemiFunc.RunIsShop())
		{
			return;
		}
		isHeldTimer = 0.2f;
		bool flag = SemiFunc.RunIsShop() && (itemType == SemiFunc.itemType.item_upgrade || itemType == SemiFunc.itemType.healthPack);
		ItemToggle itemToggle = this.itemToggle;
		if ((bool)itemToggle && !itemToggle.disabled && !flag && itemTag == "")
		{
			itemTag = InputManager.instance.InputDisplayReplaceTags("[interact]");
			if (grabbedLocal)
			{
				promptName = itemName + " <color=#FFFFFF>[" + itemTag + "]</color>";
			}
			else
			{
				promptName = itemName;
			}
		}
		else if (!flag && showInfoTimer <= 0f && (bool)itemToggle && !itemToggle.disabled && itemTag != "")
		{
			promptName = itemName + " <color=#FFFFFF>[" + itemTag + "]</color>";
		}
		else
		{
			promptName = itemName;
		}
	}

	public void ShowInfo()
	{
		if (SemiFunc.RunIsShop() && !physGrabObject.grabbedLocal)
		{
			isHeldTimer = 0.1f;
			showInfoTimer = 0.1f;
		}
	}

	private void OnDestroy()
	{
		if ((bool)icon)
		{
			Object.Destroy(icon);
		}
	}
}
