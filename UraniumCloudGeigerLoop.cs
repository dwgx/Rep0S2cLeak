using UnityEngine;

public class UraniumCloudGeigerLoop : MonoBehaviour
{
	public Sound geigerSoundLoop;

	private float timer = 6f;

	private bool isPlaying;

	private void Update()
	{
		geigerSoundLoop.PlayLoop(isPlaying, 5f, 0.5f);
		if (timer > 0f)
		{
			isPlaying = true;
			timer -= Time.deltaTime;
		}
		else
		{
			isPlaying = false;
		}
	}
}
