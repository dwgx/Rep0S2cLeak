using UnityEngine;

public class AudioButtonPushToTalk : MonoBehaviour
{
	public void ButtonPressed()
	{
		AudioManager.instance.UpdatePushToTalk();
	}
}
