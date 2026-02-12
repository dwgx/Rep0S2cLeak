using UnityEngine;

public class GraphicsButtonShadowDistance : MonoBehaviour
{
	public void ButtonPress()
	{
		GraphicsManager.instance.UpdateShadowDistance();
	}
}
