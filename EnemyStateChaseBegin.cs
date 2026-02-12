using UnityEngine;

public class EnemyStateChaseBegin : MonoBehaviour
{
	private Enemy Enemy;

	private PlayerController Player;

	[HideInInspector]
	public bool Active;

	[Space]
	public float StateTimeMin;

	public float StateTimeMax;

	private float StateTimer;

	[Space]
	internal PlayerAvatar TargetPlayer;

	[HideInInspector]
	public bool LocalEffect;

	[Space]
	public bool Stinger;

	private void Start()
	{
		Enemy = GetComponent<Enemy>();
		Player = PlayerController.instance;
	}

	private void Update()
	{
		if (Enemy.CurrentState != EnemyState.ChaseBegin)
		{
			if (Active)
			{
				Active = false;
			}
			return;
		}
		if (!Active)
		{
			if (Enemy.MasterClient)
			{
				Enemy.StateChase.ChaseCanReach = true;
				Enemy.NavMeshAgent.ResetPath();
				StateTimer = Random.Range(StateTimeMin, StateTimeMax);
			}
			TargetPlayer = PlayerController.instance.playerAvatarScript;
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (!player.isDisabled && player.photonView.ViewID == Enemy.TargetPlayerViewID)
				{
					TargetPlayer = player;
					break;
				}
			}
			foreach (PlayerAvatar player2 in GameDirector.instance.PlayerList)
			{
				if (player2.isDisabled || !player2.isLocal)
				{
					continue;
				}
				if (GameManager.instance.gameMode == 0 || TargetPlayer == player2 || Enemy.PlayerRoom.SameLocal || Enemy.OnScreen.OnScreenLocal)
				{
					LocalEffect = true;
					GameDirector.instance.CameraImpact.Shake(5f, 0.25f);
					GameDirector.instance.CameraShake.Shake(3f, 0.5f);
					if (Stinger)
					{
						CameraGlitch.Instance.PlayShort();
						AudioScare.instance.PlayImpact();
					}
				}
				else
				{
					LocalEffect = false;
					GameDirector.instance.CameraImpact.ShakeDistance(5f, 5f, 10f, base.transform.position, 0.25f);
					GameDirector.instance.CameraShake.ShakeDistance(3f, 5f, 10f, base.transform.position, 0.5f);
				}
				break;
			}
			Active = true;
		}
		Enemy.SetChaseTimer();
		if (Enemy.MasterClient)
		{
			Enemy.NavMeshAgent.UpdateAgent(0f, 5f);
			Enemy.NavMeshAgent.Stop(0.1f);
			base.transform.LookAt(TargetPlayer.transform.position);
			base.transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
			StateTimer -= Time.deltaTime;
			if (StateTimer <= 0f)
			{
				Enemy.CurrentState = EnemyState.Chase;
			}
		}
	}
}
