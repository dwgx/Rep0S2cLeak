using UnityEngine;

public class EnemyAnimalAnim : MonoBehaviour
{
	private Animator animator;

	public Enemy enemy;

	public EnemyAnimal controller;

	public Materials.MaterialTrigger material;

	private bool stun;

	private bool attack;

	private bool previousAttack;

	private float attackPitch;

	[Space]
	public ParticleSystem particleBits;

	public ParticleSystem particleImpact;

	public ParticleSystem particleDirectionalBits;

	public ParticleSystem particleLegBits;

	public ParticleSystem particleDig;

	[Space]
	public Sound stepSound;

	public Sound stompSound;

	[Space]
	public Sound moveShortSound;

	public Sound moveLongSound;

	[Space]
	public Sound spawnSound;

	public Sound despawnSound;

	[Space]
	public Sound jumpSound;

	public Sound landSound;

	public Sound noticeSound;

	[Space]
	public Sound attackStartSound;

	public Sound attackLoop;

	public Sound attackStopSound;

	[Space]
	public Sound hurtSound;

	public Sound deathSound;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
	}

	private void Update()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !animator.IsInTransition(0))
		{
			animator.speed = Mathf.Clamp(enemy.Rigidbody.velocity.magnitude / 5f, 0.6f, 2f);
			attackPitch = Mathf.Clamp(enemy.Rigidbody.velocity.magnitude / 4.75f, 0.75f, 1.25f);
			attack = true;
		}
		else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk") && !animator.IsInTransition(0))
		{
			animator.speed = Mathf.Clamp(enemy.Rigidbody.velocity.magnitude / 2f, 0.8f, 2f);
		}
		else
		{
			animator.speed = 1f;
			attack = false;
		}
		attackLoop.PlayLoop(attack, 5f, 5f, attackPitch);
		if (!previousAttack && attack)
		{
			attackStartSound.Play(base.transform.position);
			previousAttack = true;
		}
		if (previousAttack && !attack)
		{
			attackStopSound.Play(base.transform.position);
			previousAttack = false;
		}
		if (enemy.Rigidbody.frozen)
		{
			animator.speed = 0f;
		}
		if (Vector3.Dot(enemy.Rigidbody.transform.up, Vector3.up) > 0.6f)
		{
			animator.SetBool("upright", value: true);
		}
		else
		{
			animator.SetBool("upright", value: false);
		}
		if (enemy.Rigidbody.velocity.y < -2f)
		{
			animator.SetBool("falling", value: true);
		}
		else
		{
			animator.SetBool("falling", value: false);
		}
		if (enemy.Jump.jumping)
		{
			animator.SetBool("jump", value: true);
		}
		else
		{
			animator.SetBool("jump", value: false);
		}
		if (enemy.IsStunned())
		{
			stun = true;
			animator.SetBool("stun", value: true);
		}
		else
		{
			stun = false;
			animator.SetBool("stun", value: false);
		}
		if (stun && !animator.GetCurrentAnimatorStateInfo(0).IsName("Stun"))
		{
			animator.SetTrigger("Stun Impulse");
		}
		if (enemy.CurrentState == EnemyState.Despawn)
		{
			animator.SetBool("despawn", value: true);
		}
		else
		{
			animator.SetBool("despawn", value: false);
		}
		if (enemy.Rigidbody.velocity.magnitude > 0.2f)
		{
			animator.SetBool("move", value: true);
		}
		else
		{
			animator.SetBool("move", value: false);
		}
		if (controller.currentState == EnemyAnimal.State.WreakHavoc)
		{
			animator.SetBool("attack", value: true);
		}
		else
		{
			animator.SetBool("attack", value: false);
		}
	}

	public void SetSpawn()
	{
		stun = false;
		animator.Play("Spawn", 0, 0f);
	}

	public void SetDespawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void NoticeSet(int _playerID)
	{
		animator.SetTrigger("Notice");
	}

	public void MaterialImpactShake()
	{
		GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
		stompSound.Play(base.transform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Heavy, footstep: false, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void ImpactLight()
	{
		if (enemy.Grounded.grounded)
		{
			Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Light, footstep: false, footstepParticles: true, material, Materials.HostType.Enemy);
		}
	}

	public void ImpactMedium()
	{
		if (enemy.Grounded.grounded)
		{
			Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Medium, footstep: false, footstepParticles: true, material, Materials.HostType.Enemy);
		}
	}

	public void ImpactHeavy()
	{
		if (enemy.Grounded.grounded)
		{
			Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Heavy, footstep: false, footstepParticles: false, material, Materials.HostType.Enemy);
		}
	}

	public void ImpactFootstep()
	{
		if (enemy.Grounded.grounded)
		{
			stepSound.Play(base.transform.position);
			Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Heavy, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
		}
	}

	public void Dig()
	{
		particleDig.Play();
	}

	public void Notice()
	{
		noticeSound.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void StunStop()
	{
		attackStopSound.Play(base.transform.position);
	}

	public void Jump()
	{
		jumpSound.Play(base.transform.position);
	}

	public void Land()
	{
		landSound.Play(base.transform.position);
	}

	public void DespawnSound()
	{
		despawnSound.Play(base.transform.position);
	}
}
