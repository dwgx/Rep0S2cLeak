using UnityEngine;

[CreateAssetMenu(fileName = "HingeAudio - _____", menuName = "Audio/Hinge Audio Preset", order = 1)]
public class HingeAudio : ScriptableObject
{
	[Header("Move Loop")]
	public bool moveLoopEnabled = true;

	public float moveLoopVelocityMult = 1f;

	public float moveLoopThreshold = 1f;

	public float moveLoopFadeInSpeed = 5f;

	public float moveLoopFadeOutSpeed = 5f;

	public Sound moveLoop;

	public Sound moveLoopEnd;

	[Header("One Shots")]
	public Sound Close;

	public Sound CloseHeavy;

	public Sound Open;

	public Sound OpenHeavy;

	public Sound HingeBreak;

	private void OnValidate()
	{
		if (!SemiFunc.OnValidateCheck())
		{
			moveLoop.Type = AudioManager.AudioType.MaterialImpact;
			moveLoopEnd.Type = AudioManager.AudioType.MaterialImpact;
		}
	}
}
