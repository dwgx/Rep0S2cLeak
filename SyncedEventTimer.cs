using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class SyncedEventTimer : MonoBehaviour
{
	private PhotonView photonView;

	private float timer;

	public float timerMin = 4f;

	public float timerMax = 5f;

	public UnityEvent onTimerStart;

	public UnityEvent onTimerEnd;

	public UnityEvent onTimerTick;

	private bool singlePlayer;

	private bool isMaster;

	private bool timerActive;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		if (GameManager.instance.gameMode == 0)
		{
			singlePlayer = true;
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			isMaster = true;
		}
	}

	public void StartTimer()
	{
		if (singlePlayer || isMaster)
		{
			timer = Random.Range(timerMin, timerMax);
			onTimerStart.Invoke();
			StartCoroutine(Timer());
			timerActive = true;
			if (isMaster)
			{
				photonView.RPC("StartTimerRPC", RpcTarget.Others);
			}
		}
	}

	[PunRPC]
	private void StartTimerRPC()
	{
		timerActive = true;
		onTimerStart.Invoke();
	}

	private IEnumerator Timer()
	{
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
		}
		EndTimer();
		if (isMaster)
		{
			photonView.RPC("EndTimerRPC", RpcTarget.Others);
		}
	}

	private void Update()
	{
		if (timerActive)
		{
			onTimerTick.Invoke();
		}
	}

	[PunRPC]
	private void EndTimerRPC()
	{
		timerActive = false;
		onTimerEnd.Invoke();
	}

	public void EndTimer()
	{
		timerActive = false;
		onTimerEnd.Invoke();
	}
}
