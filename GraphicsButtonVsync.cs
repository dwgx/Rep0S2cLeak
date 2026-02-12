using UnityEngine;

public class GraphicsButtonVsync : MonoBehaviour
{
	public void ButtonPressed()
	{
		GraphicsManager.instance.UpdateVsync();
	}
}
