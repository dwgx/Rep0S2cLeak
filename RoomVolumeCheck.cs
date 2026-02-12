using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomVolumeCheck : MonoBehaviour
{
	public List<RoomVolume> CurrentRooms;

	public bool Continuous = true;

	internal float PauseCheckTimer;

	private LayerMask Mask;

	[Space]
	public bool DebugCheckPosition;

	public Vector3 CheckPosition = Vector3.one;

	public Vector3 currentSize = Vector3.one;

	internal bool inTruck;

	internal bool inExtractionPoint;

	private PlayerAvatar player;

	private bool checkActive;

	private bool wasInRoom;

	private void Awake()
	{
		player = GetComponentInParent<PlayerAvatar>();
		Mask = LayerMask.GetMask("RoomVolume");
		CheckStart();
	}

	private void OnEnable()
	{
		CheckStart();
	}

	private void OnDisable()
	{
		checkActive = false;
		StopAllCoroutines();
	}

	public void CheckSet()
	{
		inTruck = false;
		inExtractionPoint = false;
		CurrentRooms.Clear();
		Vector3 localScale = currentSize;
		if (localScale == Vector3.zero)
		{
			localScale = base.transform.localScale;
		}
		Collider[] array = Physics.OverlapBox(base.transform.position + base.transform.rotation * CheckPosition, localScale / 2f, base.transform.rotation, Mask);
		foreach (Collider collider in array)
		{
			RoomVolume roomVolume = collider.transform.GetComponent<RoomVolume>();
			if (!roomVolume)
			{
				roomVolume = collider.transform.GetComponentInParent<RoomVolume>();
			}
			if (!CurrentRooms.Contains(roomVolume))
			{
				CurrentRooms.Add(roomVolume);
			}
			if (roomVolume.Truck)
			{
				inTruck = true;
			}
			if (roomVolume.Extraction)
			{
				inExtractionPoint = true;
			}
		}
		if ((bool)player)
		{
			if (CurrentRooms.Count > 0)
			{
				bool flag = true;
				MapModule mapModule = CurrentRooms[0].MapModule;
				foreach (RoomVolume currentRoom in CurrentRooms)
				{
					if (mapModule != currentRoom.MapModule)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					CurrentRooms[0].SetExplored();
				}
			}
			else if (Debug.isDebugBuild && wasInRoom && (bool)player && player.isLocal)
			{
				string text = "Player: Not in RoomVolume: ";
				if (SemiFunc.GetModuleAtPosition(player.playerTransform.position, out var module, out var localPosition))
				{
					text = text + "\n     Module Name: " + module?.name;
					string text2 = text;
					Vector3 vector = localPosition;
					text = text2 + "\n     Local Position: " + vector.ToString();
				}
				else
				{
					text += "\n     Module Name: N/A";
					text = text + "\n     Position: " + player.playerTransform.position.ToString();
				}
				Debug.LogError(text + "\n", module?.gameObject ?? base.gameObject);
			}
		}
		wasInRoom = CurrentRooms.Count > 0;
	}

	private IEnumerator Check()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.5f);
		while (true)
		{
			if (PauseCheckTimer > 0f)
			{
				PauseCheckTimer -= 0.5f;
				yield return new WaitForSeconds(0.5f);
				continue;
			}
			CheckSet();
			if (Continuous)
			{
				if ((bool)player)
				{
					yield return new WaitForSeconds(0.1f);
				}
				else
				{
					yield return new WaitForSeconds(0.5f);
				}
				continue;
			}
			break;
		}
	}

	private void CheckStart()
	{
		if (!checkActive)
		{
			checkActive = true;
			StartCoroutine(Check());
		}
	}
}
