using UnityEngine;

[CreateAssetMenu(fileName = "Screen Next Message Delay Preset", menuName = "Semi Presets/Screen Next Message Delay Preset")]
public class ScreenNextMessageDelaySettings : ScriptableObject
{
	public float delay = 1f;

	public float GetDelay()
	{
		return delay;
	}
}
