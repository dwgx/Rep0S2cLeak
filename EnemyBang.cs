using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBang : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		FuseDelay,
		Fuse,
		Move,
		MoveUnder,
		MoveOver,
		MoveBack,
		Stun,
		StunEnd,
		Despawn
	}

	public EnemyBangAnim anim;

	[Space]
	public State currentState;

	private bool stateImpulse;

	private float stateTimer;

	internal Enemy enemy;

	internal PhotonView photonView;

	internal int directorIndex;

	private Vector3 moveBackPosition;

	internal bool fuseActive;

	internal float fuseLerp;

	private ParticleScriptExplosion particleScriptExplosion;

	private ParticlePrefabExplosion explosionScript;

	private bool visionPrevious;

	private float visionTimer;

	public SpringQuaternion horizontalRotationSpring;

	private Quaternion horizontalRotationTarget = Quaternion.identity;

	[Space]
	public SpringQuaternion headLookAtSpring;

	public Transform headLookAtTarget;

	public Transform headLookAtSource;

	[Space]
	public ParticleSystem[] deathEffects;

	[Space]
	public GameObject[] headObjects;

	[Space]
	public Transform particleParent;

	public Transform moveOffsetTransform;

	public Transform rotationTransform;

	private Vector3 moveOffsetPosition;

	private float moveOffsetTimer;

	private float moveOffsetSetTimer;

	public AudioSource talkSource;

	public AudioSource stunLoopSource;

	[Space]
	public Sound[] talkSoundsTest;

	[Space]
	public SpringQuaternion talkTopSpring;

	public Transform talkTopSource;

	public Transform talkTopTarget;

	[Space]
	public SpringQuaternion talkBottomSpring;

	public Transform talkBottomSource;

	public Transform talkBottomTarget;

	[Space]
	public float talkBreakerIdleTimeMin = 5f;

	public float talkBreakerIdleTimeMax = 20f;

	[Space]
	public float talkBreakerAttackTimeMin = 2f;

	public float talkBreakerAttackTimeMax = 5f;

	private float talkBreakerTimer;

	private bool explosionTell;

	private float explosionTellThreshold = 0.95f;

	internal float explosionTellFuseThreshold = 0.9f;

	private float talkClipTimer;

	private float talkClipLoudness;

	private int talkClipSampleDataLength = 1024;

	private float[] talkClipSampleData;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		talkClipSampleData = new float[talkClipSampleDataLength];
	}

	private void Start()
	{
		if (EnemyBangDirector.instance.setup)
		{
			EnemyBangDirector.instance.SetupSingle(this);
		}
	}

	private void Update()
	{
		HeadLookAtLogic();
		FuseLogic();
		TalkLogic();
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
			case State.FuseDelay:
				StateFuseDelay();
				break;
			case State.Fuse:
				StateFuse();
				break;
			case State.Move:
				StateMove();
				break;
			case State.MoveUnder:
				StateMoveUnder();
				break;
			case State.MoveOver:
				StateMoveOver();
				break;
			case State.MoveBack:
				StateMoveBack();
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
			TimerLogic();
			RotationLogic();
			MoveOffsetLogic();
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
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			stateImpulse = false;
		}
		Vector3 vector = EnemyBangDirector.instance.destinations[directorIndex];
		if (EnemyBangDirector.instance.currentState == EnemyBangDirector.State.AttackPlayer || EnemyBangDirector.instance.currentState == EnemyBangDirector.State.AttackCart)
		{
			vector = EnemyBangDirector.instance.attackPosition;
		}
		enemy.Rigidbody.DisableFollowPosition(0.1f, 5f);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, vector) > 2f)
		{
			UpdateState(State.Roam);
		}
	}

	private void StateRoam()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
		}
		Vector3 destination = EnemyBangDirector.instance.destinations[directorIndex];
		if (EnemyBangDirector.instance.currentState == EnemyBangDirector.State.AttackPlayer || EnemyBangDirector.instance.currentState == EnemyBangDirector.State.AttackCart)
		{
			destination = EnemyBangDirector.instance.attackPosition;
		}
		enemy.NavMeshAgent.SetDestination(destination);
		MoveBackPosition();
		SemiFunc.EnemyCartJump(enemy);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, destination) <= 0.5f)
		{
			UpdateState(State.Idle);
		}
		else if (Vector3.Distance(enemy.Rigidbody.transform.position, destination) <= 2f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.Idle);
			}
		}
	}

	private void StateFuseDelay()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = Random.Range(0.1f, 1f);
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
		}
		enemy.Rigidbody.DisableFollowPosition(0.1f, 5f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Fuse);
		}
	}

	private void StateFuse()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			FuseSet(_active: true, 0f);
			stateImpulse = false;
			stateTimer = 1.5f;
		}
		enemy.Rigidbody.DisableFollowPosition(0.1f, 5f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Move);
		}
	}

	private void StateMove()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		Vector3 destination = AttackPositionGet();
		if (Vector3.Distance(enemy.Rigidbody.transform.position, destination) > 1.5f)
		{
			enemy.NavMeshAgent.SetDestination(destination);
		}
		else
		{
			enemy.Rigidbody.DisableFollowPosition(0.1f, 5f);
			enemy.NavMeshAgent.Disable(0.1f);
		}
		MoveBackPosition();
		SemiFunc.EnemyCartJump(enemy);
		if (EnemyBangDirector.instance.currentState != EnemyBangDirector.State.AttackPlayer)
		{
			UpdateState(State.Idle);
		}
		else
		{
			if (enemy.NavMeshAgent.CanReach(AttackVisionDynamic(), 1f) || !(Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) < 2f))
			{
				return;
			}
			if (!enemy.Jump.jumping && !VisionBlocked() && !NavMesh.SamplePosition(AttackVisionDynamic(), out var _, 0.5f, -1))
			{
				if (EnemyBangDirector.instance.playerTargetCrawling && Mathf.Abs(AttackVisionDynamic().y - enemy.Rigidbody.transform.position.y) < 0.3f)
				{
					UpdateState(State.MoveUnder);
					return;
				}
				if (destination.y > enemy.Rigidbody.transform.position.y)
				{
					UpdateState(State.MoveOver);
					return;
				}
			}
			if (destination.y > enemy.Rigidbody.transform.position.y + 0.2f)
			{
				enemy.Jump.StuckTrigger(AttackVisionPositionGet() - enemy.Vision.VisionTransform.position);
			}
		}
	}

	private void StateMoveUnder()
	{
		if (stateImpulse)
		{
			stateTimer = 2f;
			stateImpulse = false;
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		Vector3 vector = AttackPositionGet();
		enemy.NavMeshAgent.Disable(0.1f);
		enemy.Vision.StandOverride(0.25f);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, vector) > 1f)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, vector, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
		}
		else
		{
			enemy.Rigidbody.DisableFollowPosition(0.1f, 5f);
		}
		enemy.Jump.StuckDisable(0.5f);
		SemiFunc.EnemyCartJump(enemy);
		NavMeshHit hit;
		if (EnemyBangDirector.instance.currentState != EnemyBangDirector.State.AttackPlayer)
		{
			UpdateState(State.MoveBack);
		}
		else if (NavMesh.SamplePosition(vector, out hit, 0.5f, -1))
		{
			UpdateState(State.MoveBack);
		}
		else if (VisionBlocked())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.MoveBack);
			}
		}
		else
		{
			EnemyBangDirector.instance.SeeTarget();
			stateTimer = 2f;
		}
	}

	private void StateMoveOver()
	{
		if (stateImpulse)
		{
			stateTimer = 2f;
			stateImpulse = false;
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		enemy.Vision.StandOverride(0.25f);
		Vector3 vector = AttackPositionGet();
		if (Vector3.Distance(enemy.Rigidbody.transform.position, vector) > 1f)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, vector, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
		}
		else
		{
			base.transform.position = enemy.Rigidbody.transform.position;
			enemy.Rigidbody.DisableFollowPosition(0.1f, 5f);
		}
		SemiFunc.EnemyCartJump(enemy);
		if (AttackVisionDynamic().y > enemy.Rigidbody.transform.position.y + 0.3f && !enemy.Jump.jumping)
		{
			Vector3 normalized = (AttackVisionDynamic() - enemy.Rigidbody.transform.position).normalized;
			enemy.Jump.StuckTrigger(normalized);
			enemy.Rigidbody.WarpDisable(0.25f);
			base.transform.position = enemy.Rigidbody.transform.position;
			base.transform.position = Vector3.MoveTowards(base.transform.position, AttackVisionDynamic(), 2f);
		}
		if (enemy.Jump.jumping)
		{
			return;
		}
		NavMeshHit hit;
		if (EnemyBangDirector.instance.currentState != EnemyBangDirector.State.AttackPlayer)
		{
			UpdateState(State.MoveBack);
		}
		else if (NavMesh.SamplePosition(vector, out hit, 0.5f, -1))
		{
			UpdateState(State.MoveBack);
		}
		else if (VisionBlocked())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.MoveBack);
			}
		}
		else
		{
			EnemyBangDirector.instance.SeeTarget();
			stateTimer = 2f;
		}
	}

	private void StateMoveBack()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		if (!enemy.Jump.jumping)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, moveBackPosition, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
		}
		SemiFunc.EnemyCartJump(enemy);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f && (Vector3.Distance(base.transform.position, enemy.Rigidbody.transform.position) > 2f || enemy.Rigidbody.notMovingTimer > 2f) && !enemy.Jump.jumping)
		{
			Vector3 normalized = (base.transform.position - moveBackPosition).normalized;
			enemy.Jump.StuckTrigger(base.transform.position - moveBackPosition);
			base.transform.position = enemy.Rigidbody.transform.position;
			base.transform.position += normalized * 2f;
		}
		bool flag = false;
		NavMeshHit hit;
		if (Vector3.Distance(enemy.Rigidbody.transform.position, moveBackPosition) <= 0.2f)
		{
			flag = true;
		}
		else if (NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out hit, 0.5f, -1))
		{
			flag = true;
		}
		if (flag)
		{
			if (fuseActive)
			{
				UpdateState(State.Move);
			}
			else
			{
				UpdateState(State.Idle);
			}
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
			UpdateState(State.StunEnd);
		}
	}

	private void StateStunEnd()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			stateImpulse = false;
			stateTimer = 1f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.MoveBack);
		}
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			if (!fuseActive)
			{
				enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
				enemy.NavMeshAgent.ResetPath();
			}
			stateImpulse = false;
		}
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			EnemyBangDirector.instance.OnSpawn(this);
			UpdateState(State.Spawn);
			FuseSet(_active: false, 0f);
		}
	}

	public void OnHurt()
	{
		if (talkSource.isActiveAndEnabled)
		{
			anim.soundHurt.Play(enemy.CenterTransform.position);
			anim.StunLoopPause(0.5f);
		}
	}

	public void OnDeath()
	{
		anim.soundDeathSFX.Play(enemy.CenterTransform.position);
		anim.soundDeathVO.Play(enemy.CenterTransform.position);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
		if (fuseActive && fuseLerp <= 1f)
		{
			explosionScript = particleScriptExplosion.Spawn(enemy.CenterTransform.position, 0.5f, 15, 10);
			explosionScript.HurtCollider.onImpactEnemy.AddListener(OnExplodeHitEnemy);
		}
		ParticleSystem[] array = deathEffects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	public void OnInvestigate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			EnemyBangDirector.instance.Investigate(enemy.StateInvestigate.onInvestigateTriggeredPosition);
		}
	}

	public void OnVision()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		EnemyBangDirector.instance.SetTarget(enemy.Vision.onVisionTriggeredPlayer);
		if (currentState == State.Idle || currentState == State.Roam)
		{
			if (!fuseActive)
			{
				UpdateState(State.FuseDelay);
			}
			else
			{
				UpdateState(State.Move);
			}
			EnemyBangDirector.instance.TriggerNearby(base.transform.position);
		}
	}

	public void OnExplodeHitEnemy()
	{
		if ((bool)explosionScript)
		{
			EnemyBang component = explosionScript.HurtCollider.onImpactEnemyEnemy.GetComponent<EnemyBang>();
			if ((bool)component)
			{
				component.enemy.Health.healthCurrent = 999;
				component.FuseSet(_active: true, Random.Range(0.96f, 0.98f));
			}
		}
	}

	public void OnImpactLight()
	{
		if (enemy.IsStunned())
		{
			anim.soundImpactLight.Play(enemy.CenterTransform.position);
		}
	}

	public void OnImpactMedium()
	{
		if (enemy.IsStunned())
		{
			anim.soundImpactMedium.Play(enemy.CenterTransform.position);
		}
	}

	public void OnImpactHeavy()
	{
		if (enemy.IsStunned())
		{
			anim.soundImpactHeavy.Play(enemy.CenterTransform.position);
		}
	}

	public void UpdateState(State _state)
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

	private void RotationLogic()
	{
		if (currentState != State.StunEnd)
		{
			if (currentState == State.Idle)
			{
				Vector3 position = EnemyBangDirector.instance.transform.position;
				if (Vector3.Distance(position, enemy.Rigidbody.transform.position) > 0.1f)
				{
					horizontalRotationTarget = Quaternion.LookRotation(position - enemy.Rigidbody.transform.position);
					horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
				}
			}
			else if (currentState == State.FuseDelay || currentState == State.Fuse || currentState == State.Move || currentState == State.MoveUnder || currentState == State.MoveOver || currentState == State.MoveBack)
			{
				if (enemy.Rigidbody.velocity.magnitude < 0.1f)
				{
					Vector3 vector = AttackVisionPositionGet();
					if (Vector3.Distance(vector, enemy.Rigidbody.transform.position) > 0.1f)
					{
						horizontalRotationTarget = Quaternion.LookRotation(vector - enemy.Rigidbody.transform.position);
						horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
					}
				}
				else
				{
					horizontalRotationTarget = Quaternion.LookRotation(enemy.Rigidbody.velocity.normalized);
					horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
				}
			}
			else if (enemy.Rigidbody.velocity.magnitude > 0.1f)
			{
				horizontalRotationTarget = Quaternion.LookRotation(enemy.Rigidbody.velocity.normalized);
				horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
			}
		}
		rotationTransform.rotation = SemiFunc.SpringQuaternionGet(horizontalRotationSpring, horizontalRotationTarget);
	}

	private Vector3 AttackPositionGet()
	{
		return EnemyBangDirector.instance.attackPosition;
	}

	private Vector3 AttackVisionPositionGet()
	{
		return EnemyBangDirector.instance.attackVisionPosition;
	}

	private Vector3 AttackVisionDynamic()
	{
		if (EnemyBangDirector.instance.currentState == EnemyBangDirector.State.AttackPlayer)
		{
			return AttackPositionGet();
		}
		return AttackVisionPositionGet();
	}

	private void MoveBackPosition()
	{
		if (Vector3.Distance(base.transform.position, enemy.Rigidbody.transform.position) < 1f)
		{
			moveBackPosition = base.transform.position;
		}
	}

	private bool VisionBlocked()
	{
		if (visionTimer <= 0f)
		{
			visionTimer = 0.1f;
			Vector3 direction = AttackVisionPositionGet() - enemy.Vision.VisionTransform.position;
			visionPrevious = Physics.Raycast(enemy.Vision.VisionTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
		}
		return visionPrevious;
	}

	private void HeadLookAtLogic()
	{
		bool flag = false;
		if (currentState == State.Move || currentState == State.MoveUnder || currentState == State.MoveOver || currentState == State.MoveBack)
		{
			flag = true;
		}
		if (flag)
		{
			Vector3 direction = AttackVisionPositionGet() - headLookAtTarget.position;
			direction = SemiFunc.ClampDirection(direction, headLookAtTarget.forward, 90f);
			headLookAtSource.rotation = SemiFunc.SpringQuaternionGet(headLookAtSpring, Quaternion.LookRotation(direction));
		}
		else
		{
			headLookAtSource.rotation = SemiFunc.SpringQuaternionGet(headLookAtSpring, headLookAtTarget.rotation);
		}
	}

	private void TimerLogic()
	{
		visionTimer -= Time.deltaTime;
	}

	private void FuseLogic()
	{
		if (!fuseActive)
		{
			return;
		}
		if (!EnemyBangDirector.instance.debugNoFuseProgress)
		{
			fuseLerp += Time.deltaTime / 15f;
		}
		fuseLerp = Mathf.Clamp01(fuseLerp);
		if (SemiFunc.IsMasterClientOrSingleplayer() && fuseLerp >= 1f)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ExplodeRPC", RpcTarget.All);
			}
			else
			{
				ExplodeRPC();
			}
			UpdateState(State.Despawn);
			stateImpulse = false;
			enemy.EnemyParent.Despawn();
		}
	}

	private void MoveOffsetLogic()
	{
		if ((currentState == State.Move || currentState == State.MoveUnder || currentState == State.MoveOver || currentState == State.MoveBack) && Vector3.Distance(enemy.Rigidbody.transform.position, AttackPositionGet()) > 2f)
		{
			moveOffsetTimer = 0.2f;
		}
		else if (currentState == State.Roam && Vector3.Distance(enemy.Rigidbody.transform.position, EnemyBangDirector.instance.destinations[directorIndex]) <= 2f)
		{
			moveOffsetTimer = 0.2f;
		}
		if (moveOffsetTimer > 0f)
		{
			moveOffsetTimer -= Time.deltaTime;
			if (enemy.Jump.jumping)
			{
				moveOffsetTimer = 0f;
			}
			if (moveOffsetTimer <= 0f)
			{
				moveOffsetPosition = Vector3.zero;
			}
			else
			{
				moveOffsetSetTimer -= Time.deltaTime;
				if (moveOffsetSetTimer <= 0f)
				{
					Vector3 vector = Random.insideUnitSphere.normalized * Random.Range(0.5f, 1f);
					vector.y = 0f;
					moveOffsetPosition = vector;
					moveOffsetSetTimer = Random.Range(0.5f, 2f);
				}
			}
		}
		moveOffsetTransform.localPosition = Vector3.Lerp(moveOffsetTransform.localPosition, moveOffsetPosition, Time.deltaTime * 5f);
	}

	private void TalkLogic()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && explosionTell)
		{
			bool flag = false;
			bool flag2 = false;
			if (!enemy.Jump.jumping)
			{
				if (currentState == State.Idle || currentState == State.Roam)
				{
					flag = true;
					flag2 = true;
				}
				else if (currentState == State.Move || currentState == State.MoveUnder || currentState == State.MoveOver || currentState == State.MoveBack)
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				if (!flag)
				{
					talkBreakerTimer = Mathf.Min(talkBreakerTimer, talkBreakerAttackTimeMax);
				}
				talkBreakerTimer -= Time.deltaTime;
				if (talkBreakerTimer <= 0f)
				{
					if (flag)
					{
						talkBreakerTimer = Random.Range(talkBreakerIdleTimeMin, talkBreakerIdleTimeMax);
					}
					else
					{
						talkBreakerTimer = Random.Range(talkBreakerAttackTimeMin, talkBreakerAttackTimeMax);
					}
					if (SemiFunc.IsMultiplayer())
					{
						photonView.RPC("TalkBreakerRPC", RpcTarget.All, flag);
					}
					else
					{
						TalkBreakerRPC(flag);
					}
				}
			}
			else
			{
				talkBreakerTimer = Mathf.Max(talkBreakerTimer, 2f);
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (fuseActive)
			{
				if (explosionTell && fuseLerp >= explosionTellThreshold)
				{
					explosionTell = false;
					if (SemiFunc.IsMultiplayer())
					{
						photonView.RPC("ExplosionTellRPC", RpcTarget.All);
					}
					else
					{
						ExplosionTellRPC();
					}
				}
			}
			else
			{
				explosionTell = true;
			}
		}
		if (talkClipTimer <= 0f && !enemy.Health.dead)
		{
			talkClipTimer = 0.01f;
			talkClipLoudness = 0f;
			if ((bool)talkSource.clip && talkSource.isPlaying)
			{
				talkSource.clip.GetData(talkClipSampleData, talkSource.timeSamples);
				float[] array = talkClipSampleData;
				foreach (float f in array)
				{
					talkClipLoudness += Mathf.Abs(f);
				}
				talkClipLoudness /= talkClipSampleDataLength;
			}
			if ((bool)stunLoopSource.clip && stunLoopSource.isPlaying)
			{
				stunLoopSource.clip.GetData(talkClipSampleData, stunLoopSource.timeSamples);
				float[] array = talkClipSampleData;
				foreach (float f2 in array)
				{
					talkClipLoudness += Mathf.Abs(f2);
				}
				talkClipLoudness /= talkClipSampleDataLength;
			}
		}
		else
		{
			talkClipTimer -= Time.deltaTime;
		}
		talkTopTarget.localRotation = Quaternion.Euler(talkClipLoudness * -45f, 0f, 0f);
		talkBottomTarget.localRotation = Quaternion.Euler(talkClipLoudness * 90f, 0f, 0f);
		talkTopSource.localRotation = SemiFunc.SpringQuaternionGet(talkTopSpring, talkTopTarget.localRotation);
		talkBottomSource.localRotation = SemiFunc.SpringQuaternionGet(talkBottomSpring, talkBottomTarget.localRotation);
	}

	[PunRPC]
	private void UpdateStateRPC(State _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
		}
	}

	private void FuseSet(bool _active, float _lerp)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("FuseRPC", RpcTarget.All, _active, _lerp);
		}
		else
		{
			FuseRPC(_active, _lerp);
		}
	}

	[PunRPC]
	private void FuseRPC(bool _active, float _lerp, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			fuseActive = _active;
			fuseLerp = _lerp;
		}
	}

	[PunRPC]
	private void ExplodeRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			ParticleSystem[] array = deathEffects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
			explosionScript = particleScriptExplosion.Spawn(enemy.CenterTransform.position, 1f, 30, 25, 2f);
			explosionScript.HurtCollider.onImpactEnemy.AddListener(OnExplodeHitEnemy);
		}
	}

	[PunRPC]
	private void TalkBreakerRPC(bool _idle, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			if (_idle)
			{
				anim.soundIdleBreaker.Play(talkSource.transform.position);
			}
			else
			{
				anim.soundAttackBreaker.Play(talkSource.transform.position);
			}
		}
	}

	[PunRPC]
	private void ExplosionTellRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.StunLoopPause(2f);
			anim.soundExplosionTell.Play(talkSource.transform.position);
		}
	}

	[PunRPC]
	public void SetHeadRPC(int _index, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		int num = 0;
		GameObject[] array = headObjects;
		foreach (GameObject gameObject in array)
		{
			if (num == _index)
			{
				gameObject.SetActive(value: true);
			}
			else
			{
				Object.Destroy(gameObject);
			}
			num++;
		}
	}

	[PunRPC]
	public void SetVoicePitchRPC(float _pitch, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.soundAttackBreaker.Pitch = _pitch;
			anim.soundIdleBreaker.Pitch = _pitch;
			anim.soundHurt.Pitch = _pitch;
			anim.soundDeathVO.Pitch = _pitch;
			anim.soundExplosionTell.Pitch = _pitch;
			anim.soundFuseTell.Pitch = _pitch;
			anim.soundJumpVO.Pitch = _pitch;
			anim.soundLandVO.Pitch = _pitch;
			anim.soundStunIntro.Pitch = _pitch;
			anim.soundStunLoop.Pitch = _pitch;
			anim.soundStunOutro.Pitch = _pitch;
		}
	}
}
