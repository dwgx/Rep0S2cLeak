using Photon.Pun;
using UnityEngine;

public class EnemyBeamerAnim : MonoBehaviour
{
	public Enemy enemy;

	public EnemyBeamer controller;

	public bool noseEmission;

	private Animator animator;

	private int animSpawn = Animator.StringToHash("spawn");

	private int animDespawning = Animator.StringToHash("despawning");

	private int animStun = Animator.StringToHash("stun");

	private int animStunned = Animator.StringToHash("stunned");

	private int animMoving = Animator.StringToHash("moving");

	private int animMovingFast = Animator.StringToHash("movingFast");

	private int animJump = Animator.StringToHash("jump");

	private int animFalling = Animator.StringToHash("falling");

	private int animLand = Animator.StringToHash("land");

	private int animMelee = Animator.StringToHash("melee");

	private int animAttacking = Animator.StringToHash("attacking");

	private int animAttack = Animator.StringToHash("attack");

	public float springSpeedMultiplier;

	public float springDampingMultiplier;

	[Space]
	public SpringQuaternion springNose01;

	private float springNose01Speed;

	private float springNose01Damping;

	public Transform springNose01Target;

	public Transform springNose01Source;

	public SpringQuaternion springNose02;

	private float springNose02Speed;

	private float springNose02Damping;

	public Transform springNose02Target;

	public Transform springNose02Source;

	public SpringQuaternion springNose03;

	private float springNose03Speed;

	private float springNose03Damping;

	public Transform springNose03Target;

	public Transform springNose03Source;

	private float moveTimer;

	private bool spawnImpulse = true;

	private bool stunImpulse = true;

	private bool stunEndImpulse;

	private bool jumpImpulse = true;

	private bool landImpulse = true;

	internal bool meleeImpulse = true;

	private bool attackImpulse = true;

	private Color emissionColor = Color.black;

	private Color emissionColorPrevious = Color.black;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	internal float soundHurtPauseTimer;

	public Sound soundMoveShort;

	public Sound soundMoveLong;

	[Space]
	public Sound soundFootstepSmall;

	public Sound soundFootstepBig;

	public Sound soundFootstepHuge;

	[Space]
	public Sound soundStunIntro;

	public Sound soundStunLoop;

	public Sound soundStunOutro;

	[Space]
	public Sound soundJump;

	public Sound soundLand;

	[Space]
	public Sound soundMeleeTell;

	public Sound soundMeleeKick;

	[Space]
	public bool soundAttackLoopActive;

	public Sound soundAttackIntro;

	public Sound soundAttackLoop;

	public Sound soundAttackOutro;

	[Space]
	public Sound soundHurt;

