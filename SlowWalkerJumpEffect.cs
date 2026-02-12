using System.Collections.Generic;
using UnityEngine;

public class SlowWalkerJumpEffect : MonoBehaviour
{
	public Transform rotationTransform;

	private List<ParticleSystem> particles = new List<ParticleSystem>();

	private void Start()
	{
		particles.AddRange(GetComponentsInChildren<ParticleSystem>());
	}

	private void PlayParticles()
	{
		foreach (ParticleSystem particle in particles)
		{
			particle.Play();
		}
	}

	public void JumpEffect()
	{
		rotationTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
		PlayParticles();
		GameDirector.instance.CameraImpact.ShakeDistance(4f, 6f, 15f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(4f, 6f, 15f, base.transform.position, 0.1f);
	}

	public void LandEffect()
	{
		rotationTransform.rotation = Quaternion.Euler(0f, 180f, 0f);
		GameDirector.instance.CameraImpact.ShakeDistance(6f, 6f, 15f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(6f, 6f, 15f, base.transform.position, 0.1f);
		PlayParticles();
	}
}
