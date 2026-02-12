using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
	public static ShopManager instance;

	public Transform itemRotateHelper;

	public List<ItemVolume> itemVolumes;

	public List<Item> potentialItems = new List<Item>();

	public List<Item> potentialItemConsumables = new List<Item>();

	public List<Item> potentialItemUpgrades = new List<Item>();

	public List<Item> potentialItemHealthPacks = new List<Item>();

	public Dictionary<SemiFunc.itemSecretShopType, List<ItemVolume>> secretItemVolumes = new Dictionary<SemiFunc.itemSecretShopType, List<ItemVolume>>();

	public Dictionary<SemiFunc.itemSecretShopType, List<Item>> potentialSecretItems = new Dictionary<SemiFunc.itemSecretShopType, List<Item>>();

	public int itemSpawnTargetAmount = 8;

	public int itemConsumablesAmount = 6;

	public int itemUpgradesAmount = 3;

	public int itemHealthPacksAmount = 3;

	internal List<ItemAttributes> shoppingList = new List<ItemAttributes>();

	[HideInInspector]
	public int totalCost;

	[HideInInspector]
	public int totalCurrency;

	[HideInInspector]
	public bool isThief;

	[HideInInspector]
	public Transform extractionPoint;

	internal float itemValueMultiplier = 4f;

	internal float upgradeValueIncrease = 0.5f;

	internal float healthPackValueIncrease = 0.05f;

	internal float crystalValueIncrease = 0.2f;

	private bool shopTutorial;

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		if (SemiFunc.RunIsShop() && GameDirector.instance.currentState == GameDirector.gameState.Main)
		{
			if (shopTutorial)
			{
				if (TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialShop, 1))
				{
					TutorialDirector.instance.ActivateTip("Shop", 2f, _interrupt: false);
				}
				shopTutorial = false;
			}
		}
		else
		{
			shopTutorial = true;
		}
	}

	public void ShopCheck()
	{
		totalCost = 0;
		List<ItemAttributes> list = new List<ItemAttributes>();
		foreach (ItemAttributes shopping in shoppingList)
		{
			if ((bool)shopping)
			{
				shopping.roomVolumeCheck.CheckSet();
				if (!shopping.roomVolumeCheck.inExtractionPoint)
				{
					list.Add(shopping);
				}
				else
				{
					totalCost += shopping.value;
				}
			}
			else
			{
				list.Add(shopping);
			}
		}
		foreach (ItemAttributes item in list)
		{
			shoppingList.Remove(item);
		}
	}

	public void ShoppingListItemAdd(ItemAttributes item)
	{
		shoppingList.Add(item);
		SemiFunc.ShopUpdateCost();
	}

	public void ShoppingListItemRemove(ItemAttributes item)
	{
		shoppingList.Remove(item);
		SemiFunc.ShopUpdateCost();
	}

	public void ShopInitialize()
	{
		if (SemiFunc.RunIsShop())
		{
			totalCurrency = SemiFunc.StatGetRunCurrency();
			totalCost = 0;
			shoppingList.Clear();
			GetAllItemsFromStatsManager();
			GetAllItemVolumesInScene();
			SemiFunc.ShopPopulateItemVolumes();
		}
	}

	public float UpgradeValueGet(float _value, Item item)
	{
		_value -= _value * 0.1f * (float)(GameDirector.instance.PlayerList.Count - 1);
		_value += _value * upgradeValueIncrease * (float)StatsManager.instance.GetItemsUpgradesPurchased(item.name);
		_value = Mathf.Ceil(_value);
		return _value;
	}

	public float HealthPackValueGet(float _value)
	{
		int num = Mathf.Min(RunManager.instance.levelsCompleted, 15);
		_value -= _value * 0.1f * (float)(GameDirector.instance.PlayerList.Count - 1);
		_value += _value * healthPackValueIncrease * (float)num;
		_value = Mathf.Ceil(_value);
		return _value;
	}

	public float CrystalValueGet(float _value)
	{
		int num = Mathf.Min(RunManager.instance.levelsCompleted, 15);
		_value += _value * crystalValueIncrease * (float)num;
		_value = Mathf.Ceil(_value);
		return _value;
	}

	private void GetAllItemVolumesInScene()
	{
		if (SemiFunc.IsNotMasterClient())
		{
			return;
		}
		itemVolumes.Clear();
		ItemVolume[] array = Object.FindObjectsOfType<ItemVolume>();
		foreach (ItemVolume itemVolume in array)
		{
			if (itemVolume.itemSecretShopType == SemiFunc.itemSecretShopType.none)
			{
				itemVolumes.Add(itemVolume);
				continue;
			}
			if (!secretItemVolumes.ContainsKey(itemVolume.itemSecretShopType))
			{
				secretItemVolumes.Add(itemVolume.itemSecretShopType, new List<ItemVolume>());
			}
			secretItemVolumes[itemVolume.itemSecretShopType].Add(itemVolume);
		}
		foreach (List<ItemVolume> value in secretItemVolumes.Values)
		{
			value.Shuffle();
		}
		itemVolumes.Shuffle();
	}

	private void GetAllItemsFromStatsManager()
	{
		if (SemiFunc.IsNotMasterClient())
		{
			return;
		}
		potentialItems.Clear();
		potentialItemConsumables.Clear();
		potentialItemUpgrades.Clear();
		potentialItemHealthPacks.Clear();
		potentialSecretItems.Clear();
		itemConsumablesAmount = Random.Range(4, 6);
		foreach (Item value in StatsManager.instance.itemDictionary.Values)
		{
			int num = SemiFunc.StatGetItemsPurchased(value.name);
			float num2 = value.value.valueMax / 1000f * itemValueMultiplier;
			if (value.itemType == SemiFunc.itemType.item_upgrade)
			{
				num2 = UpgradeValueGet(num2, value);
			}
			else if (value.itemType == SemiFunc.itemType.healthPack)
			{
				num2 = HealthPackValueGet(num2);
			}
			else if (value.itemType == SemiFunc.itemType.power_crystal)
			{
				num2 = CrystalValueGet(num2);
			}
			float num3 = Mathf.Clamp(num2, 1f, num2);
			bool flag = value.itemType == SemiFunc.itemType.power_crystal;
			bool flag2 = value.itemType == SemiFunc.itemType.item_upgrade;
			bool flag3 = value.itemType == SemiFunc.itemType.healthPack;
			int maxAmountInShop = value.maxAmountInShop;
			if (num >= maxAmountInShop || (value.maxPurchase && StatsManager.instance.GetItemsUpgradesPurchasedTotal(value.name) >= value.maxPurchaseAmount) || (!(num3 <= (float)totalCurrency) && Random.Range(0, 100) >= 25))
			{
				continue;
			}
			for (int i = 0; i < maxAmountInShop - num; i++)
			{
				if (flag2)
				{
					potentialItemUpgrades.Add(value);
					continue;
				}
				if (flag3)
				{
					potentialItemHealthPacks.Add(value);
					continue;
				}
				if (flag)
				{
					potentialItemConsumables.Add(value);
					continue;
				}
				if (value.itemSecretShopType == SemiFunc.itemSecretShopType.none)
				{
					potentialItems.Add(value);
					continue;
				}
				if (!potentialSecretItems.ContainsKey(value.itemSecretShopType))
				{
					potentialSecretItems.Add(value.itemSecretShopType, new List<Item>());
				}
				potentialSecretItems[value.itemSecretShopType].Add(value);
			}
		}
		potentialItems.Shuffle();
		potentialItemConsumables.Shuffle();
		potentialItemUpgrades.Shuffle();
		potentialItemHealthPacks.Shuffle();
		foreach (List<Item> value2 in potentialSecretItems.Values)
		{
			value2.Shuffle();
		}
	}
}
