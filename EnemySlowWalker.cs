using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemySlowWalker : MonoBehaviour, IPunObservable
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		Notice,
		GoToPlayer,
		Sneak,
		Attack,
		StuckAttack,
		LookUnderStart,
		LookUnderIntro,
		LookUnder,
		LookUnderAttack,
		LookUnderStop,
		Stun,
		Leave,
		Despawn
	}

	public State currentState;

	public float stateTimer;

	public EnemySlowWalkerAnim animator;

	public ParticleSystem particleDeathImpact;

	public ParticleSystem particleDeathBitsFar;

	public ParticleSystem particleDeathBitsShort;

	public ParticleSystem particleDeathSmoke;

	public SpringQuaternion horizontalRotationSpring;

	private Quaternion rotationTarget;

	private bool stateImpulse = true;

	internal PlayerAvatar targetPlayer;

	public Enemy enemy;

	private PhotonView photonView;

	private Vector3 agentDestination;

	private Vector3 backToNavMeshPosition;

	private Vector3 stuckAttackTarget;

	private Vector3 targetPosition;

	private float visionTimer;

	private bool visionPrevious;

	public Transform feetTransform;

	private float grabAggroTimer;

	private int attackCount;

	private Vector3 lookUnderPosition;

	private Vector3 lookUnderLookAtPosition;

	private Vector3 lookUnderPositionNavmesh;

	internal int lookUnderAttackCount;

	public Transform lookAtTransform;

	private float visionDotStandingDefault;

	internal bool attackOffsetActive;

	public Transform attackOffsetTransform;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		visionDotStandingDefault = enemy.Vision.VisionDotStanding;
	}

	private void Update()
	{
		HeadLookAt();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (enemy.CurrentState == EnemyState.Despawn && !enemy.IsStunned())
			{
				UpdateState(State.Despawn);
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
				break;
			case State.Roam:
				StateRoam();
				break;
			case State.Investigate:
				StateInvestigate();
				break;
			case State.Notice:
				StateNotice();
				break;
			case State.GoToPlayer:
				StateGoToPlayer();
				break;
			case State.Sneak:
				StateSneak();
				break;
			case State.Attack:
				StateAttack();
				break;
			case State.StuckAttack:
				StateStuckAttack();
				break;
			case State.LookUnderStart:
				StateLookUnderStart();
				break;
			case State.LookUnderIntro:
				StateLookUnderIntro();
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
			RotationLogic();
			TimerLogic();
			VisionDotLogic();
			AttackOffsetLogic();
		}
	}

	public void StateSpawn()
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

	public void StateIdle()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
			enemy.NavMeshAgent.Warp(feetTransform.position);
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

	public void StateRoam()
	{
		if (stateImpulse)
		{
			stateTimer = 5f;
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 10f, 25f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
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
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
		}
		else
		{
			enemy.NavMeshAgent.SetDestination(agentDestination);
			if (enemy.Rigidbody.notMovingTimer > 3f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f)
			{
				AttackNearestPhysObjectOrGoToIdle();
				return;
			}
			if (Vector3.Distance(base.transform.position, agentDestination) < 1f)
			{
				UpdateState(State.Idle);
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	public void StateInvestigate()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			enemy.Rigidbody.notMovingTimer = 0f;
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
			if (Vector3.Distance(base.transform.position, agentDestination) < 1f)
			{
				UpdateState(State.Idle);
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	public void StateNotice()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
			enemy.NavMeshAgent.Warp(feetTransform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.GoToPlayer);
		}
	}

	public void StateGoToPlayer()
	{
		if (!targetPlayer)
		{
			UpdateState(State.Idle);
			return;
		}
		if (stateImpulse)
		{
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
			stateTimer = 10f;
		}
		enemy.NavMeshAgent.OverrideAgent(0.8f, 30f, 0.2f);
		targetPosition = targetPlayer.transform.position;
		enemy.NavMeshAgent.SetDestination(targetPosition);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
		else if (Vector3.Distance(feetTransform.position, enemy.NavMeshAgent.GetPoint()) < 8f && stateTimer > 1.5f && enemy.Jump.timeSinceJumped > 1f)
		{
			UpdateState(State.Attack);
		}
		else if (SemiFunc.EnemyLookUnderCondition(enemy, stateTimer, 9f, targetPlayer))
		{
			UpdateState(State.LookUnderStart);
		}
		else if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			AttackNearestPhysObjectOrGoToIdle();
		}
	}

	public void StateSneak()
	{
		if (!targetPlayer)
		{
			UpdateState(State.Idle);
			return;
		}
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			enemy.Rigidbody.notMovingTimer = 0f;
			enemy.NavMeshAgent.Warp(feetTransform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		enemy.NavMeshAgent.OverrideAgent(0.8f, 30f, 0.2f);
		targetPosition = targetPlayer.transform.position;
		enemy.NavMeshAgent.SetDestination(targetPosition);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
		else if (Vector3.Distance(feetTransform.position, enemy.NavMeshAgent.GetPoint()) < 5f)
		{
			UpdateState(State.GoToPlayer);
		}
		else if (enemy.OnScreen.OnScreenAny)
		{
			UpdateState(State.Notice);
		}
	}

	public void StateAttack()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			attackCount++;
			enemy.NavMeshAgent.Warp(feetTransform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	public void StateStuckAttack()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(feetTransform.position);
			stateTimer = 3f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.Stop(0.2f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	public void StateLookUnderStart()
	{
		if (!targetPlayer)
		{
			UpdateState(State.Idle);
			return;
		}
		if (stateImpulse)
		{
			lookUnderPosition = targetPlayer.transform.position;
			lookUnderLookAtPosition = lookUnderPosition;
			lookUnderPositionNavmesh = targetPlayer.LastNavmeshPosition;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateTimer = 1f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.SetDestination(lookUnderPositionNavmesh);
		if (Vector3.Distance(base.transform.position, lookUnderPositionNavmesh) < 0.5f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.LookUnderIntro);
			}
		}
		else if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			UpdateState(State.Idle);
		}
	}

	public void StateLookUnderIntro()
	{
		if (stateImpulse)
		{
			lookUnderAttackCount = 0;
			stateImpulse = false;
			stateTimer = 1f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.LookUnder);
		}
	}

	public void StateLookUnder()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 3f;
			return;
		}
		stateTimer -= Time.deltaTime;
		enemy.Vision.StandOverride(0.25f);
		int num = 10;
		if (stateTimer < 2.75f && targetPlayer.isCrawling && lookUnderAttackCount < num && Vector3.Distance(enemy.Rigidbody.transform.position, targetPlayer.transform.position) < 3f && Vector3.Dot(lookAtTransform.forward, targetPlayer.transform.position - lookAtTransform.position) > 0.5f)
		{
			UpdateState(State.LookUnderAttack);
		}
		else if (stateTimer <= 0f || lookUnderAttackCount >= num)
		{
			UpdateState(State.LookUnderStop);
		}
	}

	public void StateLookUnderAttack()
	{
		if (stateImpulse)
		{
			lookUnderAttackCount++;
			stateTimer = 0.6f;
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

	public void StateLookUnderStop()
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

	public void StateStun()
	{
		if (stateImpulse)
		{
			if (!enemy.Rigidbody.grabbed)
			{
				enemy.Rigidbody.rb.AddTorque(-base.transform.right * 15f, ForceMode.Impulse);
			}
			stateImpulse = false;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = enemy.Rigidbody.transform.position;
		if (!enemy.IsStunned())
		{
			UpdateState(State.Idle);
		}
	}

	public void StateLeave()
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
		if (enemy.Rigidbody.notMovingTimer > 3f)
		{
			stateTimer -= Time.deltaTime;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (Vector3.Distance(base.transform.position, agentDestination) < 1f || stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	public void StateDespawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			enemy.NavMeshAgent.Warp(feetTransform.position);
			enemy.NavMeshAgent.ResetPath();
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
		animator.sfxHurt.Play(animator.transform.position);
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.Leave)
		{
			UpdateState(State.Idle);
		}
	}

	public void OnDeath()
	{
		particleDeathImpact.transform.position = enemy.CenterTransform.position;
		particleDeathImpact.Play();
		particleDeathBitsFar.transform.position = enemy.CenterTransform.position;
		particleDeathBitsFar.Play();
		particleDeathBitsShort.transform.position = enemy.CenterTransform.position;
		particleDeathBitsShort.Play();
		particleDeathSmoke.transform.position = enemy.CenterTransform.position;
		particleDeathSmoke.Play();
		animator.sfxDeath.Play(animator.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, base.transform.position, 0.05f);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnInvestigate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && (currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate))
		{
			agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			UpdateState(State.Investigate);
		}
	}

	public void OnVision()
	{
		if (enemy.CurrentState == EnemyState.Despawn)
		{
			return;
		}
		if (currentState == State.Roam || currentState == State.Idle || currentState == State.Investigate || currentState == State.Leave)
		{
			targetPlayer = enemy.Vision.onVisionTriggeredPlayer;
			if (!enemy.OnScreen.OnScreenAny)
			{
				UpdateState(State.Sneak);
			}
			else
			{
				UpdateState(State.Notice);
			}
			if (GameManager.Multiplayer())
			{
				photonView.RPC("TargetPlayerRPC", RpcTarget.All, targetPlayer.photonView.ViewID);
			}
		}
		else if (currentState == State.GoToPlayer || currentState == State.Sneak)
		{
			if (targetPlayer == enemy.Vision.onVisionTriggeredPlayer)
			{
				stateTimer = 10f;
			}
		}
		else if (currentState == State.LookUnderStart)
		{
			if (targetPlayer == enemy.Vision.onVisionTriggeredPlayer && !targetPlayer.isCrawling)
			{
				UpdateState(State.GoToPlayer);
			}
		}
		else if ((currentState == State.LookUnder || currentState == State.LookUnderIntro || currentState == State.LookUnderAttack) && targetPlayer == enemy.Vision.onVisionTriggeredPlayer)
		{
			if (targetPlayer.isCrawling)
			{
				lookUnderLookAtPosition = targetPlayer.transform.position;
			}
			else
			{
				UpdateState(State.LookUnderStop);
			}
		}
	}

	public void OnGrabbed()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !(grabAggroTimer > 0f) && currentState == State.Leave)
		{
			grabAggroTimer = 60f;
			PlayerAvatar onGrabbedPlayerAvatar = enemy.Rigidbody.onGrabbedPlayerAvatar;
			if (!(onGrabbedPlayerAvatar.transform.position.y - enemy.transform.position.y > 1.15f) && !(onGrabbedPlayerAvatar.transform.position.y - enemy.transform.position.y < -1f))
			{
				targetPlayer = onGrabbedPlayerAvatar;
				UpdateState(State.Notice);
			}
		}
	}

	public void OnLookUnderAttackHurtPlayer()
	{
		if (GameManager.Multiplayer())
		{
			photonView.RPC("LookUnderAttackCountResetRPC", RpcTarget.MasterClient);
		}
		else
		{
			LookUnderAttackCountResetRPC();
		}
	}

	private void UpdateState(State _state)
	{
		if (currentState != _state)
		{
			enemy.Rigidbody.notMovingTimer = 0f;
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

	private void RotationLogic()
	{
		if (currentState == State.Notice)
		{
			if ((bool)targetPlayer && Vector3.Distance(targetPlayer.transform.position, enemy.Rigidbody.transform.position) > 0.1f)
			{
				rotationTarget = Quaternion.LookRotation(targetPlayer.transform.position - enemy.Rigidbody.transform.position);
				rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
			}
		}
		else if (currentState == State.LookUnderStart || currentState == State.LookUnderIntro || currentState == State.LookUnder || currentState == State.LookUnderAttack)
		{
			if (Vector3.Distance(lookUnderPosition, base.transform.position) > 0.1f)
			{
				rotationTarget = Quaternion.LookRotation(lookUnderPosition - base.transform.position);
				rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
			}
		}
		else if (enemy.NavMeshAgent.AgentVelocity.normalized.magnitude > 0.1f)
		{
			rotationTarget = Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized);
			rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
		}
		if (currentState == State.Attack)
		{
			if ((bool)targetPlayer && Vector3.Distance(targetPlayer.transform.position, enemy.Rigidbody.transform.position) > 0.1f && stateTimer > 2.5f)
			{
				rotationTarget = Quaternion.LookRotation(targetPlayer.transform.position - enemy.Rigidbody.transform.position);
				rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
			}
			horizontalRotationSpring.speed = 15f;
			horizontalRotationSpring.damping = 0.8f;
		}
		else
		{
			horizontalRotationSpring.speed = 5f;
			horizontalRotationSpring.damping = 0.7f;
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(horizontalRotationSpring, rotationTarget);
	}

	private void HeadLookAt()
	{
		if (currentState == State.LookUnder || currentState == State.LookUnderAttack)
		{
			Vector3 direction = lookUnderLookAtPosition - lookAtTransform.position;
			direction = SemiFunc.ClampDirection(direction, animator.transform.forward, 60f);
			Quaternion localRotation = lookAtTransform.localRotation;
			lookAtTransform.rotation = Quaternion.LookRotation(direction);
			Quaternion localRotation2 = lookAtTransform.localRotation;
			localRotation2.eulerAngles = new Vector3(0f, localRotation2.eulerAngles.y, 0f);
			lookAtTransform.localRotation = Quaternion.Lerp(localRotation, localRotation2, Time.deltaTime * 10f);
			enemy.Vision.VisionTransform.rotation = lookAtTransform.rotation;
		}
		else
		{
			lookAtTransform.localRotation = Quaternion.Lerp(lookAtTransform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
			enemy.Vision.VisionTransform.localRotation = Quaternion.identity;
		}
		animator.SpringLogic();
	}

	private void VisionDotLogic()
	{
		if (currentState == State.LookUnder)
		{
			enemy.Vision.VisionDotStanding = 0f;
		}
		else
		{
			enemy.Vision.VisionDotStanding = visionDotStandingDefault;
		}
	}

	private void TimerLogic()
	{
		visionTimer -= Time.deltaTime;
	}

	private void AttackOffsetLogic()
	{
		if (currentState != State.Attack)
		{
			attackOffsetActive = false;
		}
		if (attackOffsetActive)
		{
			attackOffsetTransform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(attackOffsetTransform.localPosition.z, 1.5f, Time.deltaTime * 4f));
			enemy.Rigidbody.OverrideFollowPosition(0.2f, 5f, 40f);
		}
		else
		{
			attackOffsetTransform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(attackOffsetTransform.localPosition.z, 0f, Time.deltaTime * 1f));
		}
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

	[PunRPC]
	private void UpdateStateRPC(State _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
			if (currentState == State.Spawn)
			{
				animator.OnSpawn();
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
	private void LookUnderAttackCountResetRPC()
	{
		lookUnderAttackCount = 0;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(lookUnderLookAtPosition);
			}
			else
			{
				lookUnderLookAtPosition = (Vector3)stream.ReceiveNext();
			}
		}
	}
}
