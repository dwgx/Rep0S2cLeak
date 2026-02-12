using Photon.Pun;
using UnityEngine;

public class SyncedEventRandom : MonoBehaviour
{
	[HideInInspector]
	public float resultRandomRangeFloat;

	[HideInInspector]
	public int resultRandomRangeInt;

	[HideInInspector]
	public bool resultReceivedRandomRangeFloat;

	[HideInInspector]
	public bool resultReceivedRandomRangeInt;

	private PhotonView photonView;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
	}

	public void RandomRangeFloat(float min, float max)
	{
		if (GameManager.instance.gameMode == 0)
		{
			resultRandomRangeFloat = Random.Range(min, max);
			resultReceivedRandomRangeFloat = true;
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			resultRandomRangeFloat = Random.Range(min, max);
			resultReceivedRandomRangeFloat = true;
			photonView.RPC("RandomRangeFloatRPC", RpcTarget.Others, resultRandomRangeFloat);
		}
	}

	[PunRPC]
	private void RandomRangeFloatRPC(float result)
	{
		resultRandomRangeFloat = result;
		resultReceivedRandomRangeFloat = true;
	}

	public void RandomRangeInt(int min, int max)
	{
		if (GameManager.instance.gameMode == 0)
		{
			resultRandomRangeInt = Random.Range(min, max);
			resultReceivedRandomRangeInt = true;
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			resultRandomRangeInt = Random.Range(min, max);
			resultReceivedRandomRangeInt = true;
			photonView.RPC("RandomRangeIntRPC", RpcTarget.Others, resultRandomRangeInt);
		}
	}

	[PunRPC]
	private void RandomRangeIntRPC(int result)
	{
		resultRandomRangeInt = result;
		resultReceivedRandomRangeInt = true;
	}
}
