using UnityEngine;

public class MenuPageSettingsPage : MonoBehaviour
{
	private MenuPage menuPage;

	public DataDirector.SettingType settingType;

	private bool saveSettings;

	private void Start()
	{
		menuPage = GetComponent<MenuPage>();
	}

	private void Update()
	{
		if (menuPage.currentPageState == MenuPage.PageState.Closing && !saveSettings)
		{
			SaveSettings();
			saveSettings = true;
		}
	}

	public void ResetSettings()
	{
		DataDirector.instance.ResetSettingTypeToDefault(settingType);
		MenuManager.instance.PageCloseAllAddedOnTop();
		MenuManager.instance.PageAddOnTop(menuPage.menuPageIndex);
		if (settingType == DataDirector.SettingType.Graphics)
		{
			GraphicsManager.instance.UpdateAll();
		}
		else if (settingType == DataDirector.SettingType.Gameplay)
		{
			GameplayManager.instance.UpdateAll();
		}
		else if (settingType == DataDirector.SettingType.Audio)
		{
			AudioManager.instance.UpdateAll();
		}
	}

	public void SaveSettings()
	{
		DataDirector.instance.SaveSettings();
	}
}
