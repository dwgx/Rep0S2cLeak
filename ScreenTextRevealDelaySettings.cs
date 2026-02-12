using UnityEngine;

[CreateAssetMenu(fileName = "Screen Text Reveal Delay Preset", menuName = "Semi Presets/Screen Text Reveal Delay Preset")]
public class ScreenTextRevealDelaySettings : ScriptableObject
{
	public float delay = 0.1f;

	public float GetDelay()
	{
		return delay;
	}
}
