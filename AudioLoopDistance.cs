using System.Collections;
using UnityEngine;

public class AudioLoopDistance : MonoBehaviour
{
	private AudioSource audioSource;

	private AudioLowPassLogic audioLowPassLogic;

	private float volumeDefault;

	public ParticleSystem[] particles;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		audioLowPassLogic = GetComponent<AudioLowPassLogic>();
		audioLowPassLogic.Setup();
		volumeDefault = audioSource.volume;
		audioSource.volume = 0f;
		AudioLoopDistanceParticle[] componentsInChildren = GetComponentsInChildren<AudioLoopDistanceParticle>();
		foreach (AudioLoopDistanceParticle audioLoopDistanceParticle in componentsInChildren)
		{
			bool flag = false;
			ParticleSystem[] array = particles;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].transform == audioLoopDistanceParticle.transform)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Debug.LogError("Particle not hooked up to Audio: " + audioLoopDistanceParticle.name, audioLoopDistanceParticle.transform);
			}
		}
		StartCoroutine(Logic());
	}

	private void Start()
	{
		AudioManager.instance.audioLoopDistances.Add(this);
	}

	private void OnDestroy()
	{
		AudioManager.instance.audioLoopDistances.Remove(this);
	}

	public void Restart()
	{
		StopAllCoroutines();
		StartCoroutine(Logic());
	}

	private IEnumerator Logic()
	{
		yield return new WaitForSeconds(0.1f);
		while (true)
		{
			float _distance = Vector3.Distance(AudioManager.instance.AudioListener.transform.position, base.transform.position);
			if (_distance < audioSource.maxDistance + 5f)
			{
				if (!audioSource.isPlaying)
				{
					audioSource.time = (audioSource.clip ? Random.Range(0f, audioSource.clip.length) : 0f);
					audioSource.Play();
					audioLowPassLogic.Setup();
					ParticleSystem[] array = particles;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Play();
					}
				}
				while (audioLowPassLogic.Volume < volumeDefault)
				{
					audioLowPassLogic.Volume += Time.deltaTime;
					yield return null;
				}
			}
			else if (audioSource.isPlaying)
			{
				while (audioLowPassLogic.Volume > 0f)
				{
					audioLowPassLogic.Volume -= Time.deltaTime;
					yield return null;
				}
				audioSource.Stop();
				ParticleSystem[] array = particles;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Stop();
				}
			}
			if (Mathf.Abs(audioSource.maxDistance - _distance) > 20f)
			{
				yield return new WaitForSeconds(Random.Range(3f, 6f));
			}
			else
			{
				yield return new WaitForSeconds(Random.Range(0.5f, 2f));
			}
		}
	}
}
