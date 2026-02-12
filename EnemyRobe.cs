using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRobe : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		TargetPlayer,
		LookUnderStart,
		LookUnder,
		LookUnderAttack,
		LookUnderStop,
		SeekPlayer,
		Attack,
		StuckAttack,
		Stun,
		Leave,
		Despawn
	}

	[Header("References")]
	public EnemyRobeAnim robeAnim;

	internal Enemy enemy;

	public State currentState;

	private bool stateImpulse;

	private float stateTimer;

	internal PlayerAvatar targetPlayer;

	private PhotonView photonView;

	private float roamWaitTimer;

	private Vector3 agentDestination;

	private float overrideAgentLerp;

	private Vector3 targetPosition;

	public Transform eyeLocation;

	internal bool isOnScreen;

	internal bool attackImpulse;

	[Header("Idle Break")]
	public float idleBreakTimeMin = 45f;

	public float idleBreakTimeMax = 90f;

	private float idleBreakTimer;

	internal bool idleBreakTrigger;

	[Space]
	public SpringQuaternion rotationSpring;

	private Quaternion rotationTarget;

	[Space]
	public SpringQuaternion endPieceSpring;

	public Transform endPieceSource;

	public Transform endPieceTarget;

	private float grabAggroTimer;

	private Vector3 lookUnderPositionNavmesh;

	private Vector3 lookUnderPosition;

	internal bool lookUnderAttackImpulse;

	private Vector3 stuckAttackTarget;

	private float chaseTime;

	private float chaseTimer;

	private Vector3 previousTargetNavmeshPosition;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
		idleBreakTimer = Random.Range(idleBreakTimeMin, idleBreakTimeMax);
	}

	private void Update()
	{
		EndPieceLogic();
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (idleBreakTimer >= 0f)
		{
			idleBreakTimer -= Time.deltaTime;
			if (idleBreakTimer <= 0f && CanIdleBreak())
			{
				IdleBreak();
				idleBreakTimer = Random.Range(idleBreakTimeMin, idleBreakTimeMax);
			}
		}
		RotationLogic();
		RigidbodyRotationSpeed();
		ChaseTimer();
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
		case State.Idle:
			StateIdle();
			break;
		case State.Roam:
			StateRoam();
			break;
		case State.Investigate:
			StateInvestigate();
			break;
		case State.TargetPlayer:
			MoveTowardPlayer();
			StateTargetPlayer();
			break;
		case State.LookUnderStart:
			StateLookUnderStart();
			break;
		case State.LookUnder:
			StateLookUnder();
			break;
		case State.LookUnderAttack:
			StateLookUnderAttack();
			break;
		case State.LookUnderStop:
			StateLookUnderStop();
			break;
		case State.SeekPlayer:
			StateSeekPlayer();
			break;
		case State.Spawn:
			StateSpawn();
			break;
		case State.Attack:
			StateAttack();
			break;
		case State.StuckAttack:
			StateStuckAttack();
			break;
		case State.Stun:
			StateStun();
			break;
		case State.Leave:
			StateLeave();
			break;
		case State.Despawn:
			StateDespawn();
			break;
		}
		if (currentState != State.TargetPlayer)
		{
			overrideAgentLerp = 0f;
		}
		if (currentState != State.TargetPlayer && isOnScreen)
		{
			isOnScreen = false;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateOnScreenRPC", RpcTarget.Others, isOnScreen);
			}
		}
		if (isOnScreen && (bool)targetPlayer && targetPlayer.isLocal)
		{
			SemiFunc.DoNotLookEffect(base.gameObject);
		}
	}

	private void StateSpawn()
	{
		if (stateImpulse)
		{
			stateTimer = 3f;
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
			stateImpulse = false;
			stateTimer = Random.Range(2f, 5f);
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
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
			stateImpulse = false;
			stateTimer = 5f;
			LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 5f, 15f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
			}
			if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				agentDestination = hit.position;
			}
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (enemy.Rigidbody.notMovingTimer > 1f)
		{
			stateTimer -= Time.deltaTime;
		}
		if (stateTimer <= 0f)
		{
			AttackNearestPhysObjectOrGoToIdle();
			return;
		}
		if (Vector3.Distance(base.transform.position, agentDestination) < 2f)
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
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f)
			{
				AttackNearestPhysObjectOrGoToIdle();
				return;
			}
			if (Vector3.Distance(base.transform.position, agentDestination) < 2f)
			{
				UpdateState(State.Idle);
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateTargetPlayer()
	{
		if (stateImpulse)
		{
			stateTimer = 2f;
			stateImpulse = false;
		}
		enemy.Rigidbody.OverrideFollowPosition(0.2f, 5f, 30f);
		if (Vector3.Distance(enemy.CenterTransform.position, targetPlayer.transform.position) < 2f)
		{
			UpdateState(State.Attack);
			return;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.SeekPlayer);
			return;
		}
		if (chaseTime >= 10f && stateTimer <= 1f)
		{
			UpdateState(State.Leave);
			return;
		}
		if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			enemy.Vision.DisableVision(2f);
			UpdateState(State.SeekPlayer);
		}
		if (SemiFunc.EnemyLookUnderCondition(enemy, stateTimer, 0.5f, targetPlayer))
		{
			UpdateState(State.LookUnderStart);
		}
	}

	private void StateLookUnderStart()
	{
		if (stateImpulse)
		{
			lookUnderPosition = targetPlayer.transform.position;
			lookUnderPositionNavmesh = targetPlayer.LastNavmeshPosition;
			stateTimer = 2f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.OverrideAgent(3f, 10f, 0.2f);
		enemy.Rigidbody.OverrideFollowPosition(0.2f, 3f);
		enemy.NavMeshAgent.SetDestination(lookUnderPositionNavmesh);
		if (Vector3.Distance(base.transform.position, lookUnderPositionNavmesh) < 1f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.LookUnder);
			}
		}
		else if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			UpdateState(State.SeekPlayer);
		}
	}

	private void StateLookUnder()
	{
		if (stateImpulse)
		{
			stateTimer = 5f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		enemy.Vision.StandOverride(0.25f);
		Vector3 vector = new Vector3(enemy.Rigidbody.transform.position.x, 0f, enemy.Rigidbody.transform.position.z);
		if (Vector3.Dot((new Vector3(targetPlayer.transform.position.x, 0f, targetPlayer.transform.position.z) - vector).normalized, enemy.Rigidbody.transform.forward) > 0.75f && Vector3.Distance(enemy.Rigidbody.transform.position, targetPlayer.transform.position) < 2.5f)
		{
			UpdateState(State.LookUnderAttack);
		}
		else if (stateTimer <= 0f)
		{
			UpdateState(State.LookUnderStop);
		}
	}

	private void StateLookUnderAttack()
	{
		if (stateImpulse)
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("LookUnderAttackImpulseRPC", RpcTarget.All);
			}
			else
			{
				LookUnderAttackImpulseRPC();
			}
			stateTimer = 2f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			if (targetPlayer.isDisabled)
			{
				UpdateState(State.LookUnderStop);
			}
			else
			{
				UpdateState(State.LookUnder);
			}
		}
	}

	private void StateLookUnderStop()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.SeekPlayer);
		}
	}

	private void StateSeekPlayer()
	{
		if (stateImpulse)
		{
			stateTimer = 20f;
			stateImpulse = false;
			LevelPoint levelPointAhead = enemy.GetLevelPointAhead(targetPosition);
			if ((bool)levelPointAhead)
			{
				targetPosition = levelPointAhead.transform.position;
			}
			enemy.Rigidbody.notMovingTimer = 0f;
		}
		enemy.NavMeshAgent.OverrideAgent(3f, 3f, 0.2f);
		enemy.Rigidbody.OverrideFollowPosition(0.2f, 3f);
		if (Vector3.Distance(base.transform.position, targetPosition) < 2f)
		{
			LevelPoint levelPointAhead2 = enemy.GetLevelPointAhead(targetPosition);
			if ((bool)levelPointAhead2)
			{
				targetPosition = levelPointAhead2.transform.position;
			}
		}
		if (enemy.Rigidbody.notMovingTimer >= 3f)
		{
			AttackNearestPhysObjectOrGoToIdle();
			return;
		}
		enemy.NavMeshAgent.SetDestination(targetPosition);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f || enemy.Rigidbody.notMovingTimer > 3f)
		{
			UpdateState(State.Roam);
		}
	}

	private void StateAttack()
	{
		if (stateImpulse)
		{
			attackImpulse = true;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("AttackImpulseRPC", RpcTarget.Others);
			}
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			stateTimer = 2f;
			stateImpulse = false;
		}
		else
		{
			enemy.NavMeshAgent.Stop(0.2f);
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.SeekPlayer);
			}
		}
	}

	private void StateStuckAttack()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			stateTimer = 1.5f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.Stop(0.2f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Attack);
		}
	}

	private void StateStun()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			stateImpulse = false;
		}
		if (!enemy.IsStunned())
		{
			UpdateState(State.Idle);
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
				agentDestination = hit.position;
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			SemiFunc.EnemyLeaveStart(enemy);
			stateImpulse = false;
		}
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (Vector3.Distance(base.transform.position, agentDestination) < 1f || stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			stateImpulse = false;
		}
	}

	private void IdleBreak()
	{
		if (!GameManager.Multiplayer())
		{
			IdleBreakRPC();
		}
		else
		{
			photonView.RPC("IdleBreakRPC", RpcTarget.All);
		}
	}

	internal void UpdateState(State _state)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState != _state)
		{
			currentState = _state;
			stateImpulse = true;
			stateTimer = 0f;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateStateRPC", RpcTarget.All, currentState);
			}
			else
			{
				UpdateStateRPC(currentState);
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
		robeAnim.sfxHurt.Play(robeAnim.transform.position);
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.Leave)
		{
			UpdateState(State.Idle);
		}
	}

	public void OnDeath()
	{
		robeAnim.DeathParticlesImpulse();
		robeAnim.SfxDeath();
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.05f);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnVision()
	{
		if (enemy.CurrentState == EnemyState.Despawn)
		{
			return;
		}
		if (currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate || currentState == State.SeekPlayer || currentState == State.Leave)
		{
			targetPlayer = enemy.Vision.onVisionTriggeredPlayer;
			UpdateState(State.TargetPlayer);
			if (GameManager.Multiplayer())
			{
				photonView.RPC("TargetPlayerRPC", RpcTarget.All, targetPlayer.photonView.ViewID);
			}
		}
		else if (currentState == State.TargetPlayer)
		{
			if (targetPlayer == enemy.Vision.onVisionTriggeredPlayer)
			{
				stateTimer = Mathf.Max(stateTimer, 1f);
			}
		}
		else if (currentState == State.LookUnderStart)
		{
			if (targetPlayer == enemy.Vision.onVisionTriggeredPlayer && !targetPlayer.isCrawling)
			{
				UpdateState(State.TargetPlayer);
			}
		}
		else if (currentState == State.LookUnder && targetPlayer == enemy.Vision.onVisionTriggeredPlayer)
		{
			if (targetPlayer.isCrawling)
			{
				lookUnderPosition = targetPlayer.transform.position;
			}
			else
			{
				UpdateState(State.LookUnderStop);
			}
		}
	}

	public void OnInvestigate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate)
			{
				agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
				UpdateState(State.Investigate);
			}
			else if (currentState == State.SeekPlayer)
			{
				targetPosition = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			}
		}
	}

	public void OnGrabbed()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !(grabAggroTimer > 0f) && currentState == State.Leave)
		{
			grabAggroTimer = 60f;
			targetPlayer = enemy.Rigidbody.onGrabbedPlayerAvatar;
			UpdateState(State.TargetPlayer);
			if (GameManager.Multiplayer())
			{
				photonView.RPC("TargetPlayerRPC", RpcTarget.All, targetPlayer.photonView.ViewID);
			}
		}
	}

	private bool CanIdleBreak()
	{
		if (currentState != State.Idle && currentState != State.Investigate)
		{
			return currentState == State.Roam;
		}
		return true;
	}

	private void MoveTowardPlayer()
	{
		bool flag = false;
		if (enemy.OnScreen.GetOnScreen(targetPlayer))
		{
			flag = true;
			overrideAgentLerp += Time.deltaTime / 4f;
		}
		else
		{
			overrideAgentLerp -= Time.deltaTime / 0.01f;
		}
		if (flag != isOnScreen)
		{
			isOnScreen = flag;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateOnScreenRPC", RpcTarget.Others, isOnScreen);
			}
		}
		overrideAgentLerp = Mathf.Clamp(overrideAgentLerp, 0f, 1f);
		float num = 25f;
		float num2 = 25f;
		float speed = Mathf.Lerp(enemy.NavMeshAgent.DefaultSpeed, num, overrideAgentLerp);
		float speed2 = Mathf.Lerp(enemy.Rigidbody.positionSpeedChase, num2, overrideAgentLerp);
		enemy.NavMeshAgent.OverrideAgent(speed, enemy.NavMeshAgent.DefaultAcceleration, 0.2f);
		enemy.Rigidbody.OverrideFollowPosition(1f, speed2);
		targetPosition = targetPlayer.transform.position;
		enemy.NavMeshAgent.SetDestination(targetPosition);
	}

	private void RotationLogic()
	{
		if (currentState == State.StuckAttack)
		{
			if (Vector3.Distance(stuckAttackTarget, enemy.Rigidbody.transform.position) > 0.1f)
			{
				rotationTarget = Quaternion.LookRotation(stuckAttackTarget - enemy.Rigidbody.transform.position);
				rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
			}
		}
		else if (currentState == State.LookUnderStart || currentState == State.LookUnder || currentState == State.LookUnderAttack)
		{
			if (Vector3.Distance(lookUnderPosition, base.transform.position) > 0.1f)
			{
				rotationTarget = Quaternion.LookRotation(lookUnderPosition - base.transform.position);
				rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
			}
		}
		else if (currentState == State.TargetPlayer || currentState == State.Attack)
		{
			if ((bool)targetPlayer && Vector3.Distance(targetPlayer.transform.position, base.transform.position) > 0.1f)
			{
				rotationTarget = Quaternion.LookRotation(targetPlayer.transform.position - base.transform.position);
				rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
			}
		}
		else if (enemy.NavMeshAgent.AgentVelocity.normalized.magnitude > 0.1f)
		{
			rotationTarget = Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized);
			rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(rotationSpring, rotationTarget);
	}

	private void EndPieceLogic()
	{
		endPieceSource.rotation = SemiFunc.SpringQuaternionGet(endPieceSpring, endPieceTarget.rotation);
		endPieceTarget.localEulerAngles = new Vector3((0f - enemy.Rigidbody.physGrabObject.rbVelocity.y) * 30f, 0f, 0f);
	}

	private void AttackNearestPhysObjectOrGoToIdle()
	{
		stuckAttackTarget = Vector3.zero;
		if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			stuckAttackTarget = SemiFunc.EnemyGetNearestPhysObject(enemy);
		}
		if (stuckAttackTarget != Vector3.zero)
		{
			UpdateState(State.StuckAttack);
		}
		else
		{
			UpdateState(State.Idle);
		}
	}

	private void RigidbodyRotationSpeed()
	{
		if (currentState == State.Roam)
		{
			enemy.Rigidbody.rotationSpeedIdle = 1f;
			enemy.Rigidbody.rotationSpeedChase = 1f;
		}
		else
		{
			enemy.Rigidbody.rotationSpeedIdle = 2f;
			enemy.Rigidbody.rotationSpeedChase = 2f;
		}
	}

	private void ChaseTimer()
	{
		if (currentState == State.TargetPlayer)
		{
			chaseTimer = 3f;
		}
		if (chaseTimer > 0f)
		{
			if (previousTargetNavmeshPosition != targetPlayer.LastNavmeshPosition)
			{
				previousTargetNavmeshPosition = targetPlayer.LastNavmeshPosition;
				chaseTime = 0f;
			}
			chaseTime += Time.deltaTime;
			chaseTimer -= Time.deltaTime;
		}
		else
		{
			chaseTime = 0f;
		}
	}

	[PunRPC]
	private void UpdateStateRPC(State _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
			stateImpulse = true;
			if (currentState == State.Spawn)
			{
				robeAnim.SetSpawn();
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
	private void UpdateOnScreenRPC(bool _onScreen, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			isOnScreen = _onScreen;
		}
	}

	[PunRPC]
	private void AttackImpulseRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			attackImpulse = true;
		}
	}

	[PunRPC]
	private void LookUnderAttackImpulseRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			lookUnderAttackImpulse = true;
		}
	}

	[PunRPC]
	private void IdleBreakRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			idleBreakTrigger = true;
		}
	}
}
