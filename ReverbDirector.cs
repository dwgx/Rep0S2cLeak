using UnityEngine;
using UnityEngine.Audio;

public class ReverbDirector : MonoBehaviour
{
	public static ReverbDirector instance;

	public AudioMixer mixer;

	[Space]
	public ReverbPreset currentPreset;

	[Space]
	public AnimationCurve reverbCurve;

	public float lerpSpeed = 1f;

	private float lerpAmount;

	private float dryLevel;

	private float dryLevelOld;

	private float dryLevelNew;

	private float room;

	private float roomOld;

	private float roomNew;

	private float roomHF;

	private float roomHFOld;

	private float roomHFNew;

	private float decayTime;

	private float decayTimeOld;

	private float decayTimeNew;

	private float decayHFRatio;

	private float decayHFRatioOld;

	private float decayHFRatioNew;

	private float reflections;

	private float reflectionsOld;

	private float reflectionsNew;

	private float reflectDelay;

	private float reflectDelayOld;

	private float reflectDelayNew;

	private float reverb;

	private float reverbOld;

	private float reverbNew;

	private float reverbDelay;

	private float reverbDelayOld;

	private float reverbDelayNew;

	private float diffusion;

	private float diffusionOld;

	private float diffusionNew;

	private float density;

	private float densityOld;

	private float densityNew;

	private float hfReference;

	private float hfReferenceOld;

	private float hfReferenceNew;

	private float roomLF;

	private float roomLFOld;

	private float roomLFNew;

	private float lfReference;

	private float lfReferenceOld;

	private float lfReferenceNew;

