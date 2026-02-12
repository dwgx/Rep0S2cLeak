using UnityEngine;

public class MenuKeybindToggle : MonoBehaviour
{
	public InputKey inputKey;

	public void ToggleRebind1()
	{
		InputManager.instance.InputToggleRebind(inputKey, toggle: true);
	}

	public void ToggleRebind2()
	{
		InputManager.instance.InputToggleRebind(inputKey, toggle: false);
	}

	public void FetchSetting()
	{
		MenuTwoOptions component = GetComponent<MenuTwoOptions>();
		MenuKeybindToggle component2 = GetComponent<MenuKeybindToggle>();
		component.startSettingFetch = InputManager.instance.InputToggleGet(component2.inputKey);
	}
}
