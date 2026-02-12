using System.Collections.Generic;
using UnityEngine;

public class EnemySpinnyParticle : MonoBehaviour
{
	private ParticleSystem _particleSystem;

	public Gradient particleColor = new Gradient();

	private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	public bool playOnStart;

	private void Start()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		particleSystems = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
		if (_particleSystem != null && !particleSystems.Contains(_particleSystem))
		{
			particleSystems.Add(_particleSystem);
		}
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			ParticleSystem.MainModule main = particleSystem.main;
			if (main.startColor.gradient == null)
			{
				main.startColor = particleColor;
			}
			main.startColor = ChangeColorsOfGradient(main.startColor.gradient, particleColor);
			ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
			if (colorOverLifetime.color.gradient == null)
			{
				colorOverLifetime.color = particleColor;
			}
			ParticleSystem.MinMaxGradient colorOverTrail = particleSystem.trails.colorOverTrail;
			if (colorOverTrail.gradient == null)
			{
				colorOverTrail.gradient = particleColor;
			}
			colorOverLifetime.color = ChangeColorsOfGradient(colorOverLifetime.color.gradient, particleColor);
			colorOverTrail.gradient = ChangeColorsOfGradient(colorOverTrail.gradient, particleColor);
		}
		if (playOnStart)
		{
			PlayParticles();
		}
	}

	private Gradient ChangeColorsOfGradient(Gradient gradient, Gradient newColor)
	{
		return new Gradient
		{
			colorKeys = newColor.colorKeys,
			alphaKeys = gradient.alphaKeys
		};
	}

	public void PlayParticles()
	{
		if ((bool)_particleSystem)
		{
			_particleSystem.Play(withChildren: true);
		}
		else
		{
			Debug.LogWarning("Particle system not found on " + base.gameObject.name);
		}
	}
}
