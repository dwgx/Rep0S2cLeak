using UnityEngine;

public class EnemyRunnerAnim : MonoBehaviour
{
	private int animMoving = Animator.StringToHash("moving");

	private int animSeeking = Animator.StringToHash("seeking");

	private int animAttacking = Animator.StringToHash("attacking");

	private int animStunned = Animator.StringToHash("stunned");

	private int animFalling = Animator.StringToHash("falling");

	private int animLookingUnder = Animator.StringToHash("lookingUnder");

	private int animLeaving = Animator.StringToHash("leaving");

	private int animLand = Animator.StringToHash("Land");

	private int animLookUnder = Animator.StringToHash("LookUnder");

	private int animJump = Animator.StringToHash("Jump");

	private int animNotice = Animator.StringToHash("Notice");

	private int animStun = Animator.StringToHash("Stun");

	private int animDespawn = Animator.StringToHash("Despawn");

	public Enemy enemy;

	public EnemyRunner controller;

	internal Animator animator;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	private bool stunned;

	private float moveTimer;

	private bool stunImpulse;

	private bool despawnImpulse;

	internal bool spawnImpulse;

	private bool landImpulse;

	private bool lookUnderImpulse;

	private bool noticeImpulse;

	private bool jumpImpulse;

	private float jumpedTimer;

	[Header("One Shots")]
	public Sound sfxJump;

	public Sound sfxHurt;

	public Sound sfxDeath;

	public Sound sfxMoveShort;

	public Sound sfxMoveLong;

	public Sound sfxFootstepSlow;

	public Sound sfxFootstepFast;

	public Sound sfxAttackSlash;

	public Sound sfxAttackGrunt;

	private bool attackGruntImpulse = true;

	private int attackGruntCounter;

	[Header("Loops")]
	public Sound sfxStunnedLoop;

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
		if (!stunned && enemy.Jump.jumping)
		{
			if (jumpImpulse)
			{
				animator.SetTrigger(animJump);
				animator.SetBool(animFalling, value: false);
				jumpImpulse = false;
				landImpulse = true;
			}
			else if (controller.enemy.Rigidbody.physGrabObject.rbVelocity.y < 0f)
			{
				animator.SetBool(animFalling, value: true);
			}
		}
		else
		{
			if (landImpulse)
			{
				animator.SetTrigger(animLand);
				landImpulse = false;
			}
			animator.SetBool(animFalling, value: false);
			jumpImpulse = true;
		}
		if (controller.currentState == EnemyRunner.State.LookUnder)
		{
			if (lookUnderImpulse)
			{
				animator.SetTrigger(animLookUnder);
				lookUnderImpulse = false;
			}
			animator.SetBool(animLookingUnder, value: true);
		}
		else
		{
			animator.SetBool(animLookingUnder, value: false);
			lookUnderImpulse = true;
		}
		float num = 0.05f;
		if (IsMoving() && (enemy.Rigidbody.velocity.magnitude > num || enemy.Rigidbody.physGrabObject.rbAngularVelocity.magnitude > num))
		{
			moveTimer = 0.1f;
		}
		if (moveTimer > 0f)
		{
			moveTimer -= Time.deltaTime;
			animator.SetBool(animMoving, value: true);
		}
		else
		{
			animator.SetBool(animMoving, value: false);
		}
		if (controller.currentState == EnemyRunner.State.SeekPlayer || controller.currentState == EnemyRunner.State.Sneak)
		{
			animator.SetBool(animSeeking, value: true);
		}
		else
		{
			animator.SetBool(animSeeking, value: false);
		}
		if (controller.currentState == EnemyRunner.State.AttackPlayer || controller.currentState == EnemyRunner.State.AttackPlayerOver || controller.currentState == EnemyRunner.State.AttackPlayerBackToNavMesh || controller.currentState == EnemyRunner.State.StuckAttack || controller.currentState == EnemyRunner.State.LookUnderStart)
		{
			animator.SetBool(animAttacking, value: true);
		}
		else
		{
			animator.SetBool(animAttacking, value: false);
		}
		if (controller.currentState == EnemyRunner.State.Notice || controller.currentState == EnemyRunner.State.StuckAttackNotice)
		{
			if (noticeImpulse)
			{
				animator.SetTrigger(animNotice);
				noticeImpulse = false;
			}
		}
		else
		{
			noticeImpulse = true;
		}
		if (controller.currentState == EnemyRunner.State.Stun)
		{
			if (stunImpulse)
			{
				animator.SetTrigger(animStun);
				stunImpulse = false;
			}
			animator.SetBool(animStunned, value: true);
			stunned = true;
		}
		else
		{
			animator.SetBool(animStunned, value: false);
			stunImpulse = true;
			stunned = false;
		}
		sfxStunnedLoop.PlayLoop(stunned, 5f, 5f);
		if (controller.currentState == EnemyRunner.State.Despawn)
		{
			if (despawnImpulse)
			{
				animator.SetTrigger(animDespawn);
				despawnImpulse = false;
			}
		}
		else
		{
			despawnImpulse = true;
		}
		if (controller.currentState == EnemyRunner.State.Leave)
		{
			animator.SetBool(animLeaving, value: true);
		}
		else
		{
			animator.SetBool(animLeaving, value: false);
		}
	}

	public void OnSpawn()
	{
		animator.Play("Spawn", 0, 0f);
	}

	private bool IsMoving()
	{
		if (controller.currentState != EnemyRunner.State.Roam && controller.currentState != EnemyRunner.State.Investigate && controller.currentState != EnemyRunner.State.SeekPlayer && controller.currentState != EnemyRunner.State.Sneak)
		{
			return controller.currentState == EnemyRunner.State.Leave;
		}
		return true;
	}

	public void Despawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void LookUnderIntro()
	{
		if ((bool)Camera.main && (bool)AudioScare.instance && (bool)GameDirector.instance)
		{
			if (Vector3.Distance(base.transform.position, Camera.main.transform.position) < 10f)
			{
				AudioScare.instance.PlaySoft();
			}
			GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		}
	}

	public void SfxJump()
	{
		sfxJump.Play(base.transform.position);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 8f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 3f, 8f, base.transform.position, 0.5f);
	}

	public void SfxHurt()
	{
		sfxHurt.Play(base.transform.position);
	}

	public void SfxDeath()
	{
		sfxDeath.Play(base.transform.position);
	}

	public void SfxMoveShort()
	{
		sfxMoveShort.Play(base.transform.position);
	}

	public void SfxMoveLong()
	{
		sfxMoveLong.Play(base.transform.position);
	}

	public void SfxFootstepSlow()
	{
		sfxFootstepSlow.Play(base.transform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 1f, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void SfxFootstepFast()
	{
		sfxFootstepFast.Play(base.transform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 1f, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void SfxAttackSlash()
	{
		sfxAttackSlash.Play(base.transform.position);
	}

	public void SfxAttackGrunt()
	{
		if (attackGruntImpulse)
		{
			sfxAttackGrunt.Play(base.transform.position);
			attackGruntImpulse = false;
		}
		else
		{
			attackGruntImpulse = true;
		}
	}

	public void SfxAttackUnderGrunt()
	{
		attackGruntCounter++;
		if (attackGruntCounter >= 3)
		{
			sfxAttackGrunt.Play(base.transform.position);
			attackGruntCounter = 0;
		}
	}

	public void ResetAttackGruntCounter()
	{
		attackGruntCounter = 3;
	}

	public void SfxStunnedLoop()
	{
		sfxStunnedLoop.PlayLoop(stunned, 5f, 5f);
	}
}
