using System.Collections;
using Photon.Pun;
using UnityEngine;

public class TruckScreenOpen : MonoBehaviour
{
	public AnimationCurve openScreenCurve;

	private float openScreenCurveTimer;

	private float openScreenYPosOriginal;

	private bool openScreenActive;

	private bool doorDone;

	private bool doorLoopPlaying;

	public Sound doorLoop;

	public Sound doorLoopStart;

	public Sound doorLoopEnd;

	public Sound doorSound;

	private ParticleSystem doorParticles;

	private bool doorClose;

	private float doorOpenPosition = 4.13f;

	private PhotonView photonView;

	private void Start()
	{
		openScreenYPosOriginal = base.transform.localPosition.y;
		doorParticles = GetComponentInChildren<ParticleSystem>();
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, openScreenYPosOriginal + doorOpenPosition, base.transform.localPosition.z);
		StartCoroutine(DelayedClose());
		photonView = GetComponent<PhotonView>();
	}

	private void TruckScreenOpenStartLogic()
	{
		openScreenActive = true;
		GameDirector.instance.CameraImpact.ShakeDistance(6f, 3f, 8f, base.transform.position, 0.2f);
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, openScreenYPosOriginal, base.transform.localPosition.z);
		openScreenCurveTimer = 0f;
		doorLoopStart.Play(base.transform.position);
		doorLoopPlaying = true;
		doorDone = false;
		doorParticles.Play();
		doorClose = false;
	}

	public void TruckScreenOpenStart()
	{
		if (GameManager.Multiplayer())
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				photonView.RPC("TruckScreenOpenStartRPC", RpcTarget.All);
			}
		}
		else
		{
			TruckScreenOpenStartLogic();
		}
	}

	[PunRPC]
	private void TruckScreenOpenStartRPC()
	{
		TruckScreenOpenStartLogic();
	}

	private void TruckScreenCloseStart()
	{
		openScreenActive = true;
		GameDirector.instance.CameraImpact.ShakeDistance(6f, 3f, 8f, base.transform.position, 0.2f);
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, openScreenYPosOriginal + doorOpenPosition, base.transform.localPosition.z);
		openScreenCurveTimer = 0f;
		doorLoopStart.Play(base.transform.position);
		doorLoopPlaying = true;
		doorDone = false;
		doorParticles.Play();
		doorClose = true;
	}

	private IEnumerator DelayedClose()
	{
		yield return new WaitForSeconds(2f);
		TruckScreenCloseStart();
	}

	private IEnumerator DelayedLevelSwitch()
	{
		yield return new WaitForSeconds(2f);
		RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false);
	}

	private void Update()
	{
		doorLoop.PlayLoop(doorLoopPlaying, 2f, 2f);
		if (!openScreenActive)
		{
			return;
		}
		if (openScreenCurveTimer < 1f)
		{
			openScreenCurveTimer += Time.deltaTime;
			float time = openScreenCurveTimer;
			if (doorClose)
			{
				time = 1f - openScreenCurveTimer;
			}
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, openScreenYPosOriginal + openScreenCurve.Evaluate(time) * doorOpenPosition, base.transform.localPosition.z);
			if (!(openScreenCurveTimer > 0.8f) || doorDone)
			{
				return;
			}
			GameDirector.instance.CameraImpact.ShakeDistance(6f, 3f, 8f, base.transform.position, 0.1f);
			doorDone = true;
			doorLoopEnd.Play(base.transform.position);
			doorSound.Play(base.transform.position);
			doorLoopPlaying = false;
			doorParticles.Play();
			if (!doorClose)
			{
				if (!GameManager.Multiplayer())
				{
					StartCoroutine(DelayedLevelSwitch());
				}
				else if (SemiFunc.IsMasterClientOrSingleplayer())
				{
					StartCoroutine(DelayedLevelSwitch());
				}
			}
		}
		else
		{
			openScreenActive = false;
		}
	}
}
