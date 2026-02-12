using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
	public enum ChangeLevelType
	{
		Normal,
		RunLevel,
		Tutorial,
		LobbyMenu,
		MainMenu,
		Shop,
		Recording
	}

	public enum SaveLevel
	{
		Lobby,
		Shop
	}

	public static RunManager instance;

	internal int saveLevel;

	internal int loadLevel;

	internal Level debugLevel;

	internal bool localMultiplayerTest;

	internal bool runStarted;

	internal RunManagerPUN runManagerPUN;

	public int levelsCompleted;

	public Level levelCurrent;

	internal Level levelPrevious;

	private Level previousRunLevel;

	internal bool restarting;

	internal bool restartingDone;

	internal int levelsMax = 10;

	[Space]
	public Level levelArena;

	public Level levelLobby;

	public Level levelLobbyMenu;

	public Level levelMainMenu;

	public Level levelRecording;

	public Level levelShop;

	public Level levelSplashScreen;

	public Level levelTutorial;

	[Space]
	public List<Level> levels;

	internal int runLives = 3;

	internal bool levelFailed;

	internal bool waitToChangeScene;

	internal bool lobbyJoin;

	internal bool masterSwitched;

	internal bool gameOver;

	internal bool allPlayersDead;

	[Space]
	public List<EnemySetup> enemiesSpawned;

	private List<EnemySetup> enemiesSpawnedToDelete = new List<EnemySetup>();

	internal bool skipLoadingUI = true;

	internal Color loadingFadeColor = Color.black;

	internal float loadingAnimationTime;

	internal List<PlayerVoiceChat> voiceChats = new List<PlayerVoiceChat>();

	internal bool levelIsShop;

	private int moonLevelPrev;

	internal int moonLevel;

	internal bool moonLevelChanged;

	public List<Moon> moons = new List<Moon>();

	internal Dictionary<string, GameObject> singleplayerPool = new Dictionary<string, GameObject>();

	internal DefaultPool multiplayerPool = new DefaultPool();

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
		levelPrevious = levelCurrent;
	}

	private void Update()
	{
		if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (LevelGenerator.Instance.Generated && !SteamClient.IsValid && !SteamManager.instance.enabled)
		{
			Debug.LogError("Steam not initialized. Quitting game.");
			Application.Quit();
		}
		if (Application.isEditor)
		{
			if (Input.GetKeyDown(KeyCode.F3))
			{
				if (SemiFunc.RunIsArena())
				{
					ChangeLevel(_completedLevel: true, _levelFailed: true);
				}
				else
				{
					ChangeLevel(_completedLevel: true, _levelFailed: false);
				}
			}
			if (!restarting && SemiFunc.NoTextInputsActive() && Input.GetKeyDown(KeyCode.Backspace))
			{
				ResetProgress();
				RestartScene();
				if (levelCurrent != levelTutorial)
				{
					SemiFunc.OnSceneSwitch(gameOver, _leaveGame: false);
				}
			}
		}
		if (restarting)
		{
			RestartScene();
		}
		if (restarting || !runStarted || GameDirector.instance.PlayerList.Count <= 0 || SemiFunc.RunIsArena() || GameDirector.instance.currentState != GameDirector.gameState.Main)
		{
			return;
		}
		bool flag = true;
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (!player.isDisabled)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			allPlayersDead = true;
			if ((bool)SpectateCamera.instance && !SpectateCamera.instance.CheckState(SpectateCamera.State.Death))
			{
				ChangeLevel(_completedLevel: false, _levelFailed: true);
			}
		}
		else
		{
			allPlayersDead = false;
		}
	}

	private void OnApplicationQuit()
	{
		DataDirector.instance.SaveDeleteCheck(_leaveGame: true);
	}

	public void ChangeLevel(bool _completedLevel, bool _levelFailed, ChangeLevelType _changeLevelType = ChangeLevelType.Normal)
	{
		if ((!SemiFunc.MenuLevel() && !SemiFunc.IsMasterClientOrSingleplayer()) || restarting)
		{
			return;
		}
		gameOver = false;
		if (_levelFailed && levelCurrent != levelLobby && levelCurrent != levelShop)
		{
			if (levelCurrent == levelArena)
			{
				ResetProgress();
				if (SemiFunc.IsMultiplayer())
				{
					levelCurrent = levelLobbyMenu;
				}
				else
				{
					SetRunLevel();
				}
				gameOver = true;
			}
			else
			{
				levelCurrent = levelArena;
			}
		}
		if (!gameOver && levelCurrent != levelArena)
		{
			switch (_changeLevelType)
			{
			case ChangeLevelType.RunLevel:
				SetRunLevel();
				break;
			case ChangeLevelType.LobbyMenu:
				levelCurrent = levelLobbyMenu;
				break;
			case ChangeLevelType.MainMenu:
				levelCurrent = levelMainMenu;
				break;
			case ChangeLevelType.Tutorial:
				levelCurrent = levelTutorial;
				break;
			case ChangeLevelType.Recording:
				levelCurrent = levelRecording;
				break;
			case ChangeLevelType.Shop:
				levelCurrent = levelShop;
				break;
			default:
				if (levelCurrent == levelMainMenu || levelCurrent == levelLobbyMenu)
				{
					levelCurrent = levelLobby;
				}
				else if (_completedLevel && levelCurrent != levelLobby && levelCurrent != levelShop)
				{
					previousRunLevel = levelCurrent;
					levelsCompleted++;
					SemiFunc.StatSetRunLevel(levelsCompleted);
					SemiFunc.LevelSuccessful();
					levelCurrent = levelShop;
				}
				else if (levelCurrent == levelLobby)
				{
					SetRunLevel();
				}
				else if (levelCurrent == levelShop)
				{
					levelCurrent = levelLobby;
				}
				break;
			}
		}
		if ((bool)debugLevel && levelCurrent != levelSplashScreen && levelCurrent != levelMainMenu && levelCurrent != levelLobbyMenu)
		{
			levelCurrent = debugLevel;
		}
		if (GameManager.Multiplayer())
		{
			runManagerPUN.photonView.RPC("UpdateLevelRPC", RpcTarget.OthersBuffered, levelCurrent.name, levelsCompleted, gameOver);
		}
		Debug.Log("Changed level to: " + levelCurrent.name);
		UpdateSteamRichPresence();
		if (levelCurrent == levelShop)
		{
			saveLevel = 1;
		}
		else
		{
			saveLevel = 0;
		}
		SemiFunc.StatSetSaveLevel(saveLevel);
		RestartScene();
		if (_changeLevelType != ChangeLevelType.Tutorial)
		{
			SemiFunc.OnSceneSwitch(gameOver, _leaveGame: false);
		}
	}

	public void RestartScene()
	{
		if (!restarting)
		{
			restarting = true;
			if (!GameDirector.instance)
			{
				return;
			}
			{
				foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
				{
					player.OutroStart();
				}
				return;
			}
		}
		if (restartingDone)
		{
			return;
		}
		bool flag = true;
		if (!GameDirector.instance)
		{
			flag = false;
		}
		else
		{
			foreach (PlayerAvatar player2 in GameDirector.instance.PlayerList)
			{
				if (!player2.outroDone)
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		if (gameOver)
		{
			NetworkManager.instance.DestroyAll();
			gameOver = false;
		}
		if (lobbyJoin)
		{
			lobbyJoin = false;
			restartingDone = true;
			SceneManager.LoadSceneAsync("LobbyJoin");
		}
		else if (!waitToChangeScene)
		{
			restartingDone = true;
			if (!GameManager.Multiplayer())
			{
				SceneManager.LoadSceneAsync("Main");
			}
			else if (PhotonNetwork.IsMasterClient)
			{
				PhotonNetwork.LoadLevel("Reload");
			}
		}
	}

	public void UpdateLevel(string _levelName, int _levelsCompleted, bool _gameOver)
	{
		if ((bool)LobbyMenuOpen.instance)
		{
			DataDirector.instance.RunsPlayedAdd();
		}
		levelsCompleted = _levelsCompleted;
		SemiFunc.StatSetRunLevel(levelsCompleted);
		if (_levelName == levelLobbyMenu.name)
		{
			levelCurrent = levelLobbyMenu;
		}
		else if (_levelName == levelLobby.name)
		{
			levelCurrent = levelLobby;
		}
		else if (_levelName == levelShop.name)
		{
			levelCurrent = levelShop;
		}
		else if (_levelName == levelArena.name)
		{
			levelCurrent = levelArena;
		}
		else if (_levelName == levelRecording.name)
		{
			levelCurrent = levelRecording;
		}
		else
		{
			foreach (Level level in levels)
			{
				if (level.name == _levelName)
				{
					levelCurrent = level;
					break;
				}
			}
		}
		SemiFunc.OnSceneSwitch(_gameOver, _leaveGame: false);
		Debug.Log("updated level to: " + levelCurrent.name);
		UpdateSteamRichPresence();
	}

	public void ResetProgress()
	{
		if ((bool)StatsManager.instance)
		{
			StatsManager.instance.ResetAllStats();
		}
		levelsCompleted = 0;
		loadLevel = 0;
		UpdateMoonLevel();
	}

	public void EnemiesSpawnedRemoveStart()
	{
		enemiesSpawnedToDelete.Clear();
		foreach (EnemySetup item in enemiesSpawned)
		{
			bool flag = false;
			foreach (EnemySetup item2 in enemiesSpawnedToDelete)
			{
				if (item == item2)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				enemiesSpawnedToDelete.Add(item);
			}
		}
	}

	public void EnemiesSpawnedRemoveEnd()
	{
		foreach (EnemySetup item in enemiesSpawnedToDelete)
		{
			enemiesSpawned.Remove(item);
		}
	}

	public void SetRunLevel()
	{
		levelCurrent = previousRunLevel;
		while (levelCurrent == previousRunLevel)
		{
			levelCurrent = levels[Random.Range(0, levels.Count)];
		}
	}

	public IEnumerator LeaveToMainMenu()
	{
		while (PhotonNetwork.NetworkingClient.State != ClientState.Disconnected && PhotonNetwork.NetworkingClient.State != ClientState.PeerCreated)
		{
			yield return null;
		}
		Debug.Log("Leave to Main Menu");
		SemiFunc.OnSceneSwitch(_gameOver: false, _leaveGame: true);
		levelCurrent = levelMainMenu;
		SceneManager.LoadSceneAsync("Reload");
		UpdateSteamRichPresence();
		yield return null;
	}

	public string MoonGetName(int _moonIndex)
	{
		if (_moonIndex - 1 >= 0 && moons.Count > _moonIndex - 1)
		{
			return moons[_moonIndex - 1].moonName;
		}
		return "";
	}

	public List<string> MoonGetAttributes(int _moonIndex)
	{
		if (_moonIndex - 1 >= 0 && moons.Count > _moonIndex - 1)
		{
			return moons[_moonIndex - 1].moonAttributes;
		}
		return new List<string>();
	}

	public Texture MoonGetIcon(int _moonIndex)
	{
		if (_moonIndex - 1 >= 0 && moons.Count > _moonIndex - 1)
		{
			return moons[_moonIndex - 1].moonIcon;
		}
		return null;
	}

	public void UpdateMoonLevel()
	{
		moonLevelPrev = moonLevel;
		moonLevel = CalculateMoonLevel(levelsCompleted);
		if (moonLevel != moonLevelPrev && moons.Count >= moonLevel)
		{
			moonLevelChanged = true;
		}
	}

	public int CalculateMoonLevel(int _levelsCompleted)
	{
		return (_levelsCompleted + 1) / 5;
	}

	public void UpdateSteamRichPresence()
	{
		int num = DataDirector.instance.SettingValueFetch(DataDirector.Setting.RichPresence);
		string text = "In Menu";
		string details = levelCurrent.NarrativeName;
		if (levelCurrent == levelMainMenu || levelCurrent == levelSplashScreen || SemiFunc.RunIsTutorial() || SemiFunc.RunIsLobbyMenu())
		{
			SteamFriends.SetRichPresence("levelname", num switch
			{
				2 => levelCurrent.NarrativeName, 
				1 => text, 
				_ => null, 
			});
			SteamFriends.SetRichPresence("levelnum", null);
			SteamFriends.SetRichPresence("steam_display", "#Status_LevelName");
		}
		else
		{
			string text2 = (StatsManager.instance.GetRunStatLevel() + 1).ToString();
			text = ((num == 1) ? "In Game" : (SemiFunc.RunIsArena() ? "In Arena" : (SemiFunc.RunIsLobby() ? "In Truck" : (SemiFunc.RunIsShop() ? "In Shop" : "In Game"))));
			details = "Level " + text2 + " - " + levelCurrent.NarrativeName;
			SteamFriends.SetRichPresence("levelname", num switch
			{
				2 => levelCurrent.NarrativeName, 
				1 => text, 
				_ => null, 
			});
			SteamFriends.SetRichPresence("levelnum", (num == 2) ? text2 : null);
			SteamFriends.SetRichPresence("steam_display", (num == 2) ? "#Status_LevelNameNum" : "#Status_LevelName");
		}
		DiscordManager.instance?.UpdateDiscordRichPresence(text, details);
	}
}
