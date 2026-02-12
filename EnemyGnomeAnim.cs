using UnityEngine;

public class EnemyGnomeAnim : MonoBehaviour
{
	public Enemy enemy;

	public EnemyGnome enemyGnome;

	internal Animator animator;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	private bool attackImpulse;

	private bool stunImpulse;

	private bool jumpImpulse;

	private bool landImpulse;

	internal bool idleBreakerImpulse;

	private bool noticeImpulse;

	[Space]
	public Sound soundFootstep;

	[Space]
	public Sound soundMoveShort;

	public Sound soundMoveLong;

	[Space]
	public Sound soundPickaxeTell;

	public Sound soundPickaxeHit;

	[Space]
	public Sound soundIdleBreaker;

	public Sound soundNotice;

	[Space]
	public Sound soundSpawn;

	public Sound soundDespawn;

	[Space]
	public Sound soundJump;

	public Sound soundLand;

	[Space]
	public Sound soundStun;

	public Sound soundStunOutro;

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
			if (enemyGnome.currentState == EnemyGnome.State.Stun)
			{
				animator.speed = Mathf.Clamp(enemyGnome.enemy.Rigidbody.physGrabObject.rbVelocity.magnitude, 1f, 3f);
			}
		}
		if (enemyGnome.currentState == EnemyGnome.State.Attack)
		{
			if (attackImpulse)
			{
				animator.SetTrigger("Attack");
				attackImpulse = false;
			}
		}
		else
		{
			attackImpulse = true;
		}
		bool flag = false;
		if (enemyGnome.currentState == EnemyGnome.State.Stun)
		{
			flag = true;
			landImpulse = false;
			if (stunImpulse)
			{
				soundStun.Play(enemy.CenterTransform.position);
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
		if (!flag && enemy.Jump.jumping)
		{
			if (jumpImpulse)
			{
				animator.SetTrigger("Jump");
				animator.SetBool("Falling", value: false);
				jumpImpulse = false;
				landImpulse = true;
			}
			else if (enemyGnome.enemy.Rigidbody.physGrabObject.rbVelocity.y < 0f)
			{
				animator.SetBool("Falling", value: true);
			}
		}
		else
		{
			if (landImpulse)
			{
				animator.SetTrigger("Land");
				landImpulse = false;
			}
			animator.SetBool("Falling", value: false);
			jumpImpulse = true;
		}
		if (idleBreakerImpulse)
		{
			animator.SetTrigger("IdleBreaker");
			idleBreakerImpulse = false;
		}
		if (enemyGnome.currentState == EnemyGnome.State.Notice)
		{
			if (noticeImpulse)
			{
				animator.SetTrigger("Notice");
				noticeImpulse = false;
			}
		}
		else
		{
			noticeImpulse = true;
		}
		if (enemyGnome.enemy.Rigidbody.physGrabObject.rbVelocity.magnitude > 0.2f || enemyGnome.enemy.Rigidbody.physGrabObject.rbAngularVelocity.magnitude > 0.5f)
		{
			animator.SetBool("Moving", value: true);
		}
		else
		{
			animator.SetBool("Moving", value: false);
		}
		if (enemyGnome.currentState == EnemyGnome.State.Despawn)
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
		animator.SetBool("Despawning", value: false);
		animator.SetBool("Stunned", value: false);
		animator.Play("Spawn", 0, 0f);
	}

	public void AttackDone()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemyGnome.UpdateState(EnemyGnome.State.AttackDone);
		}
	}

	public void Footstep()
	{
		soundFootstep.Play(enemy.CenterTransform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Light, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void DespawnSet()
	{
		enemy.EnemyParent.Despawn();
	}

	public void MoveShort()
	{
		soundMoveShort.Play(enemy.CenterTransform.position);
	}

	public void MoveLong()
	{
		soundMoveLong.Play(enemy.CenterTransform.position);
	}

	public void PickaxeTell()
	{
		soundPickaxeTell.Play(enemy.CenterTransform.position);
	}

	public void PickaxeHit()
	{
		GameDirector.instance.CameraShake.ShakeDistance(2f, 2f, 5f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 2f, 5f, enemy.CenterTransform.position, 0.05f);
		soundPickaxeHit.Play(enemy.CenterTransform.position);
	}

	public void IdleBreaker()
	{
		soundIdleBreaker.Play(enemy.CenterTransform.position);
	}

	public void Notice()
	{
		soundNotice.Play(enemy.CenterTransform.position);
	}

	public void Jump()
	{
		soundJump.Play(enemy.CenterTransform.position);
	}

	public void Land()
	{
		soundLand.Play(enemy.CenterTransform.position);
	}

	public void Spawn()
	{
		soundSpawn.Play(enemy.CenterTransform.position);
	}

	public void Despawn()
	{
		soundDespawn.Play(enemy.CenterTransform.position);
	}

	public void StunOutro()
	{
		soundStunOutro.Play(enemy.CenterTransform.position);
	}
}
