using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventorySpot : SemiUI
{
	public enum SpotState
	{
		Empty,
		Occupied
	}

	[FormerlySerializedAs("SpotIndex")]
	public int inventorySpotIndex;

	private PhotonView photonView;

	private float equipCooldown = 0.2f;

	private float lastEquipTime;

	[FormerlySerializedAs("_currentState")]
	[SerializeField]
	private SpotState currentState;

	internal Image inventoryIcon;

	private bool handleInput;

	public TextMeshProUGUI noItem;

	private BatteryVisualLogic batteryVisualLogic;

	private bool stateStart;

	public ItemEquippable CurrentItem { get; private set; }

	protected override void Start()
	{
		inventoryIcon = GetComponentInChildren<Image>();
		photonView = GetComponent<PhotonView>();
		UpdateUI();
		currentState = SpotState.Empty;
		base.Start();
		uiText = null;
		SetEmoji(null);
		batteryVisualLogic = GetComponentInChildren<BatteryVisualLogic>();
		batteryVisualLogic.gameObject.SetActive(value: false);
		StartCoroutine(LateStart());
	}

	private IEnumerator LateStart()
	{
		yield return null;
		if (!SemiFunc.MenuLevel() && !SemiFunc.RunIsLobbyMenu() && !SemiFunc.RunIsArena())
		{
			Inventory.instance.InventorySpotAddAtIndex(this, inventorySpotIndex);
		}
	}

	public void SetEmoji(Sprite emoji)
	{
		if (!emoji)
		{
			inventoryIcon.enabled = false;
			noItem.enabled = true;
		}
		else
		{
			noItem.enabled = false;
			inventoryIcon.enabled = true;
			inventoryIcon.sprite = emoji;
		}
	}

	public bool IsOccupied()
	{
		return currentState == SpotState.Occupied;
	}

	public void EquipItem(ItemEquippable item)
	{
		if (currentState == SpotState.Empty)
		{
			CurrentItem = item;
			currentState = SpotState.Occupied;
			stateStart = true;
			UpdateUI();
		}
	}

	public void UnequipItem()
	{
		if (currentState == SpotState.Occupied)
		{
			CurrentItem = null;
			currentState = SpotState.Empty;
			stateStart = true;
			UpdateUI();
		}
	}

	public void UpdateUI()
	{
		if (currentState == SpotState.Occupied && (bool)CurrentItem)
		{
			SemiUISpringScale(0.5f, 2f, 0.2f);
			SetEmoji(CurrentItem.GetComponent<ItemAttributes>().icon);
		}
		else
		{
			SetEmoji(null);
			SemiUISpringScale(0.5f, 2f, 0.2f);
		}
	}

	protected override void Update()
	{
		if (SemiFunc.InputDown(InputKey.Inventory1) && inventorySpotIndex == 0)
		{
			HandleInput();
		}
		else if (SemiFunc.InputDown(InputKey.Inventory2) && inventorySpotIndex == 1)
		{
			HandleInput();
		}
		else if (SemiFunc.InputDown(InputKey.Inventory3) && inventorySpotIndex == 2)
		{
			HandleInput();
		}
		switch (currentState)
		{
		case SpotState.Empty:
			StateEmpty();
			break;
		case SpotState.Occupied:
			StateOccupied();
			break;
		}
		base.Update();
	}

	private void HandleInput()
	{
		if (!SemiFunc.RunIsArena() && !(PlayerController.instance.InputDisableTimer > 0f) && (handleInput || !(Time.time - lastEquipTime < equipCooldown)))
		{
			lastEquipTime = Time.time;
			handleInput = false;
			if (IsOccupied())
			{
				CurrentItem.RequestUnequip();
			}
			else
			{
				AttemptEquipItem();
			}
		}
	}

	private void AttemptEquipItem()
	{
		ItemEquippable itemPlayerIsHolding = GetItemPlayerIsHolding();
		if (itemPlayerIsHolding != null)
		{
			itemPlayerIsHolding.RequestEquip(inventorySpotIndex, PhysGrabber.instance.photonView.ViewID);
		}
	}

	private ItemEquippable GetItemPlayerIsHolding()
	{
		if (!PhysGrabber.instance.grabbed)
		{
			return null;
		}
		return PhysGrabber.instance.grabbedPhysGrabObject?.GetComponent<ItemEquippable>();
	}

	private void StateOccupied()
	{
		if (currentState != SpotState.Occupied || !CurrentItem)
		{
			return;
		}
		if (stateStart)
		{
			batteryVisualLogic.BatteryBarsSet();
		}
		ItemBattery component = CurrentItem.GetComponent<ItemBattery>();
		if ((bool)component)
		{
			if (!batteryVisualLogic.gameObject.activeSelf)
			{
				batteryVisualLogic.gameObject.SetActive(value: true);
			}
			if (batteryVisualLogic.targetScale == 0f)
			{
				batteryVisualLogic.ResetOutro();
			}
			if (batteryVisualLogic.itemBattery != component)
			{
				batteryVisualLogic.itemBattery = component;
				batteryVisualLogic.BatteryBarsSet();
			}
			if (SemiFunc.RunIsLobby() && batteryVisualLogic.currentBars < batteryVisualLogic.batteryBars / 2)
			{
				batteryVisualLogic.OverrideChargeNeeded(0.2f);
			}
		}
		else if (stateStart)
		{
			batteryVisualLogic.BatteryOutro();
		}
		stateStart = false;
	}

	private void StateEmpty()
	{
		if (currentState == SpotState.Empty)
		{
			if (stateStart)
			{
				batteryVisualLogic.BatteryBarsSet();
				batteryVisualLogic.BatteryOutro();
				stateStart = false;
			}
			SemiUIScoot(new Vector2(0f, -20f));
		}
	}
}
