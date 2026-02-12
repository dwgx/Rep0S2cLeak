using System.Collections.Generic;
using UnityEngine;

public class BirthdayBoyAnim : MonoBehaviour
{
	public Enemy enemy;

	public EnemyBirthdayBoy enemyBirthdayBoy;

	[HideInInspector]
	public Animator animator;

	public Transform leftEyePivot;

	public Transform rightEyePivot;

	public PlayerAvatar lookTarget;

	public Rigidbody rb;

	public Transform headPivot;

	public SpringQuaternion topHairRightSpring = new SpringQuaternion();

	public Transform topHairRightTransform;

	public Transform topHairRightTarget;

	[Space]
	public SpringQuaternion topHairLeftSpring = new SpringQuaternion();

	public Transform topHairLeftTransform;

	public Transform topHairLeftTarget;

	[Space]
	public SpringQuaternion sideHairLeftSpring = new SpringQuaternion();

	public Transform sideHairLeftTransform;

	public Transform sideHairLeftTarget;

	[Space]
	public SpringQuaternion sideHairRightSpring = new SpringQuaternion();

	public Transform sideHairRightTransform;

	public Transform sideHairRightTarget;

	[Space]
	public SpringQuaternion middleHairSpring = new SpringQuaternion();

	public Transform middleHairTransform;

	public Transform middleHairTarget;

	[Space]
	public SpringQuaternion BackHairSpring = new SpringQuaternion();

	public Transform BackHairTransform;

	public Transform BackHairTarget;

	public ParticleSystem spawnSmokeParticle;

	public ParticleSystem runningSpitParticle;

	public ParticleSystem balloonPopParticle;

	public GameObject deathParticlePrefab;

	public ParticleSystem droolParticle;

	public ParticleSystem runningSmokeParticle;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	private bool noticed;

	private bool floating;

	public bool breaker;

	private Quaternion prevHeadLookRotation;

	private Quaternion prevRigidBodyRotation;

	private float moveSoundRigidBodyTimer;

	private float moveSoundHeadTimer;

	public Sound footStepSound;

	public Sound footStepGlobalSound;

	public Sound footStepHardSound;

	public Sound balloonSqueakSound;

	public Sound moveLightSound;

	public Sound moveHeavySound;

	public Sound balloonPopSound;

	public Sound balloonBlowSound;

	public Sound balloonTieSound;

	[Space]
	public Sound deathSound;

	public Sound hurtSound;

	public Sound idleLoop;

	public Sound runLoop;

	public Sound aggroLoop;

	public Sound idleBreaker;

	public List<AudioClip> idleBreakers;

	public Sound attackSwoosh;

	public Sound jumpSound;

	public Sound landSound;

	public Sound inhaleSound;

	public Sound noticeSound;

	public Sound crackSound;

	public Sound balloonPopGlobal;

	private static readonly int animDespawn = Animator.StringToHash("despawn");

	private static readonly int animDespawnTrigger = Animator.StringToHash("despawn_trigger");

	private static readonly int animFloat = Animator.StringToHash("float");

