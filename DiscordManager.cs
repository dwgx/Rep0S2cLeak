using System;
using Discord.Sdk;
using UnityEngine;

public class DiscordManager : MonoBehaviour
{
	internal static DiscordManager instance;

	[SerializeField]
	private ulong clientId;

	internal Client client;

	internal Activity activity;

	internal ActivityParty activityParty;

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

	private void Start()
	{
		client = new Client();
		client.AddLogCallback(OnLog, LoggingSeverity.Error);
		client.SetApplicationId(clientId);
		client.RegisterLaunchSteamApplication(clientId, 3241660u);
		client.SetActivityJoinCallback(OnActivityJoinCallback);
		activity = new Activity();
		activity.SetType(ActivityTypes.Playing);
		activity.SetSupportedPlatforms(ActivityGamePlatforms.Desktop);
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			client?.ClearRichPresence();
		}
	}

	private void OnLog(string message, LoggingSeverity severity)
	{
		Debug.Log($"Log: {severity} - {message}");
	}

	private void OnActivityJoinCallback(string joinSecret)
	{
		if (ulong.TryParse(joinSecret, out var result))
		{
			SteamManager.instance.lobbyIdToAutoJoin = result;
			if (!SemiFunc.IsSplashScreen() && result != 0L)
			{
				SteamManager.instance.StartAutoJoiningLobby();
			}
		}
	}

	internal void UpdateDiscordRichPresence(string state, string details)
	{
		activity.SetState(state);
		activity.SetDetails(details);
		ActivityTimestamps activityTimestamps = new ActivityTimestamps();
		activityTimestamps.SetStart((ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
		activity.SetTimestamps(activityTimestamps);
		RefreshDiscordRichPresence();
	}

	internal void RefreshDiscordRichPresence()
	{
		ActivityAssets activityAssets = new ActivityAssets();
		switch (DataDirector.instance.SettingValueFetch(DataDirector.Setting.RichPresence))
		{
		case 1:
			activity.SetDetails(null);
			activityAssets.SetLargeImage("taxman");
			activityAssets.SetLargeText(BuildManager.instance?.version.title);
			activity.SetAssets(activityAssets);
			activity.SetParty(null);
			activity.SetSecrets(null);
			client.UpdateRichPresence(activity, delegate
			{
			});
			break;
		case 2:
		{
			if (!string.IsNullOrWhiteSpace(RunManager.instance?.levelCurrent.DiscordIcon))
			{
				activityAssets.SetLargeImage(RunManager.instance.levelCurrent.DiscordIcon.Trim().ToLower());
			}
			else
			{
				activityAssets.SetLargeImage("player_head");
				activityAssets.SetSmallImage("taxman");
				activityAssets.SetSmallText(BuildManager.instance?.version.title);
			}
			activityAssets.SetLargeText(RunManager.instance?.levelCurrent.NarrativeName);
			activity.SetAssets(activityAssets);
			activity.SetParty(activityParty);
			ActivitySecrets activitySecrets = null;
			if (activityParty != null && SemiFunc.RunIsLobbyMenu())
			{
				activitySecrets = new ActivitySecrets();
				activitySecrets.SetJoin(SteamManager.instance.currentLobby.Id.ToString());
			}
			activity.SetSecrets(activitySecrets);
			client.UpdateRichPresence(activity, delegate
			{
			});
			break;
		}
		default:
			client.ClearRichPresence();
			break;
		}
	}
}
