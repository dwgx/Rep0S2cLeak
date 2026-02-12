using UnityEngine;

[CreateAssetMenu(fileName = "Color Preset", menuName = "Semi Presets/Color Preset")]
public class ColorPresets : ScriptableObject
{
	public Color colorMain;

	public Color colorLight;

	public Color colorDark;

	public Color GetColorMain()
	{
		return colorMain;
	}

	public Color GetColorLight()
	{
		return colorLight;
	}

	public Color GetColorDark()
	{
		return colorDark;
	}
}
