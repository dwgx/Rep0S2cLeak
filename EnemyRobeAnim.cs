using UnityEngine;

public class EnemyRobeAnim : MonoBehaviour
{
	public EnemyRobe controller;

	internal Animator animator;

	public Transform visionTransform;

	public ParticleSystem teleportParticles;

	public ParticleSystem[] deathParticles;

	public ParticleSystem spawnParticles;

	[Header("Sounds")]
	public Sound sfxTargetPlayerLoop;

	public Sound sfxIdleBreak;

	public Sound sfxAttack;

	public Sound sfxAttackGlobal;

	public Sound sfxHurt;

	public Sound sfxHandIdle;

	public Sound sfxHandAggressive;

	public Sound sfxStunStart;

	public Sound sfxStunLoop;

	public Sound sfxAttackUnder;

	public Sound sfxAttackUnderGlobal;

	public Sound sfxDeath;

	public bool isPlayingTargetPlayerLoop;

	public bool isPlayingHandIdle;

	public bool isPlayingHandAggressive;

	private bool stunImpulse;

	private bool despawnImpulse;

	private bool lookUnderImpulse;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
	}

	private void Update()
	{
		if (controller.enemy.Rigidbody.frozen)
		{
			animator.speed = 0f;
		}
		else
		{
			animator.speed = 1f;
		}
		if (controller.isOnScreen)
		{
			animator.SetBool("isOnScreen", value: true);
		}
		else
		{
			animator.SetBool("isOnScreen", value: false);
		}
		if (controller.currentState == EnemyRobe.State.Despawn)
		{
			if (despawnImpulse)
			{
				animator.SetTrigger("despawn");
				despawnImpulse = false;
			}
		}
		else
		{
			despawnImpulse = true;
		}
		if (controller.attackImpulse)
		{
			controller.attackImpulse = false;
			if (!controller.enemy.IsStunned())
			{
				animator.SetTrigger("attack");
			}
		}
		if (controller.idleBreakTrigger)
		{
			controller.idleBreakTrigger = false;
			if (!controller.enemy.IsStunned())
			{
				animator.SetTrigger("idleBreak");
			}
		}
		if (controller.currentState == EnemyRobe.State.LookUnder || controller.currentState == EnemyRobe.State.LookUnderAttack)
		{
			if (lookUnderImpulse)
			{
				animator.SetTrigger("LookUnder");
				lookUnderImpulse = false;
			}
			animator.SetBool("LookingUnder", value: true);
		}
		else
		{
			animator.SetBool("LookingUnder", value: false);
			lookUnderImpulse = true;
		}
		if (controller.lookUnderAttackImpulse)
		{
			animator.SetTrigger("LookUnderAttack");
			controller.lookUnderAttackImpulse = false;
		}
		if (controller.currentState == EnemyRobe.State.Stun)
		{
			if (stunImpulse)
			{
				sfxStunStart.Play(controller.transform.position);
				animator.SetTrigger("Stun");
				stunImpulse = false;
			}
			animator.SetBool("Stunned", value: true);
			sfxStunLoop.PlayLoop(playing: true, 2f, 2f);
		}
		else
		{
			sfxStunLoop.PlayLoop(playing: false, 2f, 2f);
			animator.SetBool("Stunned", value: false);
			stunImpulse = true;
		}
		sfxTargetPlayerLoop.PlayLoop(isPlayingTargetPlayerLoop, 2f, 2f);
		sfxHandIdle.PlayLoop(isPlayingHandIdle, 2f, 2f);
		sfxHandAggressive.PlayLoop(isPlayingHandAggressive, 2f, 2f);
	}

	public void SetSpawn()
	{
		animator.Play("Robe Spawn", 0, 0f);
	}

	public void SetDespawn()
	{
		controller.enemy.EnemyParent.Despawn();
	}

	public void TeleportParticlesStart()
	{
		teleportParticles.Play();
	}

	public void TeleportParticlesStop()
	{
		teleportParticles.Stop();
	}

	public void SpawnParticlesImpulse()
	{
		spawnParticles.Play();
	}

	public void DeathParticlesImpulse()
	{
		ParticleSystem[] array = deathParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	public void LookUnderIntro()
	{
		if (controller.targetPlayer.isLocal)
		{
			AudioScare.instance.PlaySoft();
		}
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void SfxTargetPlayerLoop()
	{
		if (controller.isOnScreen)
		{
			isPlayingTargetPlayerLoop = true;
		}
		else
		{
			isPlayingTargetPlayerLoop = false;
		}
	}

	public void SfxIdleBreak()
	{
		sfxIdleBreak.Play(controller.transform.position);
	}

	public void SfxAttack()
	{
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		sfxAttack.Play(visionTransform.transform.position);
		sfxAttackGlobal.Play(visionTransform.transform.position);
	}

	public void SfxDeath()
	{
		sfxDeath.Play(visionTransform.transform.position);
	}

	public void LookUnderAttack()
	{
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		sfxAttackUnder.Play(controller.transform.position);
		sfxAttackUnderGlobal.Play(controller.transform.position);
	}
}
