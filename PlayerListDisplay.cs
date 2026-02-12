using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerListDisplay : MonoBehaviourPunCallbacks
{
	public TextMeshProUGUI playerListText;

	public TextMeshProUGUI instructionText;

	public TextMeshProUGUI roomNameText;

	public GameObject loadingUI;

	private bool loading;

	public GameObject punVoiceClient;

	internal int playersCount;

	private bool voiceInitialized;

	public MenuPageMain menuPageMain;

	private void Start()
	{
	}

	private void Update()
	{
	}

	[PunRPC]
	private void StartLoadingRPC()
	{
	}
}
