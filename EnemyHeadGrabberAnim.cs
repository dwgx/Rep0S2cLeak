using System;
using UnityEngine;

public class EnemyHeadGrabberAnim : MonoBehaviour
{
	public Enemy enemy;

	public EnemyHeadGrabber controller;

	public Transform footstepLeftTransform;

	public Transform footstepRightTransform;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	private Animator animator;

	private int animSpawn = Animator.StringToHash("spawn");

	private int animIdleBreaker01 = Animator.StringToHash("idleBreaker01");

	private int animIdleBreaker02 = Animator.StringToHash("idleBreaker02");

	private int animIdleBreaker03 = Animator.StringToHash("idleBreaker03");

	private int animMoving = Animator.StringToHash("moving");

	private int animMovingFast = Animator.StringToHash("movingFast");

	private int animMovingHead = Animator.StringToHash("movingHead");

	private int animStun = Animator.StringToHash("stun");

	private int animStunned = Animator.StringToHash("stunned");

	private int animJump = Animator.StringToHash("jump");

	private int animFalling = Animator.StringToHash("falling");

	private int animLand = Animator.StringToHash("land");

	private int animNotice = Animator.StringToHash("notice");

	private int animCantReach = Animator.StringToHash("cantReach");

	private int animGrabHead = Animator.StringToHash("grabHead");

	private int animReleaseHead = Animator.StringToHash("releaseHead");

	private int animAttack01 = Animator.StringToHash("attack01");

	private int animAttack02 = Animator.StringToHash("attack02");

	private int animAttack03 = Animator.StringToHash("attack03");

	private int animDespawning = Animator.StringToHash("despawning");

	private int animEyeOpen = Animator.StringToHash("eyeOpen");

	private bool spawnImpulse = true;

	private bool stunImpulse = true;

	private bool jumpImpulse = true;

	private bool landImpulse = true;

	private bool noticeImpulse = true;

	private bool headGrabImpulse = true;

	private bool headReleaseImpulse = true;

	private bool cantReachImpulse = true;

	private float moveTimer;

	internal float soundLoopPauseTimer;

	public SpringQuaternion springTongueMiddle;

	public Transform springTongueMiddleTransform;

	public Transform springTongueMiddleTarget;

	[Space]
	public SpringQuaternion springTongueTip;

	public Transform springTongueTipTransform;

	public Transform springTongueTipTarget;

	[Space]
	public SpringQuaternion springTorsoTalk;

	public Transform springTorsoTalkTransform;

	public Transform springTorsoTalkTarget;

	[Space]
	public SpringQuaternion springHeadTalk;

	public Transform springHeadTalkTransform;

	public Transform springHeadTalkTarget;

	public Transform lookAtParentTransform;

	public Transform lookAtParentAnimTransform;

	private Vector3 lookAtParentTransformStartPos;

	private float eyeIdleTimer;

	private Vector3 eyeIdlePosition;

	private Vector3 eyeDartPosition;

	private float eyeDartTimer;

	[Space]
	public Transform[] lookAtTransforms;

	[Space]
	public Transform[] eyeTargetTransforms;

	public Transform[] eyeTransforms;

	[Space]
	public SpringQuaternion[] eyeSprings;

	public ParticleSystem[] teleportParticles;

	public ParticleSystem[] deathParticles;

	private bool localAudio;

	public Sound soundTeleportIn;

	public Sound soundTeleportOut;

	[Space]
	public Sound soundJump;

	public Sound soundLand;

	[Space]
	public Sound soundHurt;

	public Sound soundDeath;

	[Space]
	public Sound soundAttackSwipe;

	public Sound soundDropkickTell;

	public Sound soundDropkickStart;

	public Sound soundDropkickEnd;

	public Sound soundDropkickHit;

	[Space]
	public Sound soundFootstepLight;

	public Sound soundFootstepHeavy;

	public Sound soundRun;

	[Space]
	public Sound soundLocalBreathingLoop;

	[Space]
	public Sound soundMoveShort;

	public Sound soundMoveLong;

	[Space]
	public Sound soundNotice;

	public Sound soundNoticeGlobal;

	[Space]
	public Sound soundCantReach;

	public Sound soundGrabHead;

	[Space]
	public Sound soundReleaseHeadStart;

	public Sound soundReleaseHeadEnd;

	[Space]
	public Sound soundNoiseShort;

	public Sound soundNoiseLong;

	[Space]
	public Sound soundStunIntro;

