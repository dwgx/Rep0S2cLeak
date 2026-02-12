using UnityEngine;

public class GraphicsButtonRenderSize : MonoBehaviour
{
	public void ButtonPress()
	{
		GraphicsManager.instance.UpdateRenderSize();
	}
}
