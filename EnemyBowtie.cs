using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBowtie : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		PlayerNotice,
		Yell,
		YellEnd,
		Leave,
		Stun,
		Despawn
	}

	private PhotonView photonView;

	public State currentState;

	public float stateTimer;

	private bool stateImpulse;

	[Space]
	public EnemyBowtieAnim anim;

	public Transform hurtColliderLeave;

	[Space]
	public SpringQuaternion headSpring;

	public Transform headTransform;

	public Transform HeadTargetTransform;

	[Space]
	public SpringQuaternion eyeRightSpring;

	public Transform eyeRightTransform;

	public Transform eyeRightTargetTransform;

	[Space]
	public SpringQuaternion eyeLeftSpring;

	public Transform eyeLeftTransform;

	public Transform eyeLeftTargetTransform;

	[Space]
	public SpringQuaternion horizontalRotationSpring;

	private Quaternion horizontalRotationTarget;

	[Space]
	public Transform verticalRotationTransform;

	public SpringQuaternion verticalRotationSpring;

	private Quaternion verticalRotationTarget;

	private float roamWaitTimer;

	private Vector3 agentDestination;

	internal Enemy enemy;

	private PlayerAvatar playerTarget;

	private float grabAggroTimer;

	private int attacks;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		enemy = GetComponent<Enemy>();
	}

	private void Update()
	{
		HurtColliderLeaveLogic();
		SpringLogic();
		PlayerEyesLogic();
		if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (grabAggroTimer > 0f)
		{
			grabAggroTimer -= Time.deltaTime;
		}
		if (LevelGenerator.Instance.Generated)
		{
			HorizontalRotationLogic();
			VerticalRotationLogic();
			if (enemy.IsStunned())
			{
				UpdateState(State.Stun);
			}
			else if (enemy.CurrentState == EnemyState.Despawn)
			{
				UpdateState(State.Despawn);
			}
			switch (currentState)
			{
			case State.Spawn:
				StateSpawn();
				break;
			case State.Idle:
				StateIdle();
				break;
			case State.Roam:
				StateRoam();
				break;
			case State.Investigate:
				StateInvestigate();
				break;
			case State.PlayerNotice:
				StatePlayerNotice();
				break;
			case State.Yell:
				StateYell();
				break;
			case State.YellEnd:
				StateYellEnd();
				break;
			case State.Leave:
				StateLeave();
				break;
			case State.Stun:
				StateStun();
				break;
			case State.Despawn:
				StateDespawn();
				break;
			}
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
			stateTimer = Random.Range(2f, 6f);
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
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 5f, 15f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
			}
			if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				agentDestination = hit.position;
			}
			stateTimer = 5f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		if (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 1f)
		{
			UpdateState(State.Idle);
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
			enemy.NavMeshAgent.SetDestination(agentDestination);
			SemiFunc.EnemyCartJump(enemy);
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 2f)
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
			stateTimer = 1f;
			stateImpulse = false;
		}
		enemy.Jump.SurfaceJumpDisable(0.5f);
		enemy.NavMeshAgent.ResetPath();
		enemy.NavMeshAgent.Stop(0.1f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			enemy.NavMeshAgent.Stop(0f);
			UpdateState(State.Yell);
		}
	}

	private void StateYell()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			stateTimer = 5f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		enemy.Jump.SurfaceJumpDisable(0.5f);
		if (stateTimer <= 0f)
		{
			UpdateState(State.YellEnd);
		}
	}

	private void StateYellEnd()
	{
		if (stateImpulse)
		{
			attacks++;
			stateTimer = 1f;
			stateImpulse = false;
		}
		enemy.Jump.SurfaceJumpDisable(0.5f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			if (attacks >= 3 || Random.Range(0f, 1f) <= 0.3f)
			{
				attacks = 0;
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
			LevelPoint levelPoint = SemiFunc.LevelPointGetPlayerDistance(base.transform.position, 30f, 60f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
			}
			if ((bool)levelPoint)
			{
				agentDestination = levelPoint.transform.position;
			}
			else
			{
				enemy.EnemyParent.SpawnedTimerSet(0f);
			}
			stateTimer = 5f;
			stateImpulse = false;
			SemiFunc.EnemyLeaveStart(enemy);
		}
		else
		{
			enemy.NavMeshAgent.SetDestination(agentDestination);
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			enemy.NavMeshAgent.OverrideAgent(5f, 10f, 0.25f);
			if (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 1f)
			{
				UpdateState(State.Idle);
			}
		}
	}

	private void StateStun()
	{
		if (!enemy.IsStunned())
		{
			UpdateState(State.Idle);
		}
	}

	private void StateDespawn()
	{
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Idle);
		}
		if (anim.isActiveAndEnabled)
		{
			anim.OnSpawn();
		}
	}

	public void OnHurt()
	{
		anim.GroanPause();
		anim.StunPause();
		anim.hurtSound.Play(anim.transform.position);
		if (currentState == State.Yell)
		{
			UpdateState(State.YellEnd);
		}
	}

	public void OnDeath()
	{
		anim.GroanPause();
		anim.StunPause();
		anim.deathSound.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		anim.particleImpact.Play();
		anim.particleBits.Play();
		anim.particleEyes.Play();
		anim.particleDirectionalBits.transform.rotation = Quaternion.LookRotation(-enemy.Health.hurtDirection.normalized);
		anim.particleDirectionalBits.Play();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnVisionTriggered()
	{
		if ((currentState != State.Idle && currentState != State.Roam && currentState != State.Investigate && currentState != State.Leave) || enemy.Jump.jumping || enemy.IsStunned())
		{
			return;
		}
		PlayerAvatar onVisionTriggeredPlayer = enemy.Vision.onVisionTriggeredPlayer;
		if (!(Mathf.Abs(onVisionTriggeredPlayer.transform.position.y - enemy.transform.position.y) > 4f))
		{
			playerTarget = onVisionTriggeredPlayer;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("NoticeRPC", RpcTarget.All, enemy.Vision.onVisionTriggeredID);
			}
			else
			{
				anim.NoticeSet(enemy.Vision.onVisionTriggeredID);
			}
			UpdateState(State.PlayerNotice);
			VerticalAimSet(100f);
		}
	}

	public void OnInvestigate()
	{
		if (currentState == State.Roam || currentState == State.Idle || currentState == State.Investigate)
		{
			UpdateState(State.Investigate);
			agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
		}
	}

	public void OnGrabbed()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || grabAggroTimer > 0f || currentState != State.Leave)
		{
			return;
		}
		grabAggroTimer = 60f;
		PlayerAvatar onGrabbedPlayerAvatar = enemy.Rigidbody.onGrabbedPlayerAvatar;
		if (onGrabbedPlayerAvatar.transform.position.y - enemy.transform.position.y > 1.15f || onGrabbedPlayerAvatar.transform.position.y - enemy.transform.position.y < -1f)
		{
			return;
		}
		playerTarget = onGrabbedPlayerAvatar;
		if (!enemy.IsStunned())
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("NoticeRPC", RpcTarget.All, playerTarget.photonView.ViewID);
			}
			else
			{
				anim.NoticeSet(playerTarget.photonView.ViewID);
			}
		}
		UpdateState(State.PlayerNotice);
	}

	private void UpdateState(State _state)
	{
		if (currentState != _state)
		{
			currentState = _state;
			stateImpulse = true;
			stateTimer = 0f;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateStateRPC", RpcTarget.All, currentState);
			}
		}
	}

	private void HorizontalRotationLogic()
	{
		if (currentState == State.Roam || currentState == State.Investigate || currentState == State.Leave)
		{
			if (enemy.NavMeshAgent.AgentVelocity.magnitude > 0.05f)
			{
				Quaternion quaternion = Quaternion.Euler(0f, Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized).eulerAngles.y, 0f);
				horizontalRotationTarget = quaternion;
			}
		}
		else if (currentState == State.PlayerNotice)
		{
			Quaternion quaternion2 = Quaternion.Euler(0f, Quaternion.LookRotation(playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.Rigidbody.transform.position).eulerAngles.y, 0f);
			horizontalRotationTarget = quaternion2;
		}
		else if (currentState == State.Yell)
		{
			Quaternion quaternion3 = Quaternion.Euler(0f, Quaternion.LookRotation(playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.Rigidbody.transform.position).eulerAngles.y, 0f);
			horizontalRotationTarget = Quaternion.Slerp(horizontalRotationTarget, quaternion3, 1f * Time.deltaTime);
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(horizontalRotationSpring, horizontalRotationTarget);
	}

	private void VerticalRotationLogic()
	{
		if (currentState == State.Yell)
		{
			VerticalAimSet(1f);
			verticalRotationTransform.localRotation = SemiFunc.SpringQuaternionGet(verticalRotationSpring, verticalRotationTarget);
		}
		else
		{
			verticalRotationTransform.localRotation = SemiFunc.SpringQuaternionGet(verticalRotationSpring, Quaternion.identity);
		}
	}

	private void VerticalAimSet(float _lerp)
	{
		verticalRotationTransform.LookAt(playerTarget.transform);
		float num = 45f;
		float x = verticalRotationTransform.localEulerAngles.x;
		x = ((!(x < 180f)) ? Mathf.Clamp(x, 360f - num, 360f) : Mathf.Clamp(x, 0f, num));
		verticalRotationTransform.localRotation = Quaternion.Euler(x, 0f, 0f);
		Quaternion localRotation = verticalRotationTransform.localRotation;
		verticalRotationTransform.localRotation = Quaternion.identity;
		verticalRotationTarget = Quaternion.Lerp(verticalRotationTarget, localRotation, _lerp * Time.deltaTime);
	}

	private void HurtColliderLeaveLogic()
	{
		if (!enemy.Jump.jumping && currentState == State.Leave)
		{
			hurtColliderLeave.gameObject.SetActive(value: true);
		}
		else
		{
			hurtColliderLeave.gameObject.SetActive(value: false);
		}
	}

	private void SpringLogic()
	{
		headTransform.rotation = SemiFunc.SpringQuaternionGet(headSpring, HeadTargetTransform.rotation);
		eyeRightTransform.rotation = SemiFunc.SpringQuaternionGet(eyeRightSpring, eyeRightTargetTransform.rotation);
		eyeLeftTransform.rotation = SemiFunc.SpringQuaternionGet(eyeLeftSpring, eyeLeftTargetTransform.rotation);
	}

	private void PlayerEyesLogic()
	{
		if (currentState != State.PlayerNotice && currentState != State.Yell && currentState != State.YellEnd)
		{
			return;
		}
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (Vector3.Distance(base.transform.position, player.transform.position) < 8f)
			{
				SemiFunc.PlayerEyesOverride(player, enemy.Vision.VisionTransform.position, 0.1f, base.gameObject);
			}
		}
	}

	[PunRPC]
	private void UpdateStateRPC(State _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
		}
	}

	[PunRPC]
	private void NoticeRPC(int _playerID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.NoticeSet(_playerID);
		}
	}
}
