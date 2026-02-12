using UnityEngine;

public class MenuPageMain : MonoBehaviour
{
	public static MenuPageMain instance;

	private RectTransform rectTransform;

	private float waitTimer;

	private bool animateIn;

	internal MenuPage menuPage;

	private bool introDone;

	public GameObject networkConnectPrefab;

	private float joinLobbyTimer = 0.1f;

	private float popUpTimer = 1.5f;

	private bool doIntroAnimation = true;

	public MenuButton tutorialButton;

	private bool tutorialButtonBlinkActive;

	private bool tutorialButtonBlink;

	private float tutorialButtonTimer;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		menuPage = GetComponent<MenuPage>();
		if (MainMenuOpen.instance.firstOpen)
		{
			MainMenuOpen.instance.firstOpen = false;
			menuPage.disableOutroAnimation = false;
		}
		else
		{
			menuPage.disableIntroAnimation = false;
			menuPage.disableOutroAnimation = false;
			doIntroAnimation = false;
		}
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.TutorialPlayed) <= 0)
		{
			tutorialButtonBlinkActive = true;
			tutorialButton.customColors = true;
			tutorialButton.colorNormal = new Color(1f, 0.55f, 0f);
			tutorialButton.colorHover = Color.white;
			tutorialButton.colorClick = new Color(1f, 0.55f, 0f);
		}
		if (SteamManager.instance.lobbyIdToAutoJoin != 0L)
		{
			SteamManager.instance.StartAutoJoiningLobby();
		}
	}

	private void Update()
	{
		if (tutorialButtonBlinkActive)
		{
			if (tutorialButtonTimer <= 0f)
			{
				tutorialButtonTimer = 0.5f;
				tutorialButtonBlink = !tutorialButtonBlink;
				if (tutorialButtonBlink)
				{
					tutorialButton.colorNormal = Color.white;
				}
				else
				{
					tutorialButton.colorNormal = new Color(1f, 0.55f, 0f);
				}
			}
			else
			{
				tutorialButtonTimer -= Time.deltaTime;
			}
		}
		if (menuPage.currentPageState == MenuPage.PageState.Closing)
		{
			return;
		}
		if (RunManager.instance.localMultiplayerTest)
		{
			GameManager.instance.localTest = true;
			RunManager.instance.localMultiplayerTest = false;
			RunManager.instance.ResetProgress();
			GameManager.instance.SetConnectRandom(_connectRandom: false);
			RunManager.instance.waitToChangeScene = true;
			RunManager.instance.lobbyJoin = true;
			RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.LobbyMenu);
			SteamManager.instance.joinLobby = false;
		}
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (popUpTimer > 0f)
		{
			popUpTimer -= Time.deltaTime;
			if (popUpTimer <= 0f)
			{
				MenuManager.instance.PagePopUpScheduledShow();
			}
		}
		if (doIntroAnimation)
		{
			waitTimer += Time.deltaTime;
			if (waitTimer > 3f)
			{
				animateIn = true;
			}
			else
			{
				rectTransform.localPosition = new Vector3(-600f, 0f, 0f);
			}
			if (animateIn)
			{
				rectTransform.localPosition = new Vector3(Mathf.Lerp(rectTransform.localPosition.x, 0f, Time.deltaTime * 2f), 0f, 0f);
				if (Mathf.Abs(rectTransform.localPosition.x) < 50f && !introDone)
				{
					menuPage.PageStateSet(MenuPage.PageState.Active);
					introDone = true;
				}
			}
		}
		else if (!introDone)
		{
			menuPage.PageStateSet(MenuPage.PageState.Active);
			introDone = true;
		}
		if (SteamManager.instance.joinLobby)
		{
			if (joinLobbyTimer > 0f)
			{
				joinLobbyTimer -= Time.deltaTime;
				return;
			}
			GameManager.instance.localTest = false;
			RunManager.instance.ResetProgress();
			RunManager.instance.waitToChangeScene = true;
			RunManager.instance.lobbyJoin = true;
			RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.LobbyMenu);
			SteamManager.instance.joinLobby = false;
		}
	}

	public void ButtonEventSinglePlayer()
	{
		SemiFunc.MainMenuSetSingleplayer();
		MenuManager.instance.PageCloseAll();
		MenuManager.instance.PageOpen(MenuPageIndex.Saves);
	}

	public void ButtonEventTutorial()
	{
		DataDirector.instance.TutorialPlayed();
		TutorialDirector.instance.Reset();
		RunManager.instance.ResetProgress();
		RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.Tutorial);
	}

	public void ButtonEventHostGame()
	{
		MenuManager.instance.PageCloseAll();
		MenuManager.instance.PageOpen(MenuPageIndex.Regions).GetComponent<MenuPageRegions>().type = MenuPageRegions.Type.HostGame;
	}

	public void ButtonEventJoinGame()
	{
		SteamManager.instance.OpenSteamOverlayToJoin();
	}

	public void ButtonEventPlayRandom()
	{
		MenuManager.instance.PageCloseAll();
		MenuManager.instance.PageOpen(MenuPageIndex.Regions).GetComponent<MenuPageRegions>().type = MenuPageRegions.Type.PlayRandom;
	}

	public void ButtonEventQuit()
	{
		RunManager.instance.skipLoadingUI = true;
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			player.quitApplication = true;
		}
		GameDirector.instance.OutroStart();
	}

	public void ButtonEventSettings()
	{
		MenuManager.instance.PageOpenOnTop(MenuPageIndex.Settings);
	}
}
