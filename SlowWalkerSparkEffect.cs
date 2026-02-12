using System.Collections.Generic;
using UnityEngine;

public class SlowWalkerSparkEffect : MonoBehaviour
{
	private float playSparkEffectTimer;

	private bool isPlayingSparkEffect;

	private List<ParticleSystem> sparkEffects;

	private void Start()
	{
		sparkEffects = new List<ParticleSystem>();
		sparkEffects.AddRange(GetComponentsInChildren<ParticleSystem>());
	}

	private void Update()
	{
		if (playSparkEffectTimer <= 0f && isPlayingSparkEffect)
		{
			ToggleSparkEffect(toggle: false);
			isPlayingSparkEffect = false;
		}
		if (playSparkEffectTimer > 0f)
		{
			if (!isPlayingSparkEffect)
			{
				ToggleSparkEffect(toggle: true);
				isPlayingSparkEffect = true;
			}
			playSparkEffectTimer -= Time.deltaTime;
		}
	}

	private void ToggleSparkEffect(bool toggle)
	{
		foreach (ParticleSystem sparkEffect in sparkEffects)
		{
			if (toggle)
			{
				sparkEffect.Play();
			}
			else
			{
				sparkEffect.Stop();
			}
		}
	}

	public void PlaySparkEffect()
	{
		playSparkEffectTimer = 0.2f;
	}

	public void StopSparkEffect()
	{
		playSparkEffectTimer = 0f;
	}
}
