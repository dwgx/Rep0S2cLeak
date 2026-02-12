using UnityEngine;

public class GraphicsButtonChromaticAberration : MonoBehaviour
{
	public void ButtonPressed()
	{
		GraphicsManager.instance.UpdateChromaticAberration();
	}
}
