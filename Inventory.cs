using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	public static Inventory instance;

	internal readonly List<InventorySpot> inventorySpots = new List<InventorySpot>();

	internal PhysGrabber physGrabber;

	private PlayerController playerController;

	private PlayerAvatar playerAvatar;

	internal bool spotsFeched;

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	public void InventorySpotAddAtIndex(InventorySpot spot, int index)
	{
		if (SemiFunc.RunIsArena())
		{
			return;
		}
		inventorySpots[index] = spot;
		foreach (InventorySpot inventorySpot in inventorySpots)
		{
			if (inventorySpot == null)
			{
				return;
			}
		}
		spotsFeched = true;
	}

	private void Start()
	{
		if (SemiFunc.RunIsArena())
		{
			base.enabled = false;
		}
		playerController = GetComponent<PlayerController>();
		for (int i = 0; i < 3; i++)
		{
			inventorySpots.Add(null);
		}
		StartCoroutine(LateStart());
	}

	private IEnumerator LateStart()
	{
		yield return null;
		physGrabber = playerController.playerAvatarScript.physGrabber;
		playerAvatar = playerController.playerAvatarScript;
	}

	public InventorySpot GetSpotByIndex(int index)
	{
		return inventorySpots[index];
	}

	public bool IsItemEquipped(ItemEquippable item)
	{
		foreach (InventorySpot inventorySpot in inventorySpots)
		{
			if ((bool)inventorySpot && inventorySpot.CurrentItem == item)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSpotOccupied(int index)
	{
		InventorySpot spotByIndex = GetSpotByIndex(index);
		if (spotByIndex != null)
		{
			return spotByIndex.IsOccupied();
		}
		return false;
	}

	public List<InventorySpot> GetAllSpots()
	{
		return inventorySpots;
	}

	public int GetFirstFreeInventorySpotIndex()
	{
		List<InventorySpot> allSpots = instance.GetAllSpots();
		for (int i = 0; i < allSpots.Count; i++)
		{
			if (!allSpots[i].IsOccupied())
			{
				return i;
			}
		}
		return -1;
	}

	public int InventorySpotsOccupied()
	{
		int num = 0;
		foreach (InventorySpot inventorySpot in inventorySpots)
		{
			if ((bool)inventorySpot && inventorySpot.IsOccupied())
			{
				num++;
			}
		}
		return num;
	}

	public void InventoryDropAll(Vector3 dropPosition, int playerViewID)
	{
		if (SemiFunc.RunIsArena())
		{
			return;
		}
		foreach (InventorySpot inventorySpot in inventorySpots)
		{
			if (inventorySpot.IsOccupied())
			{
				ItemEquippable currentItem = inventorySpot.CurrentItem;
				if (currentItem != null)
				{
					currentItem.ForceUnequip(dropPosition, playerViewID);
				}
			}
		}
	}

	public int GetBatteryStateFromInventorySpot(int index)
	{
		InventorySpot spotByIndex = GetSpotByIndex(index);
		if (spotByIndex != null && spotByIndex.IsOccupied())
		{
			ItemEquippable currentItem = spotByIndex.CurrentItem;
			if (currentItem != null)
			{
				ItemBattery component = currentItem.GetComponent<ItemBattery>();
				if (component != null)
				{
					return component.batteryLifeInt;
				}
			}
		}
		return -1;
	}

	public void ForceUnequip()
	{
		if (SemiFunc.RunIsArena() || SemiFunc.RunIsShop() || RunManager.instance.levelIsShop)
		{
			return;
		}
		foreach (InventorySpot inventorySpot in inventorySpots)
		{
			if (!inventorySpot.IsOccupied())
			{
				continue;
			}
			ItemEquippable currentItem = inventorySpot.CurrentItem;
			if ((bool)currentItem)
			{
				if (SemiFunc.IsMultiplayer())
				{
					currentItem.GetComponent<ItemEquippable>().ForceUnequip(playerAvatar.PlayerVisionTarget.VisionTransform.position, physGrabber.photonView.ViewID);
				}
				else
				{
					currentItem.GetComponent<ItemEquippable>().ForceUnequip(playerAvatar.PlayerVisionTarget.VisionTransform.position, -1);
				}
			}
		}
	}
}
