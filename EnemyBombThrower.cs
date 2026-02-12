using Photon.Pun;
using UnityEngine;

public class EnemyBombThrower : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		Notice,
		GotoPlayer,
		BackAwayPlayer,
		BackAwayHead,
		Attack,
		Melee,
		Stun,
		Leave,
		Despawn
	}

	private enum RotationState
	{
		LookVelocitySlow,
		LookVelocityFast,
		LookTarget,
		LookTargetDirectly,
		LookHead
	}

	internal float rotationStopTimer;

	private float rotationUpdatedLastTime;

	internal State currentState;

	private float stateTimer;

	private bool stateImpulse;

	internal State previousState;

	internal Enemy enemy;

	internal PhotonView photonView;

	internal PlayerAvatar playerTarget;

	private Vector3 agentDestination;

	private bool visionPrevious;

	private float visionPreviousTime;

	private bool navmeshPrevious;

	private float navmeshPreviousTime;

	internal bool headGrown = true;

	private float headCooldown;

	private float headAttackCooldown;

	private float meleeTimer;

	private float meleeCooldown;

	private float torsoHeadBreakerCooldown;

	private int torsoHeadBreakerPrevious;

	public EnemyBombThrowerAnim anim;

	public EnemyBombThrowerHead head;

	public SphereCollider headCollider;

	public Transform headSpawnTransform;

	public SpringQuaternion horizontalRotationSpring;

	private Quaternion horizontalRotationTarget = Quaternion.identity;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		if ((!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient) && LevelGenerator.Instance.Generated)
		{
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
			case State.Notice:
				StateNotice();
				break;
			case State.GotoPlayer:
				StateGotoPlayer();
				break;
			case State.BackAwayPlayer:
				StateBackAwayPlayer();
				break;
			case State.BackAwayHead:
				StateBackAwayHead();
				break;
			case State.Attack:
				StateAttack();
				break;
			case State.Melee:
				StateMelee();
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
			HeadLogic();
			MeleeLogic();
			TorsoHeadBreakerLogic();
		}
	}

	private void StateSpawn()
	{
		if (stateImpulse)
		{
			AgentReset();
			RigidbodyReset();
			stateImpulse = false;
			stateTimer = 2f;
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
			AgentReset();
			RigidbodyReset();
			stateImpulse = false;
			stateTimer = Random.Range(2f, 8f);
		}
		if (!SemiFunc.EnemySpawnIdlePause())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.Roam);
			}
			else if (SemiFunc.EnemyForceLeave(enemy))
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
			if (!SemiFunc.EnemyRoamPoint(enemy, out agentDestination))
			{
				return;
			}
			AgentReset();
			RigidbodyReset();
			stateImpulse = false;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		RotationStateSet(RotationState.LookVelocitySlow);
		RigidbodyNotMovingTickTimer();
		if (!GoToIdleOnDestinationReached() && SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateInvestigate()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			RigidbodyReset();
			AgentReset();
		}
		else
		{
			RotationStateSet(RotationState.LookVelocitySlow);
			enemy.NavMeshAgent.SetDestination(agentDestination);
			RigidbodyNotMovingTickTimer();
			if (GoToIdleOnDestinationReached())
			{
				return;
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateNotice()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			AgentReset();
			RigidbodyReset();
			stateImpulse = false;
		}
		RotationStateSet(RotationState.LookTargetDirectly);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.GotoPlayer);
		}
	}

	private void StateGotoPlayer()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			AgentReset();
			RigidbodyReset();
		}
		if (VisionBlocked())
		{
			stateTimer -= Time.deltaTime;
		}
		if (!playerTarget || playerTarget.isDisabled || stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
		else if (!AttackSetLogic() && !MeleeSetLogic() && !enemy.Jump.jumpingDelay && !enemy.Jump.jumping)
		{
			if (VisionBlocked() || Vector3.Distance(playerTarget.transform.position, base.transform.position) > 6f)
			{
				enemy.NavMeshAgent.SetDestination(playerTarget.transform.position);
				AgentMoveFast();
				RotationStateSet(RotationState.LookVelocityFast);
			}
			else if (!VisionBlocked() && Vector3.Distance(playerTarget.transform.position, base.transform.position) < 4f)
			{
				UpdateState(State.BackAwayPlayer);
			}
			else
			{
				AgentReset();
				RotationStateSet(RotationState.LookTarget);
			}
		}
	}

	private void StateBackAwayPlayer()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
			AgentReset();
			RigidbodyReset();
		}
		if (VisionBlocked())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.GotoPlayer);
				return;
			}
		}
		if (!playerTarget || playerTarget.isDisabled)
		{
			UpdateState(State.Idle);
		}
		else if (!VisionBlocked() && Vector3.Distance(playerTarget.transform.position, base.transform.position) >= 5f)
		{
			UpdateState(State.GotoPlayer);
		}
		else if (!AttackSetLogic() && !MeleeSetLogic())
		{
			RotationStateSet(RotationState.LookTarget);
			BackAwayLogic(playerTarget.transform.position);
			if (SemiFunc.EnemyForceLeave(enemy))
			{
				UpdateState(State.Leave);
			}
		}
	}

	private void StateBackAwayHead()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 3f;
			AgentReset();
			RigidbodyReset();
		}
		stateTimer -= Time.deltaTime;
		if (head.currentState == EnemyBombThrowerHead.State.Disabled || stateTimer <= 0f)
		{
			if ((bool)playerTarget && !playerTarget.isDisabled)
			{
				UpdateState(State.GotoPlayer);
			}
			else
			{
				UpdateState(State.Idle);
			}
		}
		else if (Vector3.Distance(head.transform.position, base.transform.position) < 6f)
		{
			RotationStateSet(RotationState.LookHead);
			BackAwayLogic(playerTarget.transform.position);
		}
	}

	private void StateAttack()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			AgentReset();
			RigidbodyReset();
			stateImpulse = false;
		}
		RotationStateSet(RotationState.LookTargetDirectly);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.BackAwayHead);
		}
	}

	private void StateMelee()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			AgentReset();
			RigidbodyReset();
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer > 1f)
		{
			RotationStateSet(RotationState.LookTargetDirectly);
		}
		else if (stateTimer <= 0f)
		{
			UpdateState(State.BackAwayHead);
		}
	}

	private void StateStun()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			enemy.Rigidbody.rb.AddTorque(-enemy.Rigidbody.transform.right * 20f, ForceMode.Impulse);
			RigidbodyReset();
			AgentReset();
		}
		enemy.Vision.DisableVision(1f);
		if (!enemy.IsStunned())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
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
			if (!SemiFunc.EnemyLeavePoint(enemy, out agentDestination))
			{
				return;
			}
			SemiFunc.EnemyLeaveStart(enemy);
			RigidbodyReset();
			stateImpulse = false;
		}
		RigidbodyNotMovingTickTimer();
		RotationStateSet(RotationState.LookVelocityFast);
		enemy.NavMeshAgent.SetDestination(agentDestination);
		AgentMoveFast();
		GoToIdleOnDestinationReached();
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			AgentReset();
			RigidbodyReset();
			stateImpulse = false;
		}
	}

	private void UpdateState(State _state)
	{
		if (_state != currentState)
		{
			previousState = currentState;
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

	private void RotationStateSet(RotationState _state)
	{
		Quaternion quaternion = horizontalRotationTarget;
		if (rotationStopTimer > 0f)
		{
			return;
		}
		if (_state == RotationState.LookTarget && !playerTarget)
		{
			_state = RotationState.LookVelocityFast;
		}
		if (_state == RotationState.LookVelocitySlow)
		{
			horizontalRotationSpring.speed = 5f;
			horizontalRotationSpring.damping = 0.7f;
		}
		else
		{
			horizontalRotationSpring.speed = 8f;
			horizontalRotationSpring.damping = 0.7f;
		}
		switch (_state)
		{
		case RotationState.LookVelocitySlow:
		case RotationState.LookVelocityFast:
			if (enemy.Rigidbody.velocity.magnitude >= 1f)
			{
				horizontalRotationTarget = Quaternion.LookRotation(enemy.Rigidbody.velocity.normalized);
			}
			break;
		case RotationState.LookTarget:
		case RotationState.LookTargetDirectly:
			if ((bool)playerTarget && Vector3.Distance(playerTarget.transform.position, enemy.Rigidbody.transform.position) >= 0.2f)
			{
				Quaternion quaternion3 = Quaternion.LookRotation(playerTarget.transform.position - enemy.Rigidbody.transform.position);
				if (_state == RotationState.LookTargetDirectly || Quaternion.Angle(quaternion3, horizontalRotationTarget) >= 60f || Time.time - rotationUpdatedLastTime > 2f)
				{
					horizontalRotationTarget = quaternion3;
				}
			}
			break;
		case RotationState.LookHead:
			if (Vector3.Distance(head.transform.position, enemy.Rigidbody.transform.position) >= 0.2f)
			{
				Quaternion quaternion2 = Quaternion.LookRotation(head.transform.position - enemy.Rigidbody.transform.position);
				if (Quaternion.Angle(quaternion2, horizontalRotationTarget) >= 60f || Time.time - rotationUpdatedLastTime > 2f)
				{
					horizontalRotationTarget = quaternion2;
				}
			}
			break;
		}
		if (quaternion != horizontalRotationTarget)
		{
			rotationUpdatedLastTime = Time.time;
		}
		horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
	}

	private void RotationLogic()
	{
		if (rotationStopTimer > 0f)
		{
			rotationStopTimer -= Time.deltaTime;
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(horizontalRotationSpring, horizontalRotationTarget);
	}

	private void AgentReset()
	{
		enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
		enemy.NavMeshAgent.ResetPath();
	}

	private void AgentMoveFast()
	{
		enemy.NavMeshAgent.OverrideAgent(2f, 15f, 0.1f);
	}

	private void RigidbodyReset()
	{
		enemy.Rigidbody.StuckReset();
		enemy.Jump.StuckReset();
		SemiFunc.EnemyCartJumpReset(enemy);
	}

	private void RigidbodyNotMovingTickTimer()
	{
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
	}

	private bool GoToIdleOnDestinationReached()
	{
		if (!enemy.Jump.jumping && (stateTimer <= 0f || Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) <= 1f || Vector3.Distance(enemy.Rigidbody.transform.position, agentDestination) <= 1f))
		{
			UpdateState(State.Idle);
			return true;
		}
		return false;
	}

	private void UpdatePlayerTarget(PlayerAvatar _player)
	{
		playerTarget = _player;
		int num = -1;
		if ((bool)playerTarget)
		{
			num = playerTarget.photonView.ViewID;
		}
		if (GameManager.Multiplayer())
		{
			photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.Others, num);
		}
	}

	internal bool VisionBlocked()
	{
		if (!playerTarget)
		{
			return true;
		}
		if (Time.time - visionPreviousTime > 0.2f && (bool)playerTarget)
		{
			Vector3 position = playerTarget.PlayerVisionTarget.VisionTransform.position;
			visionPreviousTime = Time.time;
			Vector3 direction = position - enemy.Vision.VisionTransform.position;
			visionPrevious = Physics.Raycast(enemy.Vision.VisionTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
		}
		return visionPrevious;
	}

	private bool AttackSetLogic()
	{
		if (headAttackCooldown > 0f)
		{
			return false;
		}
		if (!enemy.Jump.jumpingDelay && !enemy.Jump.jumping && !enemy.Jump.landDelay && headGrown && anim.headGrownVisually && !VisionBlocked() && (enemy.Rigidbody.notMovingTimer > 3f || (Vector3.Distance(playerTarget.transform.position, base.transform.position) <= 6f && Vector3.Distance(playerTarget.transform.position, base.transform.position) >= 4f)))
		{
			UpdateState(State.Attack);
			return true;
		}
		return false;
	}

	private bool MeleeSetLogic()
	{
		if (meleeTimer >= 1f)
		{
			meleeCooldown = 2f;
			UpdateState(State.Melee);
			return true;
		}
		return false;
	}

	private void BackAwayLogic(Vector3 _target)
	{
		enemy.NavMeshAgent.Disable(0.1f);
		Vector3 normalized = (base.transform.position - _target).normalized;
		Vector3 vector = base.transform.position + new Vector3(normalized.x, 0f, normalized.z);
		if (NavmeshCheck(vector))
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, vector, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
			if (Vector3.Distance(base.transform.position, enemy.Rigidbody.transform.position) > 3f)
			{
				base.transform.position = new Vector3(enemy.Rigidbody.transform.position.x, base.transform.position.y, enemy.Rigidbody.transform.position.z);
			}
		}
		else
		{
			base.transform.position = new Vector3(enemy.Rigidbody.transform.position.x, base.transform.position.y, enemy.Rigidbody.transform.position.z);
		}
	}

	private void HeadLogic()
	{
		if (!headGrown && head.currentState == EnemyBombThrowerHead.State.Disabled)
		{
			headCooldown -= Time.deltaTime;
			if (headCooldown <= 0f && SemiFunc.FPSImpulse5())
			{
				bool flag = false;
				Collider[] array = Physics.OverlapSphere(headCollider.transform.position, headCollider.radius, SemiFunc.LayerMaskGetVisionObstruct());
				foreach (Collider collider in array)
				{
					if (!collider.gameObject.CompareTag("Phys Grab Object") || !(collider.GetComponentInParent<PhysGrabObject>() == enemy.Rigidbody.physGrabObject))
					{
						flag = true;
					}
				}
				if (!flag)
				{
					HeadGrownSet(_state: true);
				}
			}
		}
		if (anim.headGrownVisually)
		{
			if (headAttackCooldown > 0f)
			{
				headAttackCooldown -= Time.deltaTime;
			}
		}
		else
		{
			headAttackCooldown = 0.5f;
		}
	}

	private void MeleeLogic()
	{
		if (meleeCooldown > 0f)
		{
			meleeCooldown -= Time.deltaTime;
			meleeTimer = 0f;
		}
		else if (!enemy.Jump.jumpingDelay && !enemy.Jump.jumping && !enemy.Jump.landDelay && !VisionBlocked() && currentState != State.Attack && currentState != State.Melee && Vector3.Distance(playerTarget.transform.position, base.transform.position) <= 2.5f)
		{
			meleeTimer += Time.deltaTime;
		}
		else
		{
			meleeTimer = 0f;
		}
	}

	private void TorsoHeadBreakerLogic()
	{
		if (!enemy.EnemyParent.playerClose)
		{
			return;
		}
		if (torsoHeadBreakerCooldown <= 0f)
		{
			if (Random.Range(0, 100) <= 25)
			{
				torsoHeadBreakerCooldown = 1f;
			}
			else
			{
				torsoHeadBreakerCooldown = Random.Range(5f, 20f);
			}
			int num;
			for (num = torsoHeadBreakerPrevious; num == torsoHeadBreakerPrevious; num = Random.Range(0, 12))
			{
			}
			torsoHeadBreakerPrevious = num;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("TorsoHeadBreakerRPC", RpcTarget.All, num);
			}
			else
			{
				TorsoHeadBreakerRPC(num);
			}
		}
		else
		{
			torsoHeadBreakerCooldown -= Time.deltaTime;
		}
	}

	public void HeadGrownSet(bool _state)
	{
		if (GameManager.Multiplayer())
		{
			photonView.RPC("HeadGrownSetRPC", RpcTarget.All, _state);
		}
		else
		{
			HeadGrownSetRPC(_state);
		}
	}

	private bool NavmeshCheck(Vector3 _position)
	{
		if (Time.time - navmeshPreviousTime > 0.2f)
		{
			navmeshPreviousTime = Time.time;
			navmeshPrevious = enemy.NavMeshAgent.OnNavmesh(_position, 1f, _checkPit: true);
		}
		return navmeshPrevious;
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

	[PunRPC]
	private void HeadGrownSetRPC(bool _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			headGrown = _state;
			if (headGrown)
			{
				headCollider.gameObject.SetActive(value: true);
			}
			else
			{
				headCollider.gameObject.SetActive(value: false);
			}
			if (SemiFunc.IsMasterClientOrSingleplayer() && !headGrown)
			{
				head.UpdateState(EnemyBombThrowerHead.State.Spawn);
				headCooldown = 1f;
			}
		}
	}

	[PunRPC]
	private void TorsoHeadBreakerRPC(int _index, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.TorsoHeadBreakerTrigger(_index);
		}
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
	}

	public void OnVision()
	{
		if (enemy.Jump.jumpingDelay || enemy.Jump.jumping || head.currentState != EnemyBombThrowerHead.State.Disabled)
		{
			return;
		}
		if (currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate || currentState == State.Leave)
		{
			UpdatePlayerTarget(enemy.Vision.onVisionTriggeredPlayer);
			if ((bool)playerTarget)
			{
				UpdateState(State.Notice);
			}
		}
		else if ((currentState == State.GotoPlayer || currentState == State.BackAwayPlayer) && playerTarget == enemy.Vision.onVisionTriggeredPlayer)
		{
			stateTimer = Mathf.Max(2f, stateTimer);
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

	public void OnHurt()
	{
		anim.EventHurt();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			meleeTimer = 1f;
			if (currentState == State.Leave)
			{
				UpdateState(State.Idle);
			}
		}
	}

	public void OnDeath()
	{
		anim.EventDeath();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (headGrown)
			{
				HeadGrownSet(_state: false);
			}
			enemy.EnemyParent.Despawn();
		}
	}
}
