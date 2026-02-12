using UnityEngine;

public class MaterialParticles : MonoBehaviour
{
	private ParticleSystem[] particleSystems;

	private void Awake()
	{
		Materials.Instance.Particles.Add(base.gameObject);
		particleSystems = GetComponentsInChildren<ParticleSystem>();
	}

	private void Update()
	{
		if (!SemiFunc.FPSImpulse5())
		{
			return;
		}
		bool flag = true;
		ParticleSystem[] array = particleSystems;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].isPlaying)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		Materials.Instance.Particles.Remove(base.gameObject);
	}
}
