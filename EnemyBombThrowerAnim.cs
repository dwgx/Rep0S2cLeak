using UnityEngine;

public class EnemyBombThrowerAnim : MonoBehaviour
{
	public Enemy enemy;

	public EnemyBombThrower controller;

	public bool headGrownVisually;

	public float headAdditiveIdleAmount;

	public bool torsoHeadsScreaming;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	internal Animator animator;

	private int animSpawn = Animator.StringToHash("spawn");

	private int animMoving = Animator.StringToHash("moving");

	private int animMovingFast = Animator.StringToHash("movingFast");

	private int animBackingAway = Animator.StringToHash("backingAway");

	private int animJump = Animator.StringToHash("jump");

	private int animFalling = Animator.StringToHash("falling");

	private int animLand = Animator.StringToHash("land");

	private int animNotice = Animator.StringToHash("notice");

	private int animAttack = Animator.StringToHash("attack");

	private int animMelee = Animator.StringToHash("melee");

	private int animStun = Animator.StringToHash("stun");

	private int animStunned = Animator.StringToHash("stunned");

	private int animDespawning = Animator.StringToHash("despawning");

	private int animBodyHeadGrown = Animator.StringToHash("bodyHeadGrown");

	private int animTorsoHeadsScreaming = Animator.StringToHash("torsoHeadsScreaming");

	private int animTorsoHeadsScream = Animator.StringToHash("torsoHeadsScream");

	private int animTorsoHeadsMelee = Animator.StringToHash("torsoHeadsMelee");

	private int animTorsoHeadsHurt = Animator.StringToHash("torsoHeadsHurt");

	private int animTorsoHeadsBreaker00 = Animator.StringToHash("torsoHeadsBreaker00");

	private int animTorsoHeadsBreaker01 = Animator.StringToHash("torsoHeadsBreaker01");

	private int animTorsoHeadsBreaker02 = Animator.StringToHash("torsoHeadsBreaker02");

	private int animTorsoHeadsBreaker03 = Animator.StringToHash("torsoHeadsBreaker03");

	private int animTorsoHeadsBreaker04 = Animator.StringToHash("torsoHeadsBreaker04");

	private int animTorsoHeadsBreaker05 = Animator.StringToHash("torsoHeadsBreaker05");

	private int animTorsoHeadsBreaker06 = Animator.StringToHash("torsoHeadsBreaker06");

	private int animTorsoHeadsBreaker07 = Animator.StringToHash("torsoHeadsBreaker07");

	private int animTorsoHeadsBreaker08 = Animator.StringToHash("torsoHeadsBreaker08");

	private int animTorsoHeadsBreaker09 = Animator.StringToHash("torsoHeadsBreaker09");

	private int animTorsoHeadsBreaker10 = Animator.StringToHash("torsoHeadsBreaker10");

	private int animTorsoHeadsBreaker11 = Animator.StringToHash("torsoHeadsBreaker11");

	private bool spawnImpulse = true;

	private bool jumpImpulse = true;

	private bool landImpulse = true;

	private bool noticeImpulse = true;

	private bool attackImpulse = true;

	private bool meleeImpulse = true;

	private bool stunImpulse = true;

	private bool torsoHeadsScreamingImpulse = true;

	private float moveTimer;

	private float backAwayTimer;

	public SpringQuaternionSystem springHead;

	public SpringQuaternionSystem springArmRight;

	public SpringQuaternionSystem springArmLeft;

	[Space]
	public SpringQuaternionSystem[] springTorsoHeads;

	public Transform headLookTransform;

	public Transform headLookPositionTransform;

	[Space]
	public GenericEyeLookAt eyeLookAt;

	[Space]
	public Transform footSkinTransform;

	public Transform footBootTransform;

	public ParticleSystem[] teleportParticles;

	public ParticleSystem[] headDetachParticles;

	public ParticleSystem[] deathParticles;

	public Sound soundIdleLoop;

	[Space]
	public Sound soundTeleportIn;

	public Sound soundTeleportOut;

