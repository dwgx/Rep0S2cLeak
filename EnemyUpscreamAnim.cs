using UnityEngine;

public class EnemyUpscreamAnim : MonoBehaviour
{
	public Enemy enemy;

	public EnemyUpscream controller;

	internal Animator animator;

	public Materials.MaterialTrigger material;

	private bool idleBreakImpulse;

	private bool stunImpulse;

	private bool jumpImpulse;

	public Sound sfxAttackLocal;

	public Sound sfxAttackGlobal;

	public Sound hurtSound;

	public Sound jumpSound;

	public Sound landSound;

	public Sound stepSound;

	public Sound sfxIdleBreak;

	public Sound despawnSound;

	public bool isPlayingTargetPlayerLoop;

	private float currentSpeed;

	private float moveTimer;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
	}

	private void Update()
	{
		SetAnimationSpeed();
		if (controller.enemy.CurrentState == EnemyState.Despawn)
		{
			animator.SetBool("despawn", value: true);
		}
		else
		{
			animator.SetBool("despawn", value: false);
		}
		if (controller.currentState == EnemyUpscream.State.IdleBreak)
		{
			if (idleBreakImpulse)
			{
				animator.SetTrigger("IdleBreak");
				idleBreakImpulse = false;
			}
		}
		else
		{
			idleBreakImpulse = true;
		}
		if (enemy.Jump.jumping)
		{
			animator.SetBool("jumping", value: true);
			if (jumpImpulse)
			{
				animator.SetTrigger("Jump");
				animator.SetBool("falling", value: false);
				jumpImpulse = false;
			}
		}
		else
		{
			animator.SetBool("jumping", value: false);
			jumpImpulse = true;
		}
		if (enemy.Rigidbody.physGrabObject.rbVelocity.y < -0.1f)
		{
			animator.SetBool("falling", value: true);
		}
		else
		{
			animator.SetBool("falling", value: false);
		}
		if (enemy.Rigidbody.physGrabObject.rbVelocity.magnitude > 0.1f || enemy.Rigidbody.physGrabObject.rbAngularVelocity.magnitude > 0.5f)
		{
			moveTimer = 0.2f;
		}
		if (moveTimer > 0f)
		{
			moveTimer -= Time.deltaTime;
			animator.SetBool("move", value: true);
		}
		else
		{
			animator.SetBool("move", value: false);
		}
		if (controller.currentState == EnemyUpscream.State.Stun)
		{
			if (stunImpulse)
			{
				animator.SetTrigger("Stun");
				stunImpulse = false;
			}
			animator.SetBool("stunned", value: true);
		}
		else
		{
			animator.SetBool("stunned", value: false);
			stunImpulse = true;
		}
	}

	public void SetSpawn()
	{
		animator.Play("Spawn", 0, 0f);
	}

	public void SetDespawn()
	{
		controller.UpdateState(EnemyUpscream.State.Spawn);
		controller.enemy.EnemyParent.Despawn();
	}

	public void NoticeSet(int _playerID)
	{
		animator.SetTrigger("Notice");
	}

	public void TeleportParticlesStart()
	{
	}

	public void TeleportParticlesStop()
	{
	}

	public void SfxImpactFootstep()
	{
		if (enemy.Grounded.grounded)
		{
			stepSound.Play(base.transform.position);
			Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
		}
	}

	public void SfxIdleBreak()
	{
		sfxIdleBreak.Play(base.transform.position);
	}

	public void SfxAttack()
	{
		sfxAttackLocal.Play(base.transform.position);
		sfxAttackGlobal.Play(base.transform.position);
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

	public void AttackImpulse()
	{
		if ((!SemiFunc.IsMultiplayer() || SemiFunc.IsMasterClient()) && (bool)controller.targetPlayer)
		{
			Vector3 normalized = (controller.targetPlayer.transform.position - base.transform.position).normalized;
			normalized = Vector3.Lerp(normalized, Vector3.up, 0.6f);
			controller.targetPlayer.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
			controller.targetPlayer.tumble.TumbleForce(normalized * 45f);
			controller.targetPlayer.tumble.TumbleTorque(-controller.targetPlayer.transform.right * 45f);
			controller.targetPlayer.tumble.TumbleOverrideTime(1.5f);
			controller.targetPlayer.tumble.ImpactHurtSet(3f, 10);
		}
	}

	private void SetAnimationSpeed()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
		{
			float value = enemy.Rigidbody.physGrabObject.rbVelocity.magnitude + enemy.Rigidbody.physGrabObject.rbAngularVelocity.magnitude;
			value = Mathf.Clamp(value, 0.5f, 4f);
			animator.speed = value * 0.6f;
		}
		else
		{
			animator.speed = 1f;
		}
		if (enemy.Rigidbody.frozen)
		{
			animator.speed = 0f;
		}
	}
}
