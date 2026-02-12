using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
	[Serializable]
	public class SerializableDictionary
	{
		public List<string> keys = new List<string>();

		public List<SerializableInnerDictionary> values = new List<SerializableInnerDictionary>();
	}

	[Serializable]
	public class SerializableInnerDictionary
	{
		public List<string> keys = new List<string>();

		public List<int> values = new List<int>();
	}

	public class SaveFolder
	{
		public string name;

		public bool isValid;

		public List<string> backups = new List<string>();
	}

	public static StatsManager instance;

	public string folderPath = "ScriptableObjects";

	internal string dateAndTime;

	internal string teamName = "R.E.P.O.";

	internal string totallyNormalString = "Why would you want to cheat?... :o It's no fun. :') :'D";

	public Dictionary<string, Item> itemDictionary = new Dictionary<string, Item>();

	public Dictionary<string, int> runStats = new Dictionary<string, int>();

	public Dictionary<string, int> itemsPurchased = new Dictionary<string, int>();

	public Dictionary<string, int> itemsUpgradesPurchased = new Dictionary<string, int>();

	public Dictionary<string, int> itemBatteryUpgrades = new Dictionary<string, int>();

	public Dictionary<string, int> itemsPurchasedTotal = new Dictionary<string, int>();

	public Dictionary<string, int> playerHealth = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeHealth = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeStamina = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeExtraJump = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeLaunch = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeTumbleClimb = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeMapPlayerCount = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeDeathHeadBattery = new Dictionary<string, int>();

	internal Dictionary<string, int> playerColor = new Dictionary<string, int>();

	private int playerColorIndex;

	public Dictionary<string, int> playerUpgradeSpeed = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeStrength = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeThrow = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeRange = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeCrouchRest = new Dictionary<string, int>();

	public Dictionary<string, int> playerUpgradeTumbleWings = new Dictionary<string, int>();

	public Dictionary<string, int> playerInventorySpot1 = new Dictionary<string, int>();

	public Dictionary<string, int> playerInventorySpot2 = new Dictionary<string, int>();

	public Dictionary<string, int> playerInventorySpot3 = new Dictionary<string, int>();

	public Dictionary<string, int> playerInventorySpot1Taken = new Dictionary<string, int>();

	public Dictionary<string, int> playerInventorySpot2Taken = new Dictionary<string, int>();

	public Dictionary<string, int> playerInventorySpot3Taken = new Dictionary<string, int>();

	public Dictionary<string, int> playerHasCrown = new Dictionary<string, int>();

	public Dictionary<string, int> item = new Dictionary<string, int>();

	public Dictionary<string, int> itemStatBattery = new Dictionary<string, int>();

	[HideInInspector]
	public float chargingStationCharge = 1f;

	public Dictionary<string, Dictionary<string, int>> dictionaryOfDictionaries = new Dictionary<string, Dictionary<string, int>>();

	[HideInInspector]
	public bool statsSynced;

	internal List<string> takenItemNames = new List<string>();

	internal float timePlayed;

	internal Dictionary<string, string> playerNames = new Dictionary<string, string>();

	internal int backupIndex;

	internal string saveFileCurrent;

	internal bool saveFileReady;

	internal List<string> doNotSaveTheseDictionaries = new List<string>();

	internal List<string> stripTheseDictionaries = new List<string>();

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public int TimePlayedGetHours(float _timePlayed)
	{
		return (int)(_timePlayed / 3600f);
	}

	public int TimePlayedGetMinutes(float _timePlayed)
	{
		return (int)(_timePlayed % 3600f / 60f);
	}

	public int TimePlayedGetSeconds(float _timePlayed)
	{
		return (int)(_timePlayed % 60f);
	}

	private void Update()
	{
		if (!SemiFunc.RunIsLobbyMenu() && !SemiFunc.IsMainMenu())
		{
			timePlayed += Time.deltaTime;
		}
	}

	private void Start()
	{
		dictionaryOfDictionaries.Add("runStats", runStats);
		dictionaryOfDictionaries.Add("itemsPurchased", itemsPurchased);
		dictionaryOfDictionaries.Add("itemsPurchasedTotal", itemsPurchasedTotal);
		dictionaryOfDictionaries.Add("itemsUpgradesPurchased", itemsUpgradesPurchased);
		dictionaryOfDictionaries.Add("itemBatteryUpgrades", itemBatteryUpgrades);
		dictionaryOfDictionaries.Add("playerHealth", playerHealth);
		dictionaryOfDictionaries.Add("playerUpgradeHealth", playerUpgradeHealth);
		dictionaryOfDictionaries.Add("playerUpgradeStamina", playerUpgradeStamina);
		dictionaryOfDictionaries.Add("playerUpgradeExtraJump", playerUpgradeExtraJump);
		dictionaryOfDictionaries.Add("playerUpgradeLaunch", playerUpgradeLaunch);
		dictionaryOfDictionaries.Add("playerUpgradeTumbleClimb", playerUpgradeTumbleClimb);
		dictionaryOfDictionaries.Add("playerUpgradeDeathHeadBattery", playerUpgradeDeathHeadBattery);
		dictionaryOfDictionaries.Add("playerUpgradeMapPlayerCount", playerUpgradeMapPlayerCount);
		dictionaryOfDictionaries.Add("playerColor", playerColor);
		dictionaryOfDictionaries.Add("playerUpgradeSpeed", playerUpgradeSpeed);
		dictionaryOfDictionaries.Add("playerUpgradeStrength", playerUpgradeStrength);
		dictionaryOfDictionaries.Add("playerUpgradeRange", playerUpgradeRange);
		dictionaryOfDictionaries.Add("playerUpgradeThrow", playerUpgradeThrow);
		dictionaryOfDictionaries.Add("playerUpgradeCrouchRest", playerUpgradeCrouchRest);
		dictionaryOfDictionaries.Add("playerUpgradeTumbleWings", playerUpgradeTumbleWings);
		dictionaryOfDictionaries.Add("playerInventorySpot1", playerInventorySpot1);
		dictionaryOfDictionaries.Add("playerInventorySpot2", playerInventorySpot2);
		dictionaryOfDictionaries.Add("playerInventorySpot3", playerInventorySpot3);
		dictionaryOfDictionaries.Add("playerHasCrown", playerHasCrown);
		dictionaryOfDictionaries.Add("item", item);
		dictionaryOfDictionaries.Add("itemStatBattery", itemStatBattery);
		doNotSaveTheseDictionaries.Add("playerInventorySpot1");
		doNotSaveTheseDictionaries.Add("playerInventorySpot2");
		doNotSaveTheseDictionaries.Add("playerInventorySpot3");
		doNotSaveTheseDictionaries.Add("playerColor");
		stripTheseDictionaries.Add("itemsPurchased");
		stripTheseDictionaries.Add("itemsPurchasedTotal");
		stripTheseDictionaries.Add("itemsUpgradesPurchased");
		stripTheseDictionaries.Add("itemBatteryUpgrades");
		stripTheseDictionaries.Add("playerUpgradeHealth");
		stripTheseDictionaries.Add("playerUpgradeStamina");
		stripTheseDictionaries.Add("playerUpgradeExtraJump");
		stripTheseDictionaries.Add("playerUpgradeLaunch");
		stripTheseDictionaries.Add("playerUpgradeMapPlayerCount");
		stripTheseDictionaries.Add("playerUpgradeSpeed");
		stripTheseDictionaries.Add("playerUpgradeStrength");
		stripTheseDictionaries.Add("playerUpgradeRange");
		stripTheseDictionaries.Add("playerUpgradeThrow");
		stripTheseDictionaries.Add("playerUpgradeCrouchRest");
		stripTheseDictionaries.Add("playerUpgradeTumbleWings");
		stripTheseDictionaries.Add("playerUpgradeTumbleClimb");
		stripTheseDictionaries.Add("playerUpgradeDeathHeadBattery");
		stripTheseDictionaries.Add("playerHasCrown");
		RunStartStats();
	}

	public void DictionaryFill(string dictionaryName, int value)
	{
		foreach (string item in new List<string>(dictionaryOfDictionaries[dictionaryName].Keys))
		{
			dictionaryOfDictionaries[dictionaryName][item] = value;
		}
	}

	public void PlayerAdd(string _steamID, string _playerName)
	{
		SetPlayerHealthStart(_steamID, 100);
		PlayerInventorySpotsInit(_steamID);
		PlayerAddName(_steamID, _playerName);
		foreach (Dictionary<string, int> item in AllDictionariesWithPrefix("player"))
		{
			if (!item.ContainsKey(_steamID))
			{
				item.Add(_steamID, 0);
			}
		}
		if (!playerColor.ContainsKey(_steamID))
		{
			playerColor[_steamID] = -1;
		}
	}

	private void PlayerInventorySpotsInit(string _steamID)
	{
		if (!playerInventorySpot1.ContainsKey(_steamID))
		{
			playerInventorySpot1.Add(_steamID, -1);
		}
		if (!playerInventorySpot2.ContainsKey(_steamID))
		{
			playerInventorySpot2.Add(_steamID, -1);
		}
		if (!playerInventorySpot3.ContainsKey(_steamID))
		{
			playerInventorySpot3.Add(_steamID, -1);
		}
		if (!playerInventorySpot1Taken.ContainsKey(_steamID))
		{
			playerInventorySpot1Taken.Add(_steamID, 0);
		}
		if (!playerInventorySpot2Taken.ContainsKey(_steamID))
		{
			playerInventorySpot2Taken.Add(_steamID, 0);
		}
		if (!playerInventorySpot3Taken.ContainsKey(_steamID))
		{
			playerInventorySpot3Taken.Add(_steamID, 0);
		}
	}

	public List<string> SaveFileGetPlayerNames(string folderName, string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			fileName = folderName;
		}
		string text = Application.persistentDataPath + "/saves/" + folderName + "/" + fileName + ".es3";
		if (File.Exists(text))
		{
			ES3Settings settings = new ES3Settings(text, ES3.EncryptionType.AES, totallyNormalString);
			if (ES3.KeyExists("playerNames", settings))
			{
				Dictionary<string, string> dictionary = ES3.Load<Dictionary<string, string>>("playerNames", settings);
				List<string> list = new List<string>();
				{
					foreach (KeyValuePair<string, string> item in dictionary)
					{
						list.Add(item.Value);
					}
					return list;
				}
			}
			Debug.LogWarning("Key 'playerNames' not found in save file '" + fileName + "'.");
		}
		else
		{
			Debug.LogWarning("Save file not found in " + text);
		}
		return null;
	}

	public float SaveFileGetTimePlayed(string _folderName, string _fileName = null)
	{
		if (string.IsNullOrWhiteSpace(_fileName))
		{
			_fileName = _folderName;
		}
		string text = Application.persistentDataPath + "/saves/" + _folderName + "/" + _fileName + ".es3";
		if (File.Exists(text))
		{
			ES3Settings settings = new ES3Settings(text, ES3.EncryptionType.AES, totallyNormalString);
			if (ES3.KeyExists("timePlayed", settings))
			{
				return ES3.Load<float>("timePlayed", settings);
			}
			Debug.LogWarning("Key 'timePlayed' not found in save file '" + _fileName + "'.");
		}
		else
		{
			Debug.LogWarning("Save file not found in " + text);
		}
		return 0f;
	}

	public List<string> SaveFileGetBackups(string folderName, string rootPath)
	{
		List<string> result = new List<string>();
		string path = rootPath + "/" + folderName;
		if (Directory.Exists(path))
		{
			result = (from backup in (from backup in Directory.GetFiles(path)
					where Path.GetExtension(backup) == ".es3"
					select backup).Select(Path.GetFileNameWithoutExtension)
				where backup.StartsWith(folderName + "_BACKUP")
				select backup).OrderByDescending(delegate(string backup)
			{
				Match match = Regex.Match(backup, "_BACKUP(\\d+)$");
				return match.Success ? int.Parse(match.Groups[1].Value) : 0;
			}).ToList();
		}
		return result;
	}

	public void SaveFileCreate()
	{
		backupIndex = 0;
		SaveGame(saveFileCurrent = "REPO_SAVE_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", CultureInfo.InvariantCulture));
	}

	public void SaveFileSave()
	{
		SaveGame(saveFileCurrent);
	}

	public string SaveFileGetTeamName(string folderName, string fileName = null)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			fileName = folderName;
		}
		string text = Application.persistentDataPath + "/saves/" + folderName + "/" + fileName + ".es3";
		if (File.Exists(text))
		{
			ES3Settings settings = new ES3Settings(text, ES3.EncryptionType.AES, totallyNormalString);
			if (ES3.KeyExists("teamName", settings))
			{
				return ES3.Load<string>("teamName", settings);
			}
			Debug.LogWarning("Key 'teamName' not found in save file '" + fileName + "'.");
		}
		else
		{
			Debug.LogWarning("Save file not found in " + text);
		}
		return null;
	}

	public string SaveFileGetDateAndTime(string folderName, string fileName = null)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			fileName = folderName;
		}
		string text = Application.persistentDataPath + "/saves/" + folderName + "/" + fileName + ".es3";
		if (File.Exists(text))
		{
			ES3Settings settings = new ES3Settings(text, ES3.EncryptionType.AES, totallyNormalString);
			if (ES3.KeyExists("dateAndTime", settings))
			{
				return ES3.Load<string>("dateAndTime", settings);
			}
			Debug.LogWarning("Key 'dateAndTime' not found in save file '" + fileName + "'.");
		}
		else
		{
			Debug.LogWarning("Save file not found in " + text);
		}
		return null;
	}

	private string SaveFileGetRunStat(string folderName, string fileName, string _runStat)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			fileName = folderName;
		}
		string text = Application.persistentDataPath + "/saves/" + folderName + "/" + fileName + ".es3";
		if (File.Exists(text))
		{
			ES3Settings settings = new ES3Settings(text, ES3.EncryptionType.AES, totallyNormalString);
			try
			{
				if (!ES3.KeyExists("dictionaryOfDictionaries", settings))
				{
					Debug.LogWarning("Key 'dictionaryOfDictionaries' not found in save file '" + fileName + "'.");
					return null;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Key 'dictionaryOfDictionaries' not found in save file '" + fileName + "': " + ex.Message);
				return null;
			}
			Dictionary<string, Dictionary<string, int>> dictionary = ES3.Load("dictionaryOfDictionaries", null, new Dictionary<string, Dictionary<string, int>>(), settings);
			if (dictionary.ContainsKey("runStats"))
			{
				Dictionary<string, int> dictionary2 = dictionary["runStats"];
				if (dictionary2 != null && dictionary2.ContainsKey(_runStat))
				{
					return dictionary2[_runStat].ToString();
				}
				Debug.LogWarning("Key '" + _runStat + "' not found in 'runStats' in save file '" + fileName + "'.");
			}
			else
			{
				Debug.LogWarning("Key 'runStats' not found in save file '" + fileName + "'.");
			}
		}
		else
		{
			Debug.LogWarning("Save file not found in " + text);
		}
		return null;
	}

	public string SaveFileGetRunLevel(string folderName, string fileName = null)
	{
		return SaveFileGetRunStat(folderName, fileName, "level");
	}

	public string SaveFileGetRunCurrency(string folderName, string fileName = null)
	{
		return SaveFileGetRunStat(folderName, fileName, "currency");
	}

	public string SaveFileGetTotalHaul(string folderName, string fileName = null)
	{
		return SaveFileGetRunStat(folderName, fileName, "totalHaul");
	}

	private void RunStartStats()
	{
		runStats.Clear();
		runStats.Add("level", 0);
		runStats.Add("currency", 0);
		runStats.Add("lives", 3);
		runStats.Add("chargingStationCharge", 1);
		runStats.Add("chargingStationChargeTotal", 100);
		runStats.Add("totalHaul", 0);
		statsSynced = true;
		LoadItemsFromFolder();
		DictionaryFill("itemsPurchased", 0);
		DictionaryFill("itemsPurchasedTotal", 0);
		DictionaryFill("itemsUpgradesPurchased", 0);
		itemsPurchased["Item Power Crystal"] = 1;
		itemsPurchasedTotal["Item Power Crystal"] = 1;
		itemsPurchased["Item Cart Medium"] = 1;
		itemsPurchasedTotal["Item Cart Medium"] = 1;
		playerColorIndex = 0;
	}

	private void PlayerAddName(string _steamID, string _playerName)
	{
		if (playerNames.ContainsKey(_steamID))
		{
			playerNames[_steamID] = _playerName;
		}
		else
		{
			playerNames.Add(_steamID, _playerName);
		}
	}

	public Dictionary<string, int> FetchPlayerUpgrades(string _steamID)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		Regex regex = new Regex("(?<!^)(?=[A-Z])");
		foreach (KeyValuePair<string, Dictionary<string, int>> dictionaryOfDictionary in dictionaryOfDictionaries)
		{
			if (!dictionaryOfDictionary.Key.StartsWith("playerUpgrade") || !dictionaryOfDictionary.Value.ContainsKey(_steamID))
			{
				continue;
			}
			string text = "";
			string[] array = regex.Split(dictionaryOfDictionary.Key);
			bool flag = false;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (flag)
				{
					text = text + text2 + " ";
				}
				if (text2 == "Upgrade")
				{
					flag = true;
				}
			}
			text = text.Trim();
			int value = dictionaryOfDictionary.Value[_steamID];
			dictionary.Add(text, value);
		}
		return dictionary;
	}

	public void DictionaryUpdateValue(string dictionaryName, string key, int value)
	{
		if (dictionaryOfDictionaries.ContainsKey(dictionaryName) && dictionaryOfDictionaries[dictionaryName].ContainsKey(key))
		{
			dictionaryOfDictionaries[dictionaryName][key] = value;
		}
	}

	public void ItemUpdateStatBattery(string itemName, int value, bool sync = true)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && itemStatBattery.ContainsKey(itemName))
		{
			itemStatBattery[itemName] = value;
			if (sync)
			{
				PunManager.instance.UpdateStat("itemStatBattery", itemName, value);
			}
		}
	}

	public void PlayerInventoryUpdate(string _steamID, string itemName, int spot, bool sync = true)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		int value = itemName.GetHashCode();
		if (itemName == "")
		{
			value = -1;
		}
		if (spot == 0)
		{
			playerInventorySpot1[_steamID] = value;
			if (playerInventorySpot1[_steamID] != -1)
			{
				playerInventorySpot1Taken[_steamID] = 1;
			}
			else
			{
				playerInventorySpot1Taken[_steamID] = 0;
			}
			if (sync)
			{
				PunManager.instance.UpdateStat("playerInventorySpot1", itemName, spot);
			}
		}
		if (spot == 1)
		{
			playerInventorySpot2[_steamID] = value;
			if (playerInventorySpot2[_steamID] != -1)
			{
				playerInventorySpot2Taken[_steamID] = 1;
			}
			else
			{
				playerInventorySpot2Taken[_steamID] = 0;
			}
			if (sync)
			{
				PunManager.instance.UpdateStat("playerInventorySpot2", itemName, spot);
			}
		}
		if (spot == 2)
		{
			playerInventorySpot3[_steamID] = value;
			if (playerInventorySpot3[_steamID] != -1)
			{
				playerInventorySpot3Taken[_steamID] = 1;
			}
			else
			{
				playerInventorySpot3Taken[_steamID] = 0;
			}
			if (sync)
			{
				PunManager.instance.UpdateStat("playerInventorySpot3", itemName, spot);
			}
		}
	}

	public void ItemFetchName(string itemName, ItemAttributes itemAttributes, int photonViewID)
	{
		string text = itemName;
		bool flag = false;
		foreach (string key in item.Keys)
		{
			if (key.Contains('/') && key.Split('/')[0] == itemName && !takenItemNames.Contains(key))
			{
				text = key;
				takenItemNames.Add(key);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			ItemAdd(itemName, itemAttributes, photonViewID);
		}
		else
		{
			PunManager.instance.SetItemName(text, itemAttributes, photonViewID);
		}
	}

	public void StuffNeedingResetAtTheEndOfAScene()
	{
		takenItemNames.Clear();
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			playerInventorySpot1Taken[player.steamID] = 0;
			playerInventorySpot2Taken[player.steamID] = 0;
			playerInventorySpot3Taken[player.steamID] = 0;
		}
	}

	public int GetIndexThatHoldsThisItemFromItemDictionary(string itemName)
	{
		int num = 0;
		foreach (string key in itemDictionary.Keys)
		{
			if (key == itemName)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public string GetItemNameFromIndexInItemDictionary(int index)
	{
		if (index >= 0 && index < itemDictionary.Count)
		{
			return itemDictionary.Keys.ElementAt(index);
		}
		return null;
	}

	public void ItemAdd(string itemName, ItemAttributes itemAttributes = null, int photonViewID = -1)
	{
		int num = 1;
		foreach (string key in item.Keys)
		{
			if (!key.Contains('/'))
			{
				continue;
			}
			string[] array = key.Split('/');
			if (array[0] == itemName)
			{
				int num2 = int.Parse(array[1]);
				if (num2 >= num)
				{
					num = num2 + 1;
				}
			}
		}
		int indexThatHoldsThisItemFromItemDictionary = GetIndexThatHoldsThisItemFromItemDictionary(itemName);
		itemName = itemName + "/" + num;
		AddingItem(itemName, indexThatHoldsThisItemFromItemDictionary, photonViewID, itemAttributes);
	}

	private void AddingItem(string itemName, int index, int photonViewID, ItemAttributes itemAttributes)
	{
		PunManager.instance.AddingItem(itemName, index, photonViewID, itemAttributes);
	}

	public void ItemRemove(string instanceName)
	{
		string text = instanceName.Split('/')[0];
		if (item.ContainsKey(instanceName))
		{
			itemsPurchased[text]--;
			itemsPurchased[text] = Mathf.Max(0, itemsPurchased[text]);
		}
		else
		{
			Debug.LogError("Item " + text + " not found in item dictionary");
		}
		if (item.ContainsKey(instanceName))
		{
			item.Remove(instanceName);
			itemStatBattery.Remove(instanceName);
		}
	}

	public void ItemPurchase(string itemName)
	{
		itemsPurchased[itemName]++;
		itemsPurchasedTotal[itemName]++;
		if (itemDictionary[itemName].physicalItem)
		{
			ItemAdd(itemName);
		}
	}

	private List<Dictionary<string, int>> AllDictionariesWithPrefix(string prefix)
	{
		List<Dictionary<string, int>> list = new List<Dictionary<string, int>>();
		foreach (KeyValuePair<string, Dictionary<string, int>> dictionaryOfDictionary in dictionaryOfDictionaries)
		{
			if (dictionaryOfDictionary.Key.StartsWith(prefix) && dictionaryOfDictionary.Value != null)
			{
				list.Add(dictionaryOfDictionary.Value);
			}
		}
		return list;
	}

	public int GetBatteryLevel(string itemName)
	{
		return itemStatBattery[itemName];
	}

	public void SetBatteryLevel(string itemName, int value)
	{
		itemStatBattery[itemName] = value;
	}

	public int GetItemPurchased(Item _item)
	{
		return itemsPurchased[_item.name];
	}

	public void SetItemPurchase(Item _item, int value)
	{
		itemsPurchased[_item.name] = value;
	}

	public int GetItemsUpgradesPurchased(string itemName)
	{
		return itemsUpgradesPurchased[itemName];
	}

	public int GetItemsUpgradesPurchasedTotal(string itemName)
	{
		return itemsPurchasedTotal[itemName];
	}

	public void SetItemsUpgradesPurchased(string itemName, int value)
	{
		itemsUpgradesPurchased[itemName] = value;
	}

	public void AddItemsUpgradesPurchased(string itemName)
	{
		itemsUpgradesPurchased[itemName]++;
	}

	public void SetPlayerColor(string _steamID, int _colorIndex = -1)
	{
		if (_colorIndex != -1)
		{
			playerColor[_steamID] = _colorIndex;
		}
		else if (playerColor[_steamID] == -1)
		{
			playerColor[_steamID] = playerColorIndex;
			playerColorIndex++;
		}
	}

	public int GetPlayerColor(string _steamID)
	{
		return playerColor[_steamID];
	}

	public void SetPlayerHealthStart(string _steamID, int health)
	{
		if (!playerHealth.ContainsKey(_steamID))
		{
			playerHealth[_steamID] = health;
		}
	}

	public void SetPlayerHealth(string _steamID, int health, bool setInShop)
	{
		if ((!SemiFunc.RunIsShop() && !RunManager.instance.levelIsShop) || setInShop)
		{
			playerHealth[_steamID] = health;
		}
	}

	public int GetPlayerHealth(string _steamID)
	{
		if (!playerHealth.ContainsKey(_steamID))
		{
			return 0;
		}
		return playerHealth[_steamID];
	}

	public int GetPlayerMaxHealth(string _steamID)
	{
		if (!playerUpgradeHealth.ContainsKey(_steamID))
		{
			return 0;
		}
		return playerUpgradeHealth[_steamID] * 20;
	}

	public int GetRunStatCurrency()
	{
		return runStats["currency"];
	}

	public int GetRunStatLives()
	{
		return runStats["lives"];
	}

	public int GetRunStatLevel()
	{
		return runStats["level"];
	}

	public int GetRunStatSaveLevel()
	{
		int result = 0;
		if (runStats.ContainsKey("save level"))
		{
			result = runStats["save level"];
		}
		return result;
	}

	public int GetRunStatTotalHaul()
	{
		return runStats["totalHaul"];
	}

	private void DebugSync()
	{
		int num = 0;
		foreach (KeyValuePair<string, Dictionary<string, int>> dictionaryOfDictionary in dictionaryOfDictionaries)
		{
			foreach (string item in new List<string>(dictionaryOfDictionary.Value.Keys))
			{
				dictionaryOfDictionary.Value[item] = 1;
				num++;
			}
		}
		SemiFunc.StatSyncAll();
	}

	public void ResetAllStats()
	{
		saveFileReady = false;
		ItemManager.instance.ResetAllItems();
		foreach (KeyValuePair<string, Dictionary<string, int>> dictionaryOfDictionary in dictionaryOfDictionaries)
		{
			dictionaryOfDictionary.Value.Clear();
		}
		takenItemNames.Clear();
		runStats.Clear();
		playerNames.Clear();
		timePlayed = 0f;
		RunStartStats();
	}

	private void LoadItemsFromFolder()
	{
		Item[] array = Resources.LoadAll<Item>(folderPath);
		foreach (Item item in array)
		{
			if (!string.IsNullOrEmpty(item.name))
			{
				if (!itemDictionary.ContainsKey(item.name))
				{
					itemDictionary.Add(item.name, item);
				}
				foreach (Dictionary<string, int> item2 in AllDictionariesWithPrefix("item"))
				{
					item2.Add(item.name, 0);
				}
			}
			else
			{
				Debug.LogWarning("Item with empty or null itemName found and will be skipped.");
			}
		}
	}

	public void EmptyAllBatteries()
	{
		foreach (string item in new List<string>(itemStatBattery.Keys))
		{
			itemStatBattery[item] = 0;
		}
	}

	public void BuyAllItems()
	{
		foreach (string item in new List<string>(itemDictionary.Keys))
		{
			ItemPurchase(item);
		}
	}

	private void ManualEntry()
	{
		List<KeyValuePair<string, Item>> list = new List<KeyValuePair<string, Item>>();
		foreach (KeyValuePair<string, Item> item in itemDictionary)
		{
			string text = item.Value.name;
			if (!string.IsNullOrEmpty(text))
			{
				list.Add(new KeyValuePair<string, Item>(text, item.Value));
			}
			else
			{
				Debug.LogWarning("Item with empty or null name found and will be skipped.");
			}
		}
		itemDictionary.Clear();
		foreach (KeyValuePair<string, Item> item2 in list)
		{
			if (!itemDictionary.ContainsKey(item2.Key))
			{
				itemDictionary.Add(item2.Key, item2.Value);
			}
			else
			{
				Debug.LogWarning("Duplicate key found: " + item2.Key + ". This entry will be skipped.");
			}
		}
	}

	public async Task<List<SaveFolder>> SaveFileGetAllAsync()
	{
		string savesPath = Application.persistentDataPath + "/saves";
		return await Task.Run(delegate
		{
			List<SaveFolder> list = new List<SaveFolder>();
			if (Directory.Exists(savesPath))
			{
				foreach (string item in Directory.GetDirectories(savesPath).OrderByDescending(Directory.GetCreationTime).Select(Path.GetFileName)
					.ToList())
				{
					SaveFolder saveFolder = new SaveFolder
					{
						name = item,
						isValid = File.Exists(savesPath + "/" + item + "/" + item + ".es3")
					};
					saveFolder.backups = SaveFileGetBackups(item, savesPath);
					if (saveFolder.isValid || saveFolder.backups.Count != 0)
					{
						list.Add(saveFolder);
					}
				}
			}
			else
			{
				Debug.LogWarning("Saves directory not found at " + savesPath);
			}
			return list;
		});
	}

	public void SaveFileDelete(string saveFileName)
	{
		string text = Application.persistentDataPath + "/saves/" + saveFileName;
		if (Directory.Exists(text))
		{
			int num = 0;
			string[] files = Directory.GetFiles(text);
			string[] array = files;
			foreach (string path in array)
			{
				try
				{
					File.Delete(path);
					num++;
				}
				catch (Exception ex)
				{
					Debug.LogError("Failed to delete file '" + Path.GetFileName(path) + "' for '" + saveFileName + "': " + ex.Message);
				}
			}
			string[] directories = Directory.GetDirectories(text);
			array = directories;
			foreach (string path2 in array)
			{
				try
				{
					Directory.Delete(path2, recursive: true);
					num++;
				}
				catch (Exception ex2)
				{
					Debug.LogError("Failed to delete directory '" + Path.GetDirectoryName(path2) + "' for '" + saveFileName + "': " + ex2.Message);
				}
			}
			try
			{
				Directory.Delete(text);
				num++;
			}
			catch (Exception ex3)
			{
				Debug.LogError("Failed to delete directory for '" + saveFileName + "': " + ex3.Message);
			}
			Debug.Log($"Deleted {num}/{files.Length + directories.Length + 1} files and directories for '{saveFileName}'");
		}
		else if (!string.IsNullOrEmpty(saveFileName))
		{
			Debug.LogWarning("Save folder not found: " + text);
		}
	}

	public void SaveGame(string fileName)
	{
		if (string.IsNullOrEmpty(fileName) || !SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		dateAndTime = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		string text = Application.persistentDataPath + "/saves";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = text + "/" + fileName;
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
		}
		string text3 = text2 + "/" + fileName + ".es3";
		if (File.Exists(text3))
		{
			backupIndex++;
			string destFileName = text2 + "/" + fileName + "_BACKUP" + backupIndex + ".es3";
			File.Move(text3, destFileName);
		}
		ES3Settings eS3Settings = new ES3Settings(ES3.Location.Cache);
		eS3Settings.encryptionType = ES3.EncryptionType.AES;
		eS3Settings.encryptionPassword = totallyNormalString;
		eS3Settings.path = text3;
		ES3.Save("teamName", teamName, eS3Settings);
		ES3.Save("dateAndTime", dateAndTime, eS3Settings);
		ES3.Save("timePlayed", timePlayed, eS3Settings);
		ES3.Save("playerNames", playerNames, eS3Settings);
		Dictionary<string, Dictionary<string, int>> dictionary = new Dictionary<string, Dictionary<string, int>>();
		foreach (KeyValuePair<string, Dictionary<string, int>> dictionaryOfDictionary in dictionaryOfDictionaries)
		{
			if (doNotSaveTheseDictionaries.Contains(dictionaryOfDictionary.Key))
			{
				continue;
			}
			if (stripTheseDictionaries.Contains(dictionaryOfDictionary.Key))
			{
				dictionary[dictionaryOfDictionary.Key] = dictionaryOfDictionary.Value.Where((KeyValuePair<string, int> x) => x.Value != 0).ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
			}
			else
			{
				dictionary[dictionaryOfDictionary.Key] = dictionaryOfDictionary.Value;
			}
		}
		ES3.Save("dictionaryOfDictionaries", dictionary, eS3Settings);
		ES3.StoreCachedFile(eS3Settings);
		PlayersAddAll();
		saveFileReady = true;
	}

	private void PlayersAddAll()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			PlayerAdd(player.steamID, player.playerName);
			SetPlayerColor(player.steamID);
		}
	}

	public void LoadGame(string fileName, List<string> backupList)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		string text = Application.persistentDataPath + "/saves/" + fileName + "/" + fileName + ".es3";
		if (File.Exists(text))
		{
			saveFileCurrent = fileName;
			ES3Settings settings = new ES3Settings(text, ES3.EncryptionType.AES, totallyNormalString);
			teamName = ES3.Load("teamName", null, "R.E.P.O.", settings);
			dateAndTime = ES3.Load("dateAndTime", null, DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), settings);
			timePlayed = ES3.Load("timePlayed", null, 0f, settings);
			playerNames = ES3.Load("playerNames", null, new Dictionary<string, string>(), settings);
			foreach (KeyValuePair<string, Dictionary<string, int>> item in ES3.Load("dictionaryOfDictionaries", null, new Dictionary<string, Dictionary<string, int>>(), settings))
			{
				if (dictionaryOfDictionaries.TryGetValue(item.Key, out var value))
				{
					foreach (KeyValuePair<string, int> item2 in item.Value)
					{
						value[item2.Key] = item2.Value;
					}
				}
				else
				{
					dictionaryOfDictionaries.Add(item.Key, new Dictionary<string, int>(item.Value));
				}
			}
		}
		else
		{
			Debug.LogWarning("Save file not found in " + text);
		}
		RunManager.instance.levelsCompleted = GetRunStatLevel();
		RunManager.instance.runLives = GetRunStatLives();
		RunManager.instance.loadLevel = GetRunStatSaveLevel();
		PlayersAddAll();
		if (backupList == null)
		{
			backupList = SaveFileGetBackups(fileName, Application.persistentDataPath + "/saves");
		}
		if (backupList.Count > 0)
		{
			Match match = Regex.Match(backupList[0], "_BACKUP(\\d+)$");
			backupIndex = (match.Success ? int.Parse(match.Groups[1].Value) : 0);
		}
		else
		{
			backupIndex = 0;
		}
		saveFileReady = true;
	}

	public void UpdateCrown(string steamID)
	{
		foreach (string item in new List<string>(playerHasCrown.Keys))
		{
			playerHasCrown[item] = 0;
		}
		if (playerHasCrown.ContainsKey(steamID))
		{
			playerHasCrown[steamID] = 1;
		}
	}
}
