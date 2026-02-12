using UnityEngine;

public class CameraCrouchNoise : MonoBehaviour
{
	private PlayerController Player;

	public AnimNoise AnimNoise;

	public float Strength = 1f;

	public float LerpSpeed = 2f;

	private void Start()
	{
		Player = PlayerController.instance;
		AnimNoise.MasterAmount = 0f;
		AnimNoise.enabled = false;
	}

	private void Update()
	{
		if (!SpectateCamera.instance && Player.Crouching && !RecordingDirector.instance)
		{
			AnimNoise.enabled = true;
			AnimNoise.MasterAmount = Mathf.Lerp(AnimNoise.MasterAmount, Strength * GameplayManager.instance.cameraNoise, Time.deltaTime * LerpSpeed);
		}
		else if (AnimNoise.enabled)
		{
			AnimNoise.MasterAmount = Mathf.Lerp(AnimNoise.MasterAmount, 0f, Time.deltaTime * LerpSpeed);
			if (AnimNoise.MasterAmount < 0.001f)
			{
				AnimNoise.enabled = false;
			}
		}
	}
}
