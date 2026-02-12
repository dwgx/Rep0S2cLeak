using Photon.Pun;
using UnityEngine;

public class PlayerBattery : MonoBehaviour
{
	public PlayerAvatar playerAvatar;

	private PhotonView photonView;

	private StaticGrabObject staticGrabObject;

	private bool masterCharging;

	private bool isLocal;

	private bool chargeBattery;

	private float chargeRate = 0.5f;

	private float chargeTimer;

	private int amountPlayersGrabbing;

	private int amountPlayersGrabbingPrevious;

	public Transform batteryPlacement;

	public Sound batteryChargeSound;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		staticGrabObject = GetComponent<StaticGrabObject>();
	}

	private void Update()
	{
		if ((isLocal || playerAvatar.isLocal) && !isLocal)
		{
			GetComponent<Collider>().enabled = false;
			GetComponent<MeshRenderer>().enabled = false;
			isLocal = true;
		}
		base.transform.position = batteryPlacement.position;
		base.transform.rotation = batteryPlacement.rotation;
		if (PhotonNetwork.IsMasterClient)
		{
			if (staticGrabObject.playerGrabbing.Count > 0 && !masterCharging)
			{
				masterCharging = true;
				photonView.RPC("BatteryChargeStart", RpcTarget.All);
			}
			if (staticGrabObject.playerGrabbing.Count <= 0 && masterCharging)
			{
				masterCharging = false;
				photonView.RPC("BatteryChargeEnd", RpcTarget.All);
			}
		}
		if (!chargeBattery)
		{
			return;
		}
		if (chargeTimer < chargeRate)
		{
			chargeTimer += Time.deltaTime;
			return;
		}
		batteryChargeSound.Play(base.transform.position);
		if (PhotonNetwork.IsMasterClient)
		{
			foreach (PhysGrabber item in staticGrabObject.playerGrabbing)
			{
				_ = item;
			}
			amountPlayersGrabbing = staticGrabObject.playerGrabbing.Count;
			if (amountPlayersGrabbing != amountPlayersGrabbingPrevious)
			{
				photonView.RPC("UpdateAmountPlayersGrabbing", RpcTarget.Others, amountPlayersGrabbing);
				amountPlayersGrabbingPrevious = amountPlayersGrabbing;
			}
		}
		if (playerAvatar.isLocal)
		{
			PlayerController.instance.EnergyCurrent += 1f * (float)amountPlayersGrabbing;
		}
		chargeTimer = 0f;
	}

	[PunRPC]
	private void UpdateAmountPlayersGrabbing(int amount)
	{
		amountPlayersGrabbing = amount;
	}

	[PunRPC]
	private void BatteryChargeStart()
	{
		chargeBattery = true;
	}

	[PunRPC]
	private void BatteryChargeEnd()
	{
		chargeBattery = false;
	}
}
