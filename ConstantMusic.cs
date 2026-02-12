using UnityEngine;

public class ConstantMusic : MonoBehaviour
{
	public static ConstantMusic instance;

	private AudioSource audioSource;

	private bool setup;

	private AudioClip clip;

	private float volume;

	private void Awake()
	{
		instance = this;
		audioSource = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (!setup && GameDirector.instance.currentState == GameDirector.gameState.Main)
		{
			audioSource.clip = clip;
			audioSource.volume = volume;
			audioSource.Play();
			setup = true;
		}
	}

	public void Setup()
	{
		if (!LevelGenerator.Instance.Level.ConstantMusicPreset)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		clip = LevelGenerator.Instance.Level.ConstantMusicPreset.clip;
		volume = LevelGenerator.Instance.Level.ConstantMusicPreset.volume;
	}
}
