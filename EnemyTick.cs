using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTick : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		Notice,
		Tumble,
		Bite,
		TumbleEnd,
		MoveBackToNavmesh,
		RunAway,
		RunAwayIdle,
		Leave,
		Stun,
		Despawn
	}

	public Enemy enemy;

	private PlayerAvatar playerTarget;

	private Transform lastSeenGrabbedObject;

	private PhotonView photonView;

	public EnemyTickAnim anim;

	public GameObject healAura;

	[Header("State stuff")]
	public State currentState;

	private bool stateImpulse;

	public float stateTimer;

	private float stateTicker;

	private Vector3 targetPosition;

	private Vector3 agentDestination;

	private Vector3 backToNavmeshPosition;

	[Space]
	private Quaternion rotationTarget;

	public SpringQuaternion rotationSpring;

	[Space]
	public GrabForce grabForceZero;

	public GrabForce grabForceUngrabbable;

	public GameObject forceGrabPoint;

	private float suckTimer;

	private bool instantSuck = true;

	private float suckFrequency = 1f;

	private int healthPerSuck = 10;

	private int stolenHealth;

	[Space]
	[Header("Colliders")]
	public GameObject flatCollider;

	public SphereCollider bloatCollider;

	private bool bloatColliderActive;

	internal int syncedHealth = 10;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		if (currentState == State.Bite && (bool)playerTarget && !playerTarget.isDisabled && playerTarget.isLocal)
		{
			playerTarget.physGrabber.OverrideColorToGreen();
			playerTarget.physGrabber.OverrideGrab(enemy.Rigidbody.physGrabObject, 0.2f, _grabRelease: true);
		}
		if ((GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient) || !LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (enemy.IsStunned())
		{
			UpdateState(State.Stun);
		}
		else if (enemy.CurrentState == EnemyState.Despawn)
		{
			UpdateState(State.Despawn);
		}
		if ((!playerTarget || playerTarget.isDisabled) && (currentState == State.Bite || currentState == State.Tumble || currentState == State.Notice))
		{
			UpdateState(State.MoveBackToNavmesh);
		}
		if (enemy.Rigidbody.physGrabObject.playerGrabbing.Count > 0 && instantSuck && !anim.hasBeenFull && currentState != State.Bite)
		{
			foreach (PhysGrabber item in enemy.Rigidbody.physGrabObject.playerGrabbing)
			{
				if (!item.playerAvatar)
				{
					continue;
				}
				if (playerTarget != item.playerAvatar)
				{
					playerTarget = item.playerAvatar;
					if (SemiFunc.IsMultiplayer())
					{
						photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, playerTarget.photonView.ViewID);
					}
				}
				break;
			}
			UpdateState(State.Bite);
			instantSuck = false;
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(rotationSpring, rotationTarget);
		if (bloatColliderActive)
		{
			enemy.Rigidbody.OverrideFollowRotation(0.25f, 5f);
		}
		BackToNavmeshPosition();
		TimerLogic();
		if ((double)syncedHealth >= (double)anim.maxHealth * 0.2 && !bloatColliderActive && SemiFunc.FPSImpulse5())
		{
			bool flag = false;
			Collider[] array = Physics.OverlapSphere(bloatCollider.transform.position, bloatCollider.radius, SemiFunc.LayerMaskGetVisionObstruct());
			foreach (Collider collider in array)
			{
				if (!collider.gameObject.CompareTag("Phys Grab Object") || !(collider.GetComponentInParent<PhysGrabObject>() == enemy.Rigidbody.physGrabObject))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				ToggleBloatCollider(_active: true);
			}
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
		case State.Tumble:
			StateTumble();
			break;
		case State.Bite:
			StateBite();
			break;
		case State.TumbleEnd:
			StateTumbleEnd();
			break;
		case State.MoveBackToNavmesh:
			StateMoveBackToNavMesh();
			break;
		case State.Stun:
			StateStun();
			break;
		case State.RunAway:
			StateRunAway();
			break;
		case State.RunAwayIdle:
			RunAwayIdle();
			break;
		case State.Leave:
			StateLeave();
			break;
		case State.Despawn:
			StateDespawn();
			break;
		}
	}

	private void StateSpawn()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			forceGrabPoint.SetActive(value: true);
			enemy.Rigidbody.hasShakeRelease = false;
			enemy.Health.spawnValuable = false;
			enemy.Rigidbody.grabForceNeeded = grabForceUngrabbable;
			stolenHealth = 0;
			instantSuck = true;
			UpdateSyncedHealth(enemy.Health.healthCurrent);
			UpdateMassScale();
			ToggleBloatCollider(_active: false);
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
			stateImpulse = false;
			stateTimer = Random.Range(2f, 5f);
			instantSuck = true;
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
			LeaveCheck(_setLeave: true);
		}
	}

	private void StateRoam()
	{
		RotationLogic();
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			if (!SemiFunc.EnemyRoamPoint(enemy, out agentDestination))
			{
				return;
			}
			enemy.NavMeshAgent.SetDestination(agentDestination);
			enemy.Rigidbody.notMovingTimer = 0f;
		}
		else
		{
			SemiFunc.EnemyCartJump(enemy);
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f || Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetPoint()) < 1.5f)
			{
				SemiFunc.EnemyCartJumpReset(enemy);
				UpdateState(State.Idle);
				return;
			}
		}
		LeaveCheck(_setLeave: true);
	}

	private void StateInvestigate()
	{
		RotationLogic();
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
			if (stateTimer <= 0f || Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetPoint()) < 1.5f)
			{
				SemiFunc.EnemyCartJumpReset(enemy);
				UpdateState(State.Idle);
				return;
			}
		}
		LeaveCheck(_setLeave: true);
	}

	private void StateNotice()
	{
		RotationLogic();
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			stateImpulse = false;
			stateTimer = 1f;
		}
		stateTimer -= Time.deltaTime;
		if (!(stateTimer <= 0f))
		{
			return;
		}
		if (playerTarget.physGrabber.physGrabBeamActive)
		{
			if ((bool)playerTarget.physGrabber.grabbedObject)
			{
				lastSeenGrabbedObject = playerTarget.physGrabber.grabbedObject.transform;
			}
			else
			{
				lastSeenGrabbedObject = playerTarget.playerTransform.transform;
			}
			UpdateState(State.Tumble);
		}
		else if (anim.hasBeenFull)
		{
			UpdateState(State.RunAwayIdle);
		}
		else
		{
			UpdateState(State.Idle);
		}
	}

	private void StateTumble()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 0.25f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			Vector3 normalized = Vector3.Lerp(lastSeenGrabbedObject.position - enemy.Rigidbody.transform.position, Vector3.up, 0.6f).normalized;
			enemy.Rigidbody.rb.AddForce(normalized * 10f, ForceMode.Impulse);
			enemy.Rigidbody.rb.AddTorque(enemy.Rigidbody.transform.right * -3f, ForceMode.Impulse);
			lastSeenGrabbedObject = null;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		enemy.Rigidbody.DisableFollowPosition(0.2f, 10f);
		enemy.Rigidbody.DisableFollowRotation(0.2f, 10f);
		if (enemy.Rigidbody.rb.velocity.magnitude < 1f)
		{
			stateTimer -= Time.deltaTime;
		}
		base.transform.position = enemy.Rigidbody.transform.position;
		if (playerTarget.physGrabber.physGrabBeamActive)
		{
			if ((bool)playerTarget.physGrabber.grabbedObject)
			{
				lastSeenGrabbedObject = playerTarget.physGrabber.grabbedObject.transform;
			}
			else
			{
				lastSeenGrabbedObject = playerTarget.playerTransform.transform;
			}
			if (Vector3.Distance(enemy.Rigidbody.transform.position, lastSeenGrabbedObject.position) < 2f)
			{
				UpdateState(State.Bite);
				lastSeenGrabbedObject = null;
				return;
			}
		}
		if (stateTimer <= 0f)
		{
			UpdateState(State.TumbleEnd);
		}
	}

	private void StateBite()
	{
		RotationLogic();
		if (stateImpulse)
		{
			suckTimer = 0f;
			stateImpulse = false;
			stateTimer = 0.5f;
		}
		PlayerGrabRotateLogic();
		enemy.Rigidbody.physGrabObject.OverrideAngularDrag(0f, 0.2f);
		enemy.Health.NonStunHurtOverride(0.2f);
		playerTarget.physGrabber.OverrideOverchargeDisable(0.2f);
		playerTarget.physGrabber.OverrideBeamColor(new Color(0.002349734f, 1f, 0f, 0.1568628f), 0.2f);
		enemy.NavMeshAgent.Disable(0.2f);
		enemy.Rigidbody.DisableFollowPosition(0.2f, 10f);
		enemy.Rigidbody.DisableFollowRotation(0.2f, 10f);
		if (suckTimer > 0f)
		{
			suckTimer -= Time.deltaTime / suckFrequency;
		}
		else
		{
			suckTimer = 0f;
		}
		if (suckTimer <= 0f)
		{
			Suck();
			suckTimer = 1f;
		}
		bool flag = false;
		if (stateTimer <= 0f)
		{
			foreach (PhysGrabber item in enemy.Rigidbody.physGrabObject.playerGrabbing)
			{
				if (item.playerAvatar == playerTarget)
				{
					flag = true;
					break;
				}
			}
		}
		else
		{
			stateTimer -= Time.deltaTime;
			flag = true;
		}
		if (enemy.Health.healthCurrent >= anim.maxHealth || !playerTarget.physGrabber.physGrabBeamActive || !flag)
		{
			UpdateState(State.TumbleEnd);
		}
	}

	private void StateTumbleEnd()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
			instantSuck = true;
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.MoveBackToNavmesh);
		}
	}

	private void StateMoveBackToNavMesh()
	{
		RotationLogic();
		if (stateImpulse)
		{
			stateTimer = 30f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		if (!enemy.Jump.jumping)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, backToNavmeshPosition, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
		}
		SemiFunc.EnemyCartJump(enemy);
		enemy.Vision.StandOverride(0.25f);
		if ((Vector3.Distance(base.transform.position, enemy.Rigidbody.transform.position) > 2f || enemy.Rigidbody.notMovingTimer > 2f) && !enemy.Jump.jumping)
		{
			Vector3 normalized = (backToNavmeshPosition - enemy.Rigidbody.transform.position).normalized;
			enemy.Jump.StuckTrigger(normalized);
			base.transform.position = enemy.Rigidbody.transform.position;
			base.transform.position += normalized * 2f;
		}
		stateTimer -= Time.deltaTime;
		if (Vector3.Distance(enemy.Rigidbody.transform.position, backToNavmeshPosition) <= 0f || NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out var _, 0.5f, -1))
		{
			if (anim.hasBeenFull)
			{
				forceGrabPoint.SetActive(value: false);
				enemy.Rigidbody.hasShakeRelease = true;
				enemy.Rigidbody.grabForceNeeded = grabForceZero;
				UpdateState(State.RunAwayIdle);
			}
			else
			{
				UpdateState(State.Idle);
			}
		}
		else if (stateTimer <= 0f)
		{
			enemy.EnemyParent.SpawnedTimerSet(0f);
			UpdateState(State.Despawn);
		}
	}

	private void StateStun()
	{
		playerTarget = null;
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = enemy.Rigidbody.transform.position;
		if (!enemy.IsStunned())
		{
			UpdateState(State.MoveBackToNavmesh);
		}
	}

	private void StateRunAway()
	{
		enemy.NavMeshAgent.OverrideAgent(4f, 12f, 0.25f);
		RotationLogic();
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 5f;
			playerTarget = null;
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGetPlayerDistance(base.transform.position, 8f, 50f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
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
				UpdateState(State.RunAwayIdle);
			}
		}
		LeaveCheck(_setLeave: true);
	}

	private void RunAwayIdle()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 10f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		if ((bool)playerTarget)
		{
			UpdateState(State.RunAway);
			return;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Leave);
		}
		else
		{
			LeaveCheck(_setLeave: true);
		}
	}

	private void StateLeave()
	{
		RotationLogic();
		if (stateImpulse)
		{
			stateTimer = 5f;
			if (SemiFunc.EnemyLeavePoint(enemy, out agentDestination))
			{
				enemy.NavMeshAgent.SetDestination(agentDestination);
				enemy.Rigidbody.notMovingTimer = 0f;
				stateImpulse = false;
				SemiFunc.EnemyLeaveStart(enemy);
			}
			return;
		}
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		SemiFunc.EnemyCartJump(enemy);
		if (Vector3.Distance(base.transform.position, agentDestination) < 1f || stateTimer <= 0f)
		{
			SemiFunc.EnemyCartJumpReset(enemy);
			if (anim.hasBeenFull)
			{
				UpdateState(State.RunAwayIdle);
			}
			else
			{
				UpdateState(State.Idle);
			}
		}
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			stateImpulse = false;
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
		if ((currentState != State.Idle && currentState != State.Roam && currentState != State.Investigate && currentState != State.Leave && currentState != State.RunAway && currentState != State.RunAwayIdle) || enemy.Jump.jumping || !SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (playerTarget != enemy.Vision.onVisionTriggeredPlayer)
		{
			playerTarget = enemy.Vision.onVisionTriggeredPlayer;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.Others, playerTarget.photonView.ViewID);
			}
		}
		if (enemy.Vision.onVisionTriggeredPlayer.physGrabber.physGrabBeamActive)
		{
			UpdateState(State.Notice);
		}
	}

	public void OnHurt()
	{
		anim.hurtSound.Play(base.transform.position);
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		UpdateSyncedHealth(enemy.Health.healthCurrent);
		UpdateMassScale();
		if (currentState == State.Leave)
		{
			if (anim.hasBeenFull)
			{
				UpdateState(State.RunAwayIdle);
			}
			else
			{
				UpdateState(State.Idle);
			}
		}
	}

	public void OnDeath()
	{
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.05f);
		anim.particleImpact.Play();
		anim.particleBits.Play();
		anim.particleDirectionalBits.transform.rotation = Quaternion.LookRotation(-enemy.Health.hurtDirection.normalized);
		anim.particleDirectionalBits.Play();
		anim.deathSound.Play(base.transform.position);
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (stolenHealth > 0)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				GameObject healAuraInstance = Object.Instantiate(healAura, enemy.CenterTransform.position, Quaternion.identity);
				UpdateHealAuraHealthPool(healAuraInstance, stolenHealth);
			}
			else
			{
				GameObject healAuraInstance2 = PhotonNetwork.Instantiate("Enemies/" + healAura.name, enemy.CenterTransform.position, Quaternion.identity, 0);
				UpdateHealAuraHealthPool(healAuraInstance2, stolenHealth);
			}
		}
		enemy.EnemyParent.Despawn();
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
			else
			{
				UpdateStateRPC(currentState);
			}
		}
	}

	private void UpdateHealAuraHealthPool(GameObject _healAuraInstance, int _healthPool)
	{
		_healAuraInstance.GetComponent<EnemyTickHealAura>().healthPool = _healthPool;
		stolenHealth = 0;
	}

	private void UpdateSyncedHealth(int _currentSyncedHealth)
	{
		int num = Mathf.FloorToInt((float)_currentSyncedHealth / 10f) * 10;
		if (syncedHealth != num)
		{
			syncedHealth = num;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateSyncedHealthRPC", RpcTarget.All, syncedHealth);
			}
			else
			{
				UpdateSyncedHealthRPC(syncedHealth);
			}
		}
	}

	public void OnSpawn()
	{
		anim.hasBeenFull = false;
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
	}

	private void RotationLogic()
	{
		if (currentState == State.Tumble || currentState == State.Notice)
		{
			rotationTarget = Quaternion.LookRotation(playerTarget.transform.position - enemy.Rigidbody.transform.position);
			rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
		}
		else if (currentState == State.Bite)
		{
			rotationTarget = Quaternion.LookRotation(playerTarget.transform.position - enemy.Rigidbody.transform.position);
			rotationTarget.eulerAngles = new Vector3(rotationTarget.eulerAngles.x, rotationTarget.eulerAngles.y, rotationTarget.eulerAngles.z);
		}
		else if (enemy.Rigidbody.velocity.magnitude > 0.1f)
		{
			rotationTarget = Quaternion.LookRotation(enemy.Rigidbody.velocity.normalized);
			rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
		}
		else
		{
			rotationTarget = Quaternion.LookRotation(enemy.Rigidbody.transform.forward);
			rotationTarget.eulerAngles = new Vector3(0f, rotationTarget.eulerAngles.y, 0f);
		}
	}

	private void BackToNavmeshPosition()
	{
		if (SemiFunc.FPSImpulse15() && enemy.NavMeshAgent.OnNavmesh(enemy.Rigidbody.transform.position, 0.5f, _checkPit: true))
		{
			backToNavmeshPosition = enemy.Rigidbody.transform.position;
		}
	}

	private void Suck()
	{
		int num = anim.maxHealth - enemy.Health.healthCurrent;
		if (num > 0)
		{
			enemy.Rigidbody.rb.AddTorque(enemy.Rigidbody.transform.right * 3f, ForceMode.Impulse);
			int num2 = Mathf.Min(healthPerSuck, num);
			playerTarget.playerHealth.HurtOther(num2, playerTarget.transform.position, savingGrace: false, SemiFunc.EnemyGetIndex(enemy));
			enemy.Health.Heal(num2);
			UpdateSyncedHealth(enemy.Health.healthCurrent);
			UpdateMassScale();
			stolenHealth += num2;
		}
	}

	private void PlayerGrabRotateLogic()
	{
		if (enemy.Rigidbody.physGrabObject.playerGrabbing.Count <= 0)
		{
			return;
		}
		Quaternion turnX = Quaternion.Euler(0f, 0f, 0f);
		Quaternion turnY = Quaternion.Euler(0f, 180f, 0f);
		Quaternion turnZ = Quaternion.Euler(0f, 0f, 0f);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = true;
		foreach (PhysGrabber item in enemy.Rigidbody.physGrabObject.playerGrabbing)
		{
			if (flag4)
			{
				if (item.playerAvatar.isCrouching)
				{
					flag2 = true;
				}
				if (item.playerAvatar.isCrawling)
				{
					flag3 = true;
				}
				flag4 = false;
			}
			if (item.isRotating)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			enemy.Rigidbody.physGrabObject.TurnXYZ(turnX, turnY, turnZ);
		}
		float num = 0.4f;
		if (flag2)
		{
			num += 0.5f;
		}
		if (flag3)
		{
			num -= 0.5f;
		}
		enemy.Rigidbody.physGrabObject.OverrideGrabVerticalPosition(num);
	}

	private void TimerLogic()
	{
	}

	private void UpdateMassScale()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !anim.hasBeenFull)
		{
			float value = Mathf.Clamp01((float)syncedHealth / (float)anim.maxHealth);
			float num = 0.5f;
			float num2 = 1.25f;
			float t = Mathf.InverseLerp(0.1f, 1f, value);
			float num3 = Mathf.Lerp(num, num2, t);
			enemy.Rigidbody.physGrabObject.OverrideMass(num3);
			enemy.Rigidbody.physGrabObject.massOriginal = num3;
		}
	}

	private bool LeaveCheck(bool _setLeave)
	{
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			if (_setLeave)
			{
				UpdateState(State.Leave);
			}
			return true;
		}
		return false;
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
	private void UpdateSyncedHealthRPC(int _currentSyncedHealth, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			syncedHealth = _currentSyncedHealth;
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

	private void ToggleBloatCollider(bool _active)
	{
		if (bloatColliderActive != _active)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdateBloatColliderRPC", RpcTarget.All, _active);
			}
			else
			{
				UpdateBloatColliderRPC(_active);
			}
		}
	}

	[PunRPC]
	private void UpdateBloatColliderRPC(bool _active, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			bloatColliderActive = _active;
			bloatCollider.enabled = _active;
		}
	}
}
