using UnityEngine;

public class MenuInputPercentSetting : MonoBehaviour
{
	public InputPercentSetting setting;

	private MenuSlider menuSlider;

	private void Start()
	{
		menuSlider = GetComponent<MenuSlider>();
		menuSlider.SetBar((float)InputManager.instance.inputPercentSettings[setting] / 100f);
	}
}