	private static readonly int animFloatTrigger = Animator.StringToHash("float_trigger");

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
	}

	private void LateUpdate()
	{
		if (enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.CreepyStare || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.Attack || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.AttackOver || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.AttackUnder)
		{
			PlayerAvatar playerAvatar = ((!enemyBirthdayBoy.playerTarget) ? lookTarget : enemyBirthdayBoy.playerTarget);
			if ((bool)playerAvatar)
			{
				Vector3 vector = ((!SemiFunc.IsMultiplayer() || playerAvatar.isLocal) ? playerAvatar.localCamera.transform.position : playerAvatar.playerAvatarVisuals.headLookAtTransform.position);
				vector.y -= 0.3f;
				Quaternion rotation = Quaternion.LookRotation(vector - leftEyePivot.position, Vector3.up);
				leftEyePivot.rotation = rotation;
				Quaternion rotation2 = Quaternion.LookRotation(vector - rightEyePivot.position, Vector3.up);
				rightEyePivot.rotation = rotation2;
			}
		}
		else
		{
			leftEyePivot.localRotation = Quaternion.Slerp(leftEyePivot.localRotation, Quaternion.identity, Time.deltaTime * 5f);
			rightEyePivot.localRotation = Quaternion.Slerp(rightEyePivot.localRotation, Quaternion.identity, Time.deltaTime * 5f);
		}
	}

	private void Update()
	{
		TurnSoundLogic();
		AnimateSprings();
		bool flag = enemyBirthdayBoy.AttackState() || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.GoToPlayerAngry || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.GoToBalloonAngry;
		bool flag2 = (enemy.Rigidbody.velocity.magnitude > 0.1f || enemy.Rigidbody.physGrabObject.rbAngularVelocity.magnitude > 0.5f || enemy.IsStunned()) && enemyBirthdayBoy.currentState != EnemyBirthdayBoy.State.Idle && enemyBirthdayBoy.currentState != EnemyBirthdayBoy.State.LookAround && enemyBirthdayBoy.currentState != EnemyBirthdayBoy.State.CreepyStare && enemyBirthdayBoy.currentState != EnemyBirthdayBoy.State.PlaceBalloon && !flag;
		bool playing = enemyBirthdayBoy.currentState != EnemyBirthdayBoy.State.PlaceBalloon && enemyBirthdayBoy.currentState != EnemyBirthdayBoy.State.PlayerNotice && enemyBirthdayBoy.currentState != EnemyBirthdayBoy.State.Despawn && !flag && !flag2 && !breaker;
		runLoop.PlayLoop(flag2, 1f, 1f);
		idleLoop.PlayLoop(playing, 1f, 1f);
		aggroLoop.PlayLoop(flag, 1f, 1f);
		if (flag2 || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.GoToPlayerAngry || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.GoToBalloonAngry)
		{
			animator.SetBool("move", value: true);
		}
		else
		{
			animator.SetBool("move", value: false);
		}
		if (enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.Attack && enemy.Rigidbody.velocity.magnitude > 1f && !enemy.Jump.jumping && !enemy.Jump.jumpingDelay && !enemy.Jump.landDelay)
		{
			if (!runningSmokeParticle.isPlaying)
			{
				runningSmokeParticle.Play(withChildren: true);
			}
		}
		else if (runningSmokeParticle.isPlaying)
		{
			runningSmokeParticle.Stop(withChildren: true);
		}
		if (enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.Attack)
		{
			if (!runningSpitParticle.isPlaying)
			{
				runningSpitParticle.Play();
			}
		}
		else if (runningSpitParticle.isPlaying)
		{
			runningSpitParticle.Stop();
		}
		if (enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.Attack || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.AttackUnder || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.AttackOver)
		{
			animator.SetBool("attack", value: true);
		}
		else
		{
			animator.SetBool("attack", value: false);
		}
		if (enemy.IsStunned())
		{
			animator.SetBool("stun", value: true);
		}
		else
		{
			animator.SetBool("stun", value: false);
		}
		if (!enemy.IsStunned() && (enemy.Jump.jumping || enemy.Jump.jumpingDelay))
		{
			animator.SetBool("jump", value: true);
		}
		else
		{
			animator.SetBool("jump", value: false);
		}
		if (enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.AttackUnderStart || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.AttackUnder || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.AttackUnderEnd)
		{
			animator.SetBool("crawl", value: true);
		}
		else
		{
			animator.SetBool("crawl", value: false);
		}
		if (enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.PlayerNotice && !noticed)
		{
			animator.SetTrigger("player_notice");
			noticed = true;
		}
		else if (enemyBirthdayBoy.currentState != EnemyBirthdayBoy.State.PlayerNotice)
		{
			noticed = false;
		}
		if (enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.FlyBackUp || enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.FlyBackToNavMesh)
		{
			if (!animator.GetBool(animFloat))
			{
				animator.SetTrigger(animFloatTrigger);
			}
			animator.SetBool(animFloat, value: true);
		}
		else
		{
			animator.SetBool(animFloat, value: false);
		}
		if (enemyBirthdayBoy.currentState == EnemyBirthdayBoy.State.Despawn || enemy.CurrentState == EnemyState.Despawn)
		{
			if (!animator.GetBool(animDespawn))
			{
				animator.ResetTrigger(animFloatTrigger);
				animator.ResetTrigger("stun_trigger");
				droolParticle.Stop();
				spawnSmokeParticle.Play();
				animator.SetTrigger(animDespawnTrigger);
			}
			animator.SetBool(animDespawn, value: true);
		}
		else
		{
			animator.SetBool(animDespawn, value: false);
			if (!droolParticle.isPlaying)
			{
				droolParticle.Play();
			}
		}
	}

	private void AnimateSprings()
	{
		topHairRightTransform.rotation = SemiFunc.SpringQuaternionGet(topHairRightSpring, topHairRightTarget.rotation);
		topHairLeftTransform.rotation = SemiFunc.SpringQuaternionGet(topHairLeftSpring, topHairLeftTarget.rotation);
		sideHairLeftTransform.rotation = SemiFunc.SpringQuaternionGet(sideHairLeftSpring, sideHairLeftTarget.rotation);
		sideHairRightTransform.rotation = SemiFunc.SpringQuaternionGet(sideHairRightSpring, sideHairRightTarget.rotation);
		middleHairTransform.rotation = SemiFunc.SpringQuaternionGet(middleHairSpring, middleHairTarget.rotation);
		BackHairTransform.rotation = SemiFunc.SpringQuaternionGet(BackHairSpring, BackHairTarget.rotation);
	}

	public void BlowBalloonAnimationTrigger()
	{
		animator.SetTrigger("blow_balloon_trigger");
	}

	public void DoneWithBalloonBlow()
	{
		enemyBirthdayBoy.OnBalloonBlowComplete();
	}

	public void StoreBalloonLocation()
	{
		enemyBirthdayBoy.StoreBalloonLocation();
	}

	public void SetBlowingFalse()
	{
		enemyBirthdayBoy.blowing = false;
	}

	public void SetBlowingTrue()
	{
		enemyBirthdayBoy.blowing = true;
	}

	public void SetSpawn()
	{
		animator.Play("Birthday Boy - Spawn", 0, 0f);
		spawnSmokeParticle.Play();
	}

	public void SetDespawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void CrouchedDown()
	{
		enemyBirthdayBoy.CrouchedDown();
	}

	public void FootStepSound()
	{
		footStepSound.Play(base.transform.position);
		MaterialSound();
		if (enemyBirthdayBoy.AttackState())
		{
			footStepSound.Play(base.transform.position);
			footStepHardSound.Play(base.transform.position);
		}
	}

	public void MoveLightSound()
	{
		moveLightSound.Play(base.transform.position);
	}

	public void MoveHeavySound()
	{
		moveHeavySound.Play(base.transform.position);
	}

	public void BalloonSqueakSound()
	{
		balloonSqueakSound.Play(base.transform.position);
	}

	public void InterruptBalloonPop()
	{
		balloonPopParticle.Play(withChildren: true);
		balloonPopSound.Play(base.transform.position);
	}

	public void BalloonBloWSound()
	{
		balloonBlowSound.Play(base.transform.position);
	}

	public void BalloonTieSound()
	{
		balloonTieSound.Play(base.transform.position);
	}

	private void MaterialSound()
	{
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Light, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	private void TurnSoundLogic()
	{
		if (SemiFunc.FPSImpulse5())
		{
			if (Quaternion.Angle(prevHeadLookRotation, headPivot.rotation) > 30f && moveSoundHeadTimer <= 0f)
			{
				moveLightSound.Play(headPivot.position);
				prevHeadLookRotation = headPivot.rotation;
				moveSoundHeadTimer = 0.2f;
			}
			if (Quaternion.Angle(prevRigidBodyRotation, rb.rotation) > 20f && moveSoundRigidBodyTimer <= 0f)
			{
				moveSoundRigidBodyTimer = 0.5f;
				moveHeavySound.Play(base.transform.position);
				prevRigidBodyRotation = rb.rotation;
			}
			prevHeadLookRotation = headPivot.rotation;
			prevRigidBodyRotation = rb.rotation;
		}
		if (moveSoundRigidBodyTimer > 0f)
		{
			moveSoundRigidBodyTimer -= Time.deltaTime;
		}
		if (moveSoundHeadTimer > 0f)
		{
			moveSoundHeadTimer -= Time.deltaTime;
		}
	}

	public void PlayDeathParticles()
	{
		Object.Instantiate(deathParticlePrefab, base.transform.position, Quaternion.identity).GetComponent<ParticleSystem>().Play(withChildren: true);
	}

	public void AttackSwoosh()
	{
		attackSwoosh.Play(base.transform.position);
	}

	public void JumpSound()
	{
		jumpSound.Play(base.transform.position);
	}

	public void LandSound()
	{
		landSound.Play(base.transform.position);
	}

	public void InhaleSound()
	{
		inhaleSound.Play(base.transform.position);
	}

	public void NoticeSound()
	{
		noticeSound.Play(base.transform.position);
	}

	public void CrackSound()
	{
		crackSound.Play(base.transform.position);
	}
}
