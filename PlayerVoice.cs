using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVoice : MonoBehaviour
{
	public static PlayerVoice Instance;

	public PlayerController Player;

	internal AudioSource CurrentVoiceSource;

	private List<AudioSource> VoicesToFade = new List<AudioSource>();

	[Space]
	public Sound CrouchLoop;

	public float CrouchLoopDistanceMax = 10f;

	public float CrouchLoopDistanceMin = 1f;

	public float CrouchLoopVolumeMax = 1f;

	public float CrouchLoopVolumeMin = 1f;

	private float CrouchLoopVolume;

	[Space]
	public Sound CrouchHush;

	[Space]
	private float VoicePauseTimer;

	private Enemy LevelEnemy;

	[Space]
	public Sound SprintStop;

	public Sound SprintLoop;

	public float SprintVolume;

	public float SprintVolumeSpeed;

	private bool SprintLoopPlaying;

	private float SprintingTimer;

	private float SprintLoopLerp;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		StartCoroutine(EnemySetup());
	}

	private IEnumerator EnemySetup()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
	}

	public void PlayCrouchHush()
	{
		if (CurrentVoiceSource != null && CurrentVoiceSource.isPlaying)
		{
			VoicesToFade.Add(CurrentVoiceSource);
			CurrentVoiceSource = null;
		}
		SprintingTimer = 0f;
		SprintLoop.PlayLoop(playing: false, 1f, 50f);
		CurrentVoiceSource = CrouchHush.Play(base.transform.position);
		VoicePauseTimer = CurrentVoiceSource.clip.length * 1.2f;
	}

	private void Update()
	{
		if (Player.Crouching && VoicePauseTimer <= 0f)
		{
			float num = Mathf.Clamp(LevelEnemy.PlayerDistance.PlayerDistanceLocal, CrouchLoopDistanceMin, CrouchLoopDistanceMax);
			float num2 = Mathf.Lerp(CrouchLoopVolumeMin, CrouchLoopVolumeMax, 1f - (num - CrouchLoopDistanceMin) / (CrouchLoopDistanceMax - CrouchLoopDistanceMin));
			CrouchLoopVolume = Mathf.Lerp(CrouchLoopVolume, num2, Time.deltaTime * 5f);
			CrouchLoop.LoopVolume = CrouchLoopVolume;
			CrouchLoop.PlayLoop(playing: true, 1f, 1f);
		}
		else
		{
			CrouchLoop.PlayLoop(playing: false, 1f, 1f);
		}
		if (Player.SprintSpeedLerp >= 1f)
		{
			SprintingTimer = 0.5f;
		}
		else if (SprintingTimer > 0f)
		{
			SprintingTimer -= Time.deltaTime;
		}
		if (SprintingTimer > 0f)
		{
			SprintLoop.PlayLoop(playing: true, 1f, 5f);
			if (!SprintLoopPlaying)
			{
				if (CurrentVoiceSource != null && CurrentVoiceSource.isPlaying)
				{
					VoicesToFade.Add(CurrentVoiceSource);
					CurrentVoiceSource = null;
				}
				SprintLoop.LoopVolume = 0f;
				SprintLoopPlaying = true;
			}
			SprintLoopLerp += Time.deltaTime * SprintVolumeSpeed;
			SprintLoopLerp = Mathf.Clamp01(SprintLoopLerp);
			SprintLoop.LoopVolume = Mathf.Lerp(0f, SprintVolume, SprintLoopLerp);
		}
		else
		{
			SprintLoop.PlayLoop(playing: false, 1f, 5f);
			if (SprintLoopPlaying)
			{
				if (!Player.Crouching)
				{
					if (CurrentVoiceSource != null && CurrentVoiceSource.isPlaying)
					{
						VoicesToFade.Add(CurrentVoiceSource);
						CurrentVoiceSource = null;
					}
					CurrentVoiceSource = SprintStop.Play(base.transform.position);
					VoicePauseTimer = CurrentVoiceSource.clip.length * 1.2f;
				}
				SprintLoopLerp = 0f;
				SprintLoopPlaying = false;
			}
		}
		if (VoicePauseTimer > 0f)
		{
			VoicePauseTimer -= Time.deltaTime;
		}
		foreach (AudioSource item in VoicesToFade)
		{
			if (item == null)
			{
				VoicesToFade.Remove(item);
				break;
			}
			if (item.volume <= 0.01f)
			{
				item.Stop();
				VoicesToFade.Remove(item);
				Object.Destroy(item.gameObject);
				break;
			}
			item.volume -= 2f * Time.deltaTime;
		}
	}
}
