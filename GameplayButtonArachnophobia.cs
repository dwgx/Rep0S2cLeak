using UnityEngine;

public class GameplayButtonArachnophobia : MonoBehaviour
{
	public void ButtonPressed()
	{
		GameplayManager.instance.UpdateArachnophobia();
	}
}
