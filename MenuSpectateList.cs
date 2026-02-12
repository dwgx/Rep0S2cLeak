using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuSpectateList : SemiUI
{
	public GameObject menuPlayerListedPrefab;

	internal List<PlayerAvatar> spectatingPlayers = new List<PlayerAvatar>();

	internal List<GameObject> listObjects = new List<GameObject>();

	private float listCheckTimer;

	protected override void Update()
	{
		base.Update();
		if (!SemiFunc.IsMultiplayer())
		{
			Hide();
			return;
		}
		if (!SpectateCamera.instance || SpectateCamera.instance.CheckState(SpectateCamera.State.Head))
		{
			Hide();
			return;
		}
		SemiUIScoot(new Vector2(0f, (float)listObjects.Count * 22f));
		listCheckTimer -= Time.deltaTime;
		if (!(listCheckTimer <= 0f))
		{
			return;
		}
		listCheckTimer = 0.25f;
		List<PlayerAvatar> list = SemiFunc.PlayerGetList();
		bool flag = false;
		foreach (PlayerAvatar item in list)
		{
			if (item.isDisabled)
			{
				if (!spectatingPlayers.Contains(item))
				{
					PlayerAdd(item);
					flag = true;
				}
			}
			else if (spectatingPlayers.Contains(item))
			{
				PlayerRemove(item);
				flag = true;
			}
		}
		foreach (PlayerAvatar item2 in spectatingPlayers.ToList())
		{
			if (!list.Contains(item2))
			{
				PlayerRemove(item2);
				flag = true;
			}
		}
		if (flag)
		{
			listObjects.Sort((GameObject gameObject, GameObject gameObject2) => gameObject.GetComponent<MenuPlayerListed>().playerAvatar.photonView.ViewID.CompareTo(gameObject2.GetComponent<MenuPlayerListed>().playerAvatar.photonView.ViewID));
			for (int num = 0; num < listObjects.Count; num++)
			{
				listObjects[num].GetComponent<MenuPlayerListed>().listSpot = num;
				listObjects[num].transform.SetSiblingIndex(num);
			}
		}
	}

	private void PlayerAdd(PlayerAvatar player)
	{
		spectatingPlayers.Add(player);
		GameObject gameObject = Object.Instantiate(menuPlayerListedPrefab, base.transform);
		MenuPlayerListed component = gameObject.GetComponent<MenuPlayerListed>();
		component.playerAvatar = player;
		component.playerHead.SetPlayer(player);
		listObjects.Add(gameObject);
		component.listSpot = Mathf.Max(listObjects.Count - 1, 0);
	}

	private void PlayerRemove(PlayerAvatar player)
	{
		spectatingPlayers.Remove(player);
		foreach (GameObject listObject in listObjects)
		{
			if (listObject.GetComponent<MenuPlayerListed>().playerAvatar == player)
			{
				listObject.GetComponent<MenuPlayerListed>().MenuPlayerListedOutro();
				listObjects.Remove(listObject);
				break;
			}
		}
		for (int i = 0; i < listObjects.Count; i++)
		{
			listObjects[i].GetComponent<MenuPlayerListed>().listSpot = i;
		}
	}
}
