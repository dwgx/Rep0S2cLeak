using UnityEngine;

public class EnemyBangAnim : MonoBehaviour
{
	public Enemy enemy;

	public EnemyBang controller;

	internal Animator animator;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	private int animSpawn = Animator.StringToHash("spawn");

	private int animDespawning = Animator.StringToHash("despawning");

	private int animStun = Animator.StringToHash("stun");

	private int animStunned = Animator.StringToHash("stunned");

	private int animMoving = Animator.StringToHash("moving");

	private int animJump = Animator.StringToHash("jump");

	private int animFalling = Animator.StringToHash("falling");

	private int animLand = Animator.StringToHash("land");

	private int animFuse = Animator.StringToHash("fuse");

	private float moveTimer;

	private bool spawnImpulse = true;

	private bool stunImpulse = true;

	private bool jumpImpulse = true;

	private bool landImpulse = true;

	private bool fuseImpulse = true;

	[Range(0f, 1f)]
	public float volumeMultiplier;

	[Space]
	public Sound soundImpactLight;

	public Sound soundImpactMedium;

	public Sound soundImpactHeavy;

	[Space]
	public Sound soundMoveShort;

	public Sound soundMoveLong;

	[Space]
	public Sound soundFootstep;

	[Space]
	public Sound soundHurt;

	public Sound soundDeathSFX;

	public Sound soundDeathVO;

	[Space]
	public Sound soundJumpSFX;

	public Sound soundJumpVO;

	[Space]
	public Sound soundLandSFX;

	public Sound soundLandVO;

	[Space]
	public Sound soundStunIntro;

	public Sound soundStunLoop;

	public Sound soundStunOutro;

	private float stunLoopPauseTimer;

	[Space]
	public Sound soundIdleBreaker;

	public Sound soundAttackBreaker;

	[Space]
	public Sound soundFuseTell;

	public Sound soundExplosionTell;

	[Space]
	public Sound soundFuseIgnite;

	public Sound soundFuseLoop;

	public AnimationCurve soundFuseLoopCurve;

	private float fuseLoopTimer;

