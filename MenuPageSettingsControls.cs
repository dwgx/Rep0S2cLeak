using UnityEngine;

public class MenuPageSettingsControls : MonoBehaviour
{
	private MenuPage menuPage;

	private void Start()
	{
		menuPage = GetComponent<MenuPage>();
	}

	public void ResetControls()
	{
		InputManager.instance.LoadKeyBindings("DefaultKeyBindings.es3");
		MenuManager.instance.PageCloseAllAddedOnTop();
		MenuManager.instance.PageAddOnTop(MenuPageIndex.SettingsControls);
	}

	public void SaveControls()
	{
		InputManager.instance.SaveCurrentKeyBindings();
	}
}
