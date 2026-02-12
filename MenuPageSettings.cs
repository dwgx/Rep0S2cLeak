using UnityEngine;

public class MenuPageSettings : MonoBehaviour
{
	public static MenuPageSettings instance;

	internal MenuPage menuPage;

	private void Start()
	{
		instance = this;
		menuPage = GetComponent<MenuPage>();
	}

	private void Update()
	{
		if (SemiFunc.InputDown(InputKey.Back))
		{
			ButtonEventBack();
		}
	}

	public void ButtonEventGameplay()
	{
		MenuManager.instance.PageCloseAllAddedOnTop();
		MenuManager.instance.PageAddOnTop(MenuPageIndex.SettingsGameplay);
	}

	public void ButtonEventAudio()
	{
		MenuManager.instance.PageCloseAllAddedOnTop();
		MenuManager.instance.PageAddOnTop(MenuPageIndex.SettingsAudio);
	}

	public void ButtonEventBack()
	{
		if (SemiFunc.IsMainMenu())
		{
			MenuManager.instance.PageCloseAllExcept(MenuPageIndex.Main);
			MenuManager.instance.PageSetCurrent(MenuPageIndex.Main, MenuPageMain.instance.menuPage);
		}
		else if (SemiFunc.RunIsLobbyMenu())
		{
			MenuManager.instance.PageCloseAllExcept(MenuPageIndex.Lobby);
			MenuManager.instance.PageSetCurrent(MenuPageIndex.Lobby, MenuPageLobby.instance.menuPage);
		}
		else
		{
			MenuManager.instance.PageCloseAll();
			MenuManager.instance.PageOpen(MenuPageIndex.Escape);
		}
	}

	public void ButtonEventControls()
	{
		MenuManager.instance.PageCloseAllAddedOnTop();
		MenuManager.instance.PageAddOnTop(MenuPageIndex.SettingsControls);
	}

	public void ButtonEventGraphics()
	{
		MenuManager.instance.PageCloseAllAddedOnTop();
		MenuManager.instance.PageAddOnTop(MenuPageIndex.SettingsGraphics);
	}
}