	public Sound soundDeath;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
		springNose01Speed = springNose01.speed;
		springNose01Damping = springNose01.damping;
		springNose02Speed = springNose02.speed;
		springNose02Damping = springNose02.damping;
		springNose03Speed = springNose03.speed;
		springNose03Damping = springNose03.damping;
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
		if (controller.currentState == EnemyBeamer.State.Stun)
		{
			if (stunImpulse)
			{
				soundStunIntro.Play(base.transform.position);
				animator.SetTrigger(animStun);
				stunImpulse = false;
			}
			animator.SetBool(animStunned, value: true);
			if (soundHurtPauseTimer > 0f)
			{
				soundStunLoop.PlayLoop(playing: false, 5f, 5f);
			}
			else
			{
				soundStunLoop.PlayLoop(playing: true, 5f, 5f);
			}
			stunEndImpulse = true;
		}
		else
		{
			if (stunEndImpulse)
			{
				soundStunOutro.Play(base.transform.position);
				stunEndImpulse = false;
			}
			animator.SetBool(animStunned, value: false);
			soundStunLoop.PlayLoop(playing: false, 5f, 5f);
			stunImpulse = true;
		}
		if (noseEmission)
		{
			emissionColor = Color.Lerp(emissionColor, Color.white, Time.deltaTime * 10f);
		}
		else
		{
			emissionColor = Color.Lerp(emissionColor, Color.black, Time.deltaTime * 10f);
		}
		if (emissionColor != emissionColorPrevious)
		{
			emissionColorPrevious = emissionColor;
			enemy.Health.instancedMaterials[0].SetColor("_EmissionColor", emissionColor);
		}
		springNose01.speed = springNose01Speed * springSpeedMultiplier;
		springNose01.damping = springNose01Damping * springDampingMultiplier;
		springNose01Source.rotation = SemiFunc.SpringQuaternionGet(springNose01, springNose01Target.transform.rotation);
		springNose02.speed = springNose02Speed * springSpeedMultiplier;
		springNose02.damping = springNose02Damping * springDampingMultiplier;
		springNose02Source.rotation = SemiFunc.SpringQuaternionGet(springNose02, springNose02Target.transform.rotation);
		springNose03.speed = springNose03Speed * springSpeedMultiplier;
		springNose03.damping = springNose03Damping * springDampingMultiplier;
		springNose03Source.rotation = SemiFunc.SpringQuaternionGet(springNose03, springNose03Target.transform.rotation);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!enemy.Jump.jumping && !enemy.IsStunned() && (controller.currentState == EnemyBeamer.State.MeleeStart || controller.currentState == EnemyBeamer.State.Melee))
			{
				if (meleeImpulse)
				{
					if (SemiFunc.IsMultiplayer())
					{
						controller.photonView.RPC("MeleeTriggerRPC", RpcTarget.Others);
					}
					animator.SetTrigger(animMelee);
					meleeImpulse = false;
				}
			}
			else
			{
				meleeImpulse = true;
			}
		}
		else if (meleeImpulse)
		{
			animator.SetTrigger(animMelee);
			meleeImpulse = false;
		}
		if (controller.currentState == EnemyBeamer.State.AttackStart || controller.currentState == EnemyBeamer.State.Attack)
		{
			if (attackImpulse)
			{
				attackImpulse = false;
				animator.SetTrigger(animAttack);
			}
			animator.SetBool(animAttacking, value: true);
		}
		else
		{
			attackImpulse = true;
			animator.SetBool(animAttacking, value: false);
		}
		if (soundHurtPauseTimer > 0f)
		{
			soundAttackLoop.PlayLoop(playing: false, 5f, 5f);
		}
		else
		{
			soundAttackLoop.PlayLoop(soundAttackLoopActive, 5f, 5f);
		}
		if ((controller.currentState == EnemyBeamer.State.Roam || controller.currentState == EnemyBeamer.State.Investigate || controller.currentState == EnemyBeamer.State.Seek || controller.currentState == EnemyBeamer.State.Leave) && (enemy.Rigidbody.velocity.magnitude > 0.5f || enemy.Rigidbody.physGrabObject.rbAngularVelocity.magnitude > 5f))
		{
			moveTimer = 0.25f;
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
		animator.SetBool(animMovingFast, controller.moveFast);
		if (enemy.Jump.jumping || enemy.Jump.jumpingDelay)
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
			else if (controller.enemy.Rigidbody.physGrabObject.rbVelocity.y < -0.5f)
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
		if (controller.currentState == EnemyBeamer.State.Spawn)
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
		if (controller.currentState == EnemyBeamer.State.Despawn)
		{
			animator.SetBool(animDespawning, value: true);
		}
		else
		{
			animator.SetBool(animDespawning, value: false);
		}
		if (soundHurtPauseTimer > 0f)
		{
			soundHurtPauseTimer -= Time.deltaTime;
		}
	}

	public void Despawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void FootstepSmall()
	{
		soundFootstepSmall.Play(base.transform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Light, footstep: false, footstepParticles: false, material, Materials.HostType.Enemy);
	}

	public void FootstepBig()
	{
		GameDirector.instance.CameraShake.ShakeDistance(2f, 5f, 10f, base.transform.position, 0.25f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 5f, 10f, base.transform.position, 0.1f);
		soundFootstepBig.Play(base.transform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Heavy, footstep: false, footstepParticles: false, material, Materials.HostType.Enemy);
	}

	public void FootstepHuge()
	{
		GameDirector.instance.CameraShake.ShakeDistance(2.5f, 5f, 15f, base.transform.position, 0.25f);
		GameDirector.instance.CameraImpact.ShakeDistance(2.5f, 5f, 15f, base.transform.position, 0.1f);
		soundFootstepHuge.Play(base.transform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Heavy, footstep: false, footstepParticles: false, material, Materials.HostType.Enemy);
	}

	public void Jump()
	{
		controller.particleBottomSmoke.transform.position = controller.bottomTransform.position;
		controller.particleBottomSmoke.Play();
		soundJump.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 5f, 10f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 5f, 10f, base.transform.position, 0.1f);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Heavy, footstep: false, footstepParticles: false, material, Materials.HostType.Enemy);
	}

	public void Land()
	{
		controller.particleBottomSmoke.transform.position = controller.bottomTransform.position;
		controller.particleBottomSmoke.Play();
		soundLand.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 5f, 10f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 5f, 10f, base.transform.position, 0.1f);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Heavy, footstep: false, footstepParticles: false, material, Materials.HostType.Enemy);
	}

	public void MoveShort()
	{
		soundMoveShort.Play(base.transform.position);
	}

	public void MoveLong()
	{
		soundMoveLong.Play(base.transform.position);
	}

	public void MeleeTell()
	{
		soundMeleeTell.Play(base.transform.position);
	}

	public void MeleeKick()
	{
		soundMeleeKick.Play(base.transform.position);
	}

	public void AttackIntro()
	{
		soundAttackIntro.Play(base.transform.position);
	}

	public void AttackOutro()
	{
		soundAttackOutro.Play(base.transform.position);
	}
}