	public Sound soundStunLoop;

	public Sound soundStunOutro;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
		lookAtParentTransformStartPos = lookAtParentTransform.localPosition;
		soundTeleportIn.StoreDefault();
		soundTeleportOut.StoreDefault();
		soundJump.StoreDefault();
		soundLand.StoreDefault();
		soundHurt.StoreDefault();
		soundDeath.StoreDefault();
		soundAttackSwipe.StoreDefault();
		soundDropkickTell.StoreDefault();
		soundDropkickStart.StoreDefault();
		soundDropkickEnd.StoreDefault();
		soundDropkickHit.StoreDefault();
		soundFootstepLight.StoreDefault();
		soundFootstepHeavy.StoreDefault();
		soundRun.StoreDefault();
		soundLocalBreathingLoop.StoreDefault();
		soundMoveShort.StoreDefault();
		soundMoveLong.StoreDefault();
		soundNotice.StoreDefault();
		soundNoticeGlobal.StoreDefault();
		soundCantReach.StoreDefault();
		soundGrabHead.StoreDefault();
		soundReleaseHeadStart.StoreDefault();
		soundReleaseHeadEnd.StoreDefault();
		soundNoiseShort.StoreDefault();
		soundNoiseLong.StoreDefault();
		soundStunIntro.StoreDefault();
		soundStunLoop.StoreDefault();
		soundStunOutro.StoreDefault();
	}

	private void Update()
	{
		AnimatorLogic();
		EyeLogic();
		TalkLogic();
		TongueLogic();
		LocalAudioLogic();
		if (soundLoopPauseTimer > 0f)
		{
			soundLoopPauseTimer -= Time.deltaTime;
		}
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
		if (controller.currentState == EnemyHeadGrabber.State.Spawn)
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
		if ((controller.currentState == EnemyHeadGrabber.State.Roam || controller.currentState == EnemyHeadGrabber.State.RoamFast || controller.currentState == EnemyHeadGrabber.State.Investigate || controller.currentState == EnemyHeadGrabber.State.InvestigateFast || controller.currentState == EnemyHeadGrabber.State.GotoPlayer || controller.currentState == EnemyHeadGrabber.State.GotoPlayerOver || controller.currentState == EnemyHeadGrabber.State.GotoHead || controller.currentState == EnemyHeadGrabber.State.GotoHeadOver || controller.currentState == EnemyHeadGrabber.State.BackToNavmesh || controller.currentState == EnemyHeadGrabber.State.Leave) && (enemy.Rigidbody.velocity.magnitude > 0.25f || enemy.Rigidbody.physGrabObject.rbAngularVelocity.magnitude > 5f))
		{
			moveTimer = 0.25f;
		}
		if (moveTimer > 0f)
		{
			moveTimer -= Time.deltaTime;
			if (controller.currentState == EnemyHeadGrabber.State.RoamFast || controller.currentState == EnemyHeadGrabber.State.InvestigateFast || controller.currentState == EnemyHeadGrabber.State.GotoPlayer || controller.currentState == EnemyHeadGrabber.State.GotoHead || controller.currentState == EnemyHeadGrabber.State.BackToNavmesh || controller.currentState == EnemyHeadGrabber.State.Leave)
			{
				if (controller.headTargetActive)
				{
					animator.SetBool(animMovingHead, value: true);
				}
				else
				{
					animator.SetBool(animMovingFast, value: true);
				}
				animator.SetBool(animMoving, value: false);
			}
			else if (controller.headTargetActive && (controller.currentState == EnemyHeadGrabber.State.GotoHeadOver || controller.currentState == EnemyHeadGrabber.State.GotoPlayerOver))
			{
				animator.SetBool(animMovingHead, value: true);
				animator.SetBool(animMovingFast, value: false);
				animator.SetBool(animMoving, value: false);
			}
			else
			{
				animator.SetBool(animMoving, value: true);
				animator.SetBool(animMovingFast, value: false);
				animator.SetBool(animMovingHead, value: false);
			}
		}
		else
		{
			animator.SetBool(animMoving, value: false);
			animator.SetBool(animMovingFast, value: false);
			animator.SetBool(animMovingHead, value: false);
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
		if (controller.currentState == EnemyHeadGrabber.State.Notice)
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
		if (controller.currentState == EnemyHeadGrabber.State.GrabHead)
		{
			if (headGrabImpulse)
			{
				if (!enemy.IsStunned())
				{
					animator.SetTrigger(animGrabHead);
				}
				headGrabImpulse = false;
			}
		}
		else
		{
			headGrabImpulse = true;
		}
		if (controller.currentState == EnemyHeadGrabber.State.ReleaseHead)
		{
			if (headReleaseImpulse)
			{
				if (!enemy.IsStunned())
				{
					animator.SetTrigger(animReleaseHead);
				}
				headReleaseImpulse = false;
			}
		}
		else
		{
			headReleaseImpulse = true;
		}
		if (controller.currentState == EnemyHeadGrabber.State.CantReach)
		{
			if (cantReachImpulse)
			{
				if (!enemy.IsStunned())
				{
					animator.SetTrigger(animCantReach);
				}
				cantReachImpulse = false;
			}
		}
		else
		{
			cantReachImpulse = true;
		}
		if (controller.currentState == EnemyHeadGrabber.State.Stun)
		{
			if (stunImpulse)
			{
				animator.SetTrigger(animStun);
				stunImpulse = false;
			}
			animator.SetBool(animStunned, value: true);
			if (soundLoopPauseTimer > 0f)
			{
				soundStunLoop.PlayLoop(playing: false, 5f, 5f);
			}
			else
			{
				soundStunLoop.PlayLoop(playing: true, 5f, 5f);
			}
		}
		else
		{
			animator.SetBool(animStunned, value: false);
			soundStunLoop.PlayLoop(playing: false, 5f, 5f);
			stunImpulse = true;
		}
		if (controller.currentState != EnemyHeadGrabber.State.CantReach && (controller.headTargetActive || controller.currentState == EnemyHeadGrabber.State.Stun || controller.currentState == EnemyHeadGrabber.State.GrabHead))
		{
			animator.SetBool(animEyeOpen, value: false);
		}
		else
		{
			animator.SetBool(animEyeOpen, value: true);
		}
		if (controller.currentState == EnemyHeadGrabber.State.Despawn)
		{
			animator.SetBool(animDespawning, value: true);
		}
		else
		{
			animator.SetBool(animDespawning, value: false);
		}
	}

	private void EyeLogic()
	{
		if (eyeDartTimer <= 0f)
		{
			if (UnityEngine.Random.Range(0, 4) == 0)
			{
				eyeDartTimer = UnityEngine.Random.Range(0.1f, 0.75f);
				eyeDartPosition = new Vector3(UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.05f, 0.05f), 0f);
			}
		}
		else
		{
			eyeDartTimer -= Time.deltaTime;
		}
		if (eyeIdleTimer <= 0f)
		{
			eyeIdleTimer = UnityEngine.Random.Range(1f, 3f);
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				eyeIdlePosition = Vector3.zero;
			}
			else
			{
				eyeIdlePosition = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), 0f);
			}
		}
		else
		{
			eyeIdleTimer -= Time.deltaTime;
		}
		if ((bool)controller.headTarget && (controller.currentState == EnemyHeadGrabber.State.GotoHead || controller.currentState == EnemyHeadGrabber.State.GotoHeadOver || (controller.currentState == EnemyHeadGrabber.State.CantReach && controller.previousState == EnemyHeadGrabber.State.GotoHead)))
		{
			lookAtParentTransform.position = controller.headTarget.physGrabObject.centerPoint + eyeDartPosition;
		}
		else if ((bool)controller.playerTarget && (controller.currentState == EnemyHeadGrabber.State.Notice || controller.currentState == EnemyHeadGrabber.State.GotoPlayer || controller.currentState == EnemyHeadGrabber.State.GotoPlayerOver || controller.currentState == EnemyHeadGrabber.State.Attack || (controller.currentState == EnemyHeadGrabber.State.CantReach && controller.previousState == EnemyHeadGrabber.State.GotoPlayer)))
		{
			if (controller.playerTarget.isLocal)
			{
				lookAtParentTransform.position = controller.playerTarget.localCamera.transform.position + eyeDartPosition;
			}
			else
			{
				lookAtParentTransform.position = controller.playerTarget.PlayerVisionTarget.VisionTransform.position + eyeDartPosition;
			}
		}
		else
		{
			lookAtParentTransform.localPosition = lookAtParentTransformStartPos + eyeIdlePosition + eyeDartPosition;
			lookAtParentTransform.localRotation = Quaternion.identity;
		}
		Transform[] array = eyeTargetTransforms;
		foreach (Transform transform in array)
		{
			int num = Array.IndexOf(eyeTargetTransforms, transform);
			transform.LookAt(lookAtTransforms[num]);
			transform.forward = SemiFunc.ClampDirection(transform.forward, base.transform.forward, 35f);
			eyeTransforms[num].rotation = SemiFunc.SpringQuaternionGet(eyeSprings[num], transform.rotation);
		}
	}

	private void TalkLogic()
	{
		float value = 0f;
		if (controller.headTargetActive && controller.headTarget.spectated && controller.headTarget.playerAvatar.voiceChatFetched)
		{
			value = controller.headTarget.playerAvatar.voiceChat.clipLoudness * 10f;
		}
		value = Mathf.Clamp(value, 0f, 1f);
		springTorsoTalkTarget.localEulerAngles = new Vector3(value * 20f, 0f, 0f);
		springHeadTalkTarget.localEulerAngles = new Vector3((0f - value) * 40f, 0f, 0f);
		springTorsoTalkTransform.localRotation = SemiFunc.SpringQuaternionGet(springTorsoTalk, springTorsoTalkTarget.localRotation);
		springHeadTalkTransform.localRotation = SemiFunc.SpringQuaternionGet(springHeadTalk, springHeadTalkTarget.localRotation);
	}

	private void TongueLogic()
	{
		springTongueMiddleTransform.rotation = SemiFunc.SpringQuaternionGet(springTongueMiddle, springTongueMiddleTarget.rotation);
		springTongueTipTransform.rotation = SemiFunc.SpringQuaternionGet(springTongueTip, springTongueTipTarget.rotation);
	}

	private void LocalAudioLogic()
	{
		if (controller.headTargetActive && controller.headTargetActiveLocal && controller.headTarget.spectated)
		{
			if (!localAudio)
			{
				localAudio = true;
				SoundSpatialSet();
			}
			soundLocalBreathingLoop.PlayLoop(playing: true, 5f, 5f);
		}
		else
		{
			if (localAudio)
			{
				localAudio = false;
				SoundSpatialSet();
			}
			soundLocalBreathingLoop.PlayLoop(playing: false, 5f, 5f);
		}
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

	public void AttackSet(int _index)
	{
		switch (_index)
		{
		case 0:
			animator.SetTrigger(animAttack01);
			break;
		case 1:
			animator.SetTrigger(animAttack02);
			break;
		case 2:
			animator.SetTrigger(animAttack03);
			break;
		}
	}

	public void SoundSpatialSet()
	{
		float spatialBlend = 0f;
		float num = 0.5f;
		if (!localAudio)
		{
			spatialBlend = 1f;
			num = 1f;
		}
		soundTeleportIn.SpatialBlend = spatialBlend;
		soundTeleportIn.Volume = soundTeleportIn.VolumeDefault * num;
		soundTeleportOut.SpatialBlend = spatialBlend;
		soundTeleportOut.Volume = soundTeleportOut.VolumeDefault * num;
		soundJump.SpatialBlend = spatialBlend;
		soundJump.Volume = soundJump.VolumeDefault * num;
		soundLand.SpatialBlend = spatialBlend;
		soundLand.Volume = soundLand.VolumeDefault * num;
		soundHurt.SpatialBlend = spatialBlend;
		soundHurt.Volume = soundHurt.VolumeDefault * num;
		soundDeath.SpatialBlend = spatialBlend;
		soundDeath.Volume = soundDeath.VolumeDefault * num;
		soundAttackSwipe.SpatialBlend = spatialBlend;
		soundAttackSwipe.Volume = soundAttackSwipe.VolumeDefault * num;
		soundDropkickTell.SpatialBlend = spatialBlend;
		soundDropkickTell.Volume = soundDropkickTell.VolumeDefault * num;
		soundDropkickStart.SpatialBlend = spatialBlend;
		soundDropkickStart.Volume = soundDropkickStart.VolumeDefault * num;
		soundDropkickEnd.SpatialBlend = spatialBlend;
		soundDropkickEnd.Volume = soundDropkickEnd.VolumeDefault * num;
		soundDropkickHit.SpatialBlend = spatialBlend;
		soundDropkickHit.Volume = soundDropkickHit.VolumeDefault * num;
		soundFootstepLight.SpatialBlend = spatialBlend;
		soundFootstepLight.Volume = soundFootstepLight.VolumeDefault * num;
		soundFootstepHeavy.SpatialBlend = spatialBlend;
		soundFootstepHeavy.Volume = soundFootstepHeavy.VolumeDefault * num;
		soundRun.SpatialBlend = spatialBlend;
		soundRun.Volume = soundRun.VolumeDefault * num;
		soundLocalBreathingLoop.SpatialBlend = spatialBlend;
		soundLocalBreathingLoop.Volume = soundLocalBreathingLoop.VolumeDefault * num;
		soundMoveShort.SpatialBlend = spatialBlend;
		soundMoveShort.Volume = soundMoveShort.VolumeDefault * num;
		soundMoveLong.SpatialBlend = spatialBlend;
		soundMoveLong.Volume = soundMoveLong.VolumeDefault * num;
		soundNotice.SpatialBlend = spatialBlend;
		soundNotice.Volume = soundNotice.VolumeDefault * num;
		soundNoticeGlobal.SpatialBlend = spatialBlend;
		soundNoticeGlobal.Volume = soundNoticeGlobal.VolumeDefault * num;
		soundCantReach.SpatialBlend = spatialBlend;
		soundCantReach.Volume = soundCantReach.VolumeDefault * num;
		soundGrabHead.SpatialBlend = spatialBlend;
		soundGrabHead.Volume = soundGrabHead.VolumeDefault * num;
		soundReleaseHeadStart.SpatialBlend = spatialBlend;
		soundReleaseHeadStart.Volume = soundReleaseHeadStart.VolumeDefault * num;
		soundReleaseHeadEnd.SpatialBlend = spatialBlend;
		soundReleaseHeadEnd.Volume = soundReleaseHeadEnd.VolumeDefault * num;
		soundNoiseShort.SpatialBlend = spatialBlend;
		soundNoiseShort.Volume = soundNoiseShort.VolumeDefault * num;
		soundNoiseLong.SpatialBlend = spatialBlend;
		soundNoiseLong.Volume = soundNoiseLong.VolumeDefault * num;
		soundStunIntro.SpatialBlend = spatialBlend;
		soundStunIntro.Volume = soundStunIntro.VolumeDefault * num;
		soundStunLoop.SpatialBlend = spatialBlend;
		soundStunLoop.Volume = soundStunLoop.VolumeDefault * num;
		soundStunOutro.SpatialBlend = spatialBlend;
		soundStunOutro.Volume = soundStunOutro.VolumeDefault * num;
	}

	public void EventTeleport()
	{
		if (controller.currentState == EnemyHeadGrabber.State.Despawn)
		{
			soundTeleportOut.Play(controller.enemy.CenterTransform.position);
		}
		else
		{
			soundTeleportIn.Play(controller.enemy.CenterTransform.position);
		}
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
		ParticleSystem[] array = teleportParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: true);
		}
	}

	public void EventMaterialImpact()
	{
		Materials.Instance.Impulse(enemy.CenterTransform.position, Vector3.down, Materials.SoundType.Light, footstep: false, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void EventFootstepLeftLight()
	{
		soundFootstepLight.Play(footstepLeftTransform.position);
		Materials.Instance.Impulse(footstepLeftTransform.position, Vector3.down, Materials.SoundType.Light, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void EventFootstepRightLight()
	{
		soundFootstepLight.Play(footstepRightTransform.position);
		Materials.Instance.Impulse(footstepRightTransform.position, Vector3.down, Materials.SoundType.Light, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void EventFootstepLeftMedium()
	{
		soundFootstepLight.Play(footstepRightTransform.position);
		Materials.Instance.Impulse(footstepLeftTransform.position, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void EventFootstepRightMedium()
	{
		soundFootstepLight.Play(footstepRightTransform.position);
		Materials.Instance.Impulse(footstepRightTransform.position, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void EventFootstepLeftHeavy()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 5f, enemy.CenterTransform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 5f, enemy.CenterTransform.position, 0.05f);
		soundFootstepHeavy.Play(footstepLeftTransform.position);
		Materials.Instance.Impulse(footstepLeftTransform.position, Vector3.down, Materials.SoundType.Heavy, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void EventFootstepRightHeavy()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 5f, enemy.CenterTransform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 5f, enemy.CenterTransform.position, 0.05f);
		soundFootstepHeavy.Play(footstepRightTransform.position);
		Materials.Instance.Impulse(footstepRightTransform.position, Vector3.down, Materials.SoundType.Heavy, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void EventMoveShort()
	{
		soundMoveShort.Play(enemy.CenterTransform.position);
	}

	public void EventMoveLong()
	{
		soundMoveLong.Play(enemy.CenterTransform.position);
	}

	public void EventNotice()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
		soundNotice.Play(enemy.CenterTransform.position);
		soundNoticeGlobal.Play(enemy.CenterTransform.position);
	}

	public void EventCantReach()
	{
		soundCantReach.Play(enemy.CenterTransform.position);
	}

	public void EventGrabHead()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
		soundGrabHead.Play(enemy.CenterTransform.position);
	}

	public void EventNoiseShort()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 8f, enemy.CenterTransform.position, 0.25f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
		soundNoiseShort.Play(enemy.CenterTransform.position);
	}

	public void EventNoiseLong()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 8f, enemy.CenterTransform.position, 0.25f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
		soundNoiseLong.Play(enemy.CenterTransform.position);
	}

	public void EventJump()
	{
		soundJump.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventLand()
	{
		soundLand.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventRun()
	{
		soundRun.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventStunIntro()
	{
		soundStunIntro.Play(enemy.CenterTransform.position);
	}

	public void EventStunOutro()
	{
		soundStunIntro.Play(enemy.CenterTransform.position);
	}

	public void EventAttackSwipe()
	{
		soundAttackSwipe.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			controller.rotationStopTimer = 0.5f;
			Vector3 forward = controller.enemy.Rigidbody.transform.forward;
			forward.y = 0f;
			enemy.Rigidbody.rb.AddForce(forward * 10f, ForceMode.Impulse);
			enemy.Rigidbody.DisableFollowPosition(0.5f, 10f);
		}
	}

	public void EventDropkickTell()
	{
		soundDropkickTell.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(0.25f, 3f, 8f, enemy.CenterTransform.position, 0.25f);
		GameDirector.instance.CameraImpact.ShakeDistance(0.5f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventDropkickStart()
	{
		soundDropkickStart.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			controller.rotationStopTimer = 1f;
			Vector3 forward = controller.enemy.Rigidbody.transform.forward;
			forward.y = 0f;
			enemy.Rigidbody.rb.AddForce(forward * 30f, ForceMode.Impulse);
			enemy.Rigidbody.DisableFollowPosition(1f, 10f);
		}
	}

	public void EventDropkickLand()
	{
		Materials.Instance.Impulse(enemy.CenterTransform.position, Vector3.down, Materials.SoundType.Medium, footstep: false, footstepParticles: true, material, Materials.HostType.Enemy);
		GameDirector.instance.CameraShake.ShakeDistance(0.25f, 3f, 8f, enemy.CenterTransform.position, 0.25f);
		GameDirector.instance.CameraImpact.ShakeDistance(0.5f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventDropkickEnd()
	{
		soundDropkickEnd.Play(enemy.CenterTransform.position);
	}

	public void EventDropkickHit()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
		soundDropkickHit.Play(enemy.CenterTransform.position);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			Vector3 vector = -controller.enemy.Rigidbody.transform.forward;
			vector.y = 0f;
			enemy.Rigidbody.rb.AddForce(vector * 10f, ForceMode.Impulse);
		}
	}

	public void EventReleaseHead()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			controller.headTargetRelease = true;
		}
	}

	public void EventReleaseHeadStart()
	{
		soundReleaseHeadStart.Play(enemy.CenterTransform.position);
	}

	public void EventReleaseHeadEnd()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 8f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 8f, enemy.CenterTransform.position, 0.05f);
		soundReleaseHeadEnd.Play(enemy.CenterTransform.position);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			controller.rotationStopTimer = 1.5f;
			Vector3 vector = -controller.enemy.Rigidbody.transform.forward;
			vector.y = 0f;
			enemy.Rigidbody.rb.AddForce(vector * 12f, ForceMode.Impulse);
			enemy.Rigidbody.DisableFollowPosition(1.5f, 10f);
		}
	}

	public void EventDeath()
	{
		soundDeath.Play(enemy.CenterTransform.position);
		ParticleSystem[] array = deathParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: true);
		}
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.05f);
	}

	public void EventDespawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			controller.DeathHeadRelease();
			enemy.EnemyParent.Despawn();
		}
	}

	private void OnDrawGizmos()
	{
		float num = 0.075f;
		Gizmos.color = new Color(0.99f, 0.99f, 1f, 0.6f);
		Gizmos.matrix = lookAtParentAnimTransform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one * num);
		Gizmos.color = new Color(1f, 0f, 0.17f, 0.3f);
		Gizmos.DrawCube(Vector3.zero, Vector3.one * num);
	}
}
