using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class CleanDirector : MonoBehaviour
{
	[Serializable]
	public class CleaningSpots
	{
		public Interaction.InteractionType InteractionType;

		public int CleaningSpotsMax;
	}

	public static CleanDirector instance;

	public List<GameObject> CleanList = new List<GameObject>();

	public List<CleaningSpots> cleaningSpots;

	internal bool RemoveExcessSpots;

	private PhotonView photonView;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		instance = this;
	}

	private void RandomlyRemoveExcessSpots()
	{
		if (!PhotonNetwork.IsMasterClient && GameManager.instance.gameMode != 0)
		{
			return;
		}
		foreach (CleaningSpots cleaningSpot in cleaningSpots)
		{
			Interaction.InteractionType type = cleaningSpot.InteractionType;
			int cleaningSpotsMax = cleaningSpot.CleaningSpotsMax;
			for (int num = CleanList.Count((GameObject spot) => spot.GetComponent<CleanSpotIdentifier>().InteractionType == type); num > cleaningSpotsMax; num--)
			{
				List<GameObject> list = CleanList.Where((GameObject spot) => spot.GetComponent<CleanSpotIdentifier>().InteractionType == type).ToList();
				int index = UnityEngine.Random.Range(0, list.Count);
				GameObject gameObject = list[index];
				if (GameManager.instance.gameMode == 1)
				{
					if (gameObject.GetComponent<PhotonView>() == null)
					{
						Debug.LogWarning("Photon View not found for: " + gameObject.name);
					}
					CleanList.Remove(gameObject);
					PhotonNetwork.Destroy(gameObject);
				}
				else
				{
					CleanList.Remove(gameObject);
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		}
	}

	private void Update()
	{
		if (!RemoveExcessSpots && GameDirector.instance.currentState >= GameDirector.gameState.Start)
		{
			RandomlyRemoveExcessSpots();
			RemoveExcessSpots = true;
		}
	}
}
