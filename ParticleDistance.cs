using System.Collections;
using UnityEngine;

public class ParticleDistance : MonoBehaviour
{
	public float maxDistance = 20f;

	public bool levelLight;

	private ParticleSystem[] particles;

	private bool active = true;

	private void Awake()
	{
		particles = GetComponentsInChildren<ParticleSystem>();
		StartCoroutine(Logic());
	}

	private void Start()
	{
		LevelGenerator.Instance.particleDistances.Add(this);
	}

	private void OnDestroy()
	{
		LevelGenerator.Instance.particleDistances.Remove(this);
	}

	public void Restart()
	{
		StopAllCoroutines();
		StartCoroutine(Logic());
	}

	private IEnumerator Logic()
	{
		yield return new WaitForSeconds(0.1f);
		ParticleSystem[] array;
		while (!levelLight || !RoundDirector.instance.allExtractionPointsCompleted)
		{
			float num = Vector3.Distance(AudioManager.instance.AudioListener.transform.position, base.transform.position);
			if (num < maxDistance)
			{
				if (!active)
				{
					active = true;
					array = particles;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Play();
					}
				}
			}
			else if (active)
			{
				active = false;
				array = particles;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Stop();
				}
			}
			if (Mathf.Abs(maxDistance - num) > 20f)
			{
				yield return new WaitForSeconds(Random.Range(3f, 6f));
			}
			else
			{
				yield return new WaitForSeconds(Random.Range(0.5f, 2f));
			}
		}
		SemiLogger.LogAxel("by b ye");
		array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
		active = false;
	}
}
