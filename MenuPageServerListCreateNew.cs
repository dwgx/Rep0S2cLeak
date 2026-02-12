using UnityEngine;

public class MenuPageServerListCreateNew : MonoBehaviour
{
	public MenuButton confirmButton;

	public MenuTextInput menuTextInput;

	internal MenuPage menuPageParent;

	private void Update()
	{
		if (SemiFunc.InputDown(InputKey.Confirm))
		{
			ButtonConfirm();
		}
		if (SemiFunc.InputDown(InputKey.Back))
		{
			ExitPage();
		}
	}

	public void ExitPage()
	{
		MenuManager.instance.PageCloseAllExcept(MenuPageIndex.ServerList);
		MenuManager.instance.PageSetCurrent(MenuPageIndex.ServerList, menuPageParent);
	}

	public void ButtonConfirm()
	{
		if (string.IsNullOrEmpty(menuTextInput.textCurrent))
		{
			confirmButton.OnHovering();
			MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f);
			return;
		}
		DataDirector.instance.networkServerName = menuTextInput.textCurrent;
		MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm, null, 1f, 1f);
		RunManager.instance.ResetProgress();
		StatsManager.instance.saveFileCurrent = "";
		GameManager.instance.SetConnectRandom(_connectRandom: true);
		GameManager.instance.localTest = false;
		RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.LobbyMenu);
		RunManager.instance.lobbyJoin = true;
	}
}
