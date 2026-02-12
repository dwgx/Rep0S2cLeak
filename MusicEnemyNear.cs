using System;
using System.Collections;
using UnityEngine;

public class MusicEnemyNear : MonoBehaviour
{
	[Serializable]
	public class Track
	{
		public AudioClip Clip;

		[Range(0f, 1f)]
		public float Volume = 0.5f;
	}

	public static MusicEnemyNear instance;

	public LevelMusic LevelMusic;

	public AudioSource Source;

	internal float Volume;

	[Space]
	public float MaxDistance = 15f;

	public float MinDistance = 4f;

	[Space]
	public float OnScreenDistance = 10f;

	[Space]
	public float FadeSpeed = 1f;

	private float LowerMultiplier = 1f;

	private float LowerMultiplierTarget = 1f;

	private float LowerTimer;

	private Camera Camera;

	public LayerMask Mask;

	private bool RayResult;

	private float RayTimer;

	private Track CurrentTrack;

	[Space]
	public Track[] Tracks;

	private float overrideActiveTimer;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		CurrentTrack = Tracks[UnityEngine.Random.Range(0, Tracks.Length)];
		Camera = Camera.main;
		NewTrack();
		StartCoroutine(Logic());
	}

	public void LowerVolume(float multiplier, float time)
	{
		LowerMultiplierTarget = multiplier;
		LowerTimer = time;
	}

	private IEnumerator Logic()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(1f);
		while (true)
		{
			if (LowerTimer > 0f)
			{
				LowerTimer -= Time.deltaTime;
				if (LowerTimer <= 0f)
				{
					LowerMultiplierTarget = 1f;
				}
				LowerMultiplier = Mathf.Lerp(LowerMultiplier, LowerMultiplierTarget, Time.deltaTime * 5f);
			}
			else
			{
				LowerMultiplier = Mathf.Lerp(LowerMultiplier, LowerMultiplierTarget, Time.deltaTime * 1f);
			}
			float num = 0f;
			if (RoundDirector.instance.allExtractionPointsCompleted || overrideActiveTimer > 0f)
			{
				num = CurrentTrack.Volume;
			}
			foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
			{
				float num2 = 0f;
				if (item.Spawned && item.Enemy.EnemyNearMusic)
				{
					if (!item.Enemy.HasPlayerDistance)
					{
						Debug.LogError(item.name + " needs 'player distance' component for near music.");
						continue;
					}
					if (!item.Enemy.HasOnScreen)
					{
						Debug.LogError(item.name + " needs 'on screen' component for near music.");
						continue;
					}
					if (!item.Enemy.HasPlayerRoom)
					{
						Debug.LogError(item.name + " needs 'player room' component for near music.");
						continue;
					}
					float playerDistanceLocal = item.Enemy.PlayerDistance.PlayerDistanceLocal;
					if (item.Enemy.CurrentState != EnemyState.Spawn)
					{
						if (item.Enemy.OnScreen.OnScreenLocal && playerDistanceLocal <= OnScreenDistance)
						{
							num2 = CurrentTrack.Volume;
						}
						else if (playerDistanceLocal <= MaxDistance)
						{
							if (RayTimer <= 0f)
							{
								RayTimer = 0.5f;
								Vector3 direction = Camera.transform.position - item.Enemy.CenterTransform.position;
								RayResult = Physics.Raycast(item.Enemy.CenterTransform.position, direction, out var _, direction.magnitude, Mask, QueryTriggerInteraction.Ignore);
							}
							else
							{
								RayTimer -= Time.deltaTime;
							}
							if (!RayResult || item.Enemy.PlayerRoom.SameLocal)
							{
								playerDistanceLocal = Mathf.Clamp(playerDistanceLocal, MinDistance, MaxDistance);
								num2 = Mathf.Lerp(0f, CurrentTrack.Volume, 1f - (playerDistanceLocal - MinDistance) / (MaxDistance - MinDistance));
							}
						}
					}
				}
				num = Mathf.Max(num, num2);
			}
			Volume = Mathf.Lerp(Volume, num * LowerMultiplier, Time.deltaTime * FadeSpeed);
			if (Volume > 0f)
			{
				if (!Source.isPlaying)
				{
					NewTrack();
					Source.time = (Source.clip ? UnityEngine.Random.Range(0f, Source.clip.length) : 0f);
					Source.Play();
				}
				Source.volume = Volume;
				LevelMusic.Interrupt(0.5f);
			}
			else if (Source.isPlaying)
			{
				Source.Stop();
			}
			if (overrideActiveTimer > 0f)
			{
				overrideActiveTimer -= Time.deltaTime;
			}
			yield return null;
		}
	}

	private void NewTrack()
	{
		CurrentTrack = Tracks[UnityEngine.Random.Range(0, Tracks.Length)];
		Source.clip = CurrentTrack.Clip;
	}

	public void OverrideActive(float _time)
	{
		overrideActiveTimer = _time;
	}
}
