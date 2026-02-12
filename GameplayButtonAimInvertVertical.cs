using UnityEngine;

public class GameplayButtonAimInvertVertical : MonoBehaviour
{
	public void ButtonPressed()
	{
		GameplayManager.instance.UpdateAimInvertVertical();
	}
}
