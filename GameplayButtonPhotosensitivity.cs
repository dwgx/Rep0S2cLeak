using UnityEngine;

public class GameplayButtonPhotosensitivity : MonoBehaviour
{
	public void ButtonPressed()
	{
		GameplayManager.instance.UpdatePhotosensitivity();
	}
}
