using UnityEngine;

public class MenuPageServerListSearch : MonoBehaviour
{
	public MenuButton confirmButton;

	public MenuTextInput menuTextInput;

	internal MenuPage menuPageParent;

	internal MenuPageServerList menuPageServerList;

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
		menuPageServerList.SetSearch(menuTextInput.textCurrent);
		ExitPage();
	}
}
