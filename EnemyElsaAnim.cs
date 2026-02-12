using UnityEngine;

public class EnemyElsaAnim : MonoBehaviour
{
	private int animMoving = Animator.StringToHash("moving");

	private int animStunned = Animator.StringToHash("stunned");

	private int animFalling = Animator.StringToHash("falling");

	private int animFlying = Animator.StringToHash("flying");

	private int animLookingUnder = Animator.StringToHash("lookingUnder");

	private int animStanding = Animator.StringToHash("standing");

	private int animChasing = Animator.StringToHash("chasing");

	private int animIsBig = Animator.StringToHash("isBig");

	private int animDespawn = Animator.StringToHash("Despawn");

	private int animFly = Animator.StringToHash("Fly");

	private int animIdleBreaker01 = Animator.StringToHash("IdleBreaker01");

	private int animIdleBreaker02 = Animator.StringToHash("IdleBreaker02");

	private int animIdleBreaker03 = Animator.StringToHash("IdleBreaker03");

	private int animJump = Animator.StringToHash("Jump");

	private int animLand = Animator.StringToHash("Land");

	private int animLookUnder = Animator.StringToHash("LookUnder");

	private int animNotice = Animator.StringToHash("Notice");

	private int animPet = Animator.StringToHash("Pet");

	private int animStun = Animator.StringToHash("Stun");

	private int animTransformSmallToBig = Animator.StringToHash("TransformSmallToBig");

	private int animTransformBigToSmall = Animator.StringToHash("TransformBigToSmall");

	public Enemy enemy;

	internal Animator animator;

	public EnemyElsa controller;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	public ParticleSystem petHeartParticles;

	public float springSpeedMultiplier = 1f;

	public float springDampingMultiplier = 1f;

	public Sound barkSmallSound;

	public Sound barkIdleBreaker01_01SmallSound;

	public Sound barkIdleBreaker01_02SmallSound;

	public Sound barkIdleBreaker02_01SmallSound;

	public Sound barkIdleBreaker02_02SmallSound;

	public Sound deathSmallSound;

	public Sound footstepSmallSound;

	public Sound hurtSmallSound;

	public Sound jumpSmallSound;

	public Sound landSmallSound;

	public Sound petSmallSound;

	public Sound flySmallSoundLoop;

	public Sound stunSmallSoundLoop;

	public Sound pantingSmallNormalSoundLoop;

	public Sound armSwingBigSound;

	public Sound barkBigSoundLocal;

	public Sound barkBigSoundGlobal;

	public Sound deathBigSound;

	public Sound footstepBigSound;

	public Sound hurtBigSound;

	public Sound jumpBigSound;

	public Sound landBigSound;

	public Sound lookUnderAttackBigSound;

	public Sound lookUnderIntroBigSound;

	public Sound lookUnderOutroBigSound;

	public Sound pantingBigSoundLoop;

	public Sound transformStartSound;

	public AudioClip transformStingerSound;

	public Sound transformLimbBreakSound;

	public Sound transformHeadLookUpSound;

	public Sound transformLandSound;

	public float soundHurtPauseTimer;

	private bool jumpImpulse;

	private bool flyImpulse;

	private bool landImpulse;

	private bool stunImpulse;

	private bool noticeImpulse;

	private bool lookUnderImpulse;

	private bool despawnImpulse;

