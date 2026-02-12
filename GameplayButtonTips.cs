using UnityEngine;

public class GameplayButtonTips : MonoBehaviour
{
	public void ButtonPressed()
	{
		GameplayManager.instance.UpdateTips();
	}
}
