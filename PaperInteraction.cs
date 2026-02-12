using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PaperInteraction : MonoBehaviour
{
	public List<GameObject> papers;

	[HideInInspector]
	public bool Picked;

	public Transform PaperTransform;

	[HideInInspector]
	public GameObject paperVisual;

	public CleanEffect CleanEffect;

	private PhotonView photonView;

	private bool destructionToMaster;

	private bool destructionToOthers;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		if (GameManager.instance.gameMode == 1)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				int num = Random.Range(0, papers.Count);
				Vector3 vector = new Vector3(0f, Random.Range(0, 360), 0f);
				photonView.RPC("SyncPaperVisual", RpcTarget.AllBuffered, num, vector);
			}
		}
		else
		{
			paperVisual = Object.Instantiate(papers[Random.Range(0, papers.Count)], base.transform.position, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
			paperVisual.transform.parent = PaperTransform;
		}
	}

	private void Update()
	{
		if (!Picked)
		{
			return;
		}
		if (GameManager.instance.gameMode == 1)
		{
			if (!destructionToMaster)
			{
				photonView.RPC("DestroyPaper", RpcTarget.MasterClient);
				destructionToMaster = true;
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	[PunRPC]
	public void SyncPaperVisual(int randomPaper, Vector3 randomRotation)
	{
		paperVisual = Object.Instantiate(papers[randomPaper], base.transform.position, Quaternion.Euler(randomRotation));
		paperVisual.transform.parent = PaperTransform;
	}

	[PunRPC]
	public void DestroyPaper()
	{
		if (!destructionToOthers)
		{
			PhotonNetwork.Destroy(base.gameObject);
			destructionToOthers = true;
		}
	}
}
