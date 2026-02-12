using UnityEngine;

[CreateAssetMenu(fileName = "PhysAudio - _____", menuName = "Audio/Physics Audio Preset", order = 1)]
public class PhysAudio : ScriptableObject
{
	public Sound impactLight;

	public Sound impactMedium;

	public Sound impactHeavy;

	[Space(20f)]
	public Sound breakLight;

	public Sound breakMedium;

	public Sound breakHeavy;

	[Space(20f)]
	public Sound destroy;

	private void OnValidate()
	{
		if (!SemiFunc.OnValidateCheck())
		{
			impactLight.Type = AudioManager.AudioType.MaterialImpact;
			impactMedium.Type = AudioManager.AudioType.MaterialImpact;
			impactHeavy.Type = AudioManager.AudioType.MaterialImpact;
			breakLight.Type = AudioManager.AudioType.MaterialImpact;
			breakMedium.Type = AudioManager.AudioType.MaterialImpact;
			breakHeavy.Type = AudioManager.AudioType.MaterialImpact;
			destroy.Type = AudioManager.AudioType.MaterialImpact;
		}
	}
}
