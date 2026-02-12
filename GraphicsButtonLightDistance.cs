using UnityEngine;

public class GraphicsButtonLightDistance : MonoBehaviour
{
	public void ButtonPress()
	{
		GraphicsManager.instance.UpdateLightDistance();
	}
}
