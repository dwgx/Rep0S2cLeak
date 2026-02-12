using System.Collections.Generic;
using UnityEngine;

public class ItemStunBaton : MonoBehaviour
{
	public Transform stunBatonEffects;

	public Sound stunBatonSound;

	private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	private void Start()
	{
		ParticleSystem[] componentsInChildren = stunBatonEffects.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem item in componentsInChildren)
		{
			particleSystems.Add(item);
		}
	}

	public void PlayParticles()
	{
		stunBatonSound.Play(base.transform.position);
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			particleSystem.Play();
		}
	}
}
