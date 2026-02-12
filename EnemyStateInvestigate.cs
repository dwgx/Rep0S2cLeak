using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class EnemyStateInvestigate : MonoBehaviour
{
	private Enemy Enemy;

	private PlayerController Player;

	private bool Active;

	private EnemyStateRoaming Roaming;

	private PhotonView PhotonView;

	public float rangeMultiplier = 1f;

	[Space]
	public bool OnlyEvent = true;

	private bool PhysObjectHitImpulse;

	private int PhysObjectHitCount;

	public int PhysObjectHitMax = 3;

	[Header("Movement")]
	public float Speed;

	public float Acceleration;

	[Header("Event")]
	public UnityEvent onInvestigateTriggered;

	internal Vector3 onInvestigateTriggeredPosition;

	internal bool onInvestigateTriggeredPathfindOnly;

	private LevelPoint InvestigateLevelPoint;

	private void Awake()
	{
		PhotonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		Enemy = GetComponent<Enemy>();
		Player = PlayerController.instance;
		Roaming = GetComponent<EnemyStateRoaming>();
	}

	private void Update()
	{
		if (!Enemy.MasterClient)
		{
			return;
		}
		if (Enemy.CurrentState != EnemyState.Investigate)
		{
			if (Active)
			{
				Active = false;
			}
			return;
		}
		if (!Active)
		{
			PhysObjectHitImpulse = true;
			PhysObjectHitCount = 0;
			Active = true;
		}
		Enemy.NavMeshAgent.UpdateAgent(Speed, Acceleration);
		if (Enemy.HasRigidbody)
		{
			Enemy.Rigidbody.IdleSet(0.1f);
		}
		bool flag = Enemy.AttackStuckPhysObject.Check();
		if (Enemy.AttackStuckPhysObject.Active)
		{
			if (PhysObjectHitImpulse)
			{
				PhysObjectHitCount++;
				PhysObjectHitImpulse = false;
			}
		}
		else
		{
			PhysObjectHitImpulse = true;
		}
		if (!Enemy.NavMeshAgent.Agent.hasPath || (flag && !Enemy.AttackStuckPhysObject.Active) || PhysObjectHitCount >= PhysObjectHitMax)
		{
			Enemy.CurrentState = EnemyState.Roaming;
			Roaming.RoamingLevelPoint = InvestigateLevelPoint;
			if (PhysObjectHitCount >= PhysObjectHitMax)
			{
				Roaming.RoamingChangeCurrent = 0;
			}
			else
			{
				Roaming.RoamingChangeCurrent = Random.Range(Roaming.RoamingChangeMin, Roaming.RoamingChangeMax + 1);
			}
		}
	}

	public void Set(Vector3 position, bool _pathFindOnly)
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				PhotonView.RPC("SetRPC", RpcTarget.All, position, _pathFindOnly);
			}
		}
		else
		{
			SetRPC(position, _pathFindOnly);
		}
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (Vector3.Distance(item.transform.position, position) < 1f && Vector3.Distance(item.transform.position, Enemy.transform.position) < 2f)
			{
				Enemy.SetChaseTarget(item);
				break;
			}
		}
		if (OnlyEvent)
		{
			return;
		}
		if (Enemy.CurrentState == EnemyState.Roaming || Enemy.CurrentState == EnemyState.Investigate || Enemy.CurrentState == EnemyState.ChaseEnd)
		{
			Enemy.CurrentState = EnemyState.Investigate;
			float num = float.MaxValue;
			LevelPoint investigateLevelPoint = null;
			foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
			{
				float num2 = Vector3.Distance(position, levelPathPoint.transform.position);
				if (num2 < num)
				{
					num = num2;
					investigateLevelPoint = levelPathPoint;
				}
			}
			InvestigateLevelPoint = investigateLevelPoint;
			Enemy.NavMeshAgent.SetDestination(position);
		}
		else if (Enemy.CurrentState == EnemyState.ChaseSlow)
		{
			Enemy.StateChase.ChasePosition = position;
			Enemy.NavMeshAgent.SetDestination(Enemy.StateChase.ChasePosition);
		}
	}

	[PunRPC]
	public void SetRPC(Vector3 position, bool _pathfindOnly, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			onInvestigateTriggeredPosition = position;
			onInvestigateTriggeredPathfindOnly = _pathfindOnly;
			onInvestigateTriggered.Invoke();
		}
	}
}
