using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public enum AudioType
	{
		Default,
		HighFalloff,
		Footstep,
		MaterialImpact,
		Cutscene,
		AmbienceBreaker,
		LowFalloff,
		Global,
		HigherFalloff,
		Attack,
		Persistent
	}

	public enum SoundSnapshot
	{
		Off,
		On,
		Spectate,
		CutsceneOnly
	}

	public static AudioManager instance;

	public Transform SoundsParent;

	public Transform MusicParent;

	[Space]
	public AudioMixer MasterMixer;

	public AudioMixerGroup PersistentSoundGroup;

	public AudioMixerGroup SoundMasterGroup;

	public AudioMixerGroup MusicMasterGroup;

	public AudioMixerGroup MicrophoneSoundGroup;

	public AudioMixerGroup MicrophoneSpectateGroup;

	public AudioMixerGroup TTSSoundGroup;

	public AudioMixerGroup TTSSpectateGroup;

	public AnimationCurve VolumeCurve;

	[Space]
	public AudioListenerFollow AudioListener;

	[Space]
	public float lowpassValueMin = 1000f;

	public float lowpassValueMax = 22000f;

	public GameObject AudioDefault;

	public GameObject AudioHighFalloff;

	public GameObject AudioFootstep;

	public GameObject AudioMaterialImpact;

	public GameObject AudioCutscene;

	public GameObject AudioAmbienceBreaker;

	public GameObject AudioMaterialSlidingLoop;

	public GameObject AudioLowFalloff;

	public GameObject AudioGlobal;

	public GameObject AudioHigherFalloff;

	public GameObject AudioAttack;

	public GameObject AudioPersistent;

	public AudioMixerSnapshot volumeOff;

	public AudioMixerSnapshot volumeOn;

	public AudioMixerSnapshot volumeSpectate;

	public AudioMixerSnapshot volumeCutsceneOnly;

	public List<LevelAmbience> levelAmbiences;

	private SoundSnapshot currentSnapshot;

	internal List<AudioLoopDistance> audioLoopDistances = new List<AudioLoopDistance>();

	internal bool pushToTalk;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		UpdateAll();
	}

	private void Update()
	{
		MasterMixer.SetFloat("Volume", Mathf.Lerp(-80f, 0f, VolumeCurve.Evaluate((float)DataDirector.instance.SettingValueFetch(DataDirector.Setting.MasterVolume) * 0.01f)));
		MusicMasterGroup.audioMixer.SetFloat("MusicVolume", Mathf.Lerp(-80f, 0f, VolumeCurve.Evaluate((float)DataDirector.instance.SettingValueFetch(DataDirector.Setting.MusicVolume) * 0.01f)));
		float value = Mathf.Lerp(-80f, 0f, VolumeCurve.Evaluate((float)DataDirector.instance.SettingValueFetch(DataDirector.Setting.SfxVolume) * 0.01f));
		SoundMasterGroup.audioMixer.SetFloat("SoundVolume", value);
		PersistentSoundGroup.audioMixer.SetFloat("PersistentVolume", value);
		float value2 = Mathf.Lerp(-80f, 0f, VolumeCurve.Evaluate((float)DataDirector.instance.SettingValueFetch(DataDirector.Setting.ProximityVoice) * 0.01f));
		MicrophoneSoundGroup.audioMixer.SetFloat("MicrophoneVolume", value2);
		MicrophoneSpectateGroup.audioMixer.SetFloat("MicrophoneVolume", value2);
		float value3 = Mathf.Lerp(-80f, 0f, VolumeCurve.Evaluate((float)DataDirector.instance.SettingValueFetch(DataDirector.Setting.TextToSpeechVolume) * 0.01f));
		TTSSoundGroup.audioMixer.SetFloat("TTSVolume", value3);
		TTSSpectateGroup.audioMixer.SetFloat("TTSVolume", value3);
	}

	public void UpdateAll()
	{
		UpdatePushToTalk();
	}

	public void SetSoundSnapshot(SoundSnapshot _snapShot, float _transitionTime)
	{
		if (_snapShot != currentSnapshot)
		{
			currentSnapshot = _snapShot;
			switch (_snapShot)
			{
			case SoundSnapshot.Off:
				volumeOff.TransitionTo(_transitionTime);
				break;
			case SoundSnapshot.On:
				volumeOn.TransitionTo(_transitionTime);
				break;
			case SoundSnapshot.Spectate:
				volumeSpectate.TransitionTo(_transitionTime);
				break;
			case SoundSnapshot.CutsceneOnly:
				volumeCutsceneOnly.TransitionTo(_transitionTime);
				break;
			}
		}
	}

	public void RestartAudioLoopDistances()
	{
		foreach (AudioLoopDistance audioLoopDistance in audioLoopDistances)
		{
			if (audioLoopDistance.isActiveAndEnabled)
			{
				audioLoopDistance.Restart();
			}
		}
	}

	public void UpdatePushToTalk()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.PushToTalk) == 1)
		{
			pushToTalk = true;
		}
		else
		{
			pushToTalk = false;
		}
	}
}
