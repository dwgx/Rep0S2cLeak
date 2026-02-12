using UnityEngine;

public class AudioScare : MonoBehaviour
{
	public static AudioScare instance;

	private AudioSource source;

	public AudioClip[] impactSounds;

	public AudioClip[] softSounds;

	private void Awake()
	{
		instance = this;
		source = GetComponent<AudioSource>();
	}

	public void PlayImpact()
	{
		if (base.isActiveAndEnabled && GameDirector.instance.currentState == GameDirector.gameState.Main)
		{
			LevelMusic.instance.Interrupt(20f);
			source.volume = 1f;
			source.clip = impactSounds[Random.Range(0, impactSounds.Length)];
			source.Play();
		}
	}

	public void PlaySoft()
	{
		if (base.isActiveAndEnabled && GameDirector.instance.currentState == GameDirector.gameState.Main)
		{
			LevelMusic.instance.Interrupt(20f);
			source.volume = 1f;
			source.clip = softSounds[Random.Range(0, softSounds.Length)];
			source.Play();
		}
	}

	public void PlayCustom(AudioClip _sound, float _volume = 0.3f, float _interrupt = 20f)
	{
		if (base.isActiveAndEnabled)
		{
			LevelMusic.instance.Interrupt(_interrupt);
			source.Stop();
			source.volume = _volume;
			source.clip = _sound;
			source.Play();
		}
	}
}
