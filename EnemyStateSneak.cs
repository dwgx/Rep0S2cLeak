using UnityEngine;

public class EnemyStateSneak : MonoBehaviour
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

	private PlayerAvatar TargetPlayer;

	private void Start()
	{
		Enemy = GetComponent<Enemy>();
		Player = PlayerController.instance;
	}

	private void Update()
	{
		if (Enemy.CurrentState != EnemyState.Sneak)
		{
			if (Active)
			{
				Active = false;
			}
			return;
		}
		if (!Active)
		{
			TargetPlayer = PlayerController.instance.playerAvatarScript;
			if (GameManager.instance.gameMode == 1)
			{
				foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
				{
					if (!player.isDisabled && player.photonView.ViewID == Enemy.TargetPlayerViewID)
					{
						TargetPlayer = player;
						break;
					}
				}
			}
			StateTimer = Random.Range(StateTimeMin, StateTimeMax);
			Active = true;
		}
		if (Enemy.MasterClient)
		{
			Enemy.NavMeshAgent.UpdateAgent(Speed, Acceleration);
			Enemy.NavMeshAgent.SetDestination(TargetPlayer.transform.position);
			if (Enemy.HasRigidbody)
			{
				Enemy.Rigidbody.IdleSet(0.1f);
			}
			if (Enemy.Vision.VisionsTriggered[Enemy.TargetPlayerAvatar.photonView.ViewID] >= Enemy.Vision.VisionsToTrigger)
			{
				StateTimer = Random.Range(StateTimeMin, StateTimeMax);
			}
			StateTimer -= Time.deltaTime;
			if (StateTimer <= 0f)
			{
				Enemy.CurrentState = EnemyState.Roaming;
			}
		}
	}
}