	[Space]
	public Sound soundLand;

	public Sound soundJump;

	[Space]
	public Sound soundFootstepSkinLight;

	public Sound soundFootstepSkinHeavy;

	[Space]
	public Sound soundFootstepBootLight;

	public Sound soundFootstepBootHeavy;

	[Space]
	public Sound soundMoveShort;

	public Sound soundMoveLong;

	[Space]
	public Sound soundHurt;

	public Sound soundDeath;

	[Space]
	public Sound soundHeadGrow01;

	public Sound soundHeadGrow02;

	public Sound soundHeadGrow03;

	[Space]
	public Sound soundHeadDetachTell;

	[Space]
	public Sound soundTorsoHeadsOpenSolo;

	public Sound soundTorsoHeadsOpenMultiple;

	[Space]
	public Sound soundTorsoHeadsCloseSolo;

	public Sound soundTorsoHeadsCloseMultiple;

	[Space]
	public Sound soundTorsoHeadsMelee;

	public Sound soundTorsoHeadsMeleeGlobal;

	[Space]
	public Sound soundTorsoHeadsScream;

	public Sound soundTorsoHeadsScreamGlobal;

	[Space]
	public Sound soundTorsoHeadsScreamingIntro;

	public Sound soundTorsoHeadsScreamingLoop;

	public Sound soundTorsoHeadsScreamingOutro;

	[Space]
	public Sound soundTorsoHeadsBreaker00;

	public Sound soundTorsoHeadsBreaker01;

	public Sound soundTorsoHeadsBreaker02;

	public Sound soundTorsoHeadsBreaker03;

	public Sound soundTorsoHeadsBreaker04;

	public Sound soundTorsoHeadsBreaker05;

	public Sound soundTorsoHeadsBreaker06;

	public Sound soundTorsoHeadsBreaker07;

	public Sound soundTorsoHeadsBreaker08;

	public Sound soundTorsoHeadsBreaker09;

	public Sound soundTorsoHeadsBreaker10;

