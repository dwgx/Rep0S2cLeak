using UnityEngine;

public class MenuSetting : MonoBehaviour
{
	public DataDirector.Setting setting;

	internal string settingName;

	internal int settingValue;

	private void Start()
	{
		FetchValues();
	}

	public void FetchValues()
	{
		settingValue = DataDirector.instance.SettingValueFetch(setting);
		settingName = DataDirector.instance.SettingNameGet(setting);
	}
}
