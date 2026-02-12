using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class DataDirector : MonoBehaviour
{
	public enum Setting
	{
		MusicVolume,
		SfxVolume,
		AmbienceVolume,
		MicDevice,
		ProximityVoice,
		Resolution,
		Fullscreen,
		MicVolume,
		TextToSpeechVolume,
		CameraShake,
		CameraAnimation,
		Tips,
		Vsync,
		MasterVolume,
		CameraSmoothing,
		LightDistance,
		Bloom,
		LensEffect,
		MotionBlur,
		MaxFPS,
		ShadowQuality,
		ShadowDistance,
		ChromaticAberration,
		Grain,
		WindowMode,
		RenderSize,
		GlitchLoop,
		AimSensitivity,
		CameraNoise,
		Gamma,
		PlayerNames,
		RunsPlayed,
		PushToTalk,
		TutorialPlayed,
		TutorialJumping,
		TutorialSprinting,
		TutorialSneaking,
		TutorialHiding,
		TutorialTumbling,
		TutorialPushingAndPulling,
		TutorialRotating,
		TutorialReviving,
		TutorialHealing,
		TutorialCartHandling,
		TutorialItemToggling,
		TutorialInventoryFill,
		TutorialMap,
		TutorialChargingStation,
		TutorialOnlyOneExtraction,
		TutorialChat,
		TutorialFinalExtraction,
		TutorialMultipleExtractions,
		TutorialShop,
		TutorialExpressions,
		TutorialOvercharge1,
		TutorialOvercharge2,
		AimInvertVertical,
		Photosensitivity,
		SplashScreenCount,
		RichPresence,
		ItemUnequipAutoHold,
		TutorialHeadSpectate,
		TutorialEnemyElsa,
		Arachnophobia
	}

	public enum SettingType
	{
		Audio,
		Gameplay,
		Graphics,
		None
	}

	public static DataDirector instance;

	private string playerBodyColor = "0";

	internal string micDevice = "";

	private Dictionary<Setting, string> settingsName = new Dictionary<Setting, string>();

	private Dictionary<Setting, int> settingsValue = new Dictionary<Setting, int>();

	private Dictionary<Setting, int> defaultSettingsValue = new Dictionary<Setting, int>();

	private Dictionary<SettingType, List<Setting>> settings = new Dictionary<SettingType, List<Setting>>();

	internal string networkPassword;

	internal string networkRegion;

	internal string networkServerName;

	internal string networkJoinServerName;

	public const string SERVER_NAME_PROP_KEY = "server_name";

	public TypedLobby customLobby = new TypedLobby("custom", LobbyType.Default);

	public TypedLobby privateLobby = new TypedLobby("private", LobbyType.Default);

	internal int aimSensitivityDefault;

	internal int cameraSmoothingDefault;

	internal bool toggleMute;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			InitializeSettings();
			LoadSettings();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.InputDown(InputKey.ToggleMute))
			{
				toggleMute = !toggleMute;
			}
		}
		else
		{
			toggleMute = false;
		}
	}

	private void InitializeSettings()
	{
		SettingAdd(SettingType.Audio, Setting.MasterVolume, "Master Volume", 75);
		SettingAdd(SettingType.Audio, Setting.MusicVolume, "Music Volume", 75);
		SettingAdd(SettingType.Audio, Setting.SfxVolume, "Sfx Volume", 75);
		SettingAdd(SettingType.Audio, Setting.ProximityVoice, "Proximity Voice Volume", 75);
		SettingAdd(SettingType.Audio, Setting.TextToSpeechVolume, "Text to Speech Volume", 75);
		SettingAdd(SettingType.Audio, Setting.MicDevice, "Microphone", 1);
		SettingAdd(SettingType.Audio, Setting.MicVolume, "Microphone Volume", 100);
		SettingAdd(SettingType.Audio, Setting.PushToTalk, "Push to Talk", 0);
		SettingAdd(SettingType.Graphics, Setting.Resolution, "Resolution", 0);
		SettingAdd(SettingType.Graphics, Setting.Fullscreen, "Fullscreen", 0);
		SettingAdd(SettingType.Graphics, Setting.LightDistance, "Light Distance", 3);
		SettingAdd(SettingType.Graphics, Setting.Vsync, "Vsync", 0);
		SettingAdd(SettingType.Graphics, Setting.Bloom, "Bloom", 1);
		SettingAdd(SettingType.Graphics, Setting.ChromaticAberration, "Chromatic Aberration", 1);
		SettingAdd(SettingType.Graphics, Setting.Grain, "Grain", 1);
		SettingAdd(SettingType.Graphics, Setting.MotionBlur, "Motion Blur", 1);
		SettingAdd(SettingType.Graphics, Setting.LensEffect, "Lens Effect", 1);
		SettingAdd(SettingType.Graphics, Setting.GlitchLoop, "Glitch Loop", 1);
		SettingAdd(SettingType.Graphics, Setting.MaxFPS, "Max FPS", 144);
		SettingAdd(SettingType.Graphics, Setting.ShadowQuality, "Shadow Quality", 2);
		SettingAdd(SettingType.Graphics, Setting.ShadowDistance, "Shadow Distance", 3);
		SettingAdd(SettingType.Graphics, Setting.WindowMode, "Window Mode", 0);
		SettingAdd(SettingType.Graphics, Setting.RenderSize, "Pixelation", 2);
		SettingAdd(SettingType.Graphics, Setting.Gamma, "Gamma", 40);
		SettingAdd(SettingType.Gameplay, Setting.Tips, "Tips", 1);
		aimSensitivityDefault = 35;
		SettingAdd(SettingType.Gameplay, Setting.AimSensitivity, "Aim Sensitivity", aimSensitivityDefault);
		cameraSmoothingDefault = 80;
		SettingAdd(SettingType.Gameplay, Setting.CameraSmoothing, "Camera Smoothing", cameraSmoothingDefault);
		SettingAdd(SettingType.Gameplay, Setting.CameraShake, "Camera Shake", 100);
		SettingAdd(SettingType.Gameplay, Setting.CameraNoise, "Camera Noise", 100);
		SettingAdd(SettingType.Gameplay, Setting.CameraAnimation, "Camera Animation", 4);
		SettingAdd(SettingType.Gameplay, Setting.PlayerNames, "Player Names", 1);
		SettingAdd(SettingType.Gameplay, Setting.AimInvertVertical, "Aim Invert Vertical", 0);
		SettingAdd(SettingType.Gameplay, Setting.Photosensitivity, "Photosensitivity", 0);
		SettingAdd(SettingType.Gameplay, Setting.Arachnophobia, "Arachnophobia", 0);
		SettingAdd(SettingType.Gameplay, Setting.RichPresence, "Rich Presence", 2);
		SettingAdd(SettingType.Gameplay, Setting.ItemUnequipAutoHold, "Item Unequip Auto Hold", 1);
		SettingAdd(SettingType.None, Setting.RunsPlayed, "Runs Played", 0);
		SettingAdd(SettingType.None, Setting.TutorialPlayed, "Tutorial Played", 0);
		SettingAdd(SettingType.None, Setting.SplashScreenCount, "Splash Screen Count", 0);
		SettingAdd(SettingType.None, Setting.TutorialJumping, "Tutorial Jumping", 0);
		SettingAdd(SettingType.None, Setting.TutorialSprinting, "Tutorial Sprinting", 0);
		SettingAdd(SettingType.None, Setting.TutorialSneaking, "Tutorial Sneaking", 0);
		SettingAdd(SettingType.None, Setting.TutorialHiding, "Tutorial Hiding", 0);
		SettingAdd(SettingType.None, Setting.TutorialTumbling, "Tutorial Tumbling", 0);
		SettingAdd(SettingType.None, Setting.TutorialPushingAndPulling, "Tutorial Pushing and Pulling", 0);
		SettingAdd(SettingType.None, Setting.TutorialRotating, "Tutorial Rotating", 0);
		SettingAdd(SettingType.None, Setting.TutorialReviving, "Tutorial Reviving", 0);
		SettingAdd(SettingType.None, Setting.TutorialHealing, "Tutorial Healing", 0);
		SettingAdd(SettingType.None, Setting.TutorialCartHandling, "Tutorial Cart Handling", 0);
		SettingAdd(SettingType.None, Setting.TutorialItemToggling, "Tutorial Item Toggling", 0);
		SettingAdd(SettingType.None, Setting.TutorialInventoryFill, "Tutorial Inventory Fill", 0);
		SettingAdd(SettingType.None, Setting.TutorialMap, "Tutorial Map", 0);
		SettingAdd(SettingType.None, Setting.TutorialChargingStation, "Tutorial Charging Station", 0);
		SettingAdd(SettingType.None, Setting.TutorialOnlyOneExtraction, "Tutorial Only One Extraction", 0);
		SettingAdd(SettingType.None, Setting.TutorialChat, "Tutorial Chat", 0);
		SettingAdd(SettingType.None, Setting.TutorialFinalExtraction, "Tutorial Final Extraction", 0);
		SettingAdd(SettingType.None, Setting.TutorialMultipleExtractions, "Tutorial Multiple Extractions", 0);
		SettingAdd(SettingType.None, Setting.TutorialShop, "Tutorial Shop", 0);
		SettingAdd(SettingType.None, Setting.TutorialExpressions, "Tutorial Expressions", 0);
		SettingAdd(SettingType.None, Setting.TutorialOvercharge1, "Tutorial Overcharge 1", 0);
		SettingAdd(SettingType.None, Setting.TutorialOvercharge2, "Tutorial Overcharge 2", 0);
		SettingAdd(SettingType.None, Setting.TutorialHeadSpectate, "Tutorial Head Spectate", 0);
		SettingAdd(SettingType.None, Setting.TutorialEnemyElsa, "Tutorial Enemy Elsa", 0);
	}

	private void SettingAdd(SettingType settingType, Setting setting, string _name, int value)
	{
		if (settings.ContainsKey(settingType))
		{
			settings[settingType].Add(setting);
		}
		else
		{
			settings[settingType] = new List<Setting> { setting };
		}
		if (settingsName.ContainsKey(setting))
		{
			Debug.LogError("Setting already exists: " + setting.ToString() + " " + _name);
			return;
		}
		settingsName[setting] = _name;
		settingsValue[setting] = value;
		defaultSettingsValue[setting] = value;
	}

	public int SettingValueFetch(Setting setting)
	{
		if (!settingsValue.ContainsKey(setting))
		{
			return 0;
		}
		return settingsValue[setting];
	}

	public float SettingValueFetchFloat(Setting setting)
	{
		return (float)settingsValue[setting] / 100f;
	}

	public void SettingValueSet(Setting setting, int value)
	{
		if (settingsValue.ContainsKey(setting))
		{
			settingsValue[setting] = value;
		}
		else
		{
			Debug.LogWarning("Setting not found: " + setting);
		}
	}

	public string SettingNameGet(Setting setting)
	{
		if (!settingsName.ContainsKey(setting))
		{
			return null;
		}
		return settingsName[setting];
	}

	public void SaveSettings()
	{
		SettingsSaveData settingsSaveData = new SettingsSaveData();
		settingsSaveData.settingsValue = new Dictionary<string, int>();
		foreach (KeyValuePair<Setting, int> item in settingsValue)
		{
			settingsSaveData.settingsValue[item.Key.ToString()] = item.Value;
		}
		ES3Settings eS3Settings = new ES3Settings(ES3.Location.Cache);
		eS3Settings.path = "SettingsData.es3";
		ES3.Save("Settings", settingsSaveData, eS3Settings);
		ES3.Save("PlayerBodyColor", playerBodyColor, eS3Settings);
		ES3.Save("micDevice", micDevice, eS3Settings);
		ES3.StoreCachedFile(eS3Settings);
	}

	public void ColorSetBody(int colorID)
	{
		string text = colorID.ToString();
		playerBodyColor = text;
		SaveSettings();
	}

	public int ColorGetBody()
	{
		ES3Settings eS3Settings = new ES3Settings("SettingsData.es3", ES3.Location.File);
		if (ES3.KeyExists("PlayerBodyColor", eS3Settings))
		{
			playerBodyColor = ES3.Load<string>("PlayerBodyColor", eS3Settings);
		}
		return int.Parse(playerBodyColor);
	}

	public void LoadSettings()
	{
		try
		{
			ES3Settings eS3Settings = new ES3Settings("SettingsData.es3", ES3.Location.File);
			if (ES3.FileExists(eS3Settings))
			{
				if (ES3.KeyExists("Settings", eS3Settings))
				{
					foreach (KeyValuePair<string, int> item in ES3.Load<SettingsSaveData>("Settings", eS3Settings).settingsValue)
					{
						if (Enum.TryParse<Setting>(item.Key, out var result) && settingsValue.ContainsKey(result))
						{
							settingsValue[result] = item.Value;
						}
					}
				}
				else
				{
					Debug.LogWarning("Key 'Settings' not found in file: " + eS3Settings.FullPath);
				}
				if (ES3.KeyExists("PlayerBodyColor", eS3Settings))
				{
					playerBodyColor = ES3.Load<string>("PlayerBodyColor", eS3Settings);
				}
				if (ES3.KeyExists("micDevice", eS3Settings))
				{
					micDevice = ES3.Load<string>("micDevice", eS3Settings);
				}
			}
			else
			{
				SaveSettings();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load settings: " + ex.Message);
			ES3.DeleteFile("SettingsData.es3");
			SaveSettings();
		}
	}

	public void ResetSettingToDefault(Setting setting)
	{
		if (defaultSettingsValue.ContainsKey(setting))
		{
			settingsValue[setting] = defaultSettingsValue[setting];
		}
		else
		{
			Debug.LogWarning("Default value not found for setting: " + setting);
		}
	}

	public void ResetSettingTypeToDefault(SettingType settingType)
	{
		if (settings.ContainsKey(settingType))
		{
			foreach (Setting item in settings[settingType])
			{
				if (defaultSettingsValue.ContainsKey(item))
				{
					settingsValue[item] = defaultSettingsValue[item];
				}
			}
			return;
		}
		Debug.LogWarning("SettingType not found: " + settingType);
	}

	public void RunsPlayedAdd()
	{
		int value = SettingValueFetch(Setting.RunsPlayed) + 1;
		SettingValueSet(Setting.RunsPlayed, value);
		SaveSettings();
	}

	public void TutorialPlayed()
	{
		SettingValueSet(Setting.TutorialPlayed, 1);
		SaveSettings();
	}

	public void SaveDeleteCheck(bool _leaveGame)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !(RunManager.instance.levelPrevious == RunManager.instance.levelTutorial) && !(RunManager.instance.levelPrevious == RunManager.instance.levelMainMenu) && !(RunManager.instance.levelPrevious == RunManager.instance.levelRecording))
		{
			bool flag = false;
			if (SemiFunc.RunIsArena() || GameManager.instance.connectRandom)
			{
				flag = true;
			}
			else if (RunManager.instance.allPlayersDead && RunManager.instance.levelPrevious != RunManager.instance.levelMainMenu && RunManager.instance.levelPrevious != RunManager.instance.levelLobbyMenu && RunManager.instance.levelPrevious != RunManager.instance.levelTutorial && RunManager.instance.levelPrevious != RunManager.instance.levelLobby && RunManager.instance.levelPrevious != RunManager.instance.levelShop && RunManager.instance.levelPrevious != RunManager.instance.levelRecording)
			{
				flag = true;
			}
			else if (_leaveGame && RunManager.instance.levelsCompleted == 0)
			{
				flag = true;
			}
			if (flag)
			{
				SemiFunc.SaveFileDelete(StatsManager.instance.saveFileCurrent);
			}
		}
	}

	public void PhotonSetRegion()
	{
		Debug.Log("Photon - Set Region");
		PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = instance.networkRegion;
	}

	public void PhotonSetVersion()
	{
		Debug.Log("Photon - Set Version");
		PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = BuildManager.instance.version.title;
	}

	public void PhotonSetAppId()
	{
		Debug.Log("Photon - Set AppId");
		PhotonNetwork.ServerPortOverrides = PhotonPortDefinition.AlternativeUdpPorts;
		PhotonNetwork.ObjectsInOneUpdate = 10;
		string server = "";
		string appIdRealtime = "99515094-45b7-4c98-ab70-a448c548c83d";
		string appIdVoice = "dfc40a01-c3a4-4395-9707-b71118816d2b";
		PhotonNetwork.PhotonServerSettings.AppSettings.Server = server;
		PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = appIdRealtime;
		PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = appIdVoice;
	}
}
