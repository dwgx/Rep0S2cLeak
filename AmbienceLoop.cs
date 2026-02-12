using System.Collections.Generic;
using UnityEngine;

public class AmbienceLoop : MonoBehaviour
{
	public static AmbienceLoop instance;

	public AudioSource source;

	private AudioClip clip;

	private float volume;

	[Space]
	public AnimationCurve roomCurve;

	public float roomLerpSpeed = 1f;

	private float roomLerpAmount;

	private float roomVolumePrevious;

	internal float roomVolumeCurrent;

	private RoomAmbience roomAmbience;

	[Space]
	public GameObject overrideObject;

	public List<AmbienceLoopOverride> overrideLoops = new List<AmbienceLoopOverride>();

	private float overrideLoopLerp;

	internal float overrideLoopFadeTime = 0.3f;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		bool flag = false;
		RoomVolumeCheck roomVolumeCheck = PlayerController.instance.playerAvatarScript.RoomVolumeCheck;
		if (roomVolumeCheck.CurrentRooms.Count > 0)
		{
			RoomAmbience roomAmbience = roomVolumeCheck.CurrentRooms[0].RoomAmbience;
			if ((bool)roomAmbience && roomAmbience != this.roomAmbience)
			{
				this.roomAmbience = roomAmbience;
				roomLerpAmount = 0f;
				roomVolumePrevious = roomVolumeCurrent;
			}
			LevelAmbience roomAmbienceOverride = roomVolumeCheck.CurrentRooms[0].RoomAmbienceOverride;
			if ((bool)roomAmbienceOverride)
			{
				bool flag2 = false;
				foreach (AmbienceLoopOverride overrideLoop in overrideLoops)
				{
					if (overrideLoop.ambience == roomAmbienceOverride)
					{
						flag2 = true;
						overrideLoop.Play();
						if ((bool)this.roomAmbience)
						{
							overrideLoop.volumeRoomAmbienceNew = this.roomAmbience.volume;
						}
						break;
					}
				}
				if (!flag2)
				{
					GameObject obj = Object.Instantiate(overrideObject, base.transform);
					AmbienceLoopOverride component = obj.GetComponent<AmbienceLoopOverride>();
					component.ambience = roomAmbienceOverride;
					overrideLoops.Add(component);
					obj.gameObject.name = roomAmbienceOverride.name;
					if ((bool)this.roomAmbience)
					{
						component.volumeRoomAmbienceNew = this.roomAmbience.volume;
						component.volumeRoomAmbiencePrevious = this.roomAmbience.volume;
						component.volumeRoomAmbienceCurrent = this.roomAmbience.volume;
						component.volumeRoomAmbienceLerp = 1f;
					}
				}
				flag = true;
			}
		}
		if ((bool)this.roomAmbience)
		{
			if (roomLerpAmount < 1f)
			{
				roomLerpAmount += roomLerpSpeed * Time.deltaTime;
				roomLerpAmount = Mathf.Clamp01(roomLerpAmount);
				roomVolumeCurrent = Mathf.Lerp(roomVolumePrevious, this.roomAmbience.volume, roomCurve.Evaluate(roomLerpAmount));
			}
			if (flag)
			{
				overrideLoopLerp += overrideLoopFadeTime * Time.deltaTime;
				overrideLoopLerp = Mathf.Clamp01(overrideLoopLerp);
			}
			else
			{
				overrideLoopLerp -= overrideLoopFadeTime * Time.deltaTime;
				overrideLoopLerp = Mathf.Clamp01(overrideLoopLerp);
			}
			source.volume = Mathf.Lerp(volume * roomVolumeCurrent, 0f, roomCurve.Evaluate(overrideLoopLerp));
		}
	}

	public void Setup()
	{
		foreach (LevelAmbience ambiencePreset in LevelGenerator.Instance.Level.AmbiencePresets)
		{
			if ((bool)ambiencePreset && (bool)ambiencePreset.loopClip)
			{
				clip = ambiencePreset.loopClip;
				volume = ambiencePreset.loopVolume;
			}
		}
		source.clip = clip;
		source.volume = 0f;
		source.loop = true;
		source.Play();
	}

	public void LiveUpdate()
	{
		foreach (LevelAmbience ambiencePreset in LevelGenerator.Instance.Level.AmbiencePresets)
		{
			if ((bool)ambiencePreset.loopClip)
			{
				clip = ambiencePreset.loopClip;
				volume = ambiencePreset.loopVolume;
			}
		}
		source.volume = volume;
	}
}
