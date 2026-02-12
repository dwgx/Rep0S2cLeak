using UnityEngine;

public class GameplayButtonCameraSmoothing : MonoBehaviour
{
	public void ButtonPressed()
	{
		GameplayManager.instance.UpdateCameraSmoothing();
	}
}
