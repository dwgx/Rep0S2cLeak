using UnityEngine;

public class GraphicsButtonShadowQuality : MonoBehaviour
{
	public void ButtonPress()
	{
		GraphicsManager.instance.UpdateShadowQuality();
	}
}
