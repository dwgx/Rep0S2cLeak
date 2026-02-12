using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHidden : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		PlayerNotice,
		PlayerGoTo,
		PlayerPickup,
		PlayerMove,
		PlayerRelease,
		PlayerReleaseWait,
		Leave,
		Stun,
		StunEnd,
		Despawn
	}

	[Space]
	public State currentState;

	private bool stateImpulse;

	private float stateTimer;

	[Space]
	public Enemy enemy;

	public EnemyHiddenAnim enemyHiddenAnim;

	private PhotonView photonView;

	[Space]
	public Transform playerPickupTransform;

	public AnimationCurve playerPickupCurveUp;

	public AnimationCurve playerPickupCurveSide;

	private float playerPickupLerpUp;

	private float playerPickupLerpSide;

	private Vector3 playerPickupPositionOriginal;

	[Space]
	public SpringQuaternion rotationSpring;

	private Quaternion rotationTarget;

	private Vector3 agentDestination;

	private PlayerAvatar playerTarget;

	private bool agentSet;

	private float grabAggroTimer;

	private float maxMoveTimer;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		playerPickupPositionOriginal = playerPickupTransform.localPosition;
	}

	private void Update()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (grabAggroTimer > 0f)
			{
				grabAggroTimer -= Time.deltaTime;
			}
			RotationLogic();
			PlayerPickupTransformLogic();
			if (enemy.IsStunned())
			{
				UpdateState(State.Stun);
			}
			if (enemy.CurrentState == EnemyState.Despawn && !enemy.IsStunned())
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
			case State.PlayerGoTo:
				StatePlayerGoTo();
				break;
			case State.PlayerPickup:
				StatePlayerPickup();
				break;
			case State.PlayerMove:
				StatePlayerMove();
				break;
			case State.PlayerRelease:
				StatePlayerRelease();
				break;
			case State.PlayerReleaseWait:
				StatePlayerReleaseWait();
				break;
			case State.Leave:
				StateLeave();
				break;
			case State.Stun:
				StateStun();
				break;
			case State.StunEnd:
				StateStunEnd();
				break;
			case State.Despawn:
				StateDespawn();
				break;
			}
		}
	}

	private void FixedUpdate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			PlayerTumbleLogic();
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
			stateImpulse = false;
			stateTimer = 5f;
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 10f, 25f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
			}
			if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				enemy.NavMeshAgent.SetDestination(hit.position);
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			enemy.Rigidbody.notMovingTimer = 0f;
		}
		else
		{
			SemiFunc.EnemyCartJump(enemy);
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f || !enemy.NavMeshAgent.HasPath())
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
			if (stateTimer <= 0f || !enemy.NavMeshAgent.HasPath())
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
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateImpulse = false;
			stateTimer = 2f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.PlayerGoTo);
		}
	}

	private void StatePlayerGoTo()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			agentSet = true;
		}
		stateTimer -= Time.deltaTime;
		if (!playerTarget || playerTarget.isDisabled || stateTimer <= 0f)
		{
			UpdateState(State.Leave);
			return;
		}
		SemiFunc.EnemyCartJump(enemy);
		if (enemy.Jump.jumping)
		{
			enemy.NavMeshAgent.Disable(0.5f);
			base.transform.position = Vector3.MoveTowards(base.transform.position, playerTarget.transform.position, 5f * Time.deltaTime);
			agentSet = true;
		}
		else if (!enemy.NavMeshAgent.IsDisabled())
		{
			if (!agentSet && enemy.NavMeshAgent.HasPath() && Vector3.Distance(enemy.Rigidbody.transform.position + Vector3.down * 0.75f, enemy.NavMeshAgent.GetDestination()) < 0.25f)
			{
				enemy.Jump.StuckTrigger(enemy.Rigidbody.transform.position - playerTarget.transform.position);
			}
			enemy.NavMeshAgent.SetDestination(playerTarget.transform.position);
			enemy.NavMeshAgent.OverrideAgent(5f, 10f, 0.25f);
			agentSet = false;
		}
		if (Vector3.Distance(enemy.Rigidbody.transform.position, playerTarget.transform.position) < 1.5f)
		{
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.PlayerPickup);
		}
	}

	private void StatePlayerPickup()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
		}
		if (!playerTarget || playerTarget.isDisabled)
		{
			UpdateState(State.Leave);
			return;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.PlayerMove);
		}
	}

	private void StatePlayerMove()
	{
		if (stateImpulse)
		{
			stateTimer = 5f;
			maxMoveTimer = 10f;
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGetPlayerDistance(base.transform.position, 50f, 999f);
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
				stateTimer = 0f;
			}
			stateImpulse = false;
		}
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		if (!playerTarget || playerTarget.isDisabled)
		{
			UpdateState(State.Leave);
			return;
		}
		SemiFunc.EnemyCartJump(enemy);
		enemy.NavMeshAgent.SetDestination(agentDestination);
		enemy.NavMeshAgent.OverrideAgent(5f, 10f, 0.25f);
		enemy.Jump.GapJumpOverride(0.1f, 20f, 20f);
		maxMoveTimer -= Time.deltaTime;
		if (!enemy.NavMeshAgent.HasPath() || Vector3.Distance(base.transform.position, agentDestination) < 1f || Vector3.Distance(enemy.Rigidbody.transform.position, playerTarget.transform.position) > 5f || stateTimer <= 0f || maxMoveTimer <= 0f)
		{
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.PlayerRelease);
		}
	}

	private void StatePlayerRelease()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
		}
		stateTimer -= Time.deltaTime;
		if (!playerTarget || playerTarget.isDisabled)
		{
			UpdateState(State.Leave);
		}
		else if (stateTimer <= 0f || Vector3.Distance(enemy.Rigidbody.transform.position, playerTarget.transform.position) > 5f)
		{
			UpdateState(State.PlayerReleaseWait);
		}
	}

	private void StatePlayerReleaseWait()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
		}
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
			SemiFunc.EnemyLeaveStart(enemy);
			if (!flag)
			{
				return;
			}
			stateImpulse = false;
			enemy.EnemyParent.SpawnedTimerSet(1f);
		}
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		enemy.NavMeshAgent.OverrideAgent(5f, 10f, 0.25f);
		SemiFunc.EnemyCartJump(enemy);
		if (Vector3.Distance(base.transform.position, agentDestination) < 1f || stateTimer <= 0f)
		{
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.Idle);
		}
	}

	private void StateStun()
	{
		if (!enemy.IsStunned())
		{
			UpdateState(State.StunEnd);
		}
	}

	private void StateStunEnd()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Leave);
		}
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			enemy.EnemyParent.Despawn();
			UpdateState(State.Spawn);
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
		enemyHiddenAnim.Hurt();
	}

	public void OnDeath()
	{
		enemyHiddenAnim.Death();
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
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate || currentState == State.Leave)
		{
			playerTarget = enemy.Vision.onVisionTriggeredPlayer;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, playerTarget.photonView.ViewID);
			}
			UpdateState(State.PlayerNotice);
		}
		else if (currentState == State.PlayerGoTo)
		{
			stateTimer = 2f;
		}
	}

	public void OnGrabbed()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !(grabAggroTimer > 0f) && currentState == State.Leave)
		{
			grabAggroTimer = 60f;
			playerTarget = enemy.Rigidbody.onGrabbedPlayerAvatar;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, playerTarget.photonView.ViewID);
			}
			UpdateState(State.PlayerNotice);
		}
	}

	private void UpdateState(State _state)
	{
		if (currentState != _state)
		{
			enemy.Rigidbody.StuckReset();
			currentState = _state;
			stateImpulse = true;
			stateTimer = 0f;
			if (currentState == State.Leave)
			{
				SemiFunc.EnemyLeaveStart(enemy);
			}
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
		if (currentState == State.PlayerNotice || currentState == State.PlayerGoTo)
		{
			if (Vector3.Distance(playerTarget.transform.position, base.transform.position) > 0.1f)
			{
				rotationTarget = Quaternion.LookRotation(playerTarget.transform.position - base.transform.position);
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

	private void PlayerTumbleLogic()
	{
		if ((currentState == State.PlayerPickup || currentState == State.PlayerMove || currentState == State.PlayerRelease) && (bool)playerTarget && !playerTarget.isDisabled)
		{
			if (!playerTarget.tumble.isTumbling)
			{
				playerTarget.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
			}
			playerTarget.tumble.TumbleOverrideTime(1f);
			playerTarget.FallDamageResetSet(0.1f);
			playerTarget.tumble.physGrabObject.OverrideMass(1f);
			playerTarget.tumble.physGrabObject.OverrideAngularDrag(2f);
			playerTarget.tumble.physGrabObject.OverrideDrag(1f);
			playerTarget.tumble.OverrideEnemyHurt(0.1f);
			float num = 1f;
			if (playerTarget.tumble.physGrabObject.playerGrabbing.Count > 0)
			{
				num = 0.5f;
			}
			else if (currentState == State.PlayerRelease || currentState == State.PlayerPickup)
			{
				num = 0.75f;
			}
			Vector3 vector = SemiFunc.PhysFollowPosition(playerTarget.tumble.transform.position, playerPickupTransform.position, playerTarget.tumble.rb.velocity, 10f * num);
			playerTarget.tumble.rb.AddForce(vector * (10f * Time.fixedDeltaTime * num), ForceMode.Impulse);
			Vector3 vector2 = SemiFunc.PhysFollowRotation(playerTarget.tumble.transform, playerPickupTransform.rotation, playerTarget.tumble.rb, 0.2f * num);
			playerTarget.tumble.rb.AddTorque(vector2 * (1f * Time.fixedDeltaTime * num), ForceMode.Impulse);
		}
	}

	private void PlayerPickupTransformLogic()
	{
		if (currentState == State.PlayerMove || currentState == State.PlayerPickup || currentState == State.PlayerRelease)
		{
			float value = (enemy.Rigidbody.velocity.magnitude + enemy.Rigidbody.rb.angularVelocity.magnitude) * 0.5f;
			value = Mathf.Clamp(value, 0f, 1f);
			float num = playerPickupCurveUp.Evaluate(playerPickupLerpUp) - 0.5f;
			float num2 = playerPickupCurveSide.Evaluate(playerPickupLerpSide) - 0.5f;
			playerPickupLerpUp += 2f * Time.deltaTime * value;
			if (playerPickupLerpUp > 1f)
			{
				playerPickupLerpUp -= 1f;
			}
			playerPickupLerpSide += 1f * Time.deltaTime * value;
			if (playerPickupLerpSide > 1f)
			{
				playerPickupLerpSide -= 1f;
			}
			playerPickupTransform.localPosition = Vector3.Lerp(playerPickupTransform.localPosition, new Vector3(playerPickupPositionOriginal.x + num2 * 0.2f, playerPickupPositionOriginal.y + num * 0.2f, playerPickupPositionOriginal.z), 50f * Time.deltaTime);
		}
		else
		{
			playerPickupLerpSide = 0f;
			playerPickupLerpUp = 0f;
			playerPickupTransform.localPosition = Vector3.Lerp(playerPickupTransform.localPosition, playerPickupPositionOriginal, 10f * Time.deltaTime);
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
	private void UpdatePlayerTargetRPC(int _photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item.photonView.ViewID == _photonViewID)
			{
				playerTarget = item;
				break;
			}
		}
	}
}
