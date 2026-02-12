using System.Collections;
using Photon.Pun;
using UnityEngine;

public class FlatScreenTV : MonoBehaviour
{
	private float timer;

	public Transform regularPlane;

	public Sound regularSound;

	public Sound regularSoundGlobal;

	private bool isActive;

	public Transform regularHurtCollider;

	public Transform visionPoint;

	private PhotonView photonView;

	private StaticGrabObject staticGrabObject;

	private bool broken = true;

	public Transform jumpScare;

	public Transform brokenPlane;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		staticGrabObject = GetComponent<StaticGrabObject>();
		StartCoroutine(LateStart());
	}

	private IEnumerator LateStart()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (Random.Range(0, 8) == 0)
			{
				broken = false;
			}
			BrokenOrNot();
		}
	}

	private void BrokenOrNot()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			BrokenOrNotRPC(broken);
			return;
		}
		photonView.RPC("BrokenOrNotRPC", RpcTarget.All, broken);
	}

	[PunRPC]
	public void BrokenOrNotRPC(bool _broken)
	{
		broken = _broken;
		if (broken)
		{
			jumpScare.gameObject.SetActive(value: false);
			brokenPlane.gameObject.SetActive(value: true);
		}
		else
		{
			brokenPlane.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (timer > 0f)
		{
			if (timer < 1.3f && !regularHurtCollider.gameObject.activeSelf)
			{
				regularHurtCollider.gameObject.SetActive(value: true);
			}
			timer -= Time.deltaTime;
			regularPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f + Mathf.Sin(timer * 100f) * 0.1f, 1f + Mathf.Sin(timer * 100f) * 0.1f);
			regularPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2((0f - Mathf.Sin(timer * 100f)) * 0.05f, (0f - Mathf.Sin(timer * 100f)) * 0.05f);
			isActive = true;
		}
		else
		{
			if (isActive)
			{
				regularPlane.gameObject.SetActive(value: false);
				regularHurtCollider.gameObject.SetActive(value: false);
				broken = true;
				BrokenOrNotRPC(broken);
			}
			isActive = false;
		}
	}

	public void actionTime()
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("actionTimeRPC", RpcTarget.All);
			}
		}
		else
		{
			actionTimeRPC();
		}
	}

	[PunRPC]
	public void actionTimeRPC()
	{
		if (!(timer > 0f))
		{
			timer = 1.5f;
			GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.5f);
			GameDirector.instance.CameraImpact.ShakeDistance(8f, 3f, 12f, base.transform.position, 0.1f);
			regularPlane.gameObject.SetActive(value: true);
			regularSoundGlobal.Play(base.transform.position);
			regularSound.Play(base.transform.position);
		}
	}
}
