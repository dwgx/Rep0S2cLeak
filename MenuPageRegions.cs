using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MenuPageRegions : MonoBehaviourPunCallbacks
{
	internal enum Type
	{
		HostGame,
		PlayRandom
	}

	internal Type type;

	private bool connectedToMaster;

	public GameObject regionPrefab;

	public Transform transformStartPosition;

	public MenuScrollBox menuScrollBox;

	public CanvasGroup scrollCanvasGroup;

	public MenuLoadingGraphics loadingGraphics;

	private void Start()
	{
		StartCoroutine(GetRegions());
	}

	private IEnumerator GetRegions()
	{
		PhotonNetwork.Disconnect();
		while (PhotonNetwork.NetworkingClient.State != ClientState.Disconnected && PhotonNetwork.NetworkingClient.State != ClientState.PeerCreated)
		{
			yield return null;
		}
		DataDirector.instance.PhotonSetAppId();
		SteamManager.instance.SendSteamAuthTicket();
		PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "";
		ServerSettings.ResetBestRegionCodeInPreferences();
		PhotonNetwork.ConnectUsingSettings();
		while (!connectedToMaster)
		{
			yield return null;
		}
		loadingGraphics.SetDone();
		yield return new WaitForSecondsRealtime(0.3f);
		Vector3 _position = transformStartPosition.position;
		MenuElementRegion component = Object.Instantiate(regionPrefab, _position, Quaternion.identity, transformStartPosition.parent).GetComponent<MenuElementRegion>();
		component.textName.text = "Pick Best Region";
		component.textName.color = Color.white;
		component.textPing.text = "";
		component.parentPage = this;
		component.regionCode = "";
		float _pitch = 0f;
		foreach (Region enabledRegion in PhotonNetwork.NetworkingClient.RegionHandler.EnabledRegions)
		{
			_position = new Vector3(_position.x, _position.y - 32f, _position.z);
			string text = enabledRegion.Code;
			switch (text)
			{
			case "asia":
				text = "Asia";
				break;
			case "au":
				text = "Australia";
				break;
			case "cae":
				text = "Canada East";
				break;
			case "cn":
				text = "Chinese Mainland";
				break;
			case "eu":
				text = "Europe";
				break;
			case "hk":
				text = "Hong Kong";
				break;
			case "in":
				text = "India";
				break;
			case "jp":
				text = "Japan";
				break;
			case "za":
				text = "South Africa";
				break;
			case "sa":
				text = "South America";
				break;
			case "kr":
				text = "South Korea";
				break;
			case "tr":
				text = "Turkey";
				break;
			case "uae":
				text = "United Arab Emirates";
				break;
			case "us":
				text = "USA East";
				break;
			case "usw":
				text = "USA West";
				break;
			case "ussc":
				text = "USA South Central";
				break;
			}
			MenuElementRegion component2 = Object.Instantiate(regionPrefab, _position, Quaternion.identity, transformStartPosition.parent).GetComponent<MenuElementRegion>();
			component2.textName.text = text;
			string text2;
			if (enabledRegion.Ping > 999 || enabledRegion.Ping == RegionPinger.PingWhenFailed)
			{
				text2 = ">999";
				component2.textPing.color = new Color(0.8f, 0.2f, 0.2f);
			}
			else
			{
				if (enabledRegion.Ping < 50)
				{
					component2.textPing.color = new Color(0.2f, 0.8f, 0.2f);
				}
				else if (enabledRegion.Ping < 100)
				{
					component2.textPing.color = new Color(0.8f, 0.8f, 0.2f);
				}
				else
				{
					component2.textPing.color = new Color(0.8f, 0.4f, 0.2f);
				}
				text2 = enabledRegion.Ping.ToString();
			}
			component2.textPing.text = text2 + " ms";
			component2.parentPage = this;
			component2.regionCode = enabledRegion.Code;
			float pitch = MenuManager.instance.soundPageIntro.Pitch;
			MenuManager.instance.soundPageIntro.Pitch = 1f + _pitch;
			MenuManager.instance.soundPageIntro.Play(Vector3.zero, 0.75f);
			MenuManager.instance.soundPageIntro.Pitch = pitch;
			_pitch += 0.1f;
			yield return new WaitForSecondsRealtime(0.15f);
		}
		PhotonNetwork.Disconnect();
		menuScrollBox.enabled = true;
		while (scrollCanvasGroup.alpha < 0.99f)
		{
			scrollCanvasGroup.alpha += Time.deltaTime * 5f;
			yield return null;
		}
		scrollCanvasGroup.alpha = 1f;
	}

	public override void OnConnectedToMaster()
	{
		connectedToMaster = true;
	}

	private void Update()
	{
		if (SemiFunc.InputDown(InputKey.Back) && MenuManager.instance.currentMenuPageIndex == MenuPageIndex.Regions)
		{
			ExitPage();
		}
	}

	public void ExitPage()
	{
		MenuManager.instance.PageCloseAll();
		MenuManager.instance.PageOpen(MenuPageIndex.Main);
	}

	public void PickRegion(string _region)
	{
		DataDirector.instance.networkRegion = _region;
		if (type == Type.HostGame)
		{
			SemiFunc.MainMenuSetMultiplayer();
			MenuManager.instance.PageCloseAll();
			MenuManager.instance.PageOpen(MenuPageIndex.Saves);
		}
		else
		{
			MenuManager.instance.PageCloseAll();
			MenuManager.instance.PageOpen(MenuPageIndex.PublicGameChoice);
		}
	}

	private void OnDestroy()
	{
		PhotonNetwork.Disconnect();
	}
}
