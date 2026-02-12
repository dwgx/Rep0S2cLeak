using UnityEngine;

public class AmbienceLoopOverride : MonoBehaviour
{
	internal LevelAmbience ambience;

	private AudioSource source;

	internal float volumeRoomAmbienceCurrent;

	internal float volumeRoomAmbienceNew;

	internal float volumeRoomAmbienceTarget;

	internal float volumeRoomAmbiencePrevious;

	internal float volumeRoomAmbienceLerp;

	private float volume;

	private float volumeLerp;

	private float activeTimer = 1f;

	private void Awake()
	{
		source = GetComponent<AudioSource>();
	}

	private void Start()
	{
		volume = ambience.loopVolume;
		source.clip = ambience.loopClip;
		source.volume = 0f;
		source.loop = true;
		source.Play();
	}

	private void Update()
	{
		if (volumeRoomAmbienceNew != volumeRoomAmbienceTarget)
		{
			volumeRoomAmbiencePrevious = volumeRoomAmbienceCurrent;
			volumeRoomAmbienceTarget = volumeRoomAmbienceNew;
			volumeRoomAmbienceLerp = 0f;
		}
		volumeRoomAmbienceLerp += 0.5f * Time.deltaTime;
		volumeRoomAmbienceLerp = Mathf.Clamp01(volumeRoomAmbienceLerp);
		volumeRoomAmbienceCurrent = Mathf.Lerp(volumeRoomAmbiencePrevious, volumeRoomAmbienceTarget, volumeRoomAmbienceLerp);
		if (activeTimer <= 0f)
		{
			volumeLerp -= AmbienceLoop.instance.overrideLoopFadeTime * Time.deltaTime;
			volumeLerp = Mathf.Clamp01(volumeLerp);
		}
		else
		{
			volumeLerp += AmbienceLoop.instance.overrideLoopFadeTime * Time.deltaTime;
			volumeLerp = Mathf.Clamp01(volumeLerp);
			activeTimer -= Time.deltaTime;
		}
		source.volume = Mathf.Lerp(0f, volume * volumeRoomAmbienceCurrent, AmbienceLoop.instance.roomCurve.Evaluate(volumeLerp));
		if (volumeLerp <= 0f && source.isPlaying)
		{
			AmbienceLoop.instance.overrideLoops.Remove(this);
			Object.Destroy(base.gameObject);
		}
	}

	public void Play()
	{
		activeTimer = 0.1f;
	}
}