	private bool petImpulse;

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
			switch (controller.currentState)
			{
			case EnemyElsa.State.RoamSmall:
			case EnemyElsa.State.InvestigateSmall:
				animator.speed = 0.8f;
				break;
			case EnemyElsa.State.GoToPlayerSmall:
				animator.speed = 1.2f;
				break;
			default:
				animator.speed = 1f;
				break;
			}
		}
		if (controller.currentState != EnemyElsa.State.TransformSmallToBig && controller.currentState != EnemyElsa.State.TransformBigToSmall)
		{
			if (enemy.Rigidbody.velocity.magnitude > 0.25f || enemy.Rigidbody.rb.angularVelocity.magnitude > 5f)
			{
				animator.SetBool(animMoving, value: true);
			}
			else
			{
				animator.SetBool(animMoving, value: false);
			}
			if (!enemy.IsStunned())
			{
				if (!enemy.Grounded.grounded && (controller.currentState == EnemyElsa.State.FlyBackUpSmall || controller.currentState == EnemyElsa.State.FlyBackToNavMeshSmall || controller.currentState == EnemyElsa.State.FlyBackStopSmall))
				{
					if (flyImpulse)
					{
						animator.SetTrigger(animFly);
						animator.SetBool(animFalling, value: false);
						flyImpulse = false;
						landImpulse = true;
					}
					else if (controller.currentState == EnemyElsa.State.FlyBackStopSmall)
					{
						animator.SetBool(animFalling, value: true);
					}
				}
				else if (enemy.Jump.jumpingDelay)
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
					if (enemy.Jump.landDelay && landImpulse)
					{
						animator.SetTrigger(animLand);
						landImpulse = false;
					}
					animator.SetBool(animFalling, value: false);
					jumpImpulse = true;
					flyImpulse = true;
				}
			}
		}
		if (controller.currentState == EnemyElsa.State.TransformSmallToBig || controller.IsBig())
		{
			animator.SetBool(animMoving, value: false);
			animator.SetBool(animChasing, value: true);
		}
		else
		{
			animator.SetBool(animChasing, value: false);
		}
		animator.SetBool(animIsBig, controller.IsBig());
		if (controller.currentState == EnemyElsa.State.GoToPlayerOverSmall)
		{
			animator.SetBool(animStanding, value: true);
		}
		else
		{
			animator.SetBool(animStanding, value: false);
		}
		if (controller.currentState == EnemyElsa.State.NoticeSmall)
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
		if (controller.currentState == EnemyElsa.State.StunSmall)
		{
			landImpulse = false;
			if (stunImpulse)
			{
				animator.SetTrigger(animStun);
				stunImpulse = false;
			}
			animator.SetBool(animStunned, value: true);
			if (soundHurtPauseTimer > 0f)
			{
				stunSmallSoundLoop.PlayLoop(playing: false, 5f, 2f);
			}
			else
			{
				stunSmallSoundLoop.PlayLoop(playing: true, 5f, 5f);
			}
		}
		else
		{
			animator.SetBool(animStunned, value: false);
			stunSmallSoundLoop.PlayLoop(playing: false, 5f, 1f);
			if (!stunImpulse)
			{
				stunImpulse = true;
			}
		}
		if (!controller.IsBig())
		{
			if (soundHurtPauseTimer > 0f)
			{
				pantingSmallNormalSoundLoop.PlayLoop(playing: false, 5f, 1f);
			}
			else
			{
				pantingSmallNormalSoundLoop.PlayLoop(enemy.EnemyParent.playerClose, 5f, 1f);
			}
			pantingBigSoundLoop.PlayLoop(playing: false, 5f, 1f);
		}
		else
		{
			pantingSmallNormalSoundLoop.PlayLoop(playing: false, 5f, 1f);
			if (!controller.IsTransforming())
			{
				if (soundHurtPauseTimer > 0f)
				{
					pantingBigSoundLoop.PlayLoop(playing: false, 5f, 1f);
				}
				else
				{
					pantingBigSoundLoop.PlayLoop(playing: true, 5f, 1f);
				}
			}
		}
		if (controller.currentState == EnemyElsa.State.LookUnderBig)
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
		if (controller.currentState == EnemyElsa.State.DespawnSmall)
		{
			if (lookUnderImpulse)
			{
				animator.SetTrigger(animDespawn);
				despawnImpulse = false;
			}
		}
		else if (!despawnImpulse)
		{
			despawnImpulse = true;
		}
		if (controller.currentState == EnemyElsa.State.FlyBackToNavMeshSmall || controller.currentState == EnemyElsa.State.FlyBackUpSmall)
		{
			animator.SetBool(animFlying, value: true);
			flySmallSoundLoop.PlayLoop(playing: true, 5f, 2f);
		}
		else
		{
			animator.SetBool(animFlying, value: false);
			flySmallSoundLoop.PlayLoop(playing: false, 5f, 2f);
		}
		if (controller.currentState == EnemyElsa.State.PetSmall)
		{
			if (petImpulse)
			{
				animator.SetTrigger(animPet);
				petImpulse = false;
			}
		}
		else
		{
			petImpulse = true;
		}
		if (soundHurtPauseTimer > 0f)
		{
			soundHurtPauseTimer -= Time.deltaTime;
		}
	}

	public void VFXPetParticles()
	{
		petHeartParticles.Emit(50);
		SFXPetSmall();
	}

	public void ChanceToBark()
	{
		if (Random.Range(0, 100) < 35)
		{
			if (controller.IsBig())
			{
				SFXBarkBig();
			}
			else
			{
				SFXBarkSmall();
			}
		}
	}

	public void SFXPetSmall()
	{
		petSmallSound.Play(base.transform.position);
	}

	public void SFXBarkSmall()
	{
		barkSmallSound.Play(base.transform.position);
	}

	public void SFXBarkIdleBreaker01_01Small()
	{
		barkIdleBreaker01_01SmallSound.Play(base.transform.position);
	}

	public void SFXBarkIdleBreaker01_02Small()
	{
		barkIdleBreaker01_02SmallSound.Play(base.transform.position);
	}

	public void SFXBarkIdleBreaker02_01Small()
	{
		barkIdleBreaker02_01SmallSound.Play(base.transform.position);
	}

	public void SFXBarkIdleBreaker02_02Small()
	{
		barkIdleBreaker02_02SmallSound.Play(base.transform.position);
	}

	public void SFXDeathSmall()
	{
		deathSmallSound.Play(base.transform.position);
	}

	public void SFXFootstepSmall()
	{
		footstepSmallSound.Play(base.transform.position);
		EventMaterialImpact(Materials.SoundType.Light, _footstep: true);
	}

	public void SFXHurtSmall()
	{
		hurtSmallSound.Play(base.transform.position);
	}

	public void SFXJumpSmall()
	{
		jumpSmallSound.Play(base.transform.position);
		EventMaterialImpact(Materials.SoundType.Medium, _footstep: true);
	}

	public void SFXLandSmall()
	{
		landSmallSound.Play(base.transform.position);
		EventMaterialImpact(Materials.SoundType.Medium, _footstep: true);
	}

	public void SFXArmSwingBig()
	{
		armSwingBigSound.Play(base.transform.position);
	}

	public void SFXBarkBig()
	{
		barkBigSoundLocal.Play(base.transform.position);
		barkBigSoundGlobal.Play(base.transform.position);
	}

	public void SFXDeathBig()
	{
		deathBigSound.Play(base.transform.position);
	}

	public void SFXFootstepBig()
	{
		footstepBigSound.Play(base.transform.position);
		EventMaterialImpact(Materials.SoundType.Heavy, _footstep: true);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 3f, 10f, base.transform.position, 0.15f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 10f, base.transform.position, 0.05f);
	}

	public void SFXHurtBig()
	{
		hurtBigSound.Play(base.transform.position);
	}

	public void SFXJumpBig()
	{
		jumpBigSound.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(0.75f, 3f, 10f, base.transform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(1.5f, 3f, 10f, base.transform.position, 0.05f);
		EventMaterialImpact(Materials.SoundType.Heavy);
	}

	public void SFXLandBig()
	{
		landBigSound.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(1.5f, 3f, 10f, base.transform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.05f);
		EventMaterialImpact(Materials.SoundType.Heavy);
	}

	public void SFXLookUnderAttackBig()
	{
		lookUnderAttackBigSound.Play(base.transform.position);
	}

	public void SFXLookUnderIntroBig()
	{
		lookUnderIntroBigSound.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 3f, 10f, base.transform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 10f, base.transform.position, 0.05f);
		EventMaterialImpact(Materials.SoundType.Heavy);
	}

	public void SFXLookUnderOutroBig()
	{
		lookUnderOutroBigSound.Play(base.transform.position);
	}

	public void SFXTransformStart()
	{
		transformStartSound.Play(base.transform.position);
	}

	public void SFXTransformStinger()
	{
		if (Vector3.Distance(base.transform.position, Camera.main.transform.position) <= 20f)
		{
			AudioScare.instance.PlayCustom(transformStingerSound);
		}
	}

	public void SFXTransformLimbBreak()
	{
		transformLimbBreakSound.Play(base.transform.position);
	}

	public void SFXTransformHeadLookUp()
	{
		transformHeadLookUpSound.Play(base.transform.position);
	}

	public void SFXTransformLand()
	{
		transformLandSound.Play(base.transform.position);
	}

	public void TransformCameraShake(float _strength)
	{
		GameDirector.instance.CameraShake.ShakeDistance(_strength, 3f, 10f, base.transform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(1.5f, 3f, 10f, base.transform.position, 0.05f);
	}

	public void EventMaterialImpact(Materials.SoundType _type, bool _footstep = false)
	{
		Materials.Instance.Impulse(enemy.CenterTransform.position, Vector3.down, _type, _footstep, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void OnSpawn()
	{
		animator.Play("Spawn", 0, 0f);
	}

	public void Despawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void TransformSmallToBig()
	{
		animator.SetTrigger(animTransformSmallToBig);
	}

	public void TransformBigToSmall()
	{
		animator.SetTrigger(animTransformBigToSmall);
	}

	public void IdleBreakerSet(int _index)
	{
		switch (_index)
		{
		case 0:
			animator.SetTrigger(animIdleBreaker01);
			break;
		case 1:
			animator.SetTrigger(animIdleBreaker02);
			break;
		case 2:
			animator.SetTrigger(animIdleBreaker03);
			break;
		}
	}
}
