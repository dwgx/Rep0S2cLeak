using UnityEngine;

[CreateAssetMenu(fileName = "Reverb - _____", menuName = "Audio/Reverb Preset", order = 0)]
public class ReverbPreset : ScriptableObject
{
	[Range(-10000f, 0f)]
	public float dryLevel;

	[Range(-10000f, 0f)]
	public float room = -686f;

	[Range(-10000f, 0f)]
	public float roomHF = -454f;

	[Range(0.1f, 20f)]
	public float decayTime = 1f;

	[Range(0.1f, 2f)]
	public float decayHFRatio = 0.83f;

	[Range(-10000f, 1000f)]
	public float reflections = -1646f;

	[Range(0f, 0.3f)]
	public float reflectDelay;

	[Range(-10000f, 2000f)]
	public float reverb = 53f;

	[Range(0f, 0.1f)]
	public float reverbDelay;

	[Range(0f, 100f)]
	public float diffusion = 100f;

	[Range(0f, 100f)]
	public float density = 100f;

	[Range(20f, 20000f)]
	public float hfReference = 5000f;

	[Range(-10000f, 0f)]
	public float roomLF = -4659f;

	[Range(20f, 1000f)]
	public float lfReference = 250f;

	private void OnValidate()
	{
		if (!SemiFunc.OnValidateCheck() && Application.isPlaying)
		{
			ReverbDirector.instance.Set();
		}
	}
}
