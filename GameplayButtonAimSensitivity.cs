using UnityEngine;

public class GameplayButtonAimSensitivity : MonoBehaviour
{
	public void ButtonPressed()
	{
		GameplayManager.instance.UpdateAimSensitivity();
	}
}
