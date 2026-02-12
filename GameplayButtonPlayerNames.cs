using UnityEngine;

public class GameplayButtonPlayerNames : MonoBehaviour
{
	public void ButtonPressed()
	{
		GameplayManager.instance.UpdatePlayerNames();
	}
}
