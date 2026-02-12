using Photon.Pun;
using UnityEngine;

public class EnemyStateLookUnder : MonoBehaviourPunCallbacks, IPunObservable
{
	private Enemy Enemy;

	private PlayerController Player;

	private bool Active;

	public float Speed;

	public float Acceleration;

	[Space]
	public float WaitTimerMin;

	public float WaitTimerMax;

	internal float WaitTimer = 999f;

	internal bool WaitDone;

	[Space]
	public float LookTimerMin;

	public float LookTimerMax;

	private float LookTimer = 999f;

	private void Start()
	{
		Enemy = GetComponent<Enemy>();
		Player = PlayerController.instance;
	}

	private void Update()
	{
		if (Enemy.CurrentState != EnemyState.LookUnder)
		{
			if (Active)
			{
				WaitDone = false;
				Active = false;
			}
			return;
		}
		if (!Active)
		{
			WaitDone = false;
			WaitTimer = Random.Range(WaitTimerMin, WaitTimerMax);
			LookTimer = Random.Range(LookTimerMin, LookTimerMax);
			Active = true;
		}
		if (!Enemy.MasterClient)
		{
			return;
		}
		Enemy.SetChaseTimer();
		Enemy.NavMeshAgent.SetDestination(Enemy.StateChase.SawPlayerNavmeshPosition);
		Enemy.NavMeshAgent.UpdateAgent(Speed, Acceleration);
		if (!WaitDone)
		{
			if (Vector3.Distance(base.transform.position, Enemy.StateChase.SawPlayerNavmeshPosition) < 0.1f)
			{
				WaitTimer -= Time.deltaTime;
				if (WaitTimer <= 0f)
				{
					WaitDone = true;
				}
			}
		}
		else
		{
			LookTimer -= Time.deltaTime;
			if (LookTimer <= 0f)
			{
				Enemy.CurrentState = EnemyState.ChaseSlow;
			}
		}
		if (Enemy.Vision.VisionTriggered[Enemy.TargetPlayerAvatar.photonView.ViewID] && Enemy.StateChase.SawPlayerNavmeshPosition != Enemy.TargetPlayerAvatar.LastNavmeshPosition)
		{
			Enemy.CurrentState = EnemyState.Chase;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(WaitDone);
			}
			else
			{
				WaitDone = (bool)stream.ReceiveNext();
			}
		}
	}
}
