using System.Collections;
using UnityEngine;

public class RoomVolume : MonoBehaviour
{
	public bool Truck;

	public bool Extraction;

	public Color Color = Color.blue;

	[Space]
	public ReverbPreset ReverbPreset;

	public RoomAmbience RoomAmbience;

	public LevelAmbience RoomAmbienceOverride;

	public Module Module;

	public MapModule MapModule;

	private bool Explored;

	private void Awake()
	{
		Module = GetComponentInParent<Module>();
		RoomVolume[] componentsInChildren = GetComponentsInChildren<RoomVolume>(includeInactive: true);
		foreach (RoomVolume roomVolume in componentsInChildren)
		{
			if (roomVolume != this)
			{
				Object.Destroy(roomVolume);
			}
		}
	}

	private void Start()
	{
		StartCoroutine(Setup());
	}

	private IEnumerator Setup()
	{
		yield return new WaitForSeconds(0.1f);
		BoxCollider[] componentsInChildren = GetComponentsInChildren<BoxCollider>();
		foreach (BoxCollider boxCollider in componentsInChildren)
		{
			Vector3 halfExtents = boxCollider.size * 0.5f;
			halfExtents.x *= Mathf.Abs(boxCollider.transform.lossyScale.x);
			halfExtents.y *= Mathf.Abs(boxCollider.transform.lossyScale.y);
			halfExtents.z *= Mathf.Abs(boxCollider.transform.lossyScale.z);
			Collider[] array = Physics.OverlapBox(boxCollider.transform.TransformPoint(boxCollider.center), halfExtents, boxCollider.transform.rotation, LayerMask.GetMask("Other"), QueryTriggerInteraction.Collide);
			for (int j = 0; j < array.Length; j++)
			{
				LevelPoint component = array[j].transform.GetComponent<LevelPoint>();
				if ((bool)component)
				{
					component.Room = this;
				}
			}
		}
		MeshCollider[] componentsInChildren2 = GetComponentsInChildren<MeshCollider>();
		foreach (MeshCollider meshCollider in componentsInChildren2)
		{
			Collider[] array = Physics.OverlapBox(meshCollider.bounds.center, meshCollider.bounds.size, meshCollider.transform.rotation, LayerMask.GetMask("Other"), QueryTriggerInteraction.Collide);
			for (int j = 0; j < array.Length; j++)
			{
				LevelPoint component2 = array[j].transform.GetComponent<LevelPoint>();
				if ((bool)component2)
				{
					component2.Room = this;
				}
			}
		}
		if (!Extraction && !Truck && !Module.StartRoom && !SemiFunc.RunIsShop())
		{
			componentsInChildren = GetComponentsInChildren<BoxCollider>();
			foreach (BoxCollider boxCollider2 in componentsInChildren)
			{
				Vector3 scale = boxCollider2.size * 0.5f;
				scale.x *= Mathf.Abs(boxCollider2.transform.lossyScale.x);
				scale.y *= Mathf.Abs(boxCollider2.transform.lossyScale.y);
				scale.z *= Mathf.Abs(boxCollider2.transform.lossyScale.z);
				Vector3 position = boxCollider2.transform.TransformPoint(boxCollider2.center);
				Vector3 euler = new Vector3(0f, boxCollider2.transform.rotation.eulerAngles.y, 0f);
				MapModule = Map.Instance.AddRoomVolume(base.gameObject, position, Quaternion.Euler(euler), scale, Module);
			}
			componentsInChildren2 = GetComponentsInChildren<MeshCollider>();
			foreach (MeshCollider meshCollider2 in componentsInChildren2)
			{
				Vector3 scale2 = meshCollider2.transform.lossyScale * 0.5f;
				Vector3 position2 = meshCollider2.transform.position;
				Quaternion rotation = meshCollider2.transform.rotation;
				MapModule = Map.Instance.AddRoomVolume(base.gameObject, position2, rotation, scale2, Module, meshCollider2.sharedMesh);
			}
		}
	}

	public void SetExplored()
	{
		if (!Explored)
		{
			Explored = true;
			if ((bool)MapModule)
			{
				MapModule.Hide();
			}
		}
	}
}
