using Photon.Pun;
using UnityEngine;

public class EnemyDuckAnim : MonoBehaviour
{
	public Enemy enemy;

	internal Animator animator;

	public EnemyDuck controller;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	public Sound quackSound;

	public Sound stunSound;

	public Sound stunStopSound;

	public Sound biteSound;

	public Sound transformSound;

	public Sound jumpSound;

	public Sound footstepSound;

	public Sound mouthExtendSound;

	public Sound mouthRetractSound;

	public Sound attackLoopSound;

	public Sound hurtSound;

	public Sound deathSound;

	public Sound noticeSound;

	public Sound flyFlapSound;

	public Sound flyLoopSound;

	public float soundHurtPauseTimer;

	private bool jumpImpulse;

	private bool flyImpulse;

	private bool landImpulse;

	private bool stunImpulse;

	private bool noticeImpulse;

	private bool transformImpulse;

	private float idleBreakerTimer;

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
		else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk") && !animator.IsInTransition(0))
		{
			animator.speed = Mathf.Clamp(enemy.Rigidbody.velocity.magnitude + 0.2f, 0.8f, 1.2f);
		}
		else
		{
			animator.speed = 1f;
		}
		if (controller.currentState != EnemyDuck.State.AttackStart && controller.currentState != EnemyDuck.State.Transform && controller.currentState != EnemyDuck.State.ChaseNavmesh && controller.currentState != EnemyDuck.State.ChaseTowards && controller.currentState != EnemyDuck.State.ChaseMoveBack && controller.currentState != EnemyDuck.State.DeTransform)
		{
			if (enemy.Rigidbody.velocity.magnitude > 0.1f)
			{
				animator.SetBool("move", value: true);
			}
			else
			{
				animator.SetBool("move", value: false);
			}
			if (!enemy.IsStunned())
			{
				if (!enemy.Grounded.grounded && (controller.currentState == EnemyDuck.State.FlyBackToNavmesh || controller.currentState == EnemyDuck.State.FlyBackToNavmeshStop))
				{
					if (flyImpulse)
					{
						animator.SetTrigger("fly");
						animator.SetBool("falling", value: false);
						flyImpulse = false;
						landImpulse = true;
					}
					else if (controller.currentState == EnemyDuck.State.FlyBackToNavmeshStop)
					{
						animator.SetBool("falling", value: true);
					}
				}
				else if (enemy.Jump.jumping)
				{
					if (jumpImpulse)
					{
						animator.SetTrigger("jump");
						animator.SetBool("falling", value: false);
						jumpImpulse = false;
						landImpulse = true;
					}
					else if (controller.enemy.Rigidbody.physGrabObject.rbVelocity.y < 0f)
					{
						animator.SetBool("falling", value: true);
					}
				}
				else
				{
					if (landImpulse)
					{
						animator.SetTrigger("land");
						landImpulse = false;
					}
					animator.SetBool("falling", value: false);
					jumpImpulse = true;
					flyImpulse = true;
				}
			}
		}
		if (controller.currentState == EnemyDuck.State.AttackStart)
		{
			if (transformImpulse)
			{
				animator.SetTrigger("transform");
				transformImpulse = false;
			}
		}
		else
		{
			transformImpulse = true;
		}
		if (controller.currentState == EnemyDuck.State.AttackStart || controller.currentState == EnemyDuck.State.Transform || controller.currentState == EnemyDuck.State.ChaseNavmesh || controller.currentState == EnemyDuck.State.ChaseTowards || controller.currentState == EnemyDuck.State.ChaseMoveBack)
		{
			animator.SetBool("move", value: false);
			animator.SetBool("chase", value: true);
			if (soundHurtPauseTimer > 0f)
			{
				StopAttackSound();
			}
			else
			{
				attackLoopSound.PlayLoop(playing: true, 5f, 5f);
			}
		}
		else
		{
			StopAttackSound();
			animator.SetBool("chase", value: false);
		}
		if (controller.currentState == EnemyDuck.State.Notice)
		{
			if (noticeImpulse)
			{
				animator.SetTrigger("notice");
				noticeImpulse = false;
			}
		}
		else
		{
			noticeImpulse = true;
		}
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !animator.IsInTransition(0))
		{
			idleBreakerTimer += Time.deltaTime;
			if (idleBreakerTimer > 5f)
			{
				idleBreakerTimer = 0f;
				if (Random.Range(0, 100) < 35)
				{
					controller.IdleBreakerSet();
				}
			}
		}
		if (controller.idleBreakerTrigger)
		{
			animator.SetTrigger("idlebreak");
			controller.idleBreakerTrigger = false;
		}
		if (controller.currentState == EnemyDuck.State.Stun)
		{
			landImpulse = false;
			if (stunImpulse)
			{
				animator.SetTrigger("stun");
				stunImpulse = false;
			}
			animator.SetBool("stunned", value: true);
			if (soundHurtPauseTimer > 0f)
			{
				stunSound.PlayLoop(playing: false, 5f, 2f);
			}
			else
			{
				stunSound.PlayLoop(playing: true, 5f, 5f);
			}
		}
		else
		{
			animator.SetBool("stunned", value: false);
			stunSound.PlayLoop(playing: false, 5f, 1f);
			if (!stunImpulse)
			{
				stunStopSound.Play(base.transform.position);
				stunImpulse = true;
			}
		}
		if (controller.currentState == EnemyDuck.State.Despawn)
		{
			animator.SetBool("despawning", value: true);
		}
		else
		{
			animator.SetBool("despawning", value: false);
		}
		if (controller.currentState == EnemyDuck.State.FlyBackToNavmesh)
		{
			flyLoopSound.PlayLoop(playing: true, 5f, 2f);
		}
		else
		{
			flyLoopSound.PlayLoop(playing: false, 5f, 2f);
		}
		if (soundHurtPauseTimer > 0f)
		{
			soundHurtPauseTimer -= Time.deltaTime;
		}
	}

	public void OnSpawn()
	{
		animator.Play("Spawn", 0, 0f);
	}

	private void Quack()
	{
		quackSound.Play(base.transform.position);
		if ((!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient) && controller.currentState != EnemyDuck.State.Idle && controller.currentState != EnemyDuck.State.Roam && controller.currentState != EnemyDuck.State.Investigate && controller.currentState != EnemyDuck.State.Leave && controller.currentState != EnemyDuck.State.MoveBackToNavmesh)
		{
			EnemyDirector.instance.SetInvestigate(base.transform.position, 10f);
		}
	}

	private void BiteSound()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.Rigidbody.GrabRelease();
		}
		GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
		biteSound.Play(base.transform.position);
	}

	private void TransformSound()
	{
		if (base.enabled)
		{
			if (Vector3.Distance(base.transform.position, Camera.main.transform.position) < 10f)
			{
				AudioScare.instance.PlayImpact();
			}
			transformSound.Play(base.transform.position);
		}
	}

	private void JumpSound()
	{
		jumpSound.Play(base.transform.position);
	}

	private void FootstepSound()
	{
		footstepSound.Play(base.transform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Light, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	private void MouthExtendSound()
	{
		mouthExtendSound.Play(base.transform.position);
	}

	private void MouthRetractSound()
	{
		mouthRetractSound.Play(base.transform.position);
	}

	private void StopAttackSound()
	{
		attackLoopSound.PlayLoop(playing: false, 5f, 5f);
	}

	private void NoticeSound()
	{
		noticeSound.Play(base.transform.position);
	}

	private void FlyFlapSound()
	{
		flyFlapSound.Play(base.transform.position);
	}

	public void Despawn()
	{
		enemy.EnemyParent.Despawn();
	}
}
