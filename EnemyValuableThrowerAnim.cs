using UnityEngine;

public class EnemyValuableThrowerAnim : MonoBehaviour
{
	public Transform followTarget;

	public EnemyValuableThrower controller;

	public Enemy enemy;

	public Materials.MaterialTrigger material;

	internal Animator animator;

	[Space]
	public ParticleSystem particleBits;

	public ParticleSystem particleImpact;

	public ParticleSystem particleDirectionalBits;

	private bool stun;

	[Space]
	public Sound footstepSound;

	public Sound footstepSmallSound;

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
	public Sound pickupIntroSound;

	public Sound pickupOutroTellSound;

	public Sound pickupOutroThrowSound;

	[Space]
	public Sound stunSound;

	public Sound stunStopSound;

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
		if (enemy.Rigidbody.frozen)
		{
			animator.speed = 0f;
		}
		else
		{
			animator.speed = 1f;
		}
		base.transform.position = followTarget.position;
		base.transform.rotation = followTarget.rotation;
		if (enemy.Rigidbody.velocity.magnitude > 0.5f)
		{
			animator.SetBool("Move", value: true);
			animator.SetBool("Move Slow", value: false);
		}
		else if (enemy.Rigidbody.velocity.magnitude > 0.2f)
		{
			animator.SetBool("Move", value: false);
			animator.SetBool("Move Slow", value: true);
		}
		else
		{
			animator.SetBool("Move", value: false);
			animator.SetBool("Move Slow", value: false);
		}
		if (enemy.CurrentState == EnemyState.Despawn)
		{
			stun = false;
			animator.SetBool("Despawn", value: true);
		}
		else
		{
			animator.SetBool("Despawn", value: false);
		}
		if (controller.currentState == EnemyValuableThrower.State.PickUpTarget || controller.currentState == EnemyValuableThrower.State.TargetPlayer)
		{
			animator.SetBool("Pickup", value: true);
		}
		else
		{
			animator.SetBool("Pickup", value: false);
		}
		if (enemy.Jump.jumping)
		{
			animator.SetBool("Jumping", value: true);
		}
		else
		{
			animator.SetBool("Jumping", value: false);
		}
		if (enemy.IsStunned())
		{
			stun = true;
			animator.SetBool("Stun", value: true);
		}
		else
		{
			stun = false;
			animator.SetBool("Stun", value: false);
		}
		if (stun && !animator.GetCurrentAnimatorStateInfo(0).IsName("Stun"))
		{
			animator.SetTrigger("Stun Impulse");
		}
		stunSound.PlayLoop(stun, 10f, 10f);
	}

	public void OnSpawn()
	{
		stun = false;
		animator.SetBool("Stun", value: false);
		animator.Play("Spawn", 0, 0f);
	}

	public void NoticeSet(int _playerID)
	{
		animator.SetTrigger("Notice");
	}

	public void ResetStateTimer()
	{
		controller.ResetStateTimer();
	}

	public void SpawnStart()
	{
		spawnSound.Play(base.transform.position);
	}

	public void DespawnStart()
	{
		despawnSound.Play(base.transform.position);
	}

	public void Despawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void Throw()
	{
		pickupOutroThrowSound.Play(base.transform.position);
		controller.Throw();
	}

	public void Footstep()
	{
		footstepSound.Play(base.transform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void FootstepSmall()
	{
		footstepSmallSound.Play(base.transform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Light, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void MoveShort()
	{
		moveShortSound.Play(base.transform.position);
	}

	public void MoveLong()
	{
		moveLongSound.Play(base.transform.position);
	}

	public void Jump()
	{
		jumpSound.Play(base.transform.position);
	}

	public void Land()
	{
		landSound.Play(base.transform.position);
	}

	public void PickupIntro()
	{
		pickupIntroSound.Play(base.transform.position);
	}

	public void PickupOutro()
	{
		pickupOutroTellSound.Play(base.transform.position);
	}

	public void StunStop()
	{
		stunStopSound.Play(base.transform.position);
	}

	public void Notice()
	{
		noticeSound.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
	}
}
