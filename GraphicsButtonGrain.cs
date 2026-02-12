using UnityEngine;

public class GraphicsButtonGrain : MonoBehaviour
{
	public void ButtonPressed()
	{
		GraphicsManager.instance.UpdateGrain();
	}
}
