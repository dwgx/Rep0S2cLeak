using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class PunManager : MonoBehaviour
{
	internal PhotonView photonView;

	internal StatsManager statsManager;

	private ShopManager shopManager;

	private ItemManager itemManager;

	public static PunManager instance;

	private List<ExitGames.Client.Photon.Hashtable> syncData = new List<ExitGames.Client.Photon.Hashtable>();

	public PhotonLagSimulationGui lagSimulationGui;

	private int totalHaul;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		statsManager = StatsManager.instance;
		shopManager = ShopManager.instance;
		itemManager = ItemManager.instance;
		photonView = GetComponent<PhotonView>();
	}

	public void SetItemName(string name, ItemAttributes itemAttributes, int photonViewID)
	{
		if (photonViewID != -1 && SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("SetItemNameRPC", RpcTarget.All, name, photonViewID);
			}
			else
			{
				SetItemNameLOGIC(name, photonViewID, itemAttributes);
			}
		}
	}

	private void SetItemNameLOGIC(string name, int photonViewID, ItemAttributes _itemAttributes = null)
	{
		if (photonViewID == -1 && SemiFunc.IsMultiplayer())
		{
			return;
		}
		ItemAttributes itemAttributes = _itemAttributes;
		if (SemiFunc.IsMultiplayer())
		{
			itemAttributes = PhotonView.Find(photonViewID).GetComponent<ItemAttributes>();
		}
		if (_itemAttributes == null && !SemiFunc.IsMultiplayer())
		{
			return;
		}
		itemAttributes.instanceName = name;
		ItemBattery component = itemAttributes.GetComponent<ItemBattery>();
		if ((bool)component)
		{
			component.SetBatteryLife(statsManager.itemStatBattery[name]);
		}
		ItemEquippable component2 = itemAttributes.GetComponent<ItemEquippable>();
		if (!component2)
		{
			return;
		}
		int spot = 0;
		List<PlayerAvatar> list = SemiFunc.PlayerGetList();
		int hashCode = name.GetHashCode();
		bool flag = false;
		PlayerAvatar playerAvatar = null;
		foreach (PlayerAvatar item in list)
		{
			string steamID = item.steamID;
			if (StatsManager.instance.playerInventorySpot1[steamID] == hashCode && StatsManager.instance.playerInventorySpot1Taken[steamID] == 0)
			{
				spot = 0;
				flag = true;
				playerAvatar = item;
				StatsManager.instance.playerInventorySpot1Taken[steamID] = 1;
				break;
			}
			if (StatsManager.instance.playerInventorySpot2[steamID] == hashCode && StatsManager.instance.playerInventorySpot2Taken[steamID] == 0)
			{
				spot = 1;
				flag = true;
				playerAvatar = item;
				StatsManager.instance.playerInventorySpot2Taken[steamID] = 1;
				break;
			}
			if (StatsManager.instance.playerInventorySpot3[steamID] == hashCode && StatsManager.instance.playerInventorySpot3Taken[steamID] == 0)
			{
				spot = 2;
				flag = true;
				playerAvatar = item;
				StatsManager.instance.playerInventorySpot3Taken[steamID] = 1;
				break;
			}
		}
		if (flag)
		{
			int requestingPlayerId = -1;
			if (SemiFunc.IsMultiplayer())
			{
				requestingPlayerId = playerAvatar.photonView.ViewID;
			}
			component2.RequestEquip(spot, requestingPlayerId);
		}
	}

	public void CrownPlayerSync(string _steamID)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.IsMultiplayer())
		{
			photonView.RPC("CrownPlayerRPC", RpcTarget.AllBuffered, _steamID);
		}
	}

	[PunRPC]
	public void CrownPlayerRPC(string _steamID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			SessionManager.instance.crownedPlayerSteamID = _steamID;
			PlayerCrownSet component = UnityEngine.Object.Instantiate(SessionManager.instance.crownPrefab).GetComponent<PlayerCrownSet>();
			component.crownOwnerFetched = true;
			component.crownOwnerSteamID = _steamID;
			StatsManager.instance.UpdateCrown(_steamID);
		}
	}

	[PunRPC]
	public void SetItemNameRPC(string name, int photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			SetItemNameLOGIC(name, photonViewID);
		}
	}

	public void ShopUpdateCost()
	{
		int num = 0;
		List<ItemAttributes> list = new List<ItemAttributes>();
		foreach (ItemAttributes shopping in ShopManager.instance.shoppingList)
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
					num += shopping.value;
				}
			}
			else
			{
				list.Add(shopping);
			}
		}
		foreach (ItemAttributes item in list)
		{
			ShopManager.instance.shoppingList.Remove(item);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdateShoppingCostRPC", RpcTarget.All, num);
			}
			else
			{
				UpdateShoppingCostRPC(num);
			}
		}
	}

	private void Update()
	{
		if (SemiFunc.FPSImpulse5() && SemiFunc.IsMultiplayer() && SemiFunc.IsMasterClient() && totalHaul != RoundDirector.instance.totalHaul)
		{
			totalHaul = RoundDirector.instance.totalHaul;
			photonView.RPC("SyncHaul", RpcTarget.Others, totalHaul);
		}
	}

	[PunRPC]
	public void SyncHaul(int value, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			RoundDirector.instance.totalHaul = value;
		}
	}

	[PunRPC]
	public void UpdateShoppingCostRPC(int value, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			ShopManager.instance.totalCost = value;
		}
	}

	public void ShopPopulateItemVolumes()
	{
		if (SemiFunc.IsNotMasterClient())
		{
			return;
		}
		int spawnCount = 0;
		int spawnCount2 = 0;
		int spawnCount3 = 0;
		int spawnCount4 = 0;
		foreach (KeyValuePair<SemiFunc.itemSecretShopType, List<ItemVolume>> secretItemVolume in ShopManager.instance.secretItemVolumes)
		{
			List<ItemVolume> value = secretItemVolume.Value;
			foreach (ItemVolume item in value)
			{
				if (ShopManager.instance.potentialSecretItems.ContainsKey(secretItemVolume.Key))
				{
					_ = ShopManager.instance.potentialSecretItems[secretItemVolume.Key];
					if (UnityEngine.Random.Range(0, 3) == 0 && (bool)item)
					{
						SpawnShopItem(item, ShopManager.instance.potentialSecretItems[secretItemVolume.Key], ref spawnCount, isSecret: true);
					}
				}
			}
			foreach (ItemVolume item2 in value)
			{
				if ((bool)item2)
				{
					UnityEngine.Object.Destroy(item2.gameObject);
				}
			}
		}
		foreach (ItemVolume itemVolume in shopManager.itemVolumes)
		{
			if (shopManager.potentialItems.Count == 0 && shopManager.potentialItemConsumables.Count == 0)
			{
				break;
			}
			if ((spawnCount >= shopManager.itemSpawnTargetAmount || !SpawnShopItem(itemVolume, shopManager.potentialItems, ref spawnCount)) && (spawnCount2 >= shopManager.itemConsumablesAmount || !SpawnShopItem(itemVolume, shopManager.potentialItemConsumables, ref spawnCount2)))
			{
				if (spawnCount3 < shopManager.itemUpgradesAmount)
				{
					SpawnShopItem(itemVolume, shopManager.potentialItemUpgrades, ref spawnCount3);
				}
				if (spawnCount4 < shopManager.itemHealthPacksAmount)
				{
					SpawnShopItem(itemVolume, shopManager.potentialItemHealthPacks, ref spawnCount4);
				}
			}
		}
		foreach (ItemVolume itemVolume2 in shopManager.itemVolumes)
		{
			UnityEngine.Object.Destroy(itemVolume2.gameObject);
		}
	}

	private bool SpawnShopItem(ItemVolume itemVolume, List<Item> itemList, ref int spawnCount, bool isSecret = false)
	{
		for (int num = itemList.Count - 1; num >= 0; num--)
		{
			Item item = itemList[num];
			if (item.itemVolume == itemVolume.itemVolume)
			{
				ShopManager.instance.itemRotateHelper.transform.parent = itemVolume.transform;
				ShopManager.instance.itemRotateHelper.transform.localRotation = item.spawnRotationOffset;
				Quaternion rotation = ShopManager.instance.itemRotateHelper.transform.rotation;
				ShopManager.instance.itemRotateHelper.transform.parent = ShopManager.instance.transform;
				if (SemiFunc.IsMultiplayer())
				{
					PhotonNetwork.InstantiateRoomObject(item.prefab.ResourcePath, itemVolume.transform.position, rotation, 0);
				}
				else
				{
					UnityEngine.Object.Instantiate(item.prefab.Prefab, itemVolume.transform.position, rotation);
				}
				itemList.RemoveAt(num);
				if (!isSecret)
				{
					spawnCount++;
				}
				return true;
			}
		}
		return false;
	}

	public void TruckPopulateItemVolumes()
	{
		ItemManager.instance.spawnedItems.Clear();
		if (SemiFunc.IsNotMasterClient())
		{
			return;
		}
		List<ItemVolume> list = new List<ItemVolume>(itemManager.itemVolumes);
		List<Item> list2 = new List<Item>(itemManager.purchasedItems);
		while (list.Count > 0 && list2.Count > 0)
		{
			bool flag = false;
			for (int i = 0; i < list2.Count; i++)
			{
				Item item = list2[i];
				ItemVolume itemVolume = list.Find((ItemVolume v) => v.itemVolume == item.itemVolume);
				if ((bool)itemVolume)
				{
					SpawnItem(item, itemVolume);
					list.Remove(itemVolume);
					list2.RemoveAt(i);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		foreach (ItemVolume itemVolume2 in itemManager.itemVolumes)
		{
			UnityEngine.Object.Destroy(itemVolume2.gameObject);
		}
	}

	private void SpawnItem(Item item, ItemVolume volume)
	{
		ShopManager.instance.itemRotateHelper.transform.parent = volume.transform;
		ShopManager.instance.itemRotateHelper.transform.localRotation = item.spawnRotationOffset;
		Quaternion rotation = ShopManager.instance.itemRotateHelper.transform.rotation;
		ShopManager.instance.itemRotateHelper.transform.parent = ShopManager.instance.transform;
		if (SemiFunc.IsMasterClient())
		{
			PhotonNetwork.InstantiateRoomObject(item.prefab.ResourcePath, volume.transform.position, rotation, 0);
		}
		else if (!SemiFunc.IsMultiplayer())
		{
			UnityEngine.Object.Instantiate(item.prefab.Prefab, volume.transform.position, rotation);
		}
	}

	public void AddingItem(string itemName, int index, int photonViewID, ItemAttributes itemAttributes)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("AddingItemRPC", RpcTarget.All, itemName, index, photonViewID);
		}
		else
		{
			AddingItemLOGIC(itemName, index, photonViewID, itemAttributes);
		}
	}

	private void AddingItemLOGIC(string itemName, int index, int photonViewID, ItemAttributes itemAttributes = null)
	{
		if (!StatsManager.instance.item.ContainsKey(itemName))
		{
			StatsManager.instance.item.Add(itemName, index);
			StatsManager.instance.itemStatBattery.Add(itemName, 100);
			StatsManager.instance.takenItemNames.Add(itemName);
		}
		else
		{
			Debug.LogWarning("Item " + itemName + " already exists in the dictionary");
		}
		SetItemNameLOGIC(itemName, photonViewID, itemAttributes);
	}

	[PunRPC]
	public void AddingItemRPC(string itemName, int index, int photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			AddingItemLOGIC(itemName, index, photonViewID);
		}
	}

	public void UpdateStat(string dictionaryName, string key, int value)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("UpdateStatRPC", RpcTarget.All, dictionaryName, key, value);
		}
		else
		{
			UpdateStatRPC(dictionaryName, key, value);
		}
	}

	[PunRPC]
	public void UpdateStatRPC(string dictionaryName, string key, int value, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			StatsManager.instance.DictionaryUpdateValue(dictionaryName, key, value);
		}
	}

	public int SetRunStatSet(string statName, int value)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				statsManager.runStats[statName] = value;
				photonView.RPC("SetRunStatRPC", RpcTarget.Others, statName, value);
			}
			else
			{
				statsManager.runStats[statName] = value;
			}
		}
		return statsManager.runStats[statName];
	}

	[PunRPC]
	public void SetRunStatRPC(string statName, int value, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			statsManager.runStats[statName] = value;
		}
	}

	public int UpgradeItemBattery(string itemName)
	{
		statsManager.itemBatteryUpgrades[itemName]++;
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpgradeItemBatteryRPC", RpcTarget.Others, itemName, statsManager.itemBatteryUpgrades[itemName]);
		}
		return statsManager.itemBatteryUpgrades[itemName];
	}

	[PunRPC]
	public void UpgradeItemBatteryRPC(string itemName, int value, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			statsManager.itemBatteryUpgrades[itemName] = value;
		}
	}

	public int UpgradePlayerHealth(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeHealth[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeHealth[_steamID] += num2;
		UpdateHealthRightAway(_steamID, num2);
		return statsManager.playerUpgradeHealth[_steamID];
	}

	private void UpdateHealthRightAway(string _steamID, int value)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if (!playerAvatar)
		{
			return;
		}
		playerAvatar.playerHealth.maxHealth += 20 * value;
		if (playerAvatar == SemiFunc.PlayerAvatarLocal())
		{
			if (value >= 0)
			{
				playerAvatar.playerHealth.Heal(20 * value, effect: false);
			}
			else
			{
				playerAvatar.playerHealth.Hurt(20 * -value, savingGrace: false);
			}
		}
	}

	public int UpgradePlayerEnergy(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeStamina[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeStamina[_steamID] += num2;
		UpdateEnergyRightAway(_steamID, num2);
		return statsManager.playerUpgradeStamina[_steamID];
	}

	private void UpdateEnergyRightAway(string _steamID, int value)
	{
		if (SemiFunc.PlayerAvatarGetFromSteamID(_steamID) == SemiFunc.PlayerAvatarLocal())
		{
			PlayerController.instance.EnergyStart += 10 * value;
			PlayerController.instance.EnergyCurrent = PlayerController.instance.EnergyStart;
		}
	}

	public int UpgradePlayerExtraJump(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeExtraJump[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeExtraJump[_steamID] += num2;
		UpdateExtraJumpRightAway(_steamID, num2);
		return statsManager.playerUpgradeExtraJump[_steamID];
	}

	private void UpdateExtraJumpRightAway(string _steamID, int value)
	{
		if (SemiFunc.PlayerAvatarGetFromSteamID(_steamID) == SemiFunc.PlayerAvatarLocal())
		{
			PlayerController.instance.JumpExtra += value;
		}
	}

	public int UpgradeMapPlayerCount(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeMapPlayerCount[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeMapPlayerCount[_steamID] += num2;
		UpdateMapPlayerCountRightAway(_steamID, num2);
		return statsManager.playerUpgradeMapPlayerCount[_steamID];
	}

	private void UpdateMapPlayerCountRightAway(string _steamID, int value)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.upgradeMapPlayerCount += value;
		}
	}

	public int UpgradePlayerTumbleLaunch(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeLaunch[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeLaunch[_steamID] += num2;
		UpdateTumbleLaunchRightAway(_steamID, num2);
		return statsManager.playerUpgradeLaunch[_steamID];
	}

	private void UpdateTumbleLaunchRightAway(string _steamID, int value)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.tumble.tumbleLaunch += value;
		}
	}

	public int UpgradePlayerTumbleClimb(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeTumbleClimb[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeTumbleClimb[_steamID] += num2;
		UpdateTumbleClimbRightAway(_steamID, num2);
		return statsManager.playerUpgradeTumbleClimb[_steamID];
	}

	private void UpdateTumbleClimbRightAway(string _steamID, int value)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.upgradeTumbleClimb += value;
		}
	}

	public int UpgradeDeathHeadBattery(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeDeathHeadBattery[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeDeathHeadBattery[_steamID] += num2;
		UpdateDeathHeadBatteryRightAway(_steamID, num2);
		return statsManager.playerUpgradeDeathHeadBattery[_steamID];
	}

	private void UpdateDeathHeadBatteryRightAway(string _steamID, int value)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.upgradeDeathHeadBattery += value;
		}
	}

	public int UpgradePlayerTumbleWings(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeTumbleWings[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeTumbleWings[_steamID] += num2;
		UpdateTumbleWingsRightAway(_steamID, num2);
		return statsManager.playerUpgradeTumbleWings[_steamID];
	}

	private void UpdateTumbleWingsRightAway(string _steamID, int value)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.upgradeTumbleWings += value;
		}
	}

	public int UpgradePlayerSprintSpeed(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeSpeed[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeSpeed[_steamID] += num2;
		UpdateSprintSpeedRightAway(_steamID, num2);
		return statsManager.playerUpgradeSpeed[_steamID];
	}

	private void UpdateSprintSpeedRightAway(string _steamID, int value)
	{
		if (SemiFunc.PlayerAvatarGetFromSteamID(_steamID) == SemiFunc.PlayerAvatarLocal())
		{
			PlayerController.instance.SprintSpeed += value;
			PlayerController.instance.SprintSpeedUpgrades += value;
			PlayerController.instance.playerOriginalSprintSpeed += value;
		}
	}

	public int UpgradePlayerCrouchRest(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeCrouchRest[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeCrouchRest[_steamID] += num2;
		UpdateCrouchRestRightAway(_steamID, num2);
		return statsManager.playerUpgradeCrouchRest[_steamID];
	}

	private void UpdateCrouchRestRightAway(string _steamID, int value)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.upgradeCrouchRest += value;
		}
	}

	public int UpgradePlayerGrabStrength(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeStrength[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeStrength[_steamID] += num2;
		UpdateGrabStrengthRightAway(_steamID, num2);
		return statsManager.playerUpgradeStrength[_steamID];
	}

	private void UpdateGrabStrengthRightAway(string _steamID, int value)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.physGrabber.grabStrength += 0.2f * (float)value;
		}
	}

	public int UpgradePlayerThrowStrength(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeThrow[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeThrow[_steamID] += num2;
		UpdateThrowStrengthRightAway(_steamID, num2);
		return statsManager.playerUpgradeThrow[_steamID];
	}

	private void UpdateThrowStrengthRightAway(string _steamID, int value)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.physGrabber.throwStrength += 0.3f * (float)value;
		}
	}

	public int UpgradePlayerGrabRange(string _steamID, int value = 1)
	{
		int num = statsManager.playerUpgradeRange[_steamID];
		int num2 = Math.Max(0, num + value) - num;
		if (num2 == 0)
		{
			return num;
		}
		statsManager.playerUpgradeRange[_steamID] += num2;
		UpdateGrabRangeRightAway(_steamID, num2);
		return statsManager.playerUpgradeRange[_steamID];
	}

	private void UpdateGrabRangeRightAway(string _steamID, int value)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.physGrabber.grabRange += value;
		}
	}

	[PunRPC]
	public void TesterUpgradeCommandRPC(string _steamID, string upgradeName, int upgradeNum, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if (!playerAvatar || (SemiFunc.IsMultiplayer() && (Debug.isDebugBuild ? (!SemiFunc.MasterAndOwnerOnlyRPC(_info, playerAvatar.photonView)) : (!SemiFunc.MasterOnlyRPC(_info)))))
		{
			return;
		}
		switch (upgradeName)
		{
		case "CrouchRest":
			UpgradePlayerCrouchRest(_steamID, upgradeNum);
			return;
		case "ExtraJump":
			UpgradePlayerExtraJump(_steamID, upgradeNum);
			return;
		case "Health":
			UpgradePlayerHealth(_steamID, upgradeNum);
			return;
		case "Launch":
			UpgradePlayerTumbleLaunch(_steamID, upgradeNum);
			return;
		case "MapPlayerCount":
			UpgradeMapPlayerCount(_steamID, upgradeNum);
			return;
		case "Range":
			UpgradePlayerGrabRange(_steamID, upgradeNum);
			return;
		case "Speed":
			UpgradePlayerSprintSpeed(_steamID, upgradeNum);
			return;
		case "Stamina":
			UpgradePlayerEnergy(_steamID, upgradeNum);
			return;
		case "Strength":
			UpgradePlayerGrabStrength(_steamID, upgradeNum);
			return;
		case "Throw":
			UpgradePlayerThrowStrength(_steamID, upgradeNum);
			return;
		case "TumbleWings":
			UpgradePlayerTumbleWings(_steamID, upgradeNum);
			return;
		case "TumbleClimb":
			UpgradePlayerTumbleClimb(_steamID, upgradeNum);
			return;
		case "DeathHeadBattery":
			UpgradeDeathHeadBattery(_steamID, upgradeNum);
			return;
		}
		int num = StatsManager.instance.dictionaryOfDictionaries["playerUpgrade" + upgradeName][_steamID];
		int num2 = Math.Max(0, num + upgradeNum) - num;
		if (num2 != 0)
		{
			StatsManager.instance.dictionaryOfDictionaries["playerUpgrade" + upgradeName][_steamID] += num2;
		}
	}

	public void SyncAllDictionaries()
	{
		StatsManager.instance.statsSynced = true;
		if (!SemiFunc.IsMultiplayer() || !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		syncData.Clear();
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<string, Dictionary<string, int>> dictionaryOfDictionary in statsManager.dictionaryOfDictionaries)
		{
			string key = dictionaryOfDictionary.Key;
			hashtable.Add(key, ConvertToHashtable(dictionaryOfDictionary.Value));
			num++;
			num2++;
			if (num > 3 || num2 == statsManager.dictionaryOfDictionaries.Count)
			{
				syncData.Add(hashtable);
				num = 0;
			}
		}
		for (int i = 0; i < syncData.Count; i++)
		{
			bool flag = i == syncData.Count - 1;
			ExitGames.Client.Photon.Hashtable hashtable2 = syncData[i];
			photonView.RPC("ReceiveSyncData", RpcTarget.Others, hashtable2, flag);
		}
		syncData.Clear();
	}

	private ExitGames.Client.Photon.Hashtable ConvertToHashtable(Dictionary<string, int> dictionary)
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			hashtable.Add(item.Key, item.Value);
		}
		return hashtable;
	}

	private Dictionary<K, V> ConvertToDictionary<K, V>(ExitGames.Client.Photon.Hashtable hashtable)
	{
		Dictionary<K, V> dictionary = new Dictionary<K, V>();
		foreach (DictionaryEntry item in hashtable)
		{
			dictionary.Add((K)item.Key, (V)item.Value);
		}
		return dictionary;
	}

	[PunRPC]
	public void ReceiveSyncData(ExitGames.Client.Photon.Hashtable data, bool finalChunk, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Dictionary<string, int>> dictionaryOfDictionary in statsManager.dictionaryOfDictionaries)
		{
			string key = dictionaryOfDictionary.Key;
			if (data.ContainsKey(key))
			{
				list.Add(key);
			}
		}
		foreach (string item in list)
		{
			Dictionary<string, int> dictionary = statsManager.dictionaryOfDictionaries[item];
			foreach (DictionaryEntry item2 in (ExitGames.Client.Photon.Hashtable)data[item])
			{
				string key2 = (string)item2.Key;
				int value = (int)item2.Value;
				dictionary[key2] = value;
			}
		}
		if (finalChunk)
		{
			StatsManager.instance.statsSynced = true;
		}
	}
}
