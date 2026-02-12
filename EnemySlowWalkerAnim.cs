using UnityEngine;

public class EnemySlowWalkerAnim : MonoBehaviour
{
	public Enemy enemy;

	public EnemySlowWalker controller;

	public Transform maceTransform;

	internal Animator animator;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	public SlowWalkerSparkEffect slowWalkerSparkEffect;

	private int animMoving = Animator.StringToHash("moving");

	private int animStunned = Animator.StringToHash("stunned");

	private int animDespawning = Animator.StringToHash("despawning");

	private int animFalling = Animator.StringToHash("falling");

	private int animLookingUnder = Animator.StringToHash("lookingUnder");

	private int animStun = Animator.StringToHash("Stun");

	private int animNotice = Animator.StringToHash("Notice");

	private int animAttack = Animator.StringToHash("Attack");

	private int animJump = Animator.StringToHash("Jump");

	private int animLand = Animator.StringToHash("Land");

	private int animLookUnder = Animator.StringToHash("LookUnder");

	private int animLookUnderAttack = Animator.StringToHash("LookUnderAttack");

	private int animStuckAttack = Animator.StringToHash("StuckAttack");

	public float springSpeedMultiplier = 1f;

	public float springDampingMultiplier = 1f;

	public SpringQuaternion springNeck01;

	private float springNeck01Speed;

	private float springNeck01Damping;

	public Transform springNeck01Target;

	public Transform springNeck01Source;

	public SpringQuaternion springNeck02;

	private float springNeck02Speed;

	private float springNeck02Damping;

	public Transform springNeck02Target;

	public Transform springNeck02Source;

	public SpringQuaternion springNeck03;

	private float springNeck03Speed;

	private float springNeck03Damping;

	public Transform springNeck03Target;

	public Transform springNeck03Source;

	public SpringQuaternion springEyeFlesh;

	private float springEyeFleshSpeed;

	private float springEyeFleshDamping;

	public Transform springEyeFleshTarget;

	public Transform springEyeFleshSource;

	public SpringQuaternion springEyeBall;

	private float springEyeBallSpeed;

	private float springEyeBallDamping;

	public Transform springEyeBallTarget;

	public Transform springEyeBallSource;

	private bool stunned;

	private bool stunImpulse;

	private bool noticeImpulse;

	private bool delayAttackImpulse;

	private bool attackImpulse;

	private bool chargeAttackImpulse;

	private bool jumpImpulse;

	private bool landImpulse;

	private bool lookUnderImpulse;

	private bool lookUnderAttackImpulse;

	private bool stuckAttackImpulse;

	private float moveTimer;

	private float jumpedTimer;

	public Sound sfxFootstepSmall;

	public Sound sfxFootstepBig;

	public Sound sfxJump;

	public Sound sfxLand;

	public Sound sfxMoveShort;

	public Sound sfxMoveLong;

	public Sound sfxAttackBuildupVoice;

	public Sound sfxAttackImpact;

	public Sound sfxAttackImplosionBuildup;

	public Sound sfxAttackImplosionHitLocal;

	public Sound sfxAttackImplosionHitGlobal;

	public Sound sfxAttackImplosionImpactLocal;

	public Sound sfxAttackImplosionImpactGlobal;

	public Sound sfxDeath;

	public Sound sfxHurt;

	public Sound sfxNoiseShort;

	public Sound sfxNoiseLong;

	public Sound sfxNoticeVoice;

	public Sound sfxSwingShort;

	public Sound sfxSwingLong;

	public Sound sfxMaceTrailing;

	public Sound sfxLookUnderIntro;

	public Sound sfxLookUnderAttack;

	public Sound sfxLookUnderOutro;

	public Sound sfxStunnedLoop;

	public SlowWalkerAttack slowWalkerAttack;

