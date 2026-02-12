using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyGnome : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		NoticeDelay,
		Notice,
		Move,
		MoveUnder,
		MoveOver,
		MoveBack,
		AttackMove,
		Attack,
		AttackDone,
		Stun,
		Despawn
	}

	[Space]
	public State currentState;

	private bool stateImpulse;

	private float stateTimer;

	private State attackMoveState;

	[Space]
	public Enemy enemy;

	public EnemyGnomeAnim enemyGnomeAnim;

	private PhotonView photonView;

	internal int directorIndex;

	[Space]
	public SpringQuaternion rotationSpring;

	private Quaternion rotationTarget;

	[Space]
	public BoxCollider avoidCollider;

	private float avoidTimer;

	private Vector3 avoidForce;

	[Space]
	public float speedMin = 1f;

	public float speedMax = 2f;

	[Space]
	public Transform backAwayOffset;

	public Transform moveOffsetTransform;

	public Transform rotationTransform;

	private float moveOffsetTimer;

	private float moveOffsetSetTimer;

	private Vector3 moveOffsetPosition;

	private float attackAngle;

	private Vector3 moveBackPosition;

	private float moveBackTimer;

	private bool visionPrevious;

	private float visionTimer;

	internal float attackCooldown;

	private float idleBreakerTimer;

	internal float overlapCheckTimer;

	internal float overlapCheckCooldown;

	internal bool overlapCheckPrevious;

	[Space]
	public ParticleSystem[] deathEffects;

	[Space]
	public Sound soundHurt;

	public Sound soundDeath;

	[Space]
	public Sound soundImpactLight;

	public Sound soundImpactMedium;

	public Sound soundImpactHeavy;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		enemy.NavMeshAgent.DefaultSpeed = Random.Range(speedMin, speedMax);
		enemy.NavMeshAgent.Agent.speed = enemy.NavMeshAgent.DefaultSpeed;
		if (EnemyGnomeDirector.instance.setup)
		{
			EnemyGnomeDirector.instance.SetupSingle(this);
		}
	}

	private void Update()
	{
		if ((bool)EnemyGnomeDirector.instance && EnemyGnomeDirector.instance.setup && SemiFunc.IsMasterClientOrSingleplayer())
		{
			AvoidLogic();
			RotationLogic();
			BackAwayOffsetLogic();
			MoveOffsetLogic();
			TimerLogic();
			if (enemy.CurrentState == EnemyState.Despawn)
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
			case State.Move:
				StateMove();
				break;
			case State.Notice:
				StateNotice();
				break;
			case State.NoticeDelay:
				StateNoticeDelay();
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
			case State.AttackMove:
				StateAttackMove();
				break;
			case State.Attack:
				StateAttack();
				break;
			case State.AttackDone:
				StateAttackDone();
				break;
			case State.Stun:
				StateStun();
				break;
			case State.Despawn:
				StateDespawn();
				break;
			}
		}
	}

	private void FixedUpdate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && avoidForce != Vector3.zero)
		{
			enemy.Rigidbody.rb.AddForce(avoidForce * 2f, ForceMode.Force);
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
			stateImpulse = false;
		}
		enemy.Rigidbody.DisableFollowPosition(0.1f, 0.5f);
		IdleBreakerLogic();
		if (EnemyGnomeDirector.instance.currentState == EnemyGnomeDirector.State.AttackPlayer || EnemyGnomeDirector.instance.currentState == EnemyGnomeDirector.State.AttackValuable)
		{
			UpdateState(State.NoticeDelay);
		}
		else if (Vector3.Distance(enemy.Rigidbody.transform.position, EnemyGnomeDirector.instance.destinations[directorIndex]) > 2f)
		{
			UpdateState(State.Move);
		}
	}

	private void StateMove()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
		}
		enemy.NavMeshAgent.SetDestination(EnemyGnomeDirector.instance.destinations[directorIndex]);
		MoveBackPosition();
		MoveOffsetSet();
		SemiFunc.EnemyCartJump(enemy);
		if (EnemyGnomeDirector.instance.currentState == EnemyGnomeDirector.State.AttackPlayer || EnemyGnomeDirector.instance.currentState == EnemyGnomeDirector.State.AttackValuable)
		{
			UpdateState(State.NoticeDelay);
		}
		else if (Vector3.Distance(enemy.Rigidbody.transform.position, EnemyGnomeDirector.instance.destinations[directorIndex]) <= 0.2f)
		{
			UpdateState(State.Idle);
		}
		else if (Vector3.Distance(enemy.Rigidbody.transform.position, EnemyGnomeDirector.instance.destinations[directorIndex]) <= 2f)
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.Idle);
			}
		}
	}

	private void StateNoticeDelay()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = Random.Range(0f, 1f);
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Notice);
		}
	}

	private void StateNotice()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			stateImpulse = false;
			stateTimer = 0.5f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.AttackMove);
		}
	}

	private void StateAttackMove()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		Vector3 destination = AttackPositionLogic();
		enemy.NavMeshAgent.SetDestination(destination);
		bool flag = EnemyGnomeDirector.instance.CanAttack(this);
		MoveBackPosition();
		MoveOffsetSet();
		SemiFunc.EnemyCartJump(enemy);
		if (EnemyGnomeDirector.instance.currentState != EnemyGnomeDirector.State.AttackPlayer && EnemyGnomeDirector.instance.currentState != EnemyGnomeDirector.State.AttackValuable)
		{
			UpdateState(State.Move);
		}
		else if (flag)
		{
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.Attack);
		}
		else
		{
			if (enemy.NavMeshAgent.CanReach(AttackVisionDynamic(), 1f) || !(Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) < 2f))
			{
				return;
			}
			if (AttackPositionLogic().y > enemy.Rigidbody.transform.position.y + 0.2f)
			{
				enemy.Jump.StuckTrigger(AttackVisionPosition() - enemy.Vision.VisionTransform.position);
			}
			if (!VisionBlocked() && !NavMesh.SamplePosition(AttackVisionDynamic(), out var _, 0.5f, -1))
			{
				if (Mathf.Abs(AttackVisionDynamic().y - enemy.Rigidbody.transform.position.y) < 0.2f)
				{
					UpdateState(State.MoveUnder);
				}
				else if (AttackPositionLogic().y > enemy.Rigidbody.transform.position.y)
				{
					UpdateState(State.MoveOver);
				}
			}
		}
	}

	private void StateMoveUnder()
	{
		if (stateImpulse)
		{
			stateTimer = 2f;
			stateImpulse = false;
		}
		bool flag = EnemyGnomeDirector.instance.CanAttack(this);
		Vector3 vector = AttackPositionLogic();
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, vector, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
		MoveOffsetSet();
		SemiFunc.EnemyCartJump(enemy);
		NavMeshHit hit;
		if (EnemyGnomeDirector.instance.currentState != EnemyGnomeDirector.State.AttackPlayer && EnemyGnomeDirector.instance.currentState != EnemyGnomeDirector.State.AttackValuable)
		{
			UpdateState(State.MoveBack);
		}
		else if (flag)
		{
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.Attack);
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
			EnemyGnomeDirector.instance.SeeTarget();
			stateTimer = 2f;
		}
	}

	private void StateMoveOver()
	{
		if (stateImpulse)
		{
			stateTimer = 2f;
			stateImpulse = false;
		}
		bool flag = EnemyGnomeDirector.instance.CanAttack(this);
		Vector3 vector = AttackPositionLogic();
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, vector, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
		MoveOffsetSet();
		SemiFunc.EnemyCartJump(enemy);
		if (AttackVisionDynamic().y > enemy.Rigidbody.transform.position.y + 0.2f && !flag)
		{
			enemy.Jump.StuckTrigger(AttackVisionDynamic() - enemy.Rigidbody.transform.position);
			base.transform.position = Vector3.MoveTowards(base.transform.position, AttackVisionDynamic(), 2f);
		}
		NavMeshHit hit;
		if (EnemyGnomeDirector.instance.currentState != EnemyGnomeDirector.State.AttackPlayer && EnemyGnomeDirector.instance.currentState != EnemyGnomeDirector.State.AttackValuable)
		{
			UpdateState(State.MoveBack);
		}
		else if (flag)
		{
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.Attack);
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
			EnemyGnomeDirector.instance.SeeTarget();
			stateTimer = 2f;
		}
	}

	private void StateMoveBack()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		if (!enemy.Jump.jumping)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, moveBackPosition, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
		}
		MoveOffsetSet();
		SemiFunc.EnemyCartJump(enemy);
		stateTimer -= Time.deltaTime;
		bool num = EnemyGnomeDirector.instance.CanAttack(this);
		if (stateTimer <= 0f && (Vector3.Distance(base.transform.position, enemy.Rigidbody.transform.position) > 2f || enemy.Rigidbody.notMovingTimer > 2f) && !enemy.Jump.jumping)
		{
			Vector3 normalized = (base.transform.position - moveBackPosition).normalized;
			enemy.Jump.StuckTrigger(base.transform.position - moveBackPosition);
			base.transform.position = enemy.Rigidbody.transform.position;
			base.transform.position += normalized * 2f;
		}
		NavMeshHit hit;
		if (num)
		{
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.Attack);
		}
		else if (Vector3.Distance(enemy.Rigidbody.transform.position, moveBackPosition) <= 0.2f)
		{
			UpdateState(State.AttackMove);
		}
		else if (NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out hit, 0.5f, -1))
		{
			UpdateState(State.AttackMove);
		}
	}

	private void StateAttack()
	{
		if (stateImpulse)
		{
			enemy.NavMeshAgent.ResetPath();
			enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			stateTimer = 3f;
			stateImpulse = false;
		}
		if (stateTimer > 0.5f && !enemyGnomeAnim.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
		{
			UpdateState(State.AttackMove);
			return;
		}
		enemy.StuckCount = 0;
		enemy.Rigidbody.DisableFollowPosition(0.1f, 1f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.AttackDone);
		}
	}

	private void StateAttackDone()
	{
		if (stateImpulse)
		{
			stateTimer = 1f;
			stateImpulse = false;
		}
		enemy.StuckCount = 0;
		enemy.Rigidbody.DisableFollowPosition(0.1f, 5f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			moveBackTimer = 2f;
			attackCooldown = 2f;
			if (NavMesh.SamplePosition(enemy.Rigidbody.transform.position, out var _, 0.5f, -1))
			{
				UpdateState(State.AttackMove);
			}
			else
			{
				UpdateState(attackMoveState);
			}
		}
	}

	private void StateStun()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
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
			stateImpulse = false;
		}
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			EnemyGnomeDirector.instance.OnSpawn(this);
			UpdateState(State.Spawn);
		}
	}

	public void OnHurt()
	{
		soundHurt.Play(enemy.CenterTransform.position);
	}

	public void OnDeath()
	{
		soundDeath.Play(enemy.CenterTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.05f);
		ParticleSystem[] array = deathEffects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnInvestigate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			EnemyGnomeDirector.instance.Investigate(enemy.StateInvestigate.onInvestigateTriggeredPosition);
		}
	}

	public void OnVision()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			EnemyGnomeDirector.instance.SetTarget(enemy.Vision.onVisionTriggeredPlayer);
		}
	}

	public void OnImpactLight()
	{
		if (enemy.IsStunned())
		{
			soundImpactLight.Play(enemy.CenterTransform.position);
		}
	}

	public void OnImpactMedium()
	{
		if (enemy.IsStunned())
		{
			soundImpactMedium.Play(enemy.CenterTransform.position);
		}
	}

	public void OnImpactHeavy()
	{
		if (enemy.IsStunned())
		{
			soundImpactHeavy.Play(enemy.CenterTransform.position);
		}
	}

	public void UpdateState(State _state)
	{
		if (currentState != _state)
		{
			if (_state == State.Attack)
			{
				attackMoveState = currentState;
			}
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
		if (currentState == State.Move || currentState == State.Notice || currentState == State.AttackMove || currentState == State.MoveUnder || currentState == State.MoveOver || currentState == State.MoveBack || currentState == State.Attack)
		{
			if (currentState == State.Notice || ((currentState == State.AttackMove || currentState == State.MoveUnder || currentState == State.MoveOver || currentState == State.Attack) && Vector3.Distance(enemy.Rigidbody.transform.position, EnemyGnomeDirector.instance.attackPosition) < 5f))
			{
				Quaternion rotation = rotationTransform.rotation;
				rotationTransform.rotation = Quaternion.LookRotation(AttackVisionPosition() - enemy.Rigidbody.transform.position);
				rotationTransform.eulerAngles = new Vector3(0f, rotationTransform.eulerAngles.y, 0f);
				Quaternion rotation2 = rotationTransform.rotation;
				rotationTransform.rotation = rotation;
				rotationTarget = rotation2;
			}
			else if (enemy.Rigidbody.rb.velocity.magnitude > 0.1f)
			{
				Vector3 position = rotationTransform.position;
				Quaternion rotation3 = rotationTransform.rotation;
				rotationTransform.position = enemy.Rigidbody.transform.position;
				rotationTransform.rotation = Quaternion.LookRotation(enemy.Rigidbody.rb.velocity.normalized);
				rotationTransform.eulerAngles = new Vector3(0f, rotationTransform.eulerAngles.y, 0f);
				Quaternion rotation4 = rotationTransform.rotation;
				rotationTransform.position = position;
				rotationTransform.rotation = rotation3;
				rotationTarget = rotation4;
			}
		}
		else if (currentState == State.Idle && Vector3.Distance(EnemyGnomeDirector.instance.transform.position, enemy.Rigidbody.transform.position) > 0.1f)
		{
			Quaternion rotation5 = rotationTransform.rotation;
			rotationTransform.rotation = Quaternion.LookRotation(EnemyGnomeDirector.instance.transform.position - enemy.Rigidbody.transform.position);
			rotationTransform.eulerAngles = new Vector3(0f, rotationTransform.eulerAngles.y, 0f);
			Quaternion rotation6 = rotationTransform.rotation;
			rotationTransform.rotation = rotation5;
			rotationTarget = rotation6;
		}
		rotationTransform.rotation = SemiFunc.SpringQuaternionGet(rotationSpring, rotationTarget);
	}

	private void AvoidLogic()
	{
		if (currentState == State.Move || currentState == State.AttackMove || currentState == State.MoveUnder || currentState == State.MoveOver)
		{
			if (avoidTimer <= 0f)
			{
				avoidForce = Vector3.zero;
				avoidTimer = 0.25f;
				if (enemy.Jump.jumping)
				{
					return;
				}
				Collider[] array = Physics.OverlapBox(avoidCollider.transform.position, avoidCollider.size / 2f, avoidCollider.transform.rotation, LayerMask.GetMask("PhysGrabObject"));
				for (int i = 0; i < array.Length; i++)
				{
					EnemyRigidbody componentInParent = array[i].GetComponentInParent<EnemyRigidbody>();
					if ((bool)componentInParent)
					{
						EnemyGnome component = componentInParent.enemy.GetComponent<EnemyGnome>();
						if ((bool)component)
						{
							avoidForce += (base.transform.position - component.transform.position).normalized.normalized;
						}
					}
				}
			}
			else
			{
				avoidTimer -= Time.deltaTime;
			}
		}
		else
		{
			avoidForce = Vector3.zero;
		}
	}

	private Vector3 AttackPositionLogic()
	{
		Vector3 result = EnemyGnomeDirector.instance.attackPosition + new Vector3(Mathf.Cos(attackAngle), 0f, Mathf.Sin(attackAngle)) * 0.7f;
		attackAngle += Time.deltaTime * 1f;
		return result;
	}

	private Vector3 AttackVisionPosition()
	{
		return EnemyGnomeDirector.instance.attackVisionPosition;
	}

	private Vector3 AttackVisionDynamic()
	{
		if (EnemyGnomeDirector.instance.currentState == EnemyGnomeDirector.State.AttackPlayer)
		{
			return AttackPositionLogic();
		}
		return AttackVisionPosition();
	}

	private void MoveBackPosition()
	{
		if (Vector3.Distance(base.transform.position, enemy.Rigidbody.transform.position) < 1f)
		{
			moveBackPosition = base.transform.position;
		}
	}

	private void BackAwayOffsetLogic()
	{
		if (moveBackTimer > 0f)
		{
			moveBackTimer -= Time.deltaTime;
			backAwayOffset.localPosition = Vector3.Lerp(backAwayOffset.localPosition, new Vector3(0f, 0f, -1f), Time.deltaTime * 10f);
		}
		else
		{
			backAwayOffset.localPosition = Vector3.Lerp(backAwayOffset.localPosition, Vector3.zero, Time.deltaTime * 10f);
		}
	}

	private void MoveOffsetLogic()
	{
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
					Vector3 vector = Random.insideUnitSphere * 0.5f;
					vector.y = 0f;
					moveOffsetPosition = vector;
					moveOffsetSetTimer = Random.Range(0.2f, 1f);
				}
			}
		}
		moveOffsetTransform.localPosition = Vector3.Lerp(moveOffsetTransform.localPosition, moveOffsetPosition, Time.deltaTime * 20f);
	}

	private void MoveOffsetSet()
	{
		moveOffsetTimer = 0.2f;
	}

	private bool VisionBlocked()
	{
		if (visionTimer <= 0f)
		{
			visionTimer = 0.1f;
			Vector3 direction = AttackVisionPosition() - enemy.Vision.VisionTransform.position;
			visionPrevious = Physics.Raycast(enemy.Vision.VisionTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
		}
		return visionPrevious;
	}

	private void TimerLogic()
	{
		visionTimer -= Time.deltaTime;
		attackCooldown -= Time.deltaTime;
		overlapCheckTimer -= Time.deltaTime;
		if (overlapCheckCooldown > 0f)
		{
			overlapCheckCooldown -= Time.deltaTime;
			if (overlapCheckCooldown <= 0f)
			{
				overlapCheckPrevious = false;
			}
		}
	}

	public void IdleBreakerLogic()
	{
		bool flag = false;
		foreach (EnemyGnome gnome in EnemyGnomeDirector.instance.gnomes)
		{
			if ((bool)gnome && gnome != this && Vector3.Distance(base.transform.position, gnome.transform.position) < 2f)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		if (idleBreakerTimer <= 0f)
		{
			idleBreakerTimer = Random.Range(2f, 15f);
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("IdleBreakerRPC", RpcTarget.All);
			}
			else
			{
				IdleBreakerRPC();
			}
		}
		else
		{
			idleBreakerTimer -= Time.deltaTime;
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
				enemyGnomeAnim.OnSpawn();
			}
		}
	}

	[PunRPC]
	private void IdleBreakerRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			enemyGnomeAnim.idleBreakerImpulse = true;
		}
	}
}
