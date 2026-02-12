using UnityEngine;

public class EnemyCeilingEyeAnim : MonoBehaviour
{
	public EnemyCeilingEye controller;

	private Animator animator;

	public Enemy enemy;

	public ParticleScriptExplosion particleScriptExplosion;

	public ParticleSystem TeleportParticles;

	public ParticleSystem particleImpact;

	public ParticleSystem particleBits;

	[Header("Sounds")]
	public Sound sfxBlink;

	public Sound sfxDespawn;

	public Sound sfxSpawn;

	public Sound sfxDeath;

	public Sound sfxLaserBuildup;

	public Sound sfxLaserBeam;

	public AudioClip sfxStaringStart;

	public Sound sfxStareLoop;

	public Sound sfxTwitchLoop;

	private bool isPlayingTwitchLoop = true;

	private bool isPlayingStaringLoop = true;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
	}

	private void Update()
	{
		if (controller.currentState == EnemyCeilingEye.State.HasTarget || controller.currentState == EnemyCeilingEye.State.TargetLost)
		{
			animator.SetBool("hasTarget", value: true);
		}
		else
		{
			animator.SetBool("hasTarget", value: false);
		}
		if (controller.enemy.CurrentState == EnemyState.Despawn || controller.currentState == EnemyCeilingEye.State.Move)
		{
			animator.SetBool("despawn", value: true);
		}
		else
		{
			animator.SetBool("despawn", value: false);
		}
		SfxStaringLoop();
		if (controller.deathImpulse)
		{
			controller.deathImpulse = false;
			animator.SetTrigger("Death");
		}
	}

	public void SetSpawn()
	{
		animator.Play("Ceiling Eye Spawn", 0, 0f);
	}

	public void SetAttack()
	{
		animator.Play("Ceiling Eye Attack", 0, 0f);
	}

	public void SetDespawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (controller.enemy.CurrentState == EnemyState.Despawn || controller.currentState == EnemyCeilingEye.State.Death)
			{
				controller.enemy.EnemyParent.Despawn();
			}
			else
			{
				controller.OnSpawn();
			}
		}
	}

	public void AttackFinished()
	{
		controller.enemy.EnemyParent.SpawnedTimerSet(0f);
	}

	public void Explosion()
	{
		Vector3 vector = new Vector3(0f, -0.5f, 0f);
		if (Physics.Raycast(base.transform.position + vector, Vector3.down, out var hitInfo, 30f, SemiFunc.LayerMaskGetVisionObstruct()))
		{
			particleScriptExplosion.Spawn(hitInfo.point, 2f, 50, 50);
		}
	}

	public void DeathEffect()
	{
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		particleImpact.Play();
		particleBits.Play();
		sfxDeath.Play(base.transform.position);
	}

	public void TeleportParticlesStart()
	{
		TeleportParticles.Play();
	}

	public void TeleportParticlesStop()
	{
		TeleportParticles.Stop();
	}

	public void SfxBlink()
	{
		sfxBlink.Play(base.transform.position);
	}

	public void SfxDespawn()
	{
		sfxDespawn.Play(base.transform.position);
	}

	public void SfxSpawn()
	{
		sfxSpawn.Play(base.transform.position);
	}

	public void SfxLaserBuildup()
	{
		sfxLaserBuildup.Play(base.transform.position);
	}

	public void SfxLaserBeam()
	{
		sfxLaserBeam.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void SfxStaringStart()
	{
		if (controller.currentState == EnemyCeilingEye.State.HasTarget && controller.targetPlayer.isLocal)
		{
			AudioScare.instance.PlayCustom(sfxStaringStart);
		}
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void SfxStaringLoop()
	{
		sfxStareLoop.PlayLoop(isPlayingStaringLoop, 0.05f, 0.25f);
		sfxTwitchLoop.PlayLoop(isPlayingTwitchLoop, 0.1f, 0.25f);
		if (controller.currentState == EnemyCeilingEye.State.HasTarget)
		{
			isPlayingStaringLoop = true;
		}
		else
		{
			isPlayingStaringLoop = false;
		}
		if (controller.currentState == EnemyCeilingEye.State.HasTarget && controller.targetPlayer.isLocal)
		{
			isPlayingTwitchLoop = true;
		}
		else
		{
			isPlayingTwitchLoop = false;
		}
	}
}
