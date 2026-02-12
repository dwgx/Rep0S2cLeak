using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyValuableThrower : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		PlayerNotice,
		GetValuable,
		GoToTarget,
		PickUpTarget,
		TargetPlayer,
		Throw,
		Leave
	}

	private PhotonView photonView;

	public State currentState;

	private bool stateImpulse;

	public float stateTimer;

	private Vector3 agentDestination;

	private int attacks;

	[Space]
	public EnemyValuableThrowerAnim anim;

	public Transform pickupTargetParent;

	public Transform pickupTarget;

	private Enemy enemy;

	private PlayerAvatar playerTarget;

	private PhysGrabObject valuableTarget;

	private Vector3 pickUpPosition;

	private float grabAggroTimer;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		enemy = GetComponent<Enemy>();
	}

	private void Update()
	{
		if (currentState == State.GetValuable || currentState == State.GoToTarget || currentState == State.PickUpTarget || currentState == State.TargetPlayer || currentState == State.Throw)
		{
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (Vector3.Distance(base.transform.position, player.transform.position) < 8f)
				{
					SemiFunc.PlayerEyesOverride(player, enemy.Vision.VisionTransform.position, 0.1f, base.gameObject);
				}
			}
		}
		if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (grabAggroTimer > 0f)
		{
			grabAggroTimer -= Time.deltaTime;
		}
		if (!enemy.IsStunned() && LevelGenerator.Instance.Generated)
		{
			switch (currentState)
			{
			case State.Spawn:
				StateSpawn();
				break;
			case State.Idle:
				StateIdle();
				AgentVelocityRotation();
				break;
			case State.Roam:
				StateRoam();
				AgentVelocityRotation();
				break;
			case State.Investigate:
				StateInvestigate();
				AgentVelocityRotation();
				break;
			case State.PlayerNotice:
				StatePlayerNotice();
				PlayerLookAt();
				break;
			case State.GetValuable:
				StateGetValuable();
				break;
			case State.GoToTarget:
				ValuableFailsafe();
				TargetFailsafe();
				AgentVelocityRotation();
				StateGoToTarget();
				break;
			case State.PickUpTarget:
				DropOnStun();
				TargetFailsafe();
				ValuableTargetFollow();
				StatePickUpTarget();
				break;
			case State.TargetPlayer:
				DropOnStun();
				TargetFailsafe();
				PlayerLookAt();
				ValuableTargetFollow();
				StateTargetPlayer();
				break;
			case State.Throw:
				DropOnStun();
				TargetFailsafe();
				PlayerLookAt();
				ValuableTargetFollow();
				StateThrow();
				break;
			case State.Leave:
				AgentVelocityRotation();
				StateLeave();
				break;
			}
			pickupTargetParent.position = enemy.Rigidbody.transform.position;
			Quaternion quaternion = Quaternion.Euler(0f, enemy.Rigidbody.transform.rotation.eulerAngles.y, 0f);
			pickupTargetParent.rotation = Quaternion.Slerp(pickupTargetParent.rotation, quaternion, 5f * Time.deltaTime);
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
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
			if ((stateTimer <= 0f || Vector3.Distance(enemy.Rigidbody.transform.position, agentDestination) < 2f) && !enemy.Jump.jumping)
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
			UpdateState(State.GetValuable);
		}
	}

	private void StateGetValuable()
	{
		if (stateImpulse)
		{
			valuableTarget = null;
			stateTimer = 0.2f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.ResetPath();
		enemy.NavMeshAgent.Stop(0.1f);
		stateTimer -= Time.deltaTime;
		if (!(stateTimer <= 0f))
		{
			return;
		}
		PhysGrabObject physGrabObject = null;
		PhysGrabObject physGrabObject2 = null;
		float num = 999f;
		float num2 = 999f;
		Collider[] array = Physics.OverlapSphere(playerTarget.transform.position, 10f, LayerMask.GetMask("PhysGrabObject"));
		for (int i = 0; i < array.Length; i++)
		{
			ValuableObject componentInParent = array[i].GetComponentInParent<ValuableObject>();
			if (!componentInParent || componentInParent.volumeType > ValuableVolume.Type.Big)
			{
				continue;
			}
			float num3 = Vector3.Distance(playerTarget.transform.position, componentInParent.transform.position);
			if (NavMesh.SamplePosition(componentInParent.transform.position, out var _, 1f, -1))
			{
				if (num3 < num2)
				{
					num2 = num3;
					physGrabObject2 = componentInParent.physGrabObject;
				}
			}
			else if (num3 < num)
			{
				num = num3;
				physGrabObject = componentInParent.physGrabObject;
			}
		}
		if ((bool)physGrabObject2)
		{
			valuableTarget = physGrabObject2;
		}
		else if ((bool)physGrabObject)
		{
			valuableTarget = physGrabObject;
		}
		if (!valuableTarget)
		{
			UpdateState(State.Leave);
		}
		else
		{
			UpdateState(State.GoToTarget);
		}
	}

	private void StateGoToTarget()
	{
		if (enemy.IsStunned() || !valuableTarget)
		{
			return;
		}
		enemy.NavMeshAgent.SetDestination(valuableTarget.transform.position);
		if (stateImpulse)
		{
			stateTimer = 5f;
			stateImpulse = false;
			return;
		}
		SemiFunc.EnemyCartJump(enemy);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, valuableTarget.transform.position) < 1.25f)
		{
			enemy.NavMeshAgent.ResetPath();
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.PickUpTarget);
		}
		else if (Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetDestination()) < 2f)
		{
			if (stateTimer <= 0f)
			{
				enemy.Jump.StuckReset();
				UpdateState(State.Leave);
			}
			else if (Vector3.Distance(enemy.Rigidbody.transform.position, valuableTarget.centerPoint) > 1.5f)
			{
				enemy.Jump.StuckTrigger(valuableTarget.transform.position - enemy.Rigidbody.transform.position);
				enemy.Rigidbody.DisableFollowPosition(1f, 10f);
			}
		}
		if (enemy.Rigidbody.notMovingTimer > 2f || Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) < 2f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.Leave);
			}
		}
	}

	private void StatePickUpTarget()
	{
		if (currentState != State.PickUpTarget)
		{
			return;
		}
		if (stateImpulse)
		{
			foreach (PhysGrabber item in valuableTarget.playerGrabbing.ToList())
			{
				if (!SemiFunc.IsMultiplayer())
				{
					item.ReleaseObject(photonView.ViewID, 0.5f);
					continue;
				}
				item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, 0.5f, valuableTarget.photonView.ViewID);
			}
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			Quaternion to = Quaternion.Euler(0f, Quaternion.LookRotation(pickUpPosition - enemy.Rigidbody.transform.position).eulerAngles.y, 0f);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, 180f * Time.deltaTime);
			pickUpPosition = valuableTarget.midPoint;
			stateTimer = 999f;
			stateImpulse = false;
		}
		if (stateTimer <= 0f)
		{
			UpdateState(State.TargetPlayer);
		}
	}

	private void StateTargetPlayer()
	{
		if (currentState != State.TargetPlayer)
		{
			return;
		}
		if (stateImpulse)
		{
			stateTimer = 10f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		Vector3 direction = playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.Rigidbody.transform.position;
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(enemy.Rigidbody.transform.position, direction, out hitInfo, direction.magnitude, SemiFunc.LayerMaskGetVisionObstruct());
		if (flag && (hitInfo.transform.CompareTag("Player") || (bool)hitInfo.transform.GetComponent<PlayerTumble>()))
		{
			flag = false;
		}
		if (!flag && Vector3.Distance(base.transform.position, playerTarget.transform.position) < 3f)
		{
			enemy.NavMeshAgent.SetDestination(base.transform.position - base.transform.forward * 3f);
		}
		else if (flag || Vector3.Distance(base.transform.position, playerTarget.transform.position) > 5f)
		{
			enemy.NavMeshAgent.SetDestination(playerTarget.transform.position);
		}
		else
		{
			enemy.NavMeshAgent.ResetPath();
			if (stateTimer <= 8f)
			{
				UpdateState(State.Throw);
			}
		}
		if (stateTimer <= 0f)
		{
			UpdateState(State.Throw);
		}
	}

	private void StateThrow()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			attacks++;
			stateTimer = 3f;
			stateImpulse = false;
		}
		if (!valuableTarget)
		{
			stateTimer = Mathf.Clamp(stateTimer, stateTimer, 1f);
		}
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
				UpdateState(State.GetValuable);
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
			stateTimer = 10f;
			stateImpulse = false;
			SemiFunc.EnemyLeaveStart(enemy);
		}
		else
		{
			enemy.NavMeshAgent.SetDestination(agentDestination);
			SemiFunc.EnemyCartJump(enemy);
			if (enemy.Rigidbody.notMovingTimer > 2f)
			{
				stateTimer -= Time.deltaTime;
			}
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
		if (anim.isActiveAndEnabled)
		{
			anim.OnSpawn();
		}
	}

	public void OnHurt()
	{
		anim.hurtSound.Play(anim.transform.position);
	}

	public void OnDeath()
	{
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		anim.particleImpact.Play();
		anim.particleBits.Play();
		anim.particleDirectionalBits.transform.rotation = Quaternion.LookRotation(-enemy.Health.hurtDirection.normalized);
		anim.particleDirectionalBits.Play();
		anim.deathSound.Play(anim.transform.position);
		enemy.EnemyParent.Despawn();
	}

	public void OnVisionTriggered()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || (currentState != State.Idle && currentState != State.Roam && currentState != State.Investigate && currentState != State.Leave))
		{
			return;
		}
		if (playerTarget != enemy.Vision.onVisionTriggeredPlayer)
		{
			playerTarget = enemy.Vision.onVisionTriggeredPlayer;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.Others, playerTarget.photonView.ViewID);
			}
		}
		if (!enemy.IsStunned())
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("NoticeRPC", RpcTarget.All, enemy.Vision.onVisionTriggeredID);
			}
			else
			{
				anim.NoticeSet(enemy.Vision.onVisionTriggeredID);
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
		if (playerTarget != enemy.Rigidbody.onGrabbedPlayerAvatar)
		{
			playerTarget = enemy.Rigidbody.onGrabbedPlayerAvatar;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.Others, playerTarget.photonView.ViewID);
			}
		}
		UpdateState(State.PlayerNotice);
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
	}

	private void UpdateState(State _state)
	{
		currentState = _state;
		stateImpulse = true;
		stateTimer = 0f;
		if (GameManager.Multiplayer())
		{
			photonView.RPC("UpdateStateRPC", RpcTarget.All, currentState);
		}
	}

	private void ValuableTargetFollow()
	{
		if (!valuableTarget)
		{
			return;
		}
		if (Vector3.Distance(valuableTarget.transform.position, pickupTarget.position) > 2f)
		{
			valuableTarget = null;
			UpdateState(State.Leave);
			return;
		}
		Vector3 midPoint = valuableTarget.midPoint;
		midPoint.y = valuableTarget.transform.position.y;
		Vector3 targetPosition = pickupTarget.position;
		valuableTarget.OverrideZeroGravity();
		valuableTarget.OverrideMass(0.5f);
		valuableTarget.OverrideIndestructible();
		valuableTarget.OverrideBreakEffects(0.1f);
		if (Mathf.Abs(midPoint.y - targetPosition.y) > 0.25f)
		{
			Vector3 vector = enemy.Rigidbody.transform.position + enemy.Rigidbody.transform.forward;
			targetPosition = new Vector3(vector.x, targetPosition.y, vector.z);
		}
		Vector3 vector2 = SemiFunc.PhysFollowPosition(midPoint, targetPosition, valuableTarget.rb.velocity, 5f);
		valuableTarget.rb.AddForce(vector2 * (5f * Time.fixedDeltaTime), ForceMode.Impulse);
		Vector3 vector3 = SemiFunc.PhysFollowRotation(valuableTarget.transform, pickupTarget.rotation, valuableTarget.rb, 0.5f);
		valuableTarget.rb.AddTorque(vector3 * (5f * Time.fixedDeltaTime), ForceMode.Impulse);
	}

	private void AgentVelocityRotation()
	{
		if (enemy.NavMeshAgent.AgentVelocity.magnitude > 0.05f)
		{
			Quaternion quaternion = Quaternion.Euler(0f, Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized).eulerAngles.y, 0f);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, 5f * Time.deltaTime);
		}
	}

	private void PlayerLookAt()
	{
		Quaternion quaternion = Quaternion.Euler(0f, Quaternion.LookRotation(playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.Rigidbody.transform.position).eulerAngles.y, 0f);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, 50f * Time.deltaTime);
	}

	private void ValuableFailsafe()
	{
		if (!valuableTarget)
		{
			UpdateState(State.GetValuable);
		}
	}

	private void TargetFailsafe()
	{
		if (!playerTarget || playerTarget.isDisabled)
		{
			UpdateState(State.Leave);
		}
	}

	private void DropOnStun()
	{
		if (enemy.IsStunned())
		{
			UpdateState(State.GoToTarget);
		}
	}

	public void ResetStateTimer()
	{
		stateTimer = 0f;
	}

	public void Throw()
	{
		if (!valuableTarget || !playerTarget)
		{
			return;
		}
		foreach (PhysGrabber item in valuableTarget.playerGrabbing.ToList())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				item.ReleaseObject(valuableTarget.photonView.ViewID, 0.5f);
				continue;
			}
			item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, 0.5f, valuableTarget.photonView.ViewID);
		}
		Vector3 vector = playerTarget.PlayerVisionTarget.VisionTransform.position - valuableTarget.centerPoint;
		vector = Vector3.Lerp(base.transform.forward, vector, 0.5f);
		valuableTarget.ResetMass();
		float num = 20f * valuableTarget.rb.mass;
		num = Mathf.Min(num, 100f);
		valuableTarget.ResetIndestructible();
		valuableTarget.rb.AddForce(vector * num, ForceMode.Impulse);
		valuableTarget.rb.AddTorque(valuableTarget.transform.right * 0.5f, ForceMode.Impulse);
		valuableTarget.impactDetector.PlayerHurtMultiplier(5f, 2f);
		valuableTarget = null;
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
