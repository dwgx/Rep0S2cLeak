using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EnemyOnScreen : MonoBehaviour
{
	private Enemy Enemy;

	private Camera MainCamera;

	public Transform[] points;

	[Space]
	public float maxDistance = 20f;

	[Space]
	public float paddingWidth = 0.1f;

	public float paddingHeight = 0.1f;

	private bool LogicActive;

	private float OnScreenTimer = 0.25f;

	internal bool OnScreenLocal;

	private bool OnScreenLocalPrevious;

	internal bool CulledLocal;

	private bool CulledLocalPrevious;

	internal bool OnScreenAny;

	internal bool CulledAny;

	internal Dictionary<int, bool> OnScreenPlayer = new Dictionary<int, bool>();

	internal Dictionary<int, bool> CulledPlayer = new Dictionary<int, bool>();

	private void Awake()
	{
		Enemy = GetComponent<Enemy>();
		MainCamera = Camera.main;
		if (points.Length == 0)
		{
			points = new Transform[1];
			points[0] = Enemy.CenterTransform;
		}
		LogicActive = true;
		StartCoroutine(Logic());
	}

	private void OnEnable()
	{
		if (!LogicActive)
		{
			LogicActive = true;
			StartCoroutine(Logic());
		}
	}

	private void OnDisable()
	{
		LogicActive = false;
		StopAllCoroutines();
	}

	private IEnumerator Logic()
	{
		while (OnScreenPlayer.Count == 0)
		{
			yield return new WaitForSeconds(OnScreenTimer);
		}
		while (true)
		{
			CulledLocal = true;
			CulledAny = true;
			OnScreenLocal = false;
			OnScreenAny = false;
			Transform[] array = points;
			foreach (Transform transform in array)
			{
				if (Vector3.Distance(transform.position, CameraUtils.Instance.MainCamera.transform.position) <= maxDistance && SemiFunc.OnScreen(transform.position, paddingWidth, paddingHeight))
				{
					CulledLocal = false;
					CulledAny = false;
					Vector3 direction = MainCamera.transform.position - transform.position;
					float num = Mathf.Min(Vector3.Distance(MainCamera.transform.position, transform.position), 12f);
					if (!Physics.Raycast(transform.position, direction, out var hitInfo, num, Enemy.VisionMask) || hitInfo.transform.CompareTag("Player") || (bool)hitInfo.transform.GetComponent<PlayerTumble>())
					{
						OnScreenLocal = true;
						OnScreenAny = true;
					}
				}
				if (OnScreenAny && !CulledAny)
				{
					break;
				}
			}
			if (GameManager.Multiplayer())
			{
				foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
				{
					if (!player.isDisabled && player.photonView.IsMine)
					{
						if (CulledLocal != CulledLocalPrevious || OnScreenLocal != OnScreenLocalPrevious)
						{
							CulledLocalPrevious = CulledLocal;
							OnScreenLocalPrevious = OnScreenLocal;
							OnScreenPlayerUpdate(player.photonView.ViewID, OnScreenLocal, CulledLocal);
						}
						break;
					}
				}
				foreach (PlayerAvatar player2 in GameDirector.instance.PlayerList)
				{
					if (!player2.isDisabled)
					{
						if (OnScreenPlayer[player2.photonView.ViewID])
						{
							OnScreenAny = true;
						}
						if (!CulledPlayer[player2.photonView.ViewID])
						{
							CulledAny = false;
						}
					}
				}
			}
			yield return new WaitForSeconds(OnScreenTimer);
		}
	}

	public bool GetOnScreen(PlayerAvatar _playerAvatar)
	{
		if (!GameManager.Multiplayer())
		{
			return OnScreenLocal;
		}
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player == _playerAvatar)
			{
				return OnScreenPlayer[player.photonView.ViewID];
			}
		}
		return false;
	}

	private void OnScreenPlayerUpdate(int playerID, bool onScreen, bool culled)
	{
		if (GameManager.instance.gameMode == 0)
		{
			OnScreenPlayerUpdateRPC(playerID, onScreen, culled);
			return;
		}
		Enemy.PhotonView.RPC("OnScreenPlayerUpdateRPC", RpcTarget.All, playerID, onScreen, culled);
	}

	[PunRPC]
	private void OnScreenPlayerUpdateRPC(int playerID, bool onScreen, bool culled)
	{
		CulledPlayer[playerID] = culled;
		OnScreenPlayer[playerID] = onScreen;
	}

	public void PlayerAdded(int photonID)
	{
		OnScreenPlayer.TryAdd(photonID, value: false);
		CulledPlayer.TryAdd(photonID, value: false);
	}

	public void PlayerRemoved(int photonID)
	{
		OnScreenPlayer.Remove(photonID);
		CulledPlayer.Remove(photonID);
	}
}
