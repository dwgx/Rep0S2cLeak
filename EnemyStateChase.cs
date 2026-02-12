using UnityEngine;

public class EnemyStateChase : MonoBehaviour
{
	private Enemy Enemy;

	private PlayerController Player;

	private bool Active;

	public float Speed;

	public float Acceleration;

	[Space]
	public float StateTimeMin;

	public float StateTimeMax;

	private float StateTimer;

	[Space]
	public float VisionTime;

	[HideInInspector]
	public float VisionTimer;

	public int VisionsToReset;

	[HideInInspector]
	public Vector3 ChasePosition = Vector3.zero;

	[HideInInspector]
	public bool ChaseCanReach = true;

	private bool ChaseCanReachSet;

	private bool SawPlayerHide;

	internal Vector3 SawPlayerNavmeshPosition;

	internal Vector3 SawPlayerHidePosition;

	private float CantReachTime;

	[Space]
	public bool ChaseOnlyOnNavmesh = true;

	private void Awake()
	{
		Enemy = GetComponent<Enemy>();
		Player = PlayerController.instance;
	}

	private void Update()
	{
		if (!Enemy.MasterClient)
		{
			return;
		}
		if (Enemy.CurrentState != EnemyState.Chase)
		{
			if (Active)
			{
				Active = false;
			}
			return;
		}
		if (!Active)
		{
			Enemy.TargetPlayerAvatar.LastNavMeshPositionTimer = 0f;
			ChasePosition = Enemy.TargetPlayerAvatar.transform.position;
			VisionTimer = VisionTime;
			ChaseCanReachSet = false;
			SawPlayerHide = false;
			CantReachTime = 0f;
			StateTimer = Random.Range(StateTimeMin, StateTimeMax);
			Active = true;
		}
		Enemy.SetChaseTimer();
		Enemy.NavMeshAgent.UpdateAgent(Speed, Acceleration);
		if (Enemy.Vision.VisionTriggered[Enemy.TargetPlayerAvatar.photonView.ViewID])
		{
			VisionTimer = VisionTime;
		}
		else if (VisionTimer > 0f)
		{
			VisionTimer -= Time.deltaTime;
		}
		if (VisionTimer > 0f)
		{
			if (ChaseOnlyOnNavmesh || Enemy.TargetPlayerAvatar.LastNavMeshPositionTimer <= 0.25f)
			{
				Enemy.NavMeshAgent.Enable();
				Enemy.NavMeshAgent.SetDestination(Enemy.TargetPlayerAvatar.LastNavmeshPosition);
				if (ChaseCanReachSet)
				{
					Vector3 point = Enemy.NavMeshAgent.GetPoint();
					if (Vector3.Distance(point, Enemy.TargetPlayerAvatar.transform.position) > 0.5f)
					{
						ChaseCanReach = false;
					}
					else
					{
						ChaseCanReach = true;
					}
					if (Enemy.TargetPlayerAvatar.isCrawling && !ChaseCanReach && SemiFunc.EnemyLookUnderCondition(Enemy, StateTimer, 5f, Enemy.TargetPlayerAvatar))
					{
						SawPlayerHidePosition = Enemy.TargetPlayerAvatar.transform.position;
						SawPlayerNavmeshPosition = Enemy.TargetPlayerAvatar.LastNavmeshPosition;
						SawPlayerHide = true;
					}
					ChasePosition = point;
				}
				ChaseCanReachSet = true;
			}
			else
			{
				Enemy.NavMeshAgent.Disable(0.1f);
				base.transform.position = Vector3.MoveTowards(base.transform.position, Enemy.TargetPlayerAvatar.transform.position, Speed * Time.deltaTime);
			}
		}
		else
		{
			if (SawPlayerHide)
			{
				Enemy.CurrentState = EnemyState.LookUnder;
				return;
			}
			Enemy.NavMeshAgent.SetDestination(ChasePosition);
			if (Vector3.Distance(base.transform.position, ChasePosition) < 1f)
			{
				LevelPoint levelPointAhead = Enemy.GetLevelPointAhead(ChasePosition);
				if ((bool)levelPointAhead)
				{
					Enemy.NavMeshAgent.SetDestination(levelPointAhead.transform.position);
				}
				ChasePosition = Enemy.NavMeshAgent.GetDestination();
			}
			ChaseCanReach = true;
			ChaseCanReachSet = false;
		}
		if (ChaseCanReach && Enemy.Vision.VisionsTriggered[Enemy.TargetPlayerAvatar.photonView.ViewID] >= VisionsToReset)
		{
			StateTimer = Random.Range(StateTimeMin, StateTimeMax);
		}
		if (!ChaseCanReach)
		{
			CantReachTime += Time.deltaTime;
			if (CantReachTime > 2f)
			{
				Enemy.Vision.VisionsTriggered[Enemy.TargetPlayerAvatar.photonView.ViewID] = 0;
				Enemy.CurrentState = EnemyState.ChaseSlow;
				return;
			}
		}
		else
		{
			CantReachTime = 0f;
		}
		StateTimer -= Time.deltaTime;
		if (StateTimer <= 0f)
		{
			Enemy.CurrentState = EnemyState.ChaseSlow;
		}
		if (Enemy.TargetPlayerAvatar.isDisabled)
		{
			Enemy.Vision.VisionsTriggered[Enemy.TargetPlayerAvatar.photonView.ViewID] = 0;
			Enemy.CurrentState = EnemyState.Roaming;
		}
	}
}
