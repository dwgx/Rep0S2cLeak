using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Sound
{
	public AudioSource Source;

	public AudioClip[] Sounds;

	private AudioLowPassLogic LowPassLogic;

	private bool HasLowPassLogic;

	public AudioManager.AudioType Type;

	[Range(0f, 1f)]
	public float Volume = 0.5f;

	internal float VolumeDefault = 0.5f;

	[Range(0f, 1f)]
	public float VolumeRandom = 0.1f;

	[Range(0f, 5f)]
	public float Pitch = 1f;

	[Range(0f, 2f)]
	public float PitchRandom = 0.1f;

	[Range(0f, 1f)]
	public float SpatialBlend = 1f;

	[Range(0f, 5f)]
	public float Doppler = 1f;

	[Range(0f, 1f)]
	public float ReverbMix = 1f;

	[Range(0f, 5f)]
	public float FalloffMultiplier = 1f;

	[Space]
	[Range(0f, 1f)]
	public float OffscreenVolume = 1f;

	[Range(0f, 1f)]
	public float OffscreenFalloff = 1f;

	[Space]
	public List<Collider> LowPassIgnoreColliders = new List<Collider>();

	private AudioClip LoopClip;

	internal float LoopVolume;

	internal float LoopVolumeCurrent;

	internal float LoopVolumeFinal;

	internal float LoopPitch;

	private bool LoopFalloffSetup;

	private float LoopFalloffMin;

	private float LoopFalloffMax;

	internal float LoopFalloff;

	internal float LoopFalloffFinal;

	private float LoopOffScreenTime = 0.25f;

	private float LoopOffScreenTimer;

	private bool LoopOffScreen;

	private float LoopOffScreenVolume;

	private float LoopOffScreenFalloff;

	internal float StartTimeOverride = 999999f;

	private bool AudioInfoFetched;

	public AudioSource Play(Vector3 position, float volumeMultiplier = 1f, float falloffMultiplier = 1f, float offscreenVolumeMultiplier = 1f, float offscreenFalloffMultiplier = 1f)
	{
		if (Sounds.Length == 0)
		{
			return null;
		}
		AudioClip audioClip = Sounds[UnityEngine.Random.Range(0, Sounds.Length)];
		float num = Pitch + UnityEngine.Random.Range(0f - PitchRandom, PitchRandom);
		AudioSource audioSource = Source;
		if (!audioSource)
		{
			GameObject gameObject = AudioManager.instance.AudioDefault;
			switch (Type)
			{
			case AudioManager.AudioType.HighFalloff:
				gameObject = AudioManager.instance.AudioHighFalloff;
				break;
			case AudioManager.AudioType.Footstep:
				gameObject = AudioManager.instance.AudioFootstep;
				break;
			case AudioManager.AudioType.MaterialImpact:
				gameObject = AudioManager.instance.AudioMaterialImpact;
				break;
			case AudioManager.AudioType.Cutscene:
				gameObject = AudioManager.instance.AudioCutscene;
				break;
			case AudioManager.AudioType.AmbienceBreaker:
				gameObject = AudioManager.instance.AudioAmbienceBreaker;
				break;
			case AudioManager.AudioType.LowFalloff:
				gameObject = AudioManager.instance.AudioLowFalloff;
				break;
			case AudioManager.AudioType.Global:
				gameObject = AudioManager.instance.AudioGlobal;
				break;
			case AudioManager.AudioType.HigherFalloff:
				gameObject = AudioManager.instance.AudioHigherFalloff;
				break;
			case AudioManager.AudioType.Attack:
				gameObject = AudioManager.instance.AudioAttack;
				break;
			case AudioManager.AudioType.Persistent:
				gameObject = AudioManager.instance.AudioPersistent;
				break;
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, AudioManager.instance.SoundsParent);
			if ((bool)audioClip)
			{
				gameObject2.gameObject.name = audioClip.name;
			}
			audioSource = gameObject2.GetComponent<AudioSource>();
			if (!audioClip || gameObject != AudioManager.instance.AudioPersistent)
			{
				if ((bool)audioClip)
				{
					UnityEngine.Object.Destroy(gameObject2, audioClip.length / num);
				}
				else
				{
					UnityEngine.Object.Destroy(gameObject2);
				}
			}
		}
		else if (!audioSource.enabled)
		{
			return null;
		}
		audioSource.minDistance *= FalloffMultiplier;
		audioSource.minDistance *= falloffMultiplier;
		audioSource.maxDistance *= FalloffMultiplier;
		audioSource.maxDistance *= falloffMultiplier;
		audioSource.clip = Sounds[UnityEngine.Random.Range(0, Sounds.Length)];
		audioSource.volume = (Volume + UnityEngine.Random.Range(0f - VolumeRandom, VolumeRandom)) * volumeMultiplier;
		if (SpatialBlend > 0f && (OffscreenVolume * offscreenVolumeMultiplier < 1f || OffscreenFalloff * offscreenFalloffMultiplier < 1f) && !SemiFunc.OnScreen(audioSource.transform.position, 0.1f, 0.1f))
		{
			audioSource.volume *= OffscreenVolume * offscreenVolumeMultiplier;
			audioSource.minDistance *= OffscreenFalloff * offscreenFalloffMultiplier;
			audioSource.maxDistance *= OffscreenFalloff * offscreenFalloffMultiplier;
		}
		audioSource.spatialBlend = SpatialBlend;
		audioSource.reverbZoneMix = ReverbMix;
		audioSource.dopplerLevel = Doppler;
		audioSource.pitch = num;
		audioSource.loop = false;
		if (SpatialBlend > 0f)
		{
			StartLowPass(audioSource);
		}
		audioSource.Play();
		return audioSource;
	}

	public void StoreDefault()
	{
		VolumeDefault = Volume;
	}

	public void Stop()
	{
		Source.Stop();
	}

	private void StartLowPass(AudioSource source)
	{
		LowPassLogic = source.GetComponent<AudioLowPassLogic>();
		if ((bool)LowPassLogic)
		{
			if (LowPassIgnoreColliders.Count > 0)
			{
				LowPassLogic.LowPassIgnoreColliders.AddRange(LowPassIgnoreColliders);
			}
			LowPassLogic.Setup();
			HasLowPassLogic = true;
		}
	}

	public void PlayLoop(bool playing, float fadeInSpeed, float fadeOutSpeed, float pitchMultiplier = 1f)
	{
		if (Sounds.Length == 0)
		{
			return;
		}
		if (!AudioInfoFetched)
		{
			LoopClip = Sounds[UnityEngine.Random.Range(0, Sounds.Length)];
			Source.clip = LoopClip;
		}
		if (playing)
		{
			if (!Source.isPlaying)
			{
				LoopVolume = Volume + UnityEngine.Random.Range(0f - VolumeRandom, VolumeRandom);
				LoopPitch = Pitch + UnityEngine.Random.Range(0f - PitchRandom, PitchRandom);
				LoopVolumeCurrent = 0f;
				LoopVolumeFinal = LoopVolumeCurrent;
				Source.volume = LoopVolumeCurrent;
				Source.pitch = LoopPitch * pitchMultiplier;
				Source.spatialBlend = SpatialBlend;
				Source.reverbZoneMix = ReverbMix;
				Source.dopplerLevel = Doppler;
				if (!LoopFalloffSetup)
				{
					Source.minDistance *= FalloffMultiplier;
					Source.maxDistance *= FalloffMultiplier;
					LoopFalloffMin = Source.minDistance;
					LoopFalloffMax = Source.maxDistance;
					LoopFalloff = Source.maxDistance;
					LoopFalloffSetup = true;
				}
				else
				{
					Source.minDistance = LoopFalloffMin;
					Source.maxDistance = LoopFalloffMax;
				}
				Source.time = (Source.clip ? UnityEngine.Random.Range(0f, Source.clip.length) : 0f);
				if (StartTimeOverride != 999999f)
				{
					Source.time = StartTimeOverride;
				}
				Source.loop = true;
				StartLowPass(Source);
				if (Source.gameObject.activeInHierarchy)
				{
					Source.Play();
				}
			}
			else
			{
				LoopVolumeCurrent += fadeInSpeed * Time.deltaTime;
				LoopVolumeCurrent = Mathf.Clamp(LoopVolumeCurrent, 0f, LoopVolume);
				LoopOffScreenLogic();
				Source.pitch = LoopPitch * pitchMultiplier;
				if (HasLowPassLogic)
				{
					LowPassLogic.Volume = LoopVolumeFinal;
				}
				else
				{
					Source.volume = LoopVolumeFinal;
				}
			}
		}
		else if (Source.isPlaying)
		{
			LoopVolumeCurrent -= fadeOutSpeed * Time.deltaTime;
			LoopVolumeCurrent = Mathf.Clamp(LoopVolumeCurrent, 0f, LoopVolume);
			LoopOffScreenLogic();
			Source.pitch = LoopPitch * pitchMultiplier;
			if (HasLowPassLogic)
			{
				LowPassLogic.Volume = LoopVolumeFinal;
			}
			else
			{
				Source.volume = LoopVolumeFinal;
			}
			if (LoopVolumeFinal <= 0f)
			{
				Source.Stop();
			}
		}
	}

	private void LoopOffScreenLogic()
	{
		LoopVolumeFinal = LoopVolumeCurrent;
		if (!(SpatialBlend > 0f) || (!(OffscreenVolume < 1f) && !(OffscreenFalloff < 1f)))
		{
			return;
		}
		if (LoopOffScreenTimer <= 0f)
		{
			LoopOffScreenTimer = LoopOffScreenTime;
			LoopOffScreen = !SemiFunc.OnScreen(Source.transform.position, 0.1f, 0.1f);
		}
		else
		{
			LoopOffScreenTimer -= Time.deltaTime;
		}
		if (OffscreenVolume < 1f)
		{
			if (LoopOffScreen)
			{
				LoopOffScreenVolume = Mathf.Lerp(LoopOffScreenVolume, OffscreenVolume, 15f * Time.deltaTime);
			}
			else
			{
				LoopOffScreenVolume = Mathf.Lerp(LoopOffScreenVolume, 1f, 15f * Time.deltaTime);
			}
			LoopVolumeFinal *= LoopOffScreenVolume;
		}
		if (OffscreenFalloff < 1f)
		{
			if (LoopOffScreen)
			{
				LoopFalloffFinal = Mathf.Lerp(LoopFalloffFinal, LoopFalloff * OffscreenFalloff, 15f * Time.deltaTime);
			}
			else
			{
				LoopFalloffFinal = Mathf.Lerp(LoopFalloffFinal, LoopFalloff, 15f * Time.deltaTime);
			}
			if (HasLowPassLogic)
			{
				LowPassLogic.Falloff = LoopFalloffFinal;
			}
			else
			{
				Source.maxDistance = LoopFalloffFinal;
			}
		}
	}

	public static void CopySound(Sound from, Sound to)
	{
		to.Source = from.Source;
		to.Sounds = from.Sounds;
		to.Type = from.Type;
		to.Volume = from.Volume;
		to.VolumeRandom = from.VolumeRandom;
		to.Pitch = from.Pitch;
		to.PitchRandom = from.PitchRandom;
		to.SpatialBlend = from.SpatialBlend;
		to.ReverbMix = from.ReverbMix;
		to.Doppler = from.Doppler;
	}
}
