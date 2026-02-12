using UnityEngine;

public class MenuPagePublicGameChoice : MonoBehaviour
{
	private void Update()
	{
		if (SemiFunc.InputDown(InputKey.Back) && MenuManager.instance.currentMenuPageIndex == MenuPageIndex.PublicGameChoice)
		{
			ExitPage();
		}
	}

	public void ButtonRandomMatchmaking()
	{
		RunManager.instance.ResetProgress();
		StatsManager.instance.saveFileCurrent = "";
		GameManager.instance.SetConnectRandom(_connectRandom: true);
		GameManager.instance.localTest = false;
		RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.LobbyMenu);
		RunManager.instance.lobbyJoin = true;
	}

	public void ButtonServerList()
	{
		MenuManager.instance.PageCloseAll();
		MenuManager.instance.PageOpen(MenuPageIndex.ServerList);
	}

	public void ExitPage()
	{
		MenuManager.instance.PageCloseAll();
		MenuManager.instance.PageOpen(MenuPageIndex.Regions).GetComponent<MenuPageRegions>().type = MenuPageRegions.Type.PlayRandom;
	}
}
