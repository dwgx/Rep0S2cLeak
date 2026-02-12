using UnityEngine;

public class GameplayButtonItemUnequipAutoHold : MonoBehaviour
{
	public void ButtonPressed()
	{
		GameplayManager.instance.UpdateItemUnequipAutoHold();
	}
}
