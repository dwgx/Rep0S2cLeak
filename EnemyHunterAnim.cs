using System.Collections.Generic;
using UnityEngine;

public class EnemyHunterAnim : MonoBehaviour
{
	public Enemy enemy;

	public EnemyHunter enemyHunter;

	internal Animator animator;

	public Materials.MaterialTrigger material;

	private float moveTimer;

	private bool stunImpulse;

	internal bool spawnImpulse;

	private float hummingStopTimer;

	private bool humming;

	[Space]
	public List<ParticleSystem> teleportEffects;

	[Space]
	public Sound soundFootstepShort;

	public Sound soundFootstepLong;

	public Sound soundReload01;

	public Sound soundAimStart;

	public Sound soundAimStartGlobal;

	public Sound soundReload02;

	public Sound soundMoveShort;

	public Sound soundMoveLong;

	public Sound soundGunLong;

	public Sound soundGunShort;

	public Sound soundSpawn;

	public Sound soundDespawn;

	public Sound soundLeaveStart;

	public Sound soundHumming;

	[Space]
	public AudioClip[] aimStartClips;

	public AudioClip[] aimStartGlobalClips;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
	}

	private void Update()
	{
		if (enemy.Rigidbody.frozen)
		{
			animator.speed = 0f;
		}
		else
		{
			animator.speed = 1f;
		}
		if ((enemyHunter.currentState == EnemyHunter.State.Roam || enemyHunter.currentState == EnemyHunter.State.InvestigateWalk || enemyHunter.currentState == EnemyHunter.State.Leave) && (enemy.Rigidbody.velocity.magnitude > 0.2f || enemy.Rigidbody.physGrabObject.rbAngularVelocity.magnitude > 0.25f))
		{
			moveTimer = 0.1f;
		}
		if (moveTimer > 0f)
		{
			moveTimer -= Time.deltaTime;
			animator.SetBool("Moving", value: true);
		}
		else
		{
			animator.SetBool("Moving", value: false);
		}
		if (hummingStopTimer > 0f)
		{
			hummingStopTimer -= Time.deltaTime;
			soundHumming.PlayLoop(playing: false, 2f, 20f);
		}
		else
		{
			soundHumming.PlayLoop(playing: true, 2f, 2f);
		}
		if (enemyHunter.currentState == EnemyHunter.State.LeaveStart)
		{
			animator.SetBool("Leaving", value: true);
		}
		else
		{
			animator.SetBool("Leaving", value: false);
		}
		if (enemyHunter.currentState == EnemyHunter.State.Stun)
		{
			if (stunImpulse)
			{
				animator.SetTrigger("Stun");
				stunImpulse = false;
			}
			animator.SetBool("Stunned", value: true);
		}
		else
		{
			animator.SetBool("Stunned", value: false);
			stunImpulse = true;
		}
		if (enemyHunter.currentState == EnemyHunter.State.Aim)
		{
			animator.SetBool("Aiming", value: true);
		}
		else
		{
			animator.SetBool("Aiming", value: false);
		}
		if (enemyHunter.currentState == EnemyHunter.State.Shoot || enemyHunter.currentState == EnemyHunter.State.ShootEnd)
		{
			animator.SetBool("Shooting", value: true);
		}
		else
		{
			animator.SetBool("Shooting", value: false);
		}
		if (enemyHunter.currentState == EnemyHunter.State.Despawn)
		{
			animator.SetBool("Despawning", value: true);
		}
		else
		{
			animator.SetBool("Despawning", value: false);
		}
	}

	public void OnSpawn()
	{
		animator.SetBool("Stunned", value: false);
		animator.Play("Spawn", 0, 0f);
	}

	public void StopHumming(float _multiplier)
	{
		hummingStopTimer = 30f * _multiplier;
	}

	public void TeleportEffect()
	{
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.05f);
		foreach (ParticleSystem teleportEffect in teleportEffects)
		{
			teleportEffect.Play();
		}
	}

	public void FootstepShort()
	{
		soundFootstepShort.Play(enemy.CenterTransform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 1f, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void FootstepLong()
	{
		soundFootstepLong.Play(enemy.CenterTransform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 1f, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void AimStart()
	{
		int num = Random.Range(0, aimStartClips.Length);
		soundAimStart.Sounds[0] = aimStartClips[num];
		soundAimStartGlobal.Sounds[0] = aimStartGlobalClips[num];
		soundAimStart.Play(enemy.CenterTransform.position);
		soundAimStartGlobal.Play(enemy.CenterTransform.position);
		StopHumming(1f);
	}

	public void Despawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void Reload01()
	{
		soundReload01.Play(enemy.CenterTransform.position);
	}

	public void Reload02()
	{
		soundReload02.Play(enemy.CenterTransform.position);
	}

	public void MoveShort()
	{
		soundMoveShort.Play(enemy.CenterTransform.position);
	}

	public void MoveLong()
	{
		soundMoveLong.Play(enemy.CenterTransform.position);
	}

	public void GunLong()
	{
		soundGunLong.Play(enemy.CenterTransform.position);
	}

	public void GunShort()
	{
		soundGunShort.Play(enemy.CenterTransform.position);
	}

	public void Spawn()
	{
		soundSpawn.Play(enemy.CenterTransform.position);
	}

	public void DespawnSound()
	{
		soundDespawn.Play(enemy.CenterTransform.position);
	}

	public void LeaveStartSound()
	{
		soundLeaveStart.Play(enemy.CenterTransform.position);
		StopHumming(0.25f);
	}

	public void LeaveStartDone()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && enemyHunter.currentState == EnemyHunter.State.LeaveStart)
		{
			enemyHunter.stateTimer = 0f;
		}
	}
}
