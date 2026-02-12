using TMPro;
using UnityEngine;

public class MenuPlayerListed : MonoBehaviour
{
	internal PlayerAvatar playerAvatar;

	internal int listSpot;

	public TextMeshProUGUI playerName;

	public MenuPlayerHead playerHead;

	private RectTransform parentTransform;

	private Vector3 midScreenFocus;

	public TextMeshProUGUI pingText;

	private bool localFetch;

	internal bool isLocal;

	public bool isSpectate = true;

	public GameObject leftCrown;

	public GameObject rightCrown;

	private bool fetchCrown;

	public bool forceCrown;

	private bool crownSetterWasHere;

	public bool brightName;

	[Space]
	private MenuPlayerListedSteam steamButton;

	private void Start()
	{
		parentTransform = base.transform.parent.GetComponent<RectTransform>();
		playerHead.focusPoint.SetParent(parentTransform);
		playerHead.myFocusPoint.SetParent(parentTransform);
		midScreenFocus = new Vector3(MenuManager.instance.screenUIWidth / 2, MenuManager.instance.screenUIHeight / 2, 0f) - parentTransform.localPosition - parentTransform.parent.GetComponent<RectTransform>().localPosition;
		if (forceCrown)
		{
			leftCrown.SetActive(value: true);
			rightCrown.SetActive(value: true);
			ForcePlayer(Arena.instance.winnerPlayer);
			TextMeshProUGUI componentInChildren = GetComponentInChildren<TextMeshProUGUI>();
			if ((bool)componentInChildren && (bool)playerAvatar)
			{
				componentInChildren.text = playerAvatar.playerName;
			}
		}
		steamButton = GetComponentInChildren<MenuPlayerListedSteam>();
		if (!SemiFunc.RunIsLobbyMenu())
		{
			steamButton.gameObject.SetActive(value: false);
		}
	}

	public void ForcePlayer(PlayerAvatar _playerAvatar)
	{
		playerHead.SetPlayer(_playerAvatar);
		playerAvatar = _playerAvatar;
		localFetch = false;
	}

	private void Update()
	{
		if (SemiFunc.FPSImpulse5() && !crownSetterWasHere && (bool)PlayerCrownSet.instance && PlayerCrownSet.instance.crownOwnerFetched)
		{
			if ((bool)playerAvatar && PlayerCrownSet.instance.crownOwnerSteamID == playerAvatar.steamID)
			{
				leftCrown.SetActive(value: true);
				rightCrown.SetActive(value: true);
			}
			crownSetterWasHere = true;
		}
		if (!localFetch && (bool)playerAvatar)
		{
			MenuButtonKick componentInChildren = GetComponentInChildren<MenuButtonKick>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Setup(playerAvatar);
			}
			isLocal = playerAvatar.isLocal;
			localFetch = true;
			if (isLocal)
			{
				steamButton.gameObject.SetActive(value: false);
			}
		}
		if (!forceCrown && playerHead.myFocusPoint.localPosition != midScreenFocus)
		{
			playerHead.myFocusPoint.localPosition = midScreenFocus;
		}
		if ((bool)playerAvatar)
		{
			if (!fetchCrown)
			{
				if (SessionManager.instance.CrownedPlayerGet() == playerAvatar)
				{
					leftCrown.SetActive(value: true);
					rightCrown.SetActive(value: true);
				}
				fetchCrown = true;
			}
			if (isSpectate && playerName.text != playerAvatar.playerName)
			{
				playerName.text = playerAvatar.playerName;
			}
			if ((bool)playerAvatar.playerDeathHead && playerAvatar.playerDeathHead.spectated)
			{
				Color color = Color.Lerp(Color.white, Color.black, 0.9f);
				playerName.color = Color.Lerp(playerName.color, color, Time.deltaTime * 10f);
			}
			else if (playerAvatar.voiceChatFetched && playerAvatar.voiceChat.isTalking)
			{
				Color color2 = new Color(0.6f, 0.6f, 0.4f);
				if (brightName)
				{
					color2 = Color.white;
				}
				playerName.color = Color.Lerp(playerName.color, color2, Time.deltaTime * 10f);
			}
			else
			{
				Color color3 = new Color(0.2f, 0.2f, 0.2f);
				if (brightName)
				{
					color3 = Color.white;
				}
				playerName.color = Color.Lerp(playerName.color, color3, Time.deltaTime * 10f);
			}
		}
		if (!forceCrown)
		{
			if (!SemiFunc.RunIsLobbyMenu())
			{
				base.transform.localPosition = new Vector3(-23f, -listSpot * 22, 0f);
			}
			else
			{
				base.transform.localPosition = new Vector3(0f, -listSpot * 32, 0f);
			}
		}
	}

	public void MenuPlayerListedOutro()
	{
		Object.Destroy(base.gameObject);
	}
}
