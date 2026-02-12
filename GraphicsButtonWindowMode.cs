using UnityEngine;

public class GraphicsButtonWindowMode : MonoBehaviour
{
	public static GraphicsButtonWindowMode instance;

	private MenuSlider slider;

	private void Awake()
	{
		instance = this;
		slider = GetComponent<MenuSlider>();
	}

	public void UpdateSlider()
	{
		slider.Start();
	}

	public void ButtonPressed()
	{
		GraphicsManager.instance.UpdateWindowMode(_setResolution: true);
	}
}
