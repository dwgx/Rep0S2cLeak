using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelPoint : MonoBehaviour
{
	public bool DebugMeshActive = true;

	public Mesh DebugMesh;

	internal bool inStartRoom;

	[Space]
	public bool ModuleConnect;

	public bool Truck;

	private bool ModuleConnected;

	public RoomVolume Room;

	[Space]
	public List<LevelPoint> ConnectedPoints;

	[HideInInspector]
	public List<LevelPoint> AllLevelPoints;

	private void Start()
	{
		LevelGenerator.Instance.LevelPathPoints.Add(this);
		if (Truck)
		{
			LevelGenerator.Instance.LevelPathTruck = this;
		}
		if ((bool)GetComponentInParent<StartRoom>())
		{
			inStartRoom = true;
		}
		if (ModuleConnect)
		{
			StartCoroutine(ModuleConnectSetup());
		}
		StartCoroutine(NavMeshCheck());
	}

	private IEnumerator NavMeshCheck()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.5f);
		bool flag = false;
		Module componentInParent = GetComponentInParent<Module>();
		if (!NavMesh.SamplePosition(base.transform.position, out var _, 0.5f, -1))
		{
			flag = true;
			string text = "Level Point: Not on navmesh: ";
			text = text + "\n     Point Name: " + base.name;
			if ((bool)componentInParent)
			{
				text = text + "\n     Module Name: " + componentInParent.name;
				text = text + "\n     Local Position: " + componentInParent.transform.InverseTransformPoint(base.transform.position).ToString();
			}
			else
			{
				text += "\n     Module Name: N/A";
				text = text + "\n     Position: " + base.transform.position.ToString();
			}
			Debug.LogError(text + "\n", base.gameObject);
		}
		if (!Room)
		{
			flag = true;
			string text2 = "Level Point: Missing room volume: ";
			text2 = text2 + "\n     Point Name: " + base.name;
			if ((bool)componentInParent)
			{
				text2 = text2 + "\n     Module Name: " + componentInParent.name;
				text2 = text2 + "\n     Local Position: " + componentInParent.transform.InverseTransformPoint(base.transform.position).ToString();
			}
			else
			{
				text2 += "\n     Module Name: N/A";
				text2 = text2 + "\n     Position: " + base.transform.position.ToString();
			}
			Debug.LogError(text2 + "\n", base.gameObject);
		}
		bool flag2 = true;
		foreach (LevelPoint connectedPoint in ConnectedPoints)
		{
			bool flag3 = false;
			if ((bool)connectedPoint)
			{
				foreach (LevelPoint connectedPoint2 in connectedPoint.ConnectedPoints)
				{
					if (connectedPoint2 == this)
					{
						flag3 = true;
						break;
					}
				}
			}
			if (!flag3)
			{
				flag = true;
				flag2 = false;
			}
		}
		if (!flag2)
		{
			string text3 = "Level Point: Not connected: ";
			text3 = text3 + "\n     Point Name: " + base.name;
			if ((bool)componentInParent)
			{
				text3 = text3 + "\n     Module Name: " + componentInParent.name;
				text3 = text3 + "\n     Local Position: " + componentInParent.transform.InverseTransformPoint(base.transform.position).ToString();
			}
			else
			{
				text3 += "\n     Module Name: N/A";
				text3 = text3 + "\n     Position: " + base.transform.position.ToString();
			}
			Debug.LogError(text3, base.gameObject);
			foreach (LevelPoint connectedPoint3 in ConnectedPoints)
			{
				if (!connectedPoint3)
				{
					Debug.LogError("          Point Name: N/A");
					continue;
				}
				bool flag4 = false;
				foreach (LevelPoint connectedPoint4 in connectedPoint3.ConnectedPoints)
				{
					if (connectedPoint4 == this)
					{
						flag4 = true;
						break;
					}
				}
				if (!flag4)
				{
					text3 = "          Point Name: " + connectedPoint3.name;
					componentInParent = connectedPoint3.GetComponentInParent<Module>();
					if ((bool)componentInParent)
					{
						text3 = text3 + "\n          Module Name: " + componentInParent.name;
						text3 = text3 + "\n          Local Position: " + componentInParent.transform.InverseTransformPoint(connectedPoint3.transform.position).ToString();
					}
					else
					{
						text3 += "\n          Module Name: N/A";
						text3 = text3 + "\n          Position: " + connectedPoint3.transform.position.ToString();
					}
					Debug.LogError(text3 + "\n", base.gameObject);
				}
			}
		}
		if (flag && Application.isEditor)
		{
			Object.Instantiate(AssetManager.instance.debugLevelPointError, base.transform.position, Quaternion.identity);
		}
	}

	private IEnumerator ModuleConnectSetup()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		float num = 999f;
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			if (levelPathPoint.ModuleConnect)
			{
				float num2 = Vector3.Distance(base.transform.position, levelPathPoint.transform.position);
				if (num2 < 15f && num2 < num && Vector3.Dot(levelPathPoint.transform.forward, base.transform.forward) <= -0.8f && Vector3.Dot(levelPathPoint.transform.forward, (base.transform.position - levelPathPoint.transform.position).normalized) > 0.8f)
				{
					num = num2;
					ConnectedPoints.Add(levelPathPoint);
				}
			}
		}
		ModuleConnected = true;
	}
}
