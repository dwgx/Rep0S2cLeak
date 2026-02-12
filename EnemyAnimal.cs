using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimal : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		PlayerNotice,
		GoToPlayer,
		WreakHavoc,
		Leave
	}

	private Enemy enemy;

	private PhotonView photonView;

	public EnemyAnimalAnim enemyAnimalAnim;

	public GameObject welts;

	public State currentState;

	private float havocTimer;

	private LevelPoint ignorePoint;

	private float stateTimer;

	private bool stateImpulse;

	private Vector3 agentDestination;

	private PlayerAvatar playerTarget;

	private float grabAggroTimer;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		if (currentState == State.PlayerNotice || currentState == State.GoToPlayer || currentState == State.WreakHavoc)
		{
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (Vector3.Distance(base.transform.position, player.transform.position) < 8f)
				{
					SemiFunc.PlayerEyesOverride(player, enemy.Vision.VisionTransform.position, 0.1f, base.gameObject);
				}
			}
		}
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			if (grabAggroTimer > 0f)
			{
				grabAggroTimer -= Time.deltaTime;
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
				PlayerLookAt();
				break;
			case State.GoToPlayer:
				StateGoToPlayer();
				break;
			case State.WreakHavoc:
				StateWreakHavoc();
				break;
			case State.Leave:
				StateLeave();
				break;
			}
		}
	}

	private void StateSpawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
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
			enemy.NavMeshAgent.OverrideAgent(4f, 12f, 0.25f);
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
			stateTimer = 0.5f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.ResetPath();
		enemy.NavMeshAgent.Stop(0.1f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			enemy.NavMeshAgent.Stop(0f);
			UpdateState(State.GoToPlayer);
		}
	}

	private void StateGoToPlayer()
	{
		enemy.NavMeshAgent.SetDestination(playerTarget.transform.position);
		if (stateImpulse)
		{
			stateTimer = 10f;
			stateImpulse = false;
			return;
		}
		if (Vector3.Distance(enemy.Rigidbody.transform.position, playerTarget.transform.position) < 3f)
		{
			enemy.NavMeshAgent.ResetPath();
			UpdateState(State.WreakHavoc);
			return;
		}
		if (Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetDestination()) < 1f && Vector3.Distance(enemy.Rigidbody.transform.position, playerTarget.transform.position) > 1.5f)
		{
			enemy.Jump.StuckTrigger(playerTarget.transform.position - enemy.Rigidbody.transform.position);
			enemy.Rigidbody.DisableFollowPosition(1f, 10f);
		}
		enemy.NavMeshAgent.OverrideAgent(5f, 10f, 0.25f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Leave);
		}
	}

	private void StateWreakHavoc()
	{
		if (stateImpulse)
		{
			havocTimer = 0f;
			stateTimer = 20f;
			stateImpulse = false;
		}
		if (!playerTarget)
		{
			UpdateState(State.Leave);
			return;
		}
		if (havocTimer <= 0f || Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetDestination()) < 0.25f)
		{
			LevelPoint levelPoint = SemiFunc.LevelPointInTargetRoomGet(playerTarget.RoomVolumeCheck, 1f, 10f, ignorePoint);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointInTargetRoomGet(playerTarget.RoomVolumeCheck, 0f, 999f, ignorePoint);
			}
			if (!levelPoint || !NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1))
			{
				UpdateState(State.Leave);
				return;
			}
			if (Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				ignorePoint = levelPoint;
				agentDestination = hit.position;
				enemy.NavMeshAgent.SetDestination(agentDestination);
			}
			havocTimer = 2f;
		}
		enemy.NavMeshAgent.OverrideAgent(5f, 10f, 0.25f);
		havocTimer -= Time.deltaTime;
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Leave);
		}
	}

	private void StateLeave()
	{
		if (stateImpulse)
		{
			LevelPoint levelPoint = SemiFunc.LevelPointGetPlayerDistance(base.transform.position, 25f, 50f);
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
			stateTimer = 10f;
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
			enemy.NavMeshAgent.OverrideAgent(6f, 12f, 0.25f);
			if (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 1f)
			{
				UpdateState(State.Idle);
			}
		}
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
		if (enemyAnimalAnim.isActiveAndEnabled)
		{
			enemyAnimalAnim.SetSpawn();
		}
	}

	public void OnHurt()
	{
		enemyAnimalAnim.hurtSound.Play(enemyAnimalAnim.transform.position);
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.Leave)
		{
			UpdateState(State.Idle);
		}
	}

	public void OnDeath()
	{
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		enemyAnimalAnim.particleImpact.Play();
		enemyAnimalAnim.particleBits.Play();
		Quaternion rotation = Quaternion.LookRotation(-enemy.Health.hurtDirection.normalized);
		enemyAnimalAnim.particleDirectionalBits.transform.rotation = rotation;
		enemyAnimalAnim.particleDirectionalBits.Play();
		enemyAnimalAnim.particleLegBits.transform.rotation = rotation;
		enemyAnimalAnim.particleLegBits.Play();
		enemyAnimalAnim.deathSound.Play(enemyAnimalAnim.transform.position);
		enemy.EnemyParent.Despawn();
	}

	public void OnVision()
	{
		if (currentState != State.Idle && currentState != State.Roam && currentState != State.Investigate && currentState != State.Leave)
		{
			return;
		}
		playerTarget = enemy.Vision.onVisionTriggeredPlayer;
		if (!enemy.IsStunned())
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("NoticeRPC", RpcTarget.All, enemy.Vision.onVisionTriggeredID);
			}
			else
			{
				enemyAnimalAnim.NoticeSet(enemy.Vision.onVisionTriggeredID);
			}
		}
		UpdateState(State.PlayerNotice);
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
		playerTarget = enemy.Rigidbody.onGrabbedPlayerAvatar;
		if (!enemy.IsStunned())
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("NoticeRPC", RpcTarget.All, playerTarget.photonView.ViewID);
			}
			else
			{
				enemyAnimalAnim.NoticeSet(playerTarget.photonView.ViewID);
			}
		}
		UpdateState(State.PlayerNotice);
	}

	private void UpdateState(State _nextState)
	{
		stateTimer = 0f;
		stateImpulse = true;
		currentState = _nextState;
		if (GameManager.Multiplayer())
		{
			photonView.RPC("UpdateStateRPC", RpcTarget.Others, _nextState);
		}
	}

	private void PlayerLookAt()
	{
		Quaternion quaternion = Quaternion.Euler(0f, Quaternion.LookRotation(playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.Rigidbody.transform.position).eulerAngles.y, 0f);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, 50f * Time.deltaTime);
	}

	[PunRPC]
	private void UpdateStateRPC(State _nextState, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _nextState;
		}
	}

	[PunRPC]
	private void NoticeRPC(int _playerID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			enemyAnimalAnim.NoticeSet(_playerID);
		}
	}
}
