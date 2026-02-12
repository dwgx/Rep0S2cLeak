using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyUpscream : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		PlayerNotice,
		GoToPlayer,
		Attack,
		Leave,
		IdleBreak,
		Stun
	}

	[Header("References")]
	public EnemyUpscreamAnim upscreamAnim;

	internal Enemy enemy;

	public ParticleSystem[] deathEffects;

	public State currentState;

	public State previousState;

	private float stateTimer;

	private bool attackImpulse;

	private bool stateImpulse;

	internal PlayerAvatar targetPlayer;

	private Vector3 targetPosition;

	public Transform visionTransform;

	private float hasVisionTimer;

	private Vector3 agentPoint;

	private float roamWaitTimer;

	private PhotonView photonView;

	[Header("Head")]
	public SpringQuaternion headSpring;

	public Transform headTransform;

	public Transform headIdleTransform;

	[Header("Eyes")]
	public SpringQuaternion eyeLeftSpring;

	[Space(10f)]
	public Transform eyeLeftTransform;

	public Transform eyeLeftIdle;

	public Transform eyeLeftTarget;

	[Space(10f)]
	public SpringQuaternion eyeRightSpring;

	[Space(10f)]
	public Transform eyeRightTransform;

	public Transform eyeRightIdle;

	public Transform eyeRightTarget;

	[Header("Idle Break")]
	public float idleBreakTimeMin = 45f;

	public float idleBreakTimeMax = 90f;

	private float idleBreakTimer;

	private float grabAggroTimer;

	private int attacks;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		HeadLogic();
		EyeLogic();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (grabAggroTimer > 0f)
			{
				grabAggroTimer -= Time.deltaTime;
			}
			if (enemy.IsStunned())
			{
				UpdateState(State.Stun);
			}
			switch (currentState)
			{
			case State.Spawn:
				StateSpawn();
				break;
			case State.Idle:
				StateIdle();
				IdleBreakLogic();
				break;
			case State.Investigate:
				StateInvestigate();
				AgentVelocityRotation();
				break;
			case State.Roam:
				StateRoam();
				AgentVelocityRotation();
				IdleBreakLogic();
				break;
			case State.PlayerNotice:
				StatePlayerNotice();
				break;
			case State.GoToPlayer:
				AgentVelocityRotation();
				StateGoToPlayer();
				break;
			case State.Attack:
				StateAttack();
				break;
			case State.Leave:
				StateLeave();
				break;
			case State.IdleBreak:
				StateIdleBreak();
				break;
			case State.Stun:
				StateStun();
				break;
			}
		}
		if (currentState == State.Attack && (bool)targetPlayer)
		{
			if (targetPlayer.isLocal)
			{
				PlayerController.instance.InputDisable(0.1f);
				CameraAim.Instance.AimTargetSet(visionTransform.position, 0.1f, 5f, base.gameObject, 90);
				CameraZoom.Instance.OverrideZoomSet(50f, 0.1f, 5f, 5f, base.gameObject, 50);
				Color color = new Color(0.4f, 0f, 0f, 1f);
				PostProcessing.Instance.VignetteOverride(color, 0.75f, 1f, 3.5f, 2.5f, 0.5f, base.gameObject);
			}
			if (attackImpulse)
			{
				if (targetPlayer.isLocal)
				{
					targetPlayer.physGrabber.ReleaseObject(-1);
					CameraGlitch.Instance.PlayLong();
				}
				attackImpulse = false;
				upscreamAnim.animator.SetTrigger("Attack");
			}
		}
		else
		{
			attackImpulse = true;
		}
	}

	private void StateSpawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateIdle()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			if (previousState == State.Spawn)
			{
				stateTimer = 0.5f;
			}
			else
			{
				stateTimer = Random.Range(3f, 8f);
			}
			stateImpulse = false;
		}
		if (!SemiFunc.EnemySpawnIdlePause())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.Roam);
			}
			if (SemiFunc.EnemyForceLeave(enemy))
			{
				UpdateState(State.Leave);
			}
		}
	}

	private void StateRoam()
	{
		float num = Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetDestination());
		if (stateImpulse || !enemy.NavMeshAgent.HasPath() || num < 1f)
		{
			if (stateImpulse)
			{
				roamWaitTimer = 0f;
				stateImpulse = false;
			}
			if (roamWaitTimer <= 0f)
			{
				stateTimer = 5f;
				roamWaitTimer = Random.Range(0f, 5f);
				LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 5f, 15f);
				if (!levelPoint)
				{
					levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
				}
				if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
				{
					agentPoint = hit.position;
					enemy.NavMeshAgent.SetDestination(agentPoint);
				}
			}
			else
			{
				roamWaitTimer -= Time.deltaTime;
			}
		}
		else
		{
			SemiFunc.EnemyCartJump(enemy);
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
				if (stateTimer <= 0f)
				{
					UpdateState(State.Idle);
				}
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateInvestigate()
	{
		if (stateImpulse)
		{
			stateTimer = 5f;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
		}
		else
		{
			enemy.NavMeshAgent.SetDestination(agentPoint);
			SemiFunc.EnemyCartJump(enemy);
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentPoint) < 2f)
			{
				SemiFunc.EnemyCartJumpReset(enemy);
				UpdateState(State.Idle);
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StatePlayerNotice()
	{
		if (stateImpulse)
		{
			stateTimer = 0.5f;
			stateImpulse = false;
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
		}
		enemy.NavMeshAgent.Stop(0.5f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			enemy.NavMeshAgent.Stop(0f);
			UpdateState(State.GoToPlayer);
		}
	}

	private void StateGoToPlayer()
	{
		if (!enemy.Jump.jumping)
		{
			enemy.NavMeshAgent.SetDestination(targetPlayer.transform.position);
		}
		else
		{
			enemy.NavMeshAgent.Disable(0.1f);
			base.transform.position = Vector3.MoveTowards(base.transform.position, targetPlayer.transform.position, 5f * Time.deltaTime);
		}
		SemiFunc.EnemyCartJump(enemy);
		if (stateImpulse)
		{
			stateTimer = 2f;
			stateImpulse = false;
			return;
		}
		enemy.NavMeshAgent.OverrideAgent(5f, 10f, 0.25f);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, targetPlayer.transform.position) < 1.5f && !enemy.Jump.jumping && !enemy.IsStunned())
		{
			enemy.NavMeshAgent.ResetPath();
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.Attack);
			return;
		}
		if (Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetDestination()) < 1f)
		{
			if (stateTimer <= 0f)
			{
				enemy.Jump.StuckReset();
				UpdateState(State.Leave);
			}
			else if (Vector3.Distance(enemy.Rigidbody.transform.position, targetPlayer.transform.position) > 1.5f)
			{
				enemy.Jump.StuckTrigger(targetPlayer.transform.position - enemy.Rigidbody.transform.position);
			}
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Leave);
		}
	}

	private void StateAttack()
	{
		if (stateImpulse)
		{
			attacks++;
			stateTimer = 1.5f;
			stateImpulse = false;
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
		}
		Quaternion quaternion = Quaternion.Euler(0f, Quaternion.LookRotation(targetPlayer.PlayerVisionTarget.VisionTransform.position - enemy.Rigidbody.transform.position).eulerAngles.y, 0f);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, 50f * Time.deltaTime);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			if (attacks >= 3 || Random.Range(0f, 1f) <= 0.5f)
			{
				UpdateState(State.Leave);
			}
			else
			{
				UpdateState(State.Idle);
			}
		}
	}

	private void StateLeave()
	{
		if (stateImpulse)
		{
			stateTimer = 5f;
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGetPlayerDistance(base.transform.position, 30f, 50f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
			}
			if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				agentPoint = hit.position;
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			stateImpulse = false;
			SemiFunc.EnemyLeaveStart(enemy);
		}
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		SemiFunc.EnemyCartJump(enemy);
		enemy.NavMeshAgent.SetDestination(agentPoint);
		enemy.NavMeshAgent.OverrideAgent(enemy.NavMeshAgent.DefaultSpeed + 2.5f, enemy.NavMeshAgent.DefaultAcceleration + 2.5f, 0.2f);
		enemy.Rigidbody.OverrideFollowPosition(1f, 10f);
		if (Vector3.Distance(base.transform.position, agentPoint) < 1f || stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateIdleBreak()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			stateTimer = 2f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateStun()
	{
		if (!enemy.IsStunned())
		{
			UpdateState(State.Idle);
		}
	}

	internal void UpdateState(State _state)
	{
		if ((!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient) && currentState != _state)
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateStateRPC", RpcTarget.All, _state);
			}
			else
			{
				UpdateStateRPC(_state);
			}
		}
	}

	private void IdleBreakLogic()
	{
		if (idleBreakTimer >= 0f)
		{
			idleBreakTimer -= Time.deltaTime;
			if (idleBreakTimer <= 0f)
			{
				SemiFunc.EnemyCartJumpReset(enemy);
				UpdateState(State.IdleBreak);
				idleBreakTimer = Random.Range(idleBreakTimeMin, idleBreakTimeMax);
			}
		}
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
	}

	public void OnHurt()
	{
		upscreamAnim.hurtSound.Play(upscreamAnim.transform.position);
	}

	public void OnDeath()
	{
		ParticleSystem[] array = deathEffects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.05f);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnVision()
	{
		if (currentState == State.Idle || currentState == State.Roam || currentState == State.IdleBreak || currentState == State.Investigate || currentState == State.Leave)
		{
			targetPlayer = enemy.Vision.onVisionTriggeredPlayer;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("TargetPlayerRPC", RpcTarget.All, targetPlayer.photonView.ViewID);
			}
			if (!enemy.IsStunned())
			{
				if (GameManager.Multiplayer())
				{
					photonView.RPC("NoticeSetRPC", RpcTarget.All, enemy.Vision.onVisionTriggeredID);
				}
				else
				{
					upscreamAnim.NoticeSet(enemy.Vision.onVisionTriggeredID);
				}
			}
			UpdateState(State.PlayerNotice);
		}
		else if (currentState == State.GoToPlayer && targetPlayer == enemy.Vision.onVisionTriggeredPlayer)
		{
			stateTimer = 2f;
		}
	}

	public void OnInvestigate()
	{
		if (currentState == State.Roam || currentState == State.Idle || currentState == State.IdleBreak || currentState == State.Investigate)
		{
			UpdateState(State.Investigate);
			agentPoint = enemy.StateInvestigate.onInvestigateTriggeredPosition;
		}
	}

	public void OnGrabbed()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || grabAggroTimer > 0f || currentState != State.Leave)
		{
			return;
		}
		grabAggroTimer = 60f;
		if (targetPlayer != enemy.Rigidbody.onGrabbedPlayerAvatar)
		{
			targetPlayer = enemy.Rigidbody.onGrabbedPlayerAvatar;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("TargetPlayerRPC", RpcTarget.All, targetPlayer.photonView.ViewID);
			}
		}
		if (!enemy.IsStunned())
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("NoticeSetRPC", RpcTarget.All, targetPlayer.photonView.ViewID);
			}
			else
			{
				upscreamAnim.NoticeSet(targetPlayer.photonView.ViewID);
			}
		}
		UpdateState(State.PlayerNotice);
	}

	public void HeadLogic()
	{
		Quaternion targetRotation = headIdleTransform.rotation;
		if ((bool)targetPlayer && (currentState == State.PlayerNotice || currentState == State.GoToPlayer || currentState == State.Attack) && !enemy.IsStunned())
		{
			Vector3 position = targetPlayer.PlayerVisionTarget.VisionTransform.position;
			if (targetPlayer.isLocal)
			{
				position = targetPlayer.localCamera.transform.position;
			}
			targetRotation = Quaternion.LookRotation(position - headTransform.position);
		}
		headTransform.rotation = SemiFunc.SpringQuaternionGet(headSpring, targetRotation);
	}

	public void EyeLogic()
	{
		if (currentState == State.PlayerNotice || currentState == State.GoToPlayer || currentState == State.Attack)
		{
			eyeLeftSpring.damping = 0.6f;
			eyeLeftSpring.speed = 15f;
			eyeRightSpring.damping = 0.6f;
			eyeRightSpring.speed = 15f;
			eyeLeftTransform.rotation = SemiFunc.SpringQuaternionGet(eyeLeftSpring, eyeLeftTarget.rotation);
			eyeRightTransform.rotation = SemiFunc.SpringQuaternionGet(eyeRightSpring, eyeRightTarget.rotation);
		}
		else
		{
			eyeLeftSpring.damping = 0.2f;
			eyeLeftSpring.speed = 15f;
			eyeRightSpring.damping = 0.2f;
			eyeRightSpring.speed = 15f;
			eyeLeftTransform.rotation = SemiFunc.SpringQuaternionGet(eyeLeftSpring, eyeLeftIdle.rotation);
			eyeRightTransform.rotation = SemiFunc.SpringQuaternionGet(eyeRightSpring, eyeRightIdle.rotation);
		}
	}

	private void AgentVelocityRotation()
	{
		if (enemy.NavMeshAgent.AgentVelocity.magnitude > 0.005f)
		{
			Quaternion quaternion = Quaternion.Euler(0f, Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized).eulerAngles.y, 0f);
			float num = 2f;
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, num * Time.deltaTime);
		}
	}

	[PunRPC]
	private void UpdateStateRPC(State _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			previousState = currentState;
			currentState = _state;
			stateImpulse = true;
			stateTimer = 0f;
			if (currentState == State.Spawn)
			{
				upscreamAnim.SetSpawn();
			}
		}
	}

	[PunRPC]
	private void TargetPlayerRPC(int _playerID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.photonView.ViewID == _playerID)
			{
				targetPlayer = player;
				break;
			}
		}
	}

	[PunRPC]
	private void NoticeSetRPC(int _playerID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			upscreamAnim.NoticeSet(_playerID);
		}
	}
}
