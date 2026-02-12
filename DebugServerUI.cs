using Photon.Pun;
using TMPro;
using UnityEngine;

public class DebugServerUI : MonoBehaviour
{
	public TextMeshProUGUI Text;

	private void Start()
	{
		if (GameManager.instance.gameMode == 0)
		{
			Text.text = "Local";
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			Text.text = "Server";
		}
		else
		{
			Text.text = "Client";
		}
	}
}
