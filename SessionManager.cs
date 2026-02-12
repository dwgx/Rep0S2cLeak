using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
	public static SessionManager instance;

	internal string crownedPlayerSteamID;

	public GameObject crownPrefab;

	internal List<string> micDeviceList = new List<string>();

	internal string micDeviceCurrent;

	internal int micDeviceCurrentIndex;

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
		string[] devices = Microphone.devices;
		foreach (string item in devices)
		{
			micDeviceList.Add(item);
		}
	}

	private void Start()
	{
		bool flag = false;
		int num = 0;
		foreach (string micDevice in micDeviceList)
		{
			if (micDevice == DataDirector.instance.micDevice || DataDirector.instance.micDevice == "")
			{
				micDeviceCurrent = micDevice;
				flag = true;
				break;
			}
			num++;
		}
		if (!flag && DataDirector.instance.micDevice != "NONE")
		{
			num = 0;
		}
		micDeviceCurrentIndex = num;
		DataDirector.instance.SettingValueSet(DataDirector.Setting.MicDevice, micDeviceCurrentIndex);
	}

	private void Update()
	{
		if (SemiFunc.FPSImpulse1())
		{
			string[] devices = Microphone.devices;
			foreach (string item in devices)
			{
				if (!micDeviceList.Contains(item))
				{
					micDeviceList.Add(item);
				}
			}
		}
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F12))
		{
			DebugCommandHandler.instance?.Execute("logs");
		}
	}

	public void CrownPlayer()
	{
		if (!string.IsNullOrEmpty(crownedPlayerSteamID) && SemiFunc.IsMasterClient() && SemiFunc.IsMultiplayer())
		{
			PunManager.instance.CrownPlayerSync(crownedPlayerSteamID);
		}
	}

	public PlayerAvatar CrownedPlayerGet()
	{
		return SemiFunc.PlayerAvatarGetFromSteamID(crownedPlayerSteamID);
	}

	public void ResetCrown()
	{
		crownedPlayerSteamID = "";
	}

	public void Reset()
	{
		crownedPlayerSteamID = "";
	}
}