	public SlowWalkerJumpEffect slowWalkerJumpEffect;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
		springNeck01Speed = springNeck01.speed;
		springNeck01Damping = springNeck01.damping;
		springNeck02Speed = springNeck02.speed;
		springNeck02Damping = springNeck02.damping;
		springNeck03Speed = springNeck03.speed;
		springNeck03Damping = springNeck03.damping;
		springEyeFleshSpeed = springEyeFlesh.speed;
		springEyeFleshDamping = springEyeFlesh.damping;
		springEyeBallSpeed = springEyeBall.speed;
		springEyeBallDamping = springEyeBall.damping;
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
		if (!stunned && (enemy.Jump.jumping || enemy.Jump.jumpingDelay))
		{
			if (jumpImpulse)
			{
				animator.SetTrigger(animJump);
				animator.SetBool(animFalling, value: false);
				jumpImpulse = false;
				landImpulse = true;
			}
			else if (controller.enemy.Rigidbody.physGrabObject.rbVelocity.y < -1f)
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
		if (controller.currentState == EnemySlowWalker.State.LookUnder || controller.currentState == EnemySlowWalker.State.LookUnderIntro || controller.currentState == EnemySlowWalker.State.LookUnderAttack)
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
		if (controller.currentState == EnemySlowWalker.State.LookUnderAttack)
		{
			if (lookUnderAttackImpulse)
			{
				animator.SetTrigger(animLookUnderAttack);
				lookUnderAttackImpulse = false;
			}
		}
		else
		{
			lookUnderAttackImpulse = true;
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
		if (controller.currentState == EnemySlowWalker.State.Stun)
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
		SfxStunnedLoop();
		if (controller.currentState == EnemySlowWalker.State.Notice)
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
		if (controller.currentState == EnemySlowWalker.State.Attack)
		{
			if (attackImpulse)
			{
				animator.SetTrigger(animAttack);
				attackImpulse = false;
			}
		}
		else
		{
			attackImpulse = true;
		}
		if (controller.currentState == EnemySlowWalker.State.StuckAttack)
		{
			if (stuckAttackImpulse)
			{
				animator.SetTrigger(animStuckAttack);
				stuckAttackImpulse = false;
			}
		}
		else
		{
			stuckAttackImpulse = true;
		}
		if (controller.currentState == EnemySlowWalker.State.Despawn)
		{
			animator.SetBool(animDespawning, value: true);
		}
		else
		{
			animator.SetBool(animDespawning, value: false);
		}
	}

	public void AttackOffsetStart()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			controller.attackOffsetActive = true;
		}
	}

	public void AttackOffsetStop()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			controller.attackOffsetActive = false;
		}
	}

	public void AttackStart()
	{
		slowWalkerAttack.SlowWalkerAttackStart();
	}

	private void VfxJump()
	{
		slowWalkerJumpEffect.JumpEffect();
	}

	private void VfxLand()
	{
		slowWalkerJumpEffect.LandEffect();
	}

	public void VfxSparkStart()
	{
		slowWalkerSparkEffect.PlaySparkEffect();
	}

	public void VfxSparkStop()
	{
		slowWalkerSparkEffect.StopSparkEffect();
	}

	public void SfxFootstepSmall()
	{
		sfxFootstepSmall.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 5f, 10f, base.transform.position, 0.25f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 5f, 10f, base.transform.position, 0.1f);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Light, footstep: false, footstepParticles: false, material, Materials.HostType.Enemy);
	}

	public void SfxFootstepBig()
	{
		sfxFootstepBig.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(2f, 5f, 10f, base.transform.position, 0.25f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 5f, 10f, base.transform.position, 0.1f);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Heavy, footstep: false, footstepParticles: false, material, Materials.HostType.Enemy);
	}

	public void SfxJump()
	{
		sfxJump.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 5f, 10f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 5f, 10f, base.transform.position, 0.1f);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Heavy, footstep: false, footstepParticles: false, material, Materials.HostType.Enemy);
		VfxJump();
	}

	public void SfxLand()
	{
		sfxLand.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 5f, 10f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 5f, 10f, base.transform.position, 0.1f);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Heavy, footstep: false, footstepParticles: false, material, Materials.HostType.Enemy);
		VfxLand();
	}

	public void SfxMoveShort()
	{
		sfxMoveShort.Play(base.transform.position);
		SfxNoiseShort();
	}

	public void SfxMoveLong()
	{
		sfxMoveLong.Play(base.transform.position);
		SfxNoiseLong();
	}

	public void SfxAttackBuildupVoice()
	{
		sfxAttackBuildupVoice.Play(base.transform.position);
	}

	public void SfxAttackImpact()
	{
		sfxAttackImpact.Play(base.transform.position);
	}

	public void SfxAttackImplosionBuildup()
	{
	}

	public void SfxAttackImplosionHit()
	{
	}

	public void SfxAttackImplosionImpact()
	{
	}

	public void SfxDeath()
	{
		sfxDeath.Play(base.transform.position);
	}

	public void SfxHurt()
	{
		sfxHurt.Play(base.transform.position);
	}

	public void SfxNoiseShort()
	{
		if (Random.value <= 0.6f)
		{
			sfxNoiseShort.Play(base.transform.position);
		}
	}

	public void SfxNoiseLong()
	{
		if (Random.value <= 0.6f)
		{
			sfxNoiseLong.Play(base.transform.position);
		}
	}

	public void SfxNoticeVoice()
	{
		sfxNoticeVoice.Play(base.transform.position);
	}

	public void SfxSwingShort()
	{
		sfxSwingShort.Play(base.transform.position);
	}

	public void SfxSwingLong()
	{
		sfxSwingLong.Play(base.transform.position);
	}

	public void SfxMaceTrailing()
	{
		sfxMaceTrailing.Play(maceTransform.position);
	}

	public void SfxLookUnderIntro()
	{
		sfxLookUnderIntro.Play(base.transform.position);
	}

	public void SfxLookUnderAttack()
	{
		sfxLookUnderAttack.Play(base.transform.position);
	}

	public void SfxLookUnderOutro()
	{
		sfxLookUnderOutro.Play(base.transform.position);
	}

	public void SfxStunnedLoop()
	{
		sfxStunnedLoop.PlayLoop(stunned, 5f, 5f);
	}

	public void OnSpawn()
	{
		animator.SetBool(animStunned, value: false);
		animator.Play("Spawn", 0, 0f);
	}

	public void Despawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void NoticeSet(int _playerID)
	{
		animator.SetTrigger(animNotice);
	}

	private bool IsMoving()
	{
		if (controller.currentState != EnemySlowWalker.State.Roam && controller.currentState != EnemySlowWalker.State.Investigate && controller.currentState != EnemySlowWalker.State.GoToPlayer && controller.currentState != EnemySlowWalker.State.LookUnderStart && controller.currentState != EnemySlowWalker.State.Sneak)
		{
			return controller.currentState == EnemySlowWalker.State.Leave;
		}
		return true;
	}

	public void SpringLogic()
	{
		springNeck01.speed = springNeck01Speed * springSpeedMultiplier;
		springNeck01.damping = springNeck01Damping * springDampingMultiplier;
		springNeck01Source.rotation = SemiFunc.SpringQuaternionGet(springNeck01, springNeck01Target.transform.rotation);
		springNeck02.speed = springNeck02Speed * springSpeedMultiplier;
		springNeck02.damping = springNeck02Damping * springDampingMultiplier;
		springNeck02Source.rotation = SemiFunc.SpringQuaternionGet(springNeck02, springNeck02Target.transform.rotation);
		springNeck03.speed = springNeck03Speed * springSpeedMultiplier;
		springNeck03.damping = springNeck03Damping * springDampingMultiplier;
		springNeck03Source.rotation = SemiFunc.SpringQuaternionGet(springNeck03, springNeck03Target.transform.rotation);
		springEyeFlesh.speed = springEyeFleshSpeed * springSpeedMultiplier;
		springEyeFlesh.damping = springEyeFleshDamping * springDampingMultiplier;
		springEyeFleshSource.rotation = SemiFunc.SpringQuaternionGet(springEyeFlesh, springEyeFleshTarget.transform.rotation);
		springEyeBall.speed = springEyeBallSpeed * springSpeedMultiplier;
		springEyeBall.damping = springEyeBallDamping * springDampingMultiplier;
		springEyeBallSource.rotation = SemiFunc.SpringQuaternionGet(springEyeBall, springEyeBallTarget.transform.rotation);
	}
}
