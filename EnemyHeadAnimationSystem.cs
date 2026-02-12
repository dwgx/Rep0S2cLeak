using Photon.Pun;
using UnityEngine;

public class EnemyHeadAnimationSystem : MonoBehaviour
{
	private PhotonView PhotonView;

	public Enemy Enemy;

	public Animator Animator;

	public Materials.MaterialTrigger MaterialTrigger;

	public EnemyRigidbody EnemyRigidbody;

	public AnimatedOffset LookUnderOffset;

	public EnemyHeadFloat EnemyHeadFloat;

	public ParticleSystem TeleportParticlesTop;

	public ParticleSystem TeleportParticlesBot;

	public EnemyTriggerAttack EnemyTriggerAttack;

	[Space]
	public Sound ChaseBegin;

	public Sound ChaseBeginGlobal;

	public Sound ChaseBeginToChase;

	public Sound ChaseToIdle;

	public bool ChaseLoopActive;

	public Sound ChaseLoop;

	public Sound ChaseLoop2;

	public Sound TeethChatter;

	public Sound MoveLong;

	public Sound MoveShort;

	public Sound BiteStart;

	public Sound BiteEnd;

	public Sound Spawn;

	public Sound Despawn;

	public Sound Hurt;

	[Space]
	public float IdleTeethTimeMin;

	public float IdleTeethTimeMax;

	private float IdleTeethTime;

	private bool IdleTeethTrigger;

	private bool IdleBiteTrigger;

	private int AnimatorIdle;

	private int AnimatorChaseBegin;

	private int AnimatorChase;

	private int AnimatorIdleTeeth;

	private int AnimatorIdleBite;

	private int AnimatorChaseBite;

	private int AnimatorDespawn;

	private int AnimatorSpawn;

	private bool Idle;

	private void Awake()
	{
		PhotonView = GetComponent<PhotonView>();
		Animator.keepAnimatorStateOnDisable = true;
		IdleTeethTime = Random.Range(IdleTeethTimeMin, IdleTeethTimeMax);
		AnimatorIdle = Animator.StringToHash("Idle");
		AnimatorIdleTeeth = Animator.StringToHash("IdleTeeth");
		AnimatorIdleBite = Animator.StringToHash("IdleBite");
		AnimatorChaseBite = Animator.StringToHash("ChaseBite");
		AnimatorChaseBegin = Animator.StringToHash("ChaseBegin");
		AnimatorChase = Animator.StringToHash("Chase");
		AnimatorDespawn = Animator.StringToHash("Despawn");
		AnimatorSpawn = Animator.StringToHash("Spawn");
	}

