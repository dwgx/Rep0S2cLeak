using UnityEngine;

public class MainMenuOpen : MonoBehaviour
{
	public enum MainMenuGameModeState
	{
		SinglePlayer,
		MultiPlayer
	}

	public static MainMenuOpen instance;

	public GameObject networkConnectPrefab;

	internal bool firstOpen = true;

	public MainMenuGameModeState mainMenuGameModeState;

	private void Awake()
	{
		instance = this;
	}

	public void NetworkConnect()
	{
		Object.Instantiate(networkConnectPrefab);
	}

	private void Start()
	{
		MenuManager.instance.PageOpen(MenuPageIndex.Main);
	}

	public void MainMenuSetState(int state)
	{
		mainMenuGameModeState = (MainMenuGameModeState)state;
	}

	public MainMenuGameModeState MainMenuGetState()
	{
		return mainMenuGameModeState;
	}
}
