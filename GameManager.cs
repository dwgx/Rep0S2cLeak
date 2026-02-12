using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public enum GameEvents
	{
		None,
		Halloween,
		Winter
	}

	public static GameManager instance;

	public bool localTest;

	internal bool connectRandom;

	internal Dictionary<string, float> playerMicrophoneSettings = new Dictionary<string, float>();

	public int gameMode { get; private set; }

	public GameEvents currentGameEvent { get; private set; }

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			gameMode = 0;
			CheckForSeasonalEvent();
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void CheckForSeasonalEvent()
	{
		DateTime dateTime;
		try
		{
			dateTime = SteamUtils.SteamServerTime.Date;
			if (dateTime.Year < 2025)
			{
				dateTime = DateTime.Now;
			}
		}
		catch
		{
			dateTime = DateTime.Now;
		}
		if ((dateTime.Month == 10 && dateTime.Day >= 17) || (dateTime.Month == 11 && dateTime.Day <= 3))
		{
			GameEventSet(GameEvents.Halloween);
		}
		else
		{
			GameEventSet(GameEvents.None);
		}
	}

	public void GameEventSet(GameEvents _event)
	{
		currentGameEvent = _event;
	}

	public void SetGameMode(int mode)
	{
		gameMode = mode;
	}

	public void SetConnectRandom(bool _connectRandom)
	{
		connectRandom = _connectRandom;
	}

	public static bool Multiplayer()
	{
		return instance.gameMode == 1;
	}

	public void PlayerMicrophoneSettingSet(string _name, float _value)
	{
		playerMicrophoneSettings[_name] = _value;
	}

	public float PlayerMicrophoneSettingGet(string _name)
	{
		if (playerMicrophoneSettings.ContainsKey(_name))
		{
			return playerMicrophoneSettings[_name];
		}
		return 0.5f;
	}
}
