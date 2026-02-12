using UnityEngine;

public class AudioPlay : MonoBehaviour
{
	public AudioSource source;

	public AudioClip[] sounds;

	[Range(0f, 1f)]
	public float volume = 0.5f;

	[Range(0f, 1f)]
	public float volumeRandom = 0.1f;

	[Range(0f, 5f)]
	public float pitch = 1f;

	[Range(0f, 2f)]
	public float pitchRandom = 0.1f;

	public bool playImpulse;

	public void Play(float volumeMult)
	{
		source.clip = sounds[Random.Range(0, sounds.Length)];
		source.volume = (volume + Random.Range(0f - volumeRandom, volumeRandom)) * volumeMult;
		source.pitch = pitch + Random.Range(0f - pitchRandom, pitchRandom);
		source.PlayOneShot(source.clip);
	}

	public void Update()
	{
		if (playImpulse)
		{
			Play(1f);
			playImpulse = false;
		}
	}
}
