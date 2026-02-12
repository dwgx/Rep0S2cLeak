using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBeamer : MonoBehaviour, IPunObservable
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		AttackStart,
		Attack,
		AttackEnd,
		MeleeStart,
		Melee,
		Seek,
		Leave,
		Stun,
		Despawn
	}

	public State currentState;

	public float stateTimer;

	private bool stateImpulse;

	private float stateTicker;

	internal Enemy enemy;

	internal PhotonView photonView;

	public EnemyBeamerAnim anim;

	public Transform aimVerticalTransform;

	public Transform hatTransform;

	public Transform bottomTransform;

	public SemiLaser laser;

	public Transform laserStartTransform;

	public Transform laserRayTransform;

	private Quaternion aimVerticalTarget;

	private Quaternion aimHorizontalTarget;

	public SpringQuaternion horizontalRotationSpring;

	private Quaternion horizontalRotationTarget = Quaternion.identity;

	[Space]
	public AnimationCurve aimHorizontalCurve;

	public float aimHorizontalSpread;

	public float aimHorizontalSpeed;

	private float aimHorizontalLerp;

	private float aimHorizontalResult;

	internal PlayerAvatar playerTarget;

	private Vector3 agentDestination;

	private Vector3 seekDestination;

	private Vector3 meleeTarget;

	private bool meleePlayer;

	private float laserCooldown;

	private float laserRange = 10f;

	private Vector3 hitPosition;

	private Vector3 hitPositionSmooth;

	private float hitPositionClientDistance;

	private bool hitPositionStartImpulse;

	private bool hitPositionImpact;

	private float hitPositionTimer;

	public ParticleSystem particleDeathSmoke;

	public ParticleSystem particleDeathBody;

	public ParticleSystem particleDeathNose;

	public ParticleSystem particleDeathHat;

	public ParticleSystem particleBottomSmoke;

	internal bool moveFast;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		VerticalAimLogic();
		LaserLogic();
		MoveFastLogic();
		if ((!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient) && LevelGenerator.Instance.Generated)
		{
			if (enemy.IsStunned())
			{
				UpdateState(State.Stun);
			}
			if (enemy.CurrentState == EnemyState.Despawn)
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
			case State.AttackStart:
				StateAttackStart();
				break;
			case State.Attack:
				StateAttack();
				break;
			case State.AttackEnd:
				StateAttackEnd();
				break;
			case State.MeleeStart:
				StateMeleeStart();
				break;
			case State.Melee:
				StateMelee();
				break;
			case State.Seek:
				StateSeek();
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
			RotationLogic();
		}
	}

	private void StateSpawn()
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
			UpdateState(State.Idle);
		}
	}

	private void StateIdle()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = Random.Range(4f, 8f);
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
			stateTimer = 999f;
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
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (!enemy.Jump.jumping && Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetPoint()) < 1f)
		{
			UpdateState(State.Idle);
		}
		else if (enemy.Rigidbody.notMovingTimer >= 3f)
		{
			AttackNearestPhysObjectOrGoToIdle();
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
			stateImpulse = false;
			stateTimer = 999f;
			enemy.Rigidbody.notMovingTimer = 0f;
		}
		else
		{
			enemy.NavMeshAgent.SetDestination(agentDestination);
			if (!enemy.Jump.jumping && (Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) < 1f || Vector3.Distance(enemy.Rigidbody.transform.position, agentDestination) < 1f))
			{
				UpdateState(State.Idle);
			}
			else if (enemy.Rigidbody.notMovingTimer >= 3f)
			{
				AttackNearestPhysObjectOrGoToIdle();
			}
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateAttackStart()
	{
		if (stateImpulse)
		{
			aimHorizontalResult = 0f;
			stateImpulse = false;
			stateTimer = 1.5f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			seekDestination = playerTarget.transform.position;
			aimHorizontalLerp = 0f;
		}
		aimHorizontalResult = Mathf.Lerp(0f, 0f - aimHorizontalSpread, aimHorizontalCurve.Evaluate(aimHorizontalLerp));
		aimHorizontalLerp += 1.5f * Time.deltaTime;
		aimHorizontalTarget = Quaternion.LookRotation(playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.Rigidbody.transform.position);
		aimHorizontalTarget = Quaternion.Euler(0f, aimHorizontalTarget.eulerAngles.y, 0f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Attack);
		}
	}

	private void StateAttack()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 0.5f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			aimHorizontalLerp = 0f;
		}
		aimHorizontalResult = Mathf.Lerp(0f - aimHorizontalSpread, aimHorizontalSpread, aimHorizontalCurve.Evaluate(aimHorizontalLerp));
		aimHorizontalLerp += aimHorizontalSpeed * Time.deltaTime;
		if (aimHorizontalLerp >= 1f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.AttackEnd);
			}
		}
	}

	private void StateAttackEnd()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			aimHorizontalLerp = 0f;
		}
		aimHorizontalResult = Mathf.Lerp(aimHorizontalSpread, 0f, aimHorizontalCurve.Evaluate(aimHorizontalLerp));
		aimHorizontalLerp += 1.5f * Time.deltaTime;
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Seek);
		}
	}

	private void StateMeleeStart()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 0.5f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
			seekDestination = meleeTarget;
		}
		if (meleePlayer)
		{
			meleeTarget = playerTarget.transform.position;
			seekDestination = meleeTarget;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Melee);
		}
	}

	private void StateMelee()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 3f;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Seek);
		}
	}

	private void StateSeek()
	{
		if (stateImpulse)
		{
			stateTimer = 20f;
			stateImpulse = false;
			enemy.Rigidbody.notMovingTimer = 0f;
			if (Vector3.Distance(base.transform.position, seekDestination) >= 3f)
			{
				moveFast = true;
				if (SemiFunc.IsMultiplayer())
				{
					photonView.RPC("MoveFastRPC", RpcTarget.Others, moveFast);
				}
			}
		}
		enemy.NavMeshAgent.SetDestination(seekDestination);
		if (moveFast)
		{
			enemy.NavMeshAgent.OverrideAgent(enemy.NavMeshAgent.DefaultSpeed * 2f, enemy.NavMeshAgent.DefaultAcceleration * 2f, 0.1f);
		}
		if (Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetPoint()) < 1f)
		{
			LevelPoint levelPointAhead = enemy.GetLevelPointAhead(seekDestination);
			if ((bool)levelPointAhead)
			{
				seekDestination = levelPointAhead.transform.position;
			}
			if (moveFast)
			{
				moveFast = false;
				if (SemiFunc.IsMultiplayer())
				{
					photonView.RPC("MoveFastRPC", RpcTarget.Others, moveFast);
				}
			}
		}
		if (enemy.Rigidbody.notMovingTimer >= 3f)
		{
			AttackNearestPhysObjectOrGoToIdle();
			return;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	public void StateLeave()
	{
		if (stateImpulse)
		{
			stateTimer = 999f;
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
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		if (Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetPoint()) < 1f)
		{
			UpdateState(State.Idle);
		}
		else if (enemy.Rigidbody.notMovingTimer >= 3f)
		{
			AttackNearestPhysObjectOrGoToIdle();
		}
	}

	private void StateStun()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			enemy.NavMeshAgent.ResetPath();
		}
		if (!enemy.IsStunned())
		{
			UpdateState(State.Idle);
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

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
	}

	public void OnVision()
	{
		if (enemy.Jump.jumping)
		{
			return;
		}
		if (currentState == State.Roam || currentState == State.Idle || currentState == State.Seek || currentState == State.Leave || currentState == State.Investigate)
		{
			if (playerTarget != enemy.Vision.onVisionTriggeredPlayer)
			{
				playerTarget = enemy.Vision.onVisionTriggeredPlayer;
				if (GameManager.Multiplayer())
				{
					photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, playerTarget.photonView.ViewID);
				}
			}
			if ((bool)playerTarget && Vector3.Distance(base.transform.position, playerTarget.transform.position) < 2.5f && Mathf.Abs(base.transform.position.y - playerTarget.transform.position.y) < 1f)
			{
				meleeTarget = playerTarget.transform.position;
				meleePlayer = true;
				UpdateState(State.MeleeStart);
			}
			else if (laserCooldown <= 0f)
			{
				UpdateState(State.AttackStart);
			}
			else
			{
				seekDestination = playerTarget.transform.position;
				UpdateState(State.Seek);
			}
		}
		else if ((currentState == State.AttackStart || currentState == State.Attack || currentState == State.AttackEnd) && playerTarget == enemy.Vision.onVisionTriggeredPlayer)
		{
			seekDestination = playerTarget.transform.position;
		}
	}

	public void OnInvestigate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && (currentState == State.Idle || currentState == State.Roam || currentState == State.Seek || currentState == State.Investigate))
		{
			agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			UpdateState(State.Investigate);
		}
	}

	public void OnHurt()
	{
		anim.soundHurt.Play(anim.transform.position);
		anim.soundHurtPauseTimer = 0.5f;
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.Leave)
		{
			UpdateState(State.Idle);
		}
	}

	public void OnDeath()
	{
		anim.soundDeath.Play(anim.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.05f);
		particleDeathSmoke.transform.position = enemy.CenterTransform.position;
		particleDeathSmoke.Play();
		particleDeathBody.transform.position = enemy.CenterTransform.position;
		particleDeathBody.Play();
		particleDeathNose.transform.position = laserStartTransform.position;
		particleDeathNose.Play();
		particleDeathHat.transform.position = hatTransform.position;
		particleDeathHat.Play();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	private void UpdateState(State _state)
	{
		if (_state != currentState)
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

	private void AttackNearestPhysObjectOrGoToIdle()
	{
		meleeTarget = SemiFunc.EnemyGetNearestPhysObject(enemy);
		if (meleeTarget != Vector3.zero)
		{
			meleePlayer = false;
			UpdateState(State.Melee);
		}
		else
		{
			UpdateState(State.Idle);
		}
	}

	private void RotationLogic()
	{
		if (currentState == State.AttackStart || currentState == State.Attack || currentState == State.AttackEnd)
		{
			horizontalRotationTarget = Quaternion.Euler(aimHorizontalTarget.eulerAngles.x, aimHorizontalTarget.eulerAngles.y + aimHorizontalResult, aimHorizontalTarget.eulerAngles.z);
		}
		else if (currentState == State.MeleeStart)
		{
			if (Vector3.Distance(meleeTarget, enemy.Rigidbody.transform.position) > 0.1f)
			{
				horizontalRotationTarget = Quaternion.LookRotation(meleeTarget - enemy.Rigidbody.transform.position);
				horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
			}
		}
		else if (enemy.NavMeshAgent.AgentVelocity.normalized.magnitude > 0.1f)
		{
			horizontalRotationTarget = Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized);
			horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
		}
		if (currentState == State.Spawn || currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate || currentState == State.Leave)
		{
			horizontalRotationSpring.speed = 5f;
			horizontalRotationSpring.damping = 0.7f;
		}
		else if (currentState == State.AttackStart || currentState == State.Attack || currentState == State.AttackEnd)
		{
			horizontalRotationSpring.speed = 15f;
			horizontalRotationSpring.damping = 0.8f;
		}
		else
		{
			horizontalRotationSpring.speed = 10f;
			horizontalRotationSpring.damping = 0.8f;
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(horizontalRotationSpring, horizontalRotationTarget);
	}

	private void VerticalAimLogic()
	{
		if (currentState != State.AttackStart && currentState != State.Attack && currentState != State.AttackEnd)
		{
			aimVerticalTarget = Quaternion.identity;
		}
		else if (currentState == State.AttackStart || (currentState != State.Attack && aimHorizontalLerp < 0.1f))
		{
			Quaternion quaternion = Quaternion.LookRotation(playerTarget.PlayerVisionTarget.VisionTransform.position - laserRayTransform.position);
			if (aimVerticalTarget == Quaternion.identity)
			{
				aimVerticalTarget = quaternion;
			}
			else
			{
				aimVerticalTarget = Quaternion.Lerp(aimVerticalTarget, quaternion, 2f * Time.deltaTime);
			}
			Quaternion rotation = laserRayTransform.rotation;
			laserRayTransform.rotation = aimVerticalTarget;
			aimVerticalTarget = laserRayTransform.localRotation;
			aimVerticalTarget = Quaternion.Euler(laserRayTransform.eulerAngles.x, 0f, 0f);
			laserRayTransform.rotation = rotation;
		}
		aimVerticalTransform.localRotation = Quaternion.Lerp(aimVerticalTransform.localRotation, aimVerticalTarget, 20f * Time.deltaTime);
		laserRayTransform.localRotation = aimVerticalTarget;
	}

	private void LaserLogic()
	{
		if (currentState != State.Attack && currentState != State.AttackStart && currentState != State.AttackEnd)
		{
			laserCooldown -= Time.deltaTime;
		}
		if (currentState == State.Attack)
		{
			laserCooldown = 3f;
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				Transform transform = laserStartTransform;
				Vector3 direction = laserRayTransform.position - laserStartTransform.position;
				if (Physics.Raycast(laserStartTransform.position, direction, out var _, direction.magnitude, LayerMask.GetMask("Default")))
				{
					transform = laserRayTransform;
				}
				else if (Physics.OverlapSphere(laserStartTransform.position, 0.25f, LayerMask.GetMask("Default")).Length != 0)
				{
					transform = laserRayTransform;
				}
				if (hitPositionTimer <= 0f)
				{
					hitPositionTimer = 0.05f;
					if (Physics.Raycast(transform.position, transform.forward, out var hitInfo2, laserRange, LayerMask.GetMask("Default")))
					{
						hitPosition = hitInfo2.point;
						hitPositionImpact = true;
					}
					else
					{
						hitPosition = transform.position + transform.forward * laserRange;
						hitPositionImpact = false;
					}
				}
				else
				{
					hitPositionTimer -= Time.deltaTime;
				}
				hitPositionSmooth = Vector3.Lerp(hitPositionSmooth, hitPosition, 20f * Time.deltaTime);
			}
			else
			{
				hitPositionSmooth = Vector3.MoveTowards(hitPositionSmooth, hitPosition, hitPositionClientDistance * Time.deltaTime * ((float)PhotonNetwork.SerializationRate * 0.8f));
			}
			if (hitPositionStartImpulse)
			{
				hitPositionSmooth = hitPosition;
				hitPositionStartImpulse = false;
			}
			Vector3 vector = laserRayTransform.position - hitPosition;
			Vector3 vector2 = laserRayTransform.position - hitPositionSmooth;
			if (vector.magnitude < vector2.magnitude)
			{
				vector2 = Vector3.ClampMagnitude(vector2, vector.magnitude);
				hitPositionSmooth = laserRayTransform.position - vector2;
			}
			laser.LaserActive(laserStartTransform.position, hitPositionSmooth, hitPositionImpact);
		}
		else
		{
			hitPositionTimer = 0f;
			hitPositionStartImpulse = true;
		}
	}

	private void MoveFastLogic()
	{
		if (currentState != State.Seek && moveFast)
		{
			moveFast = false;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("MoveFastRPC", RpcTarget.Others, moveFast);
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
	private void MeleeTriggerRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.meleeImpulse = true;
		}
	}

	[PunRPC]
	private void MoveFastRPC(bool _moveFast, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			moveFast = _moveFast;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(aimVerticalTarget);
				stream.SendNext(hitPosition);
				stream.SendNext(hitPositionImpact);
			}
			else
			{
				aimVerticalTarget = (Quaternion)stream.ReceiveNext();
				hitPosition = (Vector3)stream.ReceiveNext();
				hitPositionImpact = (bool)stream.ReceiveNext();
				hitPositionClientDistance = Vector3.Distance(hitPositionSmooth, hitPosition);
			}
		}
	}
}
