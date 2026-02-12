using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TrapDirector : MonoBehaviourPunCallbacks, IPunObservable
{
	public static TrapDirector instance;

	public bool DebugAllTraps;

	[Space]
	public List<GameObject> TrapList = new List<GameObject>();

	public List<GameObject> SelectedTraps = new List<GameObject>();

	public float TrapCooldown;

	internal bool TrapListUpdated;

	public int TrapCount = 2;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		StartCoroutine(Generate());
	}

	private void Update()
	{
		if (TrapCooldown > 0f)
		{
			TrapCooldown -= Time.deltaTime;
		}
	}

	private IEnumerator Generate()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (GameManager.instance.gameMode == 0 || PhotonNetwork.IsMasterClient)
		{
			UpdateTrapList();
		}
		TrapListUpdated = true;
	}

	private void UpdateTrapList()
	{
		Dictionary<string, List<GameObject>> dictionary = new Dictionary<string, List<GameObject>>();
		foreach (GameObject trap in TrapList)
		{
			TrapTypeIdentifier component = trap.GetComponent<TrapTypeIdentifier>();
			if (component != null)
			{
				string trapType = component.trapType;
				if (!dictionary.ContainsKey(trapType))
				{
					dictionary[trapType] = new List<GameObject>();
				}
				dictionary[trapType].Add(trap);
			}
		}
		if (DebugAllTraps)
		{
			return;
		}
		foreach (KeyValuePair<string, List<GameObject>> item2 in dictionary)
		{
			if (item2.Value.Count > 0)
			{
				GameObject item = item2.Value[Random.Range(0, item2.Value.Count)];
				SelectedTraps.Add(item);
			}
		}
		foreach (GameObject trap2 in TrapList)
		{
			if (SelectedTraps.Contains(trap2))
			{
				continue;
			}
			TrapTypeIdentifier component2 = trap2.GetComponent<TrapTypeIdentifier>();
			if (!(component2 != null))
			{
				continue;
			}
			if (component2.Trigger != null && component2.OnlyRemoveTrigger)
			{
				if (GameManager.instance.gameMode == 0)
				{
					Object.Destroy(component2.Trigger);
					component2.TriggerRemoved = true;
				}
				else
				{
					trap2.GetComponent<PhotonView>().RPC("DestroyTrigger", RpcTarget.AllBuffered);
				}
			}
			else if (GameManager.instance.gameMode == 0)
			{
				Object.Destroy(trap2);
			}
			else
			{
				trap2.GetComponent<PhotonView>().RPC("DestroyTrap", RpcTarget.AllBuffered);
			}
		}
		while (SelectedTraps.Count > TrapCount)
		{
			int index = Random.Range(0, SelectedTraps.Count);
			GameObject gameObject = SelectedTraps[index];
			SelectedTraps.RemoveAt(index);
			TrapTypeIdentifier component3 = gameObject.GetComponent<TrapTypeIdentifier>();
			if (!(component3 != null))
			{
				continue;
			}
			if (component3.Trigger != null)
			{
				if (GameManager.instance.gameMode == 0)
				{
					Object.Destroy(component3.Trigger);
					component3.TriggerRemoved = true;
				}
				else
				{
					gameObject.GetComponent<PhotonView>().RPC("DestroyTrigger", RpcTarget.AllBuffered);
				}
			}
			else if (GameManager.instance.gameMode == 0)
			{
				Object.Destroy(gameObject);
			}
			else
			{
				gameObject.GetComponent<PhotonView>().RPC("DestroyTrap", RpcTarget.AllBuffered);
			}
		}
	}

	private string RandomType(Dictionary<string, List<GameObject>> trapsByType)
	{
		List<string> list = new List<string>(trapsByType.Keys);
		int index = Random.Range(0, list.Count);
		return list[index];
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(TrapListUpdated);
			}
			else
			{
				TrapListUpdated = (bool)stream.ReceiveNext();
			}
		}
	}
}
