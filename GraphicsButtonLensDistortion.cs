using UnityEngine;

public class GraphicsButtonLensDistortion : MonoBehaviour
{
	public void ButtonPressed()
	{
		GraphicsManager.instance.UpdateLensDistortion();
	}
}
