using UnityEngine;

public class CameraNoise : MonoBehaviour
{
	public static CameraNoise Instance;

	public float StrengthDefault = 0.2f;

	public AnimNoise AnimNoise;

	private float Strength = 1f;

	private float OverrideStrength = 1f;

	private float OverrideTimer;

	private void Awake()
	{
		Instance = this;
		Strength = StrengthDefault;
	}

	private void Update()
	{
		if (OverrideTimer > 0f)
		{
			OverrideTimer -= Time.deltaTime;
			if (OverrideTimer <= 0f)
			{
				OverrideTimer = 0f;
			}
			else
			{
				Strength = Mathf.Lerp(Strength, OverrideStrength * GameplayManager.instance.cameraNoise, 5f * Time.deltaTime);
			}
			AnimNoise.noiseStrengthDefault = Strength;
		}
		else if (Mathf.Abs(Strength - StrengthDefault * GameplayManager.instance.cameraNoise) > 0.001f)
		{
			Strength = Mathf.Lerp(Strength, StrengthDefault * GameplayManager.instance.cameraNoise, 5f * Time.deltaTime);
		}
		AnimNoise.noiseStrengthDefault = Strength;
	}

	public void Override(float strength, float time)
	{
		OverrideStrength = strength;
		OverrideTimer = time;
	}
}
