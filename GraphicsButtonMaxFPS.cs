using UnityEngine;

public class GraphicsButtonMaxFPS : MonoBehaviour
{
	public void ButtonPress()
	{
		GraphicsManager.instance.UpdateMaxFPS();
	}
}
