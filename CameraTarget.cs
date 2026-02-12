using UnityEngine;

public class CameraTarget : MonoBehaviour
{
	[HideInInspector]
	public bool targetActiveImpulse;

	[HideInInspector]
	public bool targetActive;

	[HideInInspector]
	public float targetLerpAmount;

	public float targetLerpSpeed = 0.01f;

	public AnimationCurve targetLerpCurve;

	public AnimNoise camNoise;

	public AudioPlay targetToggleAudio;

	private void Update()
	{
		if (targetActiveImpulse)
		{
			if (targetActive)
			{
				if (targetActive)
				{
					targetToggleAudio.Play(1f);
					camNoise.NoiseOverride(1.5f, 1f, 2f, 0.5f, 0.5f);
				}
				targetActive = false;
			}
			else
			{
				if (!targetActive)
				{
					targetToggleAudio.Play(1f);
					camNoise.NoiseOverride(1.5f, 1f, 2f, 0.5f, 0.5f);
				}
				targetActive = true;
			}
			targetActiveImpulse = false;
		}
		if (targetActive)
		{
			targetLerpAmount = Mathf.Clamp(targetLerpAmount + targetLerpSpeed * Time.deltaTime, 0f, 1f);
		}
		else
		{
			targetLerpAmount = Mathf.Clamp(targetLerpAmount - targetLerpSpeed * Time.deltaTime, 0f, 1f);
		}
	}
}
