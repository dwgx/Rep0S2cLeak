using UnityEngine;

public class ParticlesOnDestroy : MonoBehaviour
{
	public ParticleSystem[] destroyParticles;

	public void DestroyParticles()
	{
		if (destroyParticles.Length != 0)
		{
			ParticleSystem[] array = destroyParticles;
			foreach (ParticleSystem obj in array)
			{
				obj.gameObject.transform.parent = null;
				obj.Play();
			}
		}
	}
}
