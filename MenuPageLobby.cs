using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class MenuPageLobby : MonoBehaviour
{
	public static MenuPageLobby instance;

	internal MenuPage menuPage;

	private float listCheckTimer;

	internal List<PlayerAvatar> lobbyPlayers = new List<PlayerAvatar>();

	internal List<GameObject> listObjects = new List<GameObject>();

	internal List<MenuPlayerListed> menuPlayerListedList = new List<MenuPlayerListed>();

	public GameObject menuPlayerListedPrefab;

	public RectTransform playerListTransform;

	public TextMeshProUGUI roomNameText;

	public TextMeshProUGUI chatPromptText;

	public MenuButton startButton;

	public MenuButton inviteButton;

	public CanvasGroup joiningPlayersCanvasGroup;

	private List<string> joiningPlayers = new List<string>();

	private float joiningPlayersTimer;

	private float joiningPlayersEndTimer;

	private bool joiningPlayer;

	private void Awake()
	{
		instance = this;
		menuPage = GetComponent<MenuPage>();
		roomNameText.text = PhotonNetwork.CloudRegion + " " + PhotonNetwork.CurrentRoom?.Name;
		UpdateChatPrompt();
	}

	private void Start()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			inviteButton.gameObject.SetActive(value: false);
		}
		else if (!SemiFunc.IsMasterClient())
		{
			inviteButton.transform.localPosition = new Vector3(startButton.transform.localPosition.x + 40f, startButton.transform.localPosition.y, startButton.transform.localPosition.z);
			inviteButton.buttonText.alignment = TextAlignmentOptions.Right;
			startButton.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (joiningPlayersTimer > 0f)
		{
			joiningPlayersTimer -= Time.deltaTime;
		}
		else if (joiningPlayers.Count > 0)
		{
			joiningPlayers.Clear();
		}
		if (joiningPlayers.Count > 0 || joiningPlayersEndTimer > 0f)
		{
			joiningPlayer = true;
			joiningPlayersCanvasGroup.alpha = Mathf.Lerp(joiningPlayersCanvasGroup.alpha, 1f, Time.deltaTime * 10f);
			startButton.disabled = true;
		}
		else
		{
			joiningPlayersCanvasGroup.alpha = Mathf.Lerp(joiningPlayersCanvasGroup.alpha, 0f, Time.deltaTime * 10f);
			joiningPlayer = false;
			startButton.disabled = false;
		}
		if (joiningPlayersEndTimer > 0f)
		{
			joiningPlayersEndTimer -= Time.deltaTime;
		}
		listCheckTimer -= Time.deltaTime;
		if (!(listCheckTimer <= 0f))
		{
			return;
		}
		listCheckTimer = 1f;
		List<PlayerAvatar> list = SemiFunc.PlayerGetList();
		bool flag = false;
		foreach (PlayerAvatar item in list)
		{
			if (!lobbyPlayers.Contains(item) && item.playerAvatarVisuals.colorSet)
			{
				PlayerAdd(item);
				flag = true;
			}
		}
		foreach (PlayerAvatar item2 in lobbyPlayers.ToList())
		{
			if (!list.Contains(item2))
			{
				PlayerRemove(item2);
				flag = true;
			}
		}
		if (flag)
		{
			listObjects.Sort((GameObject gameObject, GameObject gameObject2) => gameObject.GetComponent<MenuPlayerListed>().playerAvatar.photonView.ViewID.CompareTo(gameObject2.GetComponent<MenuPlayerListed>().playerAvatar.photonView.ViewID));
			for (int num = 0; num < listObjects.Count; num++)
			{
				listObjects[num].GetComponent<MenuPlayerListed>().listSpot = num;
				listObjects[num].transform.SetSiblingIndex(num);
			}
		}
		foreach (GameObject listObject in listObjects)
		{
			PlayerAvatar playerAvatar = listObject.GetComponent<MenuPlayerListed>().playerAvatar;
			if ((bool)playerAvatar)
			{
				if (playerAvatar.photonView.Owner == PhotonNetwork.MasterClient)
				{
					listObject.GetComponent<MenuPlayerListed>().playerName.text = playerAvatar.playerName;
				}
				else
				{
					listObject.GetComponent<MenuPlayerListed>().playerName.text = playerAvatar.playerName;
				}
				SetPingText(listObject.GetComponent<MenuPlayerListed>().pingText, playerAvatar.playerPing);
			}
		}
	}

	private void PlayerAdd(PlayerAvatar player)
	{
		lobbyPlayers.Add(player);
		GameObject gameObject = Object.Instantiate(menuPlayerListedPrefab, base.transform);
		MenuPlayerListed component = gameObject.GetComponent<MenuPlayerListed>();
		component.playerAvatar = player;
		component.playerHead.SetPlayer(player);
		component.GetComponent<RectTransform>().SetParent(playerListTransform);
		MenuSliderPlayerMicGain componentInChildren = component.GetComponentInChildren<MenuSliderPlayerMicGain>();
		componentInChildren.playerAvatar = player;
		if (player.isLocal)
		{
			Object.Destroy(componentInChildren.gameObject);
		}
		component.transform.localPosition = Vector3.zero;
		listObjects.Add(gameObject);
		menuPlayerListedList.Add(component);
		component.listSpot = Mathf.Max(listObjects.Count - 1, 0);
		foreach (string joiningPlayer in joiningPlayers)
		{
			if (player.playerName == joiningPlayer)
			{
				joiningPlayers.Remove(joiningPlayer);
				joiningPlayersEndTimer = 1f;
				break;
			}
		}
	}

	private void PlayerRemove(PlayerAvatar player)
	{
		lobbyPlayers.Remove(player);
		foreach (GameObject listObject in listObjects)
		{
			if (listObject.GetComponent<MenuPlayerListed>().playerAvatar == player)
			{
				listObject.GetComponent<MenuPlayerListed>().MenuPlayerListedOutro();
				listObjects.Remove(listObject);
				menuPlayerListedList.Remove(listObject.GetComponent<MenuPlayerListed>());
				break;
			}
		}
		for (int i = 0; i < listObjects.Count; i++)
		{
			listObjects[i].GetComponent<MenuPlayerListed>().listSpot = i;
		}
	}

	private void SetPingText(TextMeshProUGUI text, int ping)
	{
		if (ping < 50)
		{
			text.color = new Color(0.2f, 0.8f, 0.2f);
		}
		else if (ping < 100)
		{
			text.color = new Color(0.8f, 0.8f, 0.2f);
		}
		else if (ping < 200)
		{
			text.color = new Color(0.8f, 0.4f, 0.2f);
		}
		else
		{
			text.color = new Color(0.8f, 0.2f, 0.2f);
		}
		text.text = ping + " ms";
	}

	public void JoiningPlayer(string playerName)
	{
		if (!joiningPlayers.Contains(playerName))
		{
			joiningPlayers.Add(playerName);
			joiningPlayersTimer = 10f;
		}
	}

	public void ChangeColorButton()
	{
		MenuManager.instance.PageOpenOnTop(MenuPageIndex.Color);
	}

	public void UpdateChatPrompt()
	{
		chatPromptText.text = InputManager.instance.InputDisplayReplaceTags("Press [chat] to chat");
	}

	public void ButtonLeave()
	{
		GameDirector.instance.OutroStart();
		NetworkManager.instance.leavePhotonRoom = true;
	}

	public void ButtonSettings()
	{
		MenuManager.instance.PageOpenOnTop(MenuPageIndex.Settings);
	}

	public void ButtonStart()
	{
		if (joiningPlayer)
		{
			MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny);
			return;
		}
		if (PhotonNetwork.CurrentRoom != null)
		{
			PhotonNetwork.CurrentRoom.IsOpen = false;
			PhotonNetwork.CurrentRoom.IsVisible = false;
		}
		SteamManager.instance.LockLobby();
		DataDirector.instance.RunsPlayedAdd();
		if (RunManager.instance.loadLevel == 0)
		{
			RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.RunLevel);
		}
		else
		{
			RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.Shop);
		}
	}

	public void ButtonInvite()
	{
		SteamManager.instance.OpenSteamOverlayToInvite();
	}
}