	public Sound soundTorsoHeadsBreaker11;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
		SpringQuaternionSystem[] array = springTorsoHeads;
		foreach (SpringQuaternionSystem obj in array)
		{
			obj.spring.speed = 8f + Random.Range(-2f, 2f);
			obj.spring.damping = 0.2f + Random.Range(-0.05f, 0.05f);
		}
	}

	private void Update()
	{
		AnimatorLogic();
		SpringLogic();
		HeadLookAtLogic();
		IdleLoopLogic();
	}

	private void AnimatorLogic()
	{
		if (enemy.Rigidbody.frozen)
		{
			animator.speed = 0f;
		}
		else
		{
			animator.speed = 1f;
		}
		animator.SetLayerWeight(3, headAdditiveIdleAmount);
		if (torsoHeadsScreaming)
		{
			if (torsoHeadsScreamingImpulse)
			{
				soundTorsoHeadsScreamingIntro.Play(enemy.CenterTransform.position);
				torsoHeadsScreamingImpulse = false;
			}
		}
		else if (!torsoHeadsScreamingImpulse)
		{
			soundTorsoHeadsScreamingOutro.Play(enemy.CenterTransform.position);
			torsoHeadsScreamingImpulse = true;
		}
		soundTorsoHeadsScreamingLoop.PlayLoop(torsoHeadsScreaming, 5f, 10f);
		animator.SetBool(animTorsoHeadsScreaming, torsoHeadsScreaming);
		if (controller.currentState == EnemyBombThrower.State.Spawn)
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
		if ((controller.currentState == EnemyBombThrower.State.Roam || controller.currentState == EnemyBombThrower.State.Investigate || controller.currentState == EnemyBombThrower.State.GotoPlayer || controller.currentState == EnemyBombThrower.State.Leave) && (enemy.Rigidbody.velocity.magnitude >= 0.5f || enemy.Rigidbody.physGrabObject.rbAngularVelocity.magnitude > 5f))
		{
			moveTimer = 0.2f;
		}
		if (moveTimer > 0f)
		{
			moveTimer -= Time.deltaTime;
			if (controller.currentState == EnemyBombThrower.State.BackAwayHead)
			{
				animator.SetBool(animMoving, value: false);
				animator.SetBool(animMovingFast, value: false);
			}
			else if (controller.currentState == EnemyBombThrower.State.GotoPlayer || controller.currentState == EnemyBombThrower.State.Leave)
			{
				animator.SetBool(animMoving, value: false);
				animator.SetBool(animMovingFast, value: true);
			}
			else
			{
				animator.SetBool(animMoving, value: true);
				animator.SetBool(animMovingFast, value: false);
			}
		}
		else
		{
			animator.SetBool(animMoving, value: false);
			animator.SetBool(animMovingFast, value: false);
		}
		if ((controller.currentState == EnemyBombThrower.State.BackAwayPlayer || controller.currentState == EnemyBombThrower.State.BackAwayHead) && (enemy.Rigidbody.velocity.magnitude >= 0.1f || enemy.Rigidbody.physGrabObject.rbAngularVelocity.magnitude > 1f))
		{
			backAwayTimer = 0.2f;
		}
		if (backAwayTimer > 0f)
		{
			backAwayTimer -= Time.deltaTime;
			animator.SetBool(animBackingAway, value: true);
		}
		else
		{
			animator.SetBool(animBackingAway, value: false);
		}
		if (enemy.Jump.jumping || enemy.Jump.jumpingDelay)
		{
			if (jumpImpulse)
			{
				if (!enemy.IsStunned())
				{
					animator.SetTrigger(animJump);
					animator.SetBool(animFalling, value: false);
				}
				jumpImpulse = false;
				landImpulse = true;
			}
			else if (controller.enemy.Rigidbody.physGrabObject.rbVelocity.y < -0.5f)
			{
				animator.SetBool(animFalling, value: true);
			}
			if (animator.GetBool(animFalling))
			{
				controller.rotationStopTimer = 0.25f;
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
		if (controller.currentState == EnemyBombThrower.State.Notice)
		{
			if (noticeImpulse)
			{
				if (!enemy.IsStunned())
				{
					animator.SetTrigger(animNotice);
				}
				noticeImpulse = false;
			}
		}
		else
		{
			noticeImpulse = true;
		}
		if (controller.currentState == EnemyBombThrower.State.Attack)
		{
			if (attackImpulse)
			{
				if (!enemy.IsStunned())
				{
					animator.SetTrigger(animAttack);
				}
				attackImpulse = false;
			}
		}
		else
		{
			attackImpulse = true;
		}
		if (controller.currentState == EnemyBombThrower.State.Melee)
		{
			if (meleeImpulse)
			{
				if (!enemy.IsStunned())
				{
					animator.SetTrigger(animMelee);
				}
				meleeImpulse = false;
			}
		}
		else
		{
			meleeImpulse = true;
		}
		if (controller.headGrown && controller.head.currentState == EnemyBombThrowerHead.State.Disabled)
		{
			animator.SetBool(animBodyHeadGrown, value: true);
		}
		else
		{
			animator.SetBool(animBodyHeadGrown, value: false);
		}
		if (enemy.IsStunned())
		{
			if (stunImpulse)
			{
				animator.SetTrigger(animStun);
				stunImpulse = false;
			}
			animator.SetBool(animStunned, value: true);
		}
		else
		{
			animator.SetBool(animStunned, value: false);
			stunImpulse = true;
		}
		if (controller.currentState == EnemyBombThrower.State.Despawn)
		{
			animator.SetBool(animDespawning, value: true);
		}
		else
		{
			animator.SetBool(animDespawning, value: false);
		}
	}

	private void SpringLogic()
	{
		springHead.UpdateWorldSpace();
		springArmRight.UpdateWorldSpace();
		springArmLeft.UpdateWorldSpace();
		SpringQuaternionSystem[] array = springTorsoHeads;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateWorldSpace();
		}
	}

	private void HeadLookAtLogic()
	{
		Vector3 zero = Vector3.zero;
		if (controller.headGrown && (controller.currentState == EnemyBombThrower.State.Notice || controller.currentState == EnemyBombThrower.State.GotoPlayer || controller.currentState == EnemyBombThrower.State.BackAwayPlayer || controller.currentState == EnemyBombThrower.State.Attack || controller.currentState == EnemyBombThrower.State.Melee || controller.currentState == EnemyBombThrower.State.Stun) && (bool)controller.playerTarget && !controller.playerTarget.isDisabled && !controller.VisionBlocked())
		{
			eyeLookAt.SetTargetPlayer(controller.playerTarget);
		}
		if (zero != Vector3.zero)
		{
			Quaternion localRotation = headLookTransform.localRotation;
			Vector3 localPosition = headLookTransform.localPosition;
			headLookTransform.position = headLookPositionTransform.position;
			headLookTransform.LookAt(zero);
			headLookTransform.forward = SemiFunc.ClampDirection(headLookTransform.forward, base.transform.forward, 75f);
			Quaternion localRotation2 = headLookTransform.localRotation;
			headLookTransform.localPosition = localPosition;
			headLookTransform.localRotation = Quaternion.Slerp(localRotation, localRotation2, Time.deltaTime * 5f);
		}
		else
		{
			headLookTransform.localRotation = Quaternion.Slerp(headLookTransform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
		}
	}

	private void IdleLoopLogic()
	{
		soundIdleLoop.PlayLoop(enemy.EnemyParent.playerClose, 0.5f, 0.5f);
	}

	public void TorsoHeadBreakerTrigger(int _index)
	{
		switch (_index)
		{
		case 0:
			animator.SetTrigger(animTorsoHeadsBreaker00);
			break;
		case 1:
			animator.SetTrigger(animTorsoHeadsBreaker01);
			break;
		case 2:
			animator.SetTrigger(animTorsoHeadsBreaker02);
			break;
		case 3:
			animator.SetTrigger(animTorsoHeadsBreaker03);
			break;
		case 4:
			animator.SetTrigger(animTorsoHeadsBreaker04);
			break;
		case 5:
			animator.SetTrigger(animTorsoHeadsBreaker05);
			break;
		case 6:
			animator.SetTrigger(animTorsoHeadsBreaker06);
			break;
		case 7:
			animator.SetTrigger(animTorsoHeadsBreaker07);
			break;
		case 8:
			animator.SetTrigger(animTorsoHeadsBreaker08);
			break;
		case 9:
			animator.SetTrigger(animTorsoHeadsBreaker09);
			break;
		case 10:
			animator.SetTrigger(animTorsoHeadsBreaker10);
			break;
		case 11:
			animator.SetTrigger(animTorsoHeadsBreaker11);
			break;
		}
	}

	public void EventTeleportIn()
	{
		soundTeleportIn.Play(controller.enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
		ParticleSystem[] array = teleportParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: true);
		}
	}

	public void EventFootstepSkinLight()
	{
		soundFootstepSkinLight.Play(footSkinTransform.position);
		Materials.Instance.Impulse(footSkinTransform.position, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void EventFootstepSkinHeavy()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 5f, enemy.CenterTransform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 5f, enemy.CenterTransform.position, 0.05f);
		soundFootstepSkinHeavy.Play(footSkinTransform.position);
		Materials.Instance.Impulse(footSkinTransform.position, Vector3.down, Materials.SoundType.Heavy, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void EventFootstepBootLight()
	{
		soundFootstepBootLight.Play(footBootTransform.position);
		Materials.Instance.Impulse(footBootTransform.position, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void EventFootstepBootHeavy()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 5f, enemy.CenterTransform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 5f, enemy.CenterTransform.position, 0.05f);
		soundFootstepBootHeavy.Play(footBootTransform.position);
		Materials.Instance.Impulse(footBootTransform.position, Vector3.down, Materials.SoundType.Heavy, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void EventMoveShort()
	{
		soundMoveShort.Play(enemy.CenterTransform.position);
	}

	public void EventMoveLong()
	{
		soundMoveLong.Play(enemy.CenterTransform.position);
	}

	public void EventLand()
	{
		soundLand.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 8f, 12f, enemy.CenterTransform.position, 1f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 8f, 12f, enemy.CenterTransform.position, 0.1f);
	}

	public void EventJump()
	{
		soundJump.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 8f, 12f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 8f, 12f, enemy.CenterTransform.position, 0.1f);
	}

	public void EventHeadDetachTell()
	{
		soundHeadDetachTell.Play(enemy.CenterTransform.position);
	}

	public void EventAttack()
	{
		GameDirector.instance.CameraShake.ShakeDistance(1f, 8f, 12f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 8f, 12f, enemy.CenterTransform.position, 0.1f);
		ParticleSystem[] array = headDetachParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: true);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			controller.HeadGrownSet(_state: false);
		}
	}

	public void EventMelee()
	{
		soundTorsoHeadsMelee.Play(enemy.CenterTransform.position);
		soundTorsoHeadsMeleeGlobal.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 8f, 12f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 8f, 12f, enemy.CenterTransform.position, 0.1f);
	}

	public void EventMeleeEnd()
	{
		soundTorsoHeadsCloseMultiple.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 5f, enemy.CenterTransform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 5f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventTorsoHeadGrow01()
	{
		soundHeadGrow01.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 5f, 10f, enemy.CenterTransform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 5f, 10f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventTorsoHeadGrow02()
	{
		soundHeadGrow02.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 5f, 10f, enemy.CenterTransform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 5f, 10f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventTorsoHeadGrow03()
	{
		soundHeadGrow03.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 5f, 10f, enemy.CenterTransform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 5f, 10f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventTorsoHeadsOpenSolo()
	{
		soundTorsoHeadsOpenSolo.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsOpenMultiple()
	{
		soundTorsoHeadsOpenMultiple.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsCloseSolo()
	{
		soundTorsoHeadsCloseSolo.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsCloseMultiple()
	{
		soundTorsoHeadsCloseMultiple.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsMeleeTrigger()
	{
		animator.SetTrigger(animTorsoHeadsMelee);
	}

	public void EventTorsoHeadsScreamTrigger()
	{
		animator.SetTrigger(animTorsoHeadsScream);
	}

	public void EventTorsoHeadsScream()
	{
		soundTorsoHeadsScream.Play(enemy.CenterTransform.position);
		soundTorsoHeadsScreamGlobal.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 8f, 12f, enemy.CenterTransform.position, 1f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 8f, 12f, enemy.CenterTransform.position, 0.25f);
	}

	public void EventTorsoHeadsBreakerShake()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 5f, 10f, enemy.CenterTransform.position, 0.05f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 5f, 10f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventTorsoHeadsBreaker00()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker00.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsBreaker01()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker01.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsBreaker02()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker02.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsBreaker03()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker03.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsBreaker04()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker04.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsBreaker05()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker05.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsBreaker06()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker06.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsBreaker07()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker07.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsBreaker08()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker08.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsBreaker09()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker09.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsBreaker10()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker10.Play(enemy.CenterTransform.position);
	}

	public void EventTorsoHeadsBreaker11()
	{
		EventTorsoHeadsBreakerShake();
		soundTorsoHeadsBreaker11.Play(enemy.CenterTransform.position);
	}

	public void EventTeleportOut()
	{
		soundTeleportOut.Play(controller.enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
		ParticleSystem[] array = teleportParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: true);
		}
	}

	public void EventHurt()
	{
		soundHurt.Play(enemy.CenterTransform.position);
		animator.SetTrigger(animTorsoHeadsHurt);
	}

	public void EventDeath()
	{
		soundDeath.Play(enemy.CenterTransform.position);
		ParticleSystem[] array = deathParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: true);
		}
		GameDirector.instance.CameraShake.ShakeDistance(3f, 5f, 10f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 5f, 10f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventDespawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}
}
