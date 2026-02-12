using UnityEngine;

public class GraphicsButtonGlitchLoop : MonoBehaviour
{
	public void ButtonPressed()
	{
		GraphicsManager.instance.UpdateGlitchLoop();
	}
}
