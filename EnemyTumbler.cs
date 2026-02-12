using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTumbler : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Notice,
		Investigate,
		MoveToPlayer,
		Tell,
		Tumble,
		TumbleEnd,
		BackToNavmesh,
		Leave,
		Stunned,
		Dead,
		Despawn
	}

	public bool debugSpawn;

	public State currentState;

	private bool stateImpulse;

	private float stateTimer;

	internal PlayerAvatar targetPlayer;

	public Enemy enemy;

	public EnemyTumblerAnim enemyTumblerAnim;

	private PhotonView photonView;

	public HurtCollider hurtCollider;

	private float hurtColliderTimer;

	private float roamWaitTimer;

	private Vector3 roamPoint;

	private Vector3 backToNavmeshPosition;

	private Vector3 agentDestination;

	private Quaternion lookDirection;

	private float visionTimer;

	private bool visionPrevious;

	private bool groundedPrevious;

	private float hopMoveTimer;

	public ParticleSystem particleDeathImpact;

	public ParticleSystem particleDeathBitsFar;

	public ParticleSystem particleDeathBitsShort;

	public ParticleSystem particleDeathSmoke;

	public ParticleSystem particleDeathHat;

	[Space]
	public SpringQuaternion headSpring;

	public Transform headTransform;

	public Transform headTargetTransform;

	public Transform headTargetCodeTransform;

	[Space]
	public SpringQuaternion hatSpring;

	public Transform hatTransform;

	public Transform hatTargetTransform;

	[Space]
	public SpringQuaternion mainMeshSpring;

	private Quaternion mainMeshTargetRotation;

	private float grabAggroTimer;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			HopLogic();
			RotationLogic();
			if (visionTimer > 0f)
			{
				visionTimer -= Time.deltaTime;
			}
			if (enemy.IsStunned())
			{
				UpdateState(State.Stunned);
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
			case State.Notice:
				StateNotice();
				break;
			case State.Investigate:
				StateInvestigate();
				break;
			case State.MoveToPlayer:
				StateMoveToPlayer();
				break;
			case State.Tell:
				StateTell();
				break;
			case State.Tumble:
				StateTumble();
				break;
			case State.TumbleEnd:
				StateTumbleEnd();
				break;
			case State.BackToNavmesh:
				StateBackToNavmesh();
				break;
			case State.Leave:
				StateLeave();
				break;
			case State.Stunned:
				StateStunned();
				break;
			case State.Dead:
				StateDead();
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
			stateTimer = 1f;
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
			stateTimer = Random.Range(3f, 8f);
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
			if (!enemy.Jump.jumping && (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 1f))
			{
				UpdateState(State.Idle);
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
			stateTimer = 1f;
			stateImpulse = false;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.MoveToPlayer);
		}
	}

	private void StateInvestigate()
	{
		if (stateImpulse)
		{
			if (!enemy.Jump.jumping)
			{
				enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
				enemy.NavMeshAgent.ResetPath();
			}
			enemy.NavMeshAgent.SetDestination(agentDestination);
			stateTimer = 5f;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
		}
		else
		{
			enemy.NavMeshAgent.SetDestination(agentDestination);
			SemiFunc.EnemyCartJump(enemy);
			if (enemy.Rigidbody.notMovingTimer > 3f)
			{
				stateTimer -= Time.deltaTime;
			}
			if (!enemy.Jump.jumping && (stateTimer <= 0f || Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetDestination()) < 1f))
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

	private void StateMoveToPlayer()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
		}
		agentDestination = targetPlayer.transform.position;
		if (enemy.Grounded.grounded)
		{
			enemy.NavMeshAgent.SetDestination(agentDestination);
		}
		stateTimer -= Time.deltaTime;
		Vector3 position = targetPlayer.transform.position;
		position.y = enemy.Rigidbody.transform.position.y;
		if (Vector3.Distance(enemy.Rigidbody.transform.position, position) < 7f && !enemy.Jump.jumping && !VisionBlocked())
		{
			UpdateState(State.Tell);
		}
		else if (stateTimer <= 0f || enemy.Rigidbody.notMovingTimer > 3f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateTell()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Tumble);
		}
	}

	private void StateTumble()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			Vector3 normalized = Vector3.Lerp(targetPlayer.transform.position - enemy.Rigidbody.transform.position, Vector3.up, 0.6f).normalized;
			enemy.Rigidbody.rb.AddForce(normalized * 40f, ForceMode.Impulse);
			enemy.Rigidbody.rb.AddTorque(enemy.Rigidbody.transform.right * 8f, ForceMode.Impulse);
		}
		enemy.NavMeshAgent.Disable(0.1f);
		enemy.Rigidbody.DisableFollowPosition(0.2f, 10f);
		enemy.Rigidbody.DisableFollowRotation(0.2f, 10f);
		if (enemy.Rigidbody.rb.velocity.magnitude < 1f)
		{
			stateTimer -= Time.deltaTime;
		}
		base.transform.position = enemy.Rigidbody.transform.position;
		if (NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out var hit, 1f, -1))
		{
			backToNavmeshPosition = hit.position;
		}
		if (stateTimer <= 0f)
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
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.BackToNavmesh);
		}
	}

	private void StateBackToNavmesh()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		if (NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out var hit, 1f, -1))
		{
			enemy.NavMeshAgent.Warp(hit.position);
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
			stateImpulse = false;
			SemiFunc.EnemyLeaveStart(enemy);
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

	private void StateStunned()
	{
		if (NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out var hit, 1f, -1))
		{
			backToNavmeshPosition = hit.position;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = enemy.Rigidbody.transform.position;
		if (!enemy.IsStunned())
		{
			UpdateState(State.BackToNavmesh);
		}
	}

	private void StateDead()
	{
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
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
		enemyTumblerAnim.sfxHurt.Play(base.transform.position);
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
		particleDeathHat.transform.position = enemy.CenterTransform.position;
		particleDeathHat.Play();
		enemyTumblerAnim.sfxDeath.Play(enemyTumblerAnim.transform.position);
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
			UpdateState(State.Notice);
			if (GameManager.Multiplayer())
			{
				photonView.RPC("TargetPlayerRPC", RpcTarget.All, targetPlayer.photonView.ViewID);
			}
		}
		else if (currentState == State.MoveToPlayer && targetPlayer == enemy.Vision.onVisionTriggeredPlayer)
		{
			stateTimer = 2f;
		}
	}

	public void OnGrabbed()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !(grabAggroTimer > 0f) && currentState == State.Leave)
		{
			grabAggroTimer = 60f;
			targetPlayer = enemy.Rigidbody.onGrabbedPlayerAvatar;
			UpdateState(State.Notice);
			if (GameManager.Multiplayer())
			{
				photonView.RPC("TargetPlayerRPC", RpcTarget.All, targetPlayer.photonView.ViewID);
			}
		}
	}

	public void OnHurtColliderImpactAny()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("OnHurtColliderImpactAnyRPC", RpcTarget.All);
			}
			else
			{
				OnHurtColliderImpactAnyRPC();
			}
		}
	}

	public void OnHurtColliderImpactPlayer()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			OnHurtColliderImpactPlayerRPC(hurtCollider.onImpactPlayerAvatar.photonView.ViewID);
			return;
		}
		photonView.RPC("OnHurtColliderImpactPlayerRPC", RpcTarget.All, hurtCollider.onImpactPlayerAvatar.photonView.ViewID);
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
		if ((currentState == State.Notice || currentState == State.MoveToPlayer || currentState == State.Tell) && !VisionBlocked())
		{
			Quaternion quaternion = Quaternion.Euler(0f, Quaternion.LookRotation(targetPlayer.PlayerVisionTarget.VisionTransform.position - enemy.Rigidbody.transform.position).eulerAngles.y, 0f);
			mainMeshTargetRotation = quaternion;
		}
		else
		{
			Vector3 agentVelocity = enemy.NavMeshAgent.AgentVelocity;
			agentVelocity.y = 0f;
			if (agentVelocity.magnitude > 1f)
			{
				mainMeshTargetRotation = Quaternion.Euler(0f, Quaternion.LookRotation(enemy.Rigidbody.rb.velocity.normalized).eulerAngles.y, 0f);
			}
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(mainMeshSpring, mainMeshTargetRotation);
		headTargetCodeTransform.localEulerAngles = new Vector3(enemy.Rigidbody.rb.velocity.y * 5f, 0f, 0f);
		headTransform.rotation = SemiFunc.SpringQuaternionGet(headSpring, headTargetTransform.rotation);
		hatTransform.rotation = SemiFunc.SpringQuaternionGet(hatSpring, hatTargetTransform.rotation);
	}

	private bool VisionBlocked()
	{
		if (visionTimer <= 0f)
		{
			visionTimer = 0.1f;
			Vector3 direction = targetPlayer.PlayerVisionTarget.VisionTransform.position - enemy.Vision.VisionTransform.position;
			visionPrevious = Physics.Raycast(enemy.Vision.VisionTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
		}
		return visionPrevious;
	}

	private void HopLogic()
	{
		bool flag = currentState == State.BackToNavmesh;
		if (currentState == State.Roam || currentState == State.Investigate || currentState == State.MoveToPlayer || currentState == State.Leave || flag)
		{
			float num = 1f;
			if (currentState == State.MoveToPlayer)
			{
				num = 2f;
			}
			if (enemy.Grounded.grounded && !enemy.Jump.jumping)
			{
				enemy.NavMeshAgent.Stop(0.1f);
				if (groundedPrevious != enemy.Grounded.grounded)
				{
					if (flag)
					{
						base.transform.position = enemy.Rigidbody.transform.position;
					}
					else
					{
						enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
					}
				}
			}
			if (hopMoveTimer <= 0f)
			{
				Vector3 steeringTarget = enemy.NavMeshAgent.Agent.steeringTarget;
				Vector3 normalized = (enemy.NavMeshAgent.Agent.steeringTarget - enemy.Rigidbody.physGrabObject.centerPoint).normalized;
				steeringTarget.y = enemy.Rigidbody.physGrabObject.centerPoint.y;
				Vector3 normalized2 = (steeringTarget - enemy.Rigidbody.physGrabObject.centerPoint).normalized;
				bool flag2 = false;
				bool flag3 = false;
				int num2 = 10;
				float num3 = 0.5f;
				float maxDistance = 2f;
				if (!flag)
				{
					Vector3 origin = enemy.Rigidbody.physGrabObject.centerPoint + normalized2 * num3;
					bool flag4 = false;
					for (int i = 0; i < num2; i++)
					{
						if (Physics.Raycast(origin, Vector3.down, maxDistance, SemiFunc.LayerMaskGetVisionObstruct()))
						{
							if (flag4)
							{
								flag2 = true;
							}
						}
						else
						{
							if (i < 3)
							{
								flag4 = true;
							}
							flag3 = true;
						}
						origin += normalized2 * num3;
					}
					enemy.NavMeshAgent.Stop(0f);
				}
				if (flag2)
				{
					enemy.Rigidbody.rb.AddForce(Vector3.up * 30f + normalized * 20f, ForceMode.Impulse);
					enemy.NavMeshAgent.Warp(enemy.Rigidbody.physGrabObject.centerPoint + normalized * 5f);
					hopMoveTimer = 2.25f;
				}
				else if (!flag && Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetPoint()) < 1f)
				{
					enemy.Rigidbody.rb.AddForce(Vector3.up * 20f, ForceMode.Impulse);
					enemy.NavMeshAgent.Warp(enemy.NavMeshAgent.GetPoint());
					hopMoveTimer = 0.75f;
				}
				else if (flag3)
				{
					enemy.Rigidbody.rb.AddForce(Vector3.up * 20f, ForceMode.Impulse);
					enemy.NavMeshAgent.Warp(enemy.Rigidbody.physGrabObject.centerPoint + normalized * 0.5f);
					hopMoveTimer = 0.75f;
				}
				else
				{
					enemy.Rigidbody.rb.AddForce(Vector3.up * 25f + normalized2 * 10f, ForceMode.Impulse);
					if (flag)
					{
						base.transform.position = Vector3.MoveTowards(base.transform.position, backToNavmeshPosition, 2f);
					}
					else
					{
						enemy.NavMeshAgent.Warp(enemy.Rigidbody.physGrabObject.centerPoint + normalized * num);
					}
					hopMoveTimer = 1.25f;
				}
				enemy.Jump.JumpingSet(_jumping: true, EnemyJump.Type.Stuck);
				enemy.Rigidbody.WarpDisable(2f);
				enemy.Grounded.GroundedDisable(0.25f);
			}
			else
			{
				hopMoveTimer -= Time.deltaTime;
			}
		}
		groundedPrevious = enemy.Grounded.grounded;
	}

	[PunRPC]
	private void UpdateStateRPC(State _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
			if (currentState == State.Spawn)
			{
				enemyTumblerAnim.OnSpawn();
			}
			if (currentState == State.Tumble)
			{
				enemyTumblerAnim.OnTumble();
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
	private void OnHurtColliderImpactAnyRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			enemyTumblerAnim.SfxOnHurtColliderImpactAny();
		}
	}

	[PunRPC]
	private void OnHurtColliderImpactPlayerRPC(int _playerID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item.photonView.ViewID == _playerID)
			{
				item.tumble.OverrideEnemyHurt(3f);
				break;
			}
		}
	}
}