	private void Awake()
	{
		soundImpactLight.Volume *= volumeMultiplier;
		soundImpactMedium.Volume *= volumeMultiplier;
		soundImpactHeavy.Volume *= volumeMultiplier;
		soundMoveShort.Volume *= volumeMultiplier;
		soundMoveLong.Volume *= volumeMultiplier;
		soundFootstep.Volume *= volumeMultiplier;
		soundHurt.Volume *= volumeMultiplier;
		soundDeathSFX.Volume *= volumeMultiplier;
		soundDeathVO.Volume *= volumeMultiplier;
		soundJumpSFX.Volume *= volumeMultiplier;
		soundJumpVO.Volume *= volumeMultiplier;
		soundLandSFX.Volume *= volumeMultiplier;
		soundLandVO.Volume *= volumeMultiplier;
		soundStunIntro.Volume *= volumeMultiplier;
		soundStunLoop.Volume *= volumeMultiplier;
		soundStunOutro.Volume *= volumeMultiplier;
		soundIdleBreaker.Volume *= volumeMultiplier;
		soundAttackBreaker.Volume *= volumeMultiplier;
		soundFuseTell.Volume *= volumeMultiplier;
		soundFuseIgnite.Volume *= volumeMultiplier;
		soundFuseLoop.Volume *= volumeMultiplier;
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
		if (controller.currentState == EnemyBang.State.Spawn)
		{
			if (spawnImpulse)
			{
				spawnImpulse = false;
				animator.SetTrigger(animSpawn);
			}
		}
		else
		{
			spawnImpulse = true;
		}
		if ((controller.currentState == EnemyBang.State.Roam || controller.currentState == EnemyBang.State.Move || controller.currentState == EnemyBang.State.MoveUnder || controller.currentState == EnemyBang.State.MoveOver || controller.currentState == EnemyBang.State.MoveBack) && (enemy.Rigidbody.velocity.magnitude > 1f || enemy.Rigidbody.physGrabObject.rbAngularVelocity.magnitude > 1f))
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
		if (enemy.Jump.jumping)
		{
			if (jumpImpulse)
			{
				if (!enemy.IsStunned())
				{
					animator.SetTrigger(animJump);
				}
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
				if (!enemy.IsStunned())
				{
					animator.SetTrigger(animLand);
				}
				moveTimer = 0f;
				landImpulse = false;
			}
			animator.SetBool(animFalling, value: false);
			jumpImpulse = true;
		}
		if (controller.currentState == EnemyBang.State.Fuse)
		{
			if (fuseImpulse)
			{
				animator.SetTrigger(animFuse);
				fuseImpulse = false;
			}
		}
		else
		{
			fuseImpulse = true;
		}
		if (fuseLoopTimer > 0f)
		{
			fuseLoopTimer -= Time.deltaTime;
			soundFuseLoop.PlayLoop(playing: true, 2f, 2f, 1f + soundFuseLoopCurve.Evaluate(controller.fuseLerp) * 4f);
		}
		else
		{
			soundFuseLoop.PlayLoop(playing: false, 2f, 2f, 1f + soundFuseLoopCurve.Evaluate(controller.fuseLerp) * 4f);
		}
		if (controller.currentState == EnemyBang.State.Stun)
		{
			if (stunImpulse)
			{
				soundStunIntro.Play(controller.enemy.CenterTransform.position);
				animator.SetTrigger(animStun);
				stunImpulse = false;
			}
			if (stunLoopPauseTimer <= 0f)
			{
				soundStunLoop.PlayLoop(playing: true, 5f, 5f);
			}
			else
			{
				soundStunLoop.PlayLoop(playing: false, 5f, 5f);
			}
			animator.SetBool(animStunned, value: true);
		}
		else
		{
			soundStunLoop.PlayLoop(playing: false, 5f, 5f);
			animator.SetBool(animStunned, value: false);
			if (!stunImpulse)
			{
				soundStunOutro.Play(controller.enemy.CenterTransform.position);
				stunImpulse = true;
			}
		}
		if (stunLoopPauseTimer > 0f)
		{
			stunLoopPauseTimer -= Time.deltaTime;
		}
		if (controller.currentState == EnemyBang.State.Despawn)
		{
			animator.SetBool(animDespawning, value: true);
		}
		else
		{
			animator.SetBool(animDespawning, value: false);
		}
	}

	public void Despawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void FuseTellPlay()
	{
		soundFuseTell.Play(controller.enemy.CenterTransform.position);
	}

	public void FootstepPlay()
	{
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.25f, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
		soundFootstep.Play(controller.enemy.CenterTransform.position);
	}

	public void JumpPlay()
	{
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.25f, Vector3.down, Materials.SoundType.Medium, footstep: false, footstepParticles: false, material, Materials.HostType.Enemy);
		soundJumpSFX.Play(controller.enemy.CenterTransform.position);
		soundJumpVO.Play(controller.enemy.CenterTransform.position);
	}

	public void LandPlay()
	{
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.25f, Vector3.down, Materials.SoundType.Heavy, footstep: true, footstepParticles: false, material, Materials.HostType.Enemy);
		soundLandSFX.Play(controller.enemy.CenterTransform.position);
		soundLandVO.Play(controller.enemy.CenterTransform.position);
	}

	public void MoveShortPlay()
	{
		soundMoveShort.Play(controller.enemy.CenterTransform.position);
	}

	public void MoveLongPlay()
	{
		soundMoveLong.Play(controller.enemy.CenterTransform.position);
	}

	public void FuseLoop()
	{
		fuseLoopTimer = 0.1f;
	}

	public void StunLoopPause(float _time)
	{
		stunLoopPauseTimer = _time;
	}
}
