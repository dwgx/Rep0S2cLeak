using System.Collections;
using UnityEngine;

public class MusicEnemySighting : MonoBehaviour
{
	public AudioSource Source;

	public LevelMusic LevelMusic;

	public float Cooldown;

	private float CooldownTimer;

	public float DistanceMax;

	internal bool Active;

	public float ActiveTime;

	private float ActiveTimer;

	public AudioClip[] Sounds;

	private void Start()
	{
		StartCoroutine(Logic());
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
			bool flag = false;
			foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
			{
				if (item.Spawned && item.Enemy.SightingStinger)
				{
					if (!item.Enemy.HasPlayerDistance)
					{
						Debug.LogError(item.name + " needs 'player distance' component for sighting stinger.");
					}
					else if (!item.Enemy.HasOnScreen)
					{
						Debug.LogError(item.name + " needs 'on screen' component for sighting stinger.");
					}
					else if (item.Enemy.PlayerDistance.PlayerDistanceLocal <= DistanceMax && item.Enemy.OnScreen.OnScreenLocal && item.Enemy.TeleportedTimer <= 0f && item.Enemy.CurrentState != EnemyState.Spawn)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				if (!Active && CooldownTimer <= 0f)
				{
					LevelMusic.Interrupt(10f);
					GameDirector.instance.CameraImpact.Shake(2f, 0.1f);
					GameDirector.instance.CameraShake.Shake(2f, 1f);
					Active = true;
					Source.clip = Sounds[Random.Range(0, Sounds.Length)];
					Source.Play();
					CooldownTimer = Cooldown;
					ActiveTimer = ActiveTime;
				}
			}
			else if (CooldownTimer > 0f)
			{
				CooldownTimer -= Time.deltaTime;
			}
			if (ActiveTimer > 0f)
			{
				ActiveTimer -= Time.deltaTime;
				if (ActiveTimer <= 0f)
				{
					Active = false;
				}
			}
			yield return null;
		}
	}
}
