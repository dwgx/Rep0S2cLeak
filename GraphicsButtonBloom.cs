using UnityEngine;

public class GraphicsButtonBloom : MonoBehaviour
{
	public void ButtonPressed()
	{
		GraphicsManager.instance.UpdateBloom();
	}
}
