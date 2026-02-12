using Photon.Pun;
using UnityEngine;

public class EnemyBowtieAnim : MonoBehaviour
{
	public Transform followTarget;

	public EnemyBowtie controller;

	public Enemy enemy;

	public Materials.MaterialTrigger material;

	internal Animator animator;

	private bool attackImpulse;

	private bool stunImpulse;

	private bool jumpImpulse;

	private bool landImpulse;

	private bool noticeImpulse;

	[Space]
	public ParticleSystem particleBits;

	public ParticleSystem particleImpact;

	public ParticleSystem particleDirectionalBits;

	public ParticleSystem particleEyes;

	public ParticleSystem particleDespawnSpark;

	public ParticleSystem particleYell;

	public ParticleSystem particleYellSmall;

	public ParticleSystem particleStompL;

	public ParticleSystem particleStompR;

	private float soundStunPauseTimer;

	private float soundGroanPauseTimer;

	[Space]
	public Sound footstepSound;

	public Sound footstepSmallSound;

	[Space]
	public Sound moveShortSound;

	public Sound moveLongSound;

	public Sound GroanLoopSound;

	[Space]
	public Sound despawnSound;

	public Sound despawnSparkSound;

	[Space]
	public Sound jumpSound;

	public Sound landSound;

	public Sound noticeSound;

	[Space]
	public Sound yellStartSound;

	public Sound yellStartSoundGlobal;

	public Sound yellEndSound;

	public Sound yellEndSoundGlobal;

	public Sound YellLoopSound;

	public Sound YellLoopSoundGlobal;

	private bool yell;

	[Space]
	public Sound stunSound;

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
		if (enemy.Rigidbody.velocity.magnitude > 0.1f)
		{
			animator.SetBool("move", value: true);
		}
		else
		{
			animator.SetBool("move", value: false);
		}
		if (controller.currentState == EnemyBowtie.State.Leave)
		{
			animator.SetBool("leaving", value: true);
		}
		else
		{
			animator.SetBool("leaving", value: false);
		}
		if (controller.currentState == EnemyBowtie.State.Idle || controller.currentState == EnemyBowtie.State.Roam || controller.currentState == EnemyBowtie.State.Investigate)
		{
			if (soundGroanPauseTimer > 0f)
			{
				StopGroaning();
			}
			else
			{
				GroanLoopSound.PlayLoop(playing: true, 5f, 5f);
			}
		}
		else
		{
			StopGroaning();
		}
		if (soundGroanPauseTimer > 0f)
		{
			soundGroanPauseTimer -= Time.deltaTime;
		}
		if (controller.currentState == EnemyBowtie.State.Yell)
		{
			animator.SetBool("yell", value: true);
			yell = true;
		}
		else
		{
			animator.SetBool("yell", value: false);
			yell = false;
			particleYell.Stop();
			particleYellSmall.Stop();
		}
		YellLoopSound.PlayLoop(yell, 5f, 5f);
		YellLoopSoundGlobal.PlayLoop(yell, 5f, 5f);
		if (controller.currentState == EnemyBowtie.State.Despawn)
		{
			animator.SetBool("despawn", value: true);
			particleYell.Stop();
			particleYellSmall.Stop();
		}
		else
		{
			animator.SetBool("despawn", value: false);
		}
		bool flag = false;
		if (controller.currentState == EnemyBowtie.State.Stun)
		{
			flag = true;
			stunSound.PlayLoop(playing: true, 2f, 5f);
			landImpulse = false;
			if (stunImpulse)
			{
				StopGroaning();
				stunSound.Play(enemy.CenterTransform.position);
				animator.SetTrigger("Stun Impulse");
				stunImpulse = false;
			}
			animator.SetBool("stunned", value: true);
			if (soundStunPauseTimer > 0f)
			{
				StopStunSound();
			}
			else
			{
				stunSound.PlayLoop(playing: true, 2f, 5f);
			}
		}
		else
		{
			StopStunSound();
			animator.SetBool("stunned", value: false);
			stunImpulse = true;
		}
		if (soundStunPauseTimer > 0f)
		{
			soundStunPauseTimer -= Time.deltaTime;
		}
		if (!flag && enemy.Jump.jumping)
		{
			if (jumpImpulse)
			{
				animator.SetTrigger("Jump Impulse");
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
				animator.SetTrigger("Land Impulse");
				landImpulse = false;
			}
			animator.SetBool("falling", value: false);
			jumpImpulse = true;
		}
	}

	private void OnDisable()
	{
		particleYell.Stop();
		particleYellSmall.Stop();
	}

	public void OnSpawn()
	{
		animator.SetBool("stunned", value: false);
		animator.Play("Spawn", 0, 0f);
	}

	public void NoticeSet(int _playerID)
	{
		animator.SetTrigger("Notice");
	}

	public void DespawnStart()
	{
		despawnSound.Play(base.transform.position);
	}

	public void Despawn()
	{
		particleDespawnSpark.Play();
		despawnSparkSound.Play(base.transform.position);
		enemy.EnemyParent.Despawn();
	}

	public void Footstep()
	{
		footstepSound.Play(base.transform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Heavy, footstep: true, footstepParticles: false, material, Materials.HostType.Enemy);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.3f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.1f);
	}

	public void FootstepSmall()
	{
		footstepSmallSound.Play(base.transform.position);
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Heavy, footstep: true, footstepParticles: false, material, Materials.HostType.Enemy);
		GameDirector.instance.CameraShake.ShakeDistance(1.5f, 3f, 10f, base.transform.position, 0.3f);
		GameDirector.instance.CameraImpact.ShakeDistance(1.5f, 3f, 10f, base.transform.position, 0.1f);
	}

	public void StompLeft()
	{
		particleStompL.Play();
	}

	public void StompRight()
	{
		particleStompR.Play();
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

	public void Notice()
	{
		noticeSound.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void YellStart()
	{
		yellStartSound.Play(base.transform.position);
		yellStartSoundGlobal.Play(base.transform.position);
		particleYell.Play();
		particleYellSmall.Play();
	}

	public void yellShake()
	{
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 12f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 12f, base.transform.position, 0.1f);
	}

	public void EnemyInvestigate()
	{
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			EnemyDirector.instance.SetInvestigate(base.transform.position, 15f);
		}
	}

	public void YellStop()
	{
		yellEndSound.Play(base.transform.position);
		yellEndSoundGlobal.Play(base.transform.position);
	}

	public void StopStunSound()
	{
		stunSound.PlayLoop(playing: false, 2f, 5f);
	}

	public void StopGroaning()
	{
		GroanLoopSound.PlayLoop(playing: false, 5f, 5f);
	}

	public void StunPause()
	{
		soundStunPauseTimer = 0.5f;
	}

	public void GroanPause()
	{
		soundGroanPauseTimer = 1f;
	}
}
