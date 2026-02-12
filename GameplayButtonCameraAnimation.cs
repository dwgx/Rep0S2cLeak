using UnityEngine;

public class GameplayButtonCameraAnimation : MonoBehaviour
{
	public void ButtonPressed()
	{
		GameplayManager.instance.UpdateCameraAnimation();
	}
}
