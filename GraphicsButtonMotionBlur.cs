using UnityEngine;

public class GraphicsButtonMotionBlur : MonoBehaviour
{
	public void ButtonPressed()
	{
		GraphicsManager.instance.UpdateMotionBlur();
	}
}
