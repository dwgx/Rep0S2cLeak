using System.Collections;
using Photon.Pun;
using UnityEngine;

public class ModuleConnectObject : MonoBehaviourPunCallbacks, IPunObservable
{
	public bool ModuleConnecting;

	public bool MasterSetup;

	private void Start()
	{
		StartCoroutine(ConnectingCheck());
	}

	private IEnumerator ConnectingCheck()
	{
		while (!MasterSetup)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (!base.transform.parent)
		{
			base.transform.parent = LevelGenerator.Instance.LevelParent.transform;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(ModuleConnecting);
				stream.SendNext(MasterSetup);
			}
			else
			{
				ModuleConnecting = (bool)stream.ReceiveNext();
				MasterSetup = (bool)stream.ReceiveNext();
			}
		}
	}
}