	public void SetChaseBeginToChase()
	{
		ChaseBeginToChase.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void SetChaseToIdle()
	{
		ChaseToIdle.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void SetChaseBegin()
	{
		ChaseBegin.Play(base.transform.position);
		ChaseBeginGlobal.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void PlayTeethChatter()
	{
		TeethChatter.Play(base.transform.position);
	}

	public void MaterialImpact()
	{
		Materials.Instance.Impulse(base.transform.position, Vector3.down, Materials.SoundType.Heavy, footstep: false, footstepParticles: false, MaterialTrigger, Materials.HostType.Enemy);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void Slide()
	{
		Materials.Instance.Slide(base.transform.position, MaterialTrigger, 1f, isPlayer: false);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void PlayMoveLong()
	{
		MoveLong.Play(base.transform.position);
	}

	public void PlayMoveShort()
	{
		MoveShort.Play(base.transform.position);
	}

	public void AttackStuckPhysObject()
	{
		if (!IdleBiteTrigger)
		{
			IdleBiteTrigger = true;
			IdleBite();
		}
	}

	private void Update()
	{
		if (Enemy.CurrentState == EnemyState.Spawn || Enemy.CurrentState == EnemyState.Roaming || Enemy.CurrentState == EnemyState.Investigate || Enemy.CurrentState == EnemyState.ChaseEnd || Enemy.IsStunned())
		{
			if (Enemy.MasterClient && IdleTeethTime > 0f)
			{
				IdleTeethTime -= Time.deltaTime;
				if (IdleTeethTime <= 0f)
				{
					IdleTeeth();
					IdleTeethTime = Random.Range(IdleTeethTimeMin, IdleTeethTimeMax);
				}
			}
			if (IdleTeethTrigger)
			{
				Animator.SetTrigger(AnimatorIdleTeeth);
				IdleTeethTrigger = false;
			}
			Animator.SetBool(AnimatorIdle, value: true);
		}
		else
		{
			Animator.SetBool(AnimatorIdle, value: false);
			if (IdleTeethTime < IdleTeethTimeMin)
			{
				IdleTeethTime = Random.Range(IdleTeethTimeMin, IdleTeethTimeMax);
			}
		}
		if (Enemy.CurrentState == EnemyState.ChaseBegin)
		{
			Animator.SetBool(AnimatorChaseBegin, value: true);
		}
		else
		{
			Animator.SetBool(AnimatorChaseBegin, value: false);
		}
		if (Enemy.CurrentState == EnemyState.Chase || Enemy.CurrentState == EnemyState.ChaseSlow)
		{
			Animator.SetBool(AnimatorChase, value: true);
		}
		else
		{
			Animator.SetBool(AnimatorChase, value: false);
		}
		if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Chase Bite"))
		{
			EnemyTriggerAttack.Attack = false;
		}
		else if (EnemyTriggerAttack.Attack)
		{
			ChaseBiteTrigger();
		}
		if (Enemy.CurrentState == EnemyState.LookUnder && Enemy.StateLookUnder.WaitDone)
		{
			EnemyRigidbody.OverrideFollowPosition(0.1f, 50f);
			EnemyRigidbody.OverrideFollowRotation(0.1f, 2f);
			LookUnderOffset.Active(0.1f);
			EnemyHeadFloat.Disable(0.5f);
		}
		ChaseLoop.PlayLoop(ChaseLoopActive, 5f, 5f);
		ChaseLoop2.PlayLoop(ChaseLoopActive, 5f, 5f);
		if ((Enemy.CurrentState == EnemyState.Despawn || Enemy.Health.dead) && !Animator.GetCurrentAnimatorStateInfo(0).IsName("Despawn") && !Animator.GetCurrentAnimatorStateInfo(0).IsName("Chase Bite"))
		{
			Animator.SetTrigger(AnimatorDespawn);
		}
		if (Enemy.CurrentState == EnemyState.Spawn && !Animator.GetCurrentAnimatorStateInfo(0).IsName("Spawn"))
		{
			Animator.SetTrigger(AnimatorSpawn);
		}
	}

	private void IdleTeeth()
	{
		if (GameManager.instance.gameMode == 0)
		{
			IdleTeethRPC();
		}
		else
		{
			PhotonView.RPC("IdleTeethRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void IdleTeethRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			IdleTeethTrigger = true;
		}
	}

	private void ChaseBiteTrigger()
	{
		if (GameManager.instance.gameMode == 0)
		{
			ChaseBiteTriggerRPC();
		}
		else
		{
			PhotonView.RPC("ChaseBiteTriggerRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void ChaseBiteTriggerRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			Animator.SetTrigger(AnimatorChaseBite);
		}
	}

	public void PlayChaseBiteStart()
	{
		BiteStart.Play(base.transform.position);
	}

	public void PlayChaseBiteImpact()
	{
		Enemy.Rigidbody.GrabRelease();
		BiteEnd.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		EnemyRigidbody.DisableFollowPosition(0.25f, 1f);
		EnemyRigidbody.DisableFollowRotation(0.25f, 2f);
		EnemyRigidbody.rb.AddForce(EnemyRigidbody.transform.forward * 10f, ForceMode.Impulse);
	}

	public void PlayBiteStart()
	{
		BiteStart.Play(base.transform.position);
	}

	private void IdleBite()
	{
		if (GameManager.instance.gameMode == 0)
		{
			IdleBiteRPC();
		}
		else
		{
			PhotonView.RPC("IdleBiteRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void IdleBiteRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			Animator.SetTrigger(AnimatorIdleBite);
		}
	}

	public void IdleBiteImpact()
	{
		BiteEnd.Play(base.transform.position);
		EnemyRigidbody.DisableFollowPosition(1f, 1f);
		EnemyRigidbody.DisableFollowRotation(1f, 1f);
		EnemyRigidbody.rb.AddForce(EnemyRigidbody.transform.forward * 10f, ForceMode.Impulse);
	}

	public void IdleBiteDone()
	{
		Enemy.AttackStuckPhysObject.Reset();
		IdleBiteTrigger = false;
		EnemyRigidbody.DisableFollowPosition(0.2f, 1f);
		EnemyRigidbody.DisableFollowRotation(0.5f, 1f);
		EnemyRigidbody.rb.AddForce(-EnemyRigidbody.transform.forward * 2f, ForceMode.Impulse);
	}

	public void OnSpawn()
	{
		Animator.Play("Spawn", 0, 0f);
	}

	public void PlayDespawn()
	{
		Despawn.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void PlaySpawn()
	{
		Spawn.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void TeleportParticlesStart()
	{
		TeleportParticlesTop.Play();
		TeleportParticlesBot.Play();
	}

	public void TeleportParticlesStop()
	{
		TeleportParticlesTop.Stop();
		TeleportParticlesBot.Stop();
	}

	private void DespawnSet()
	{
		if (GameManager.instance.gameMode == 0)
		{
			DespawnSetRPC();
		}
		else
		{
			PhotonView.RPC("DespawnSetRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void DespawnSetRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info) && (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient))
		{
			Enemy.StateDespawn.Despawn();
		}
	}
}