	private float spawnTimer;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		Set();
	}

	public void Set()
	{
		dryLevel = currentPreset.dryLevel;
		dryLevelNew = dryLevel;
		dryLevelOld = dryLevel;
		mixer.SetFloat("ReverbDryLevel", dryLevel);
		room = currentPreset.room;
		roomNew = room;
		roomOld = room;
		mixer.SetFloat("ReverbRoom", room);
		roomHF = currentPreset.roomHF;
		roomHFNew = roomHF;
		roomHFOld = roomHF;
		mixer.SetFloat("ReverbRoomHF", roomHF);
		decayTime = currentPreset.decayTime;
		decayTimeNew = decayTime;
		decayTimeOld = decayTime;
		mixer.SetFloat("ReverbDecayTime", decayTime);
		decayHFRatio = currentPreset.decayHFRatio;
		decayHFRatioNew = decayHFRatio;
		decayHFRatioOld = decayHFRatio;
		mixer.SetFloat("ReverbDecayHFRatio", decayHFRatio);
		reflections = currentPreset.reflections;
		reflectionsNew = reflections;
		reflectionsOld = reflections;
		mixer.SetFloat("ReverbReflections", reflections);
		reflectDelay = currentPreset.reflectDelay;
		reflectDelayNew = reflectDelay;
		reflectDelayOld = reflectDelay;
		mixer.SetFloat("ReverbReflectDelay", reflectDelay);
		reverb = currentPreset.reverb;
		reverbNew = reverb;
		reverbOld = reverb;
		mixer.SetFloat("ReverbReverb", reverb);
		reverbDelay = currentPreset.reverbDelay;
		reverbDelayNew = reverbDelay;
		reverbDelayOld = reverbDelay;
		mixer.SetFloat("ReverbReverbDelay", reverbDelay);
		diffusion = currentPreset.diffusion;
		diffusionNew = diffusion;
		diffusionOld = diffusion;
		mixer.SetFloat("ReverbDiffusion", diffusion);
		density = currentPreset.density;
		densityNew = density;
		densityOld = density;
		mixer.SetFloat("ReverbDensity", density);
		hfReference = currentPreset.hfReference;
		hfReferenceNew = hfReference;
		hfReferenceOld = hfReference;
		mixer.SetFloat("ReverbHFReference", hfReference);
		roomLF = currentPreset.roomLF;
		roomLFNew = roomLF;
		roomLFOld = roomLF;
		mixer.SetFloat("ReverbRoomLF", roomLF);
		lfReference = currentPreset.lfReference;
		lfReferenceNew = lfReference;
		lfReferenceOld = lfReference;
		mixer.SetFloat("ReverbLFReference", lfReference);
	}

	private void NewPreset()
	{
		dryLevelOld = dryLevel;
		dryLevelNew = currentPreset.dryLevel;
		roomOld = room;
		roomNew = currentPreset.room;
		roomHFOld = roomHF;
		roomHFNew = currentPreset.roomHF;
		decayTimeOld = decayTime;
		decayTimeNew = currentPreset.decayTime;
		decayHFRatioOld = decayHFRatio;
		decayHFRatioNew = currentPreset.decayHFRatio;
		reflectionsOld = reflections;
		reflectionsNew = currentPreset.reflections;
		reflectDelayOld = reflectDelay;
		reflectDelayNew = currentPreset.reflectDelay;
		reverbOld = reverb;
		reverbNew = currentPreset.reverb;
		reverbDelayOld = reverbDelay;
		reverbDelayNew = currentPreset.reverbDelay;
		diffusionOld = diffusion;
		diffusionNew = currentPreset.diffusion;
		densityOld = density;
		densityNew = currentPreset.density;
		hfReferenceOld = hfReference;
		hfReferenceNew = currentPreset.hfReference;
		roomLFOld = roomLF;
		roomLFNew = currentPreset.roomLF;
		lfReferenceOld = lfReference;
		lfReferenceNew = currentPreset.lfReference;
		lerpAmount = 0f;
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (PlayerController.instance.playerAvatarScript.RoomVolumeCheck.CurrentRooms.Count > 0)
		{
			ReverbPreset reverbPreset = PlayerController.instance.playerAvatarScript.RoomVolumeCheck.CurrentRooms[0].ReverbPreset;
			if ((bool)reverbPreset && reverbPreset != currentPreset)
			{
				currentPreset = reverbPreset;
				NewPreset();
			}
		}
		if (lerpAmount < 1f)
		{
			lerpAmount += lerpSpeed * Time.deltaTime;
			float t = reverbCurve.Evaluate(lerpAmount);
			dryLevel = Mathf.Lerp(dryLevelOld, dryLevelNew, t);
			mixer.SetFloat("ReverbDryLevel", dryLevel);
			room = Mathf.Lerp(roomOld, roomNew, t);
			mixer.SetFloat("ReverbRoom", room);
			roomHF = Mathf.Lerp(roomHFOld, roomHFNew, t);
			mixer.SetFloat("ReverbRoomHF", roomHF);
			decayTime = Mathf.Lerp(decayTimeOld, decayTimeNew, t);
			mixer.SetFloat("ReverbDecayTime", decayTime);
			decayHFRatio = Mathf.Lerp(decayHFRatioOld, decayHFRatioNew, t);
			mixer.SetFloat("ReverbDecayHFRatio", decayHFRatio);
			reflections = Mathf.Lerp(reflectionsOld, reflectionsNew, t);
			mixer.SetFloat("ReverbReflections", reflections);
			reflectDelay = Mathf.Lerp(reflectDelayOld, reflectDelayNew, t);
			mixer.SetFloat("ReverbReflectDelay", reflectDelay);
			reverb = Mathf.Lerp(reverbOld, reverbNew, t);
			mixer.SetFloat("ReverbReverb", reverb);
			reverbDelay = Mathf.Lerp(reverbDelayOld, reverbDelayNew, t);
			mixer.SetFloat("ReverbReverbDelay", reverbDelay);
			diffusion = Mathf.Lerp(diffusionOld, diffusionNew, t);
			mixer.SetFloat("ReverbDiffusion", diffusion);
			density = Mathf.Lerp(densityOld, densityNew, t);
			mixer.SetFloat("ReverbDensity", density);
			hfReference = Mathf.Lerp(hfReferenceOld, hfReferenceNew, t);
			mixer.SetFloat("ReverbHFReference", hfReference);
			roomLF = Mathf.Lerp(roomLFOld, roomLFNew, t);
			mixer.SetFloat("ReverbRoomLF", roomLF);
			lfReference = Mathf.Lerp(lfReferenceOld, lfReferenceNew, t);
			mixer.SetFloat("ReverbLFReference", lfReference);
		}
	}
}
