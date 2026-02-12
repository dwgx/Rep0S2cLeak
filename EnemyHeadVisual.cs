using Photon.Pun;
using UnityEngine;

public class EnemyHeadVisual : MonoBehaviour, IPunObservable
{
	public EnemyHeadController Controller;

	public Enemy enemy;

	private float spawnTimer = 1f;

	[Space]
	public Transform FollowPosition;

	public Transform FollowRotation;

	public Transform TargetRotation;

	private float PositionFollowCurrent;

	private float RotationFollowCurrent;

	[Space]
	[Header("Idle")]
	public float PositionFollowIdle;

	public float RotationFollowIdle;

	[Space]
	[Header("Chasing")]
	public float PositionFollowChasing;

	public float RotationFollowChasing;

	private void Update()
	{
		if (enemy.FreezeTimer > 0f)
		{
			return;
		}
		if (enemy.MasterClient)
		{
			if (enemy.CheckChase())
			{
				PositionFollowCurrent = PositionFollowChasing;
				RotationFollowCurrent = RotationFollowChasing;
			}
			else
			{
				PositionFollowCurrent = PositionFollowIdle;
				RotationFollowCurrent = RotationFollowIdle;
			}
		}
		if (spawnTimer > 0f || enemy.TeleportedTimer > 0f)
		{
			base.transform.position = FollowPosition.position;
			TargetRotation.rotation = FollowRotation.rotation;
			if (LevelGenerator.Instance.Generated)
			{
				spawnTimer -= Time.deltaTime;
			}
		}
		else
		{
			base.transform.position = Vector3.Lerp(base.transform.position, FollowPosition.position, PositionFollowCurrent * Time.deltaTime);
			TargetRotation.rotation = Quaternion.Lerp(TargetRotation.rotation, FollowRotation.rotation, RotationFollowCurrent * Time.deltaTime);
		}
	}

	public void Spawn()
	{
		base.transform.position = FollowPosition.position;
		TargetRotation.rotation = FollowRotation.rotation;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(PositionFollowCurrent);
				stream.SendNext(RotationFollowCurrent);
			}
			else
			{
				PositionFollowCurrent = (float)stream.ReceiveNext();
				RotationFollowCurrent = (float)stream.ReceiveNext();
			}
		}
	}
}
