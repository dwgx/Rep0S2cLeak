using UnityEngine;

public class GraphicsButtonGamma : MonoBehaviour
{
	public void ButtonPress()
	{
		GraphicsManager.instance.UpdateGamma();
	}
}
