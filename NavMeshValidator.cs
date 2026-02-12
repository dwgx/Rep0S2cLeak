using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshValidator
{
	internal static void SafetyCheck()
	{
		NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
		List<List<int>> list = FindIslands(tri);
		if (list == null || list.Count == 0)
		{
			return;
		}
		List<int> mainIsland = GetMainIsland(list);
		if (mainIsland == null || mainIsland.Count == 0)
		{
			return;
		}
		Vector3 sourcePosition = GetBestReportPoint(tri, mainIsland, mainIsland);
		if (NavMesh.SamplePosition(sourcePosition, out var hit, 2f, -1))
		{
			sourcePosition = hit.position;
		}
		foreach (List<int> item in list)
		{
			if (item == mainIsland || item.Count == 0)
			{
				continue;
			}
			Vector3 vector = GetBestReportPoint(tri, item, mainIsland);
			if (NavMesh.SamplePosition(vector, out var hit2, 0.25f, -1))
			{
				vector = hit2.position;
			}
			bool flag = IsPathConnected(sourcePosition, vector, -1);
			if (!flag)
			{
				GameObject context = null;
				if (Application.isEditor)
				{
					context = Object.Instantiate(AssetManager.instance.debugNavMeshError, vector, Quaternion.identity);
					DrawBoundsWire(ComputeIslandBounds(tri, item), flag ? Color.green : Color.red, 120f);
				}
				string text = "Navmesh: Unattached region: ";
				if (SemiFunc.GetRoomVolumeAtPosition(vector, out var room, out var localPosition))
				{
					text = text + "\n     Module Name: " + room.Module?.name;
					string text2 = text;
					Vector3 vector2 = localPosition;
					text = text2 + "\n     Local Position: " + vector2.ToString();
				}
				else
				{
					text += "\n     Module Name: N/A";
					string text3 = text;
					Vector3 vector2 = vector;
					text = text3 + "\n     Position: " + vector2.ToString();
				}
				Debug.LogError(text + "\n", context);
			}
		}
	}

	private static Vector3 GetBestReportPoint(NavMeshTriangulation tri, List<int> island, List<int> mainIsland)
	{
		if (TryGetClosestVertexBetweenIslands(tri, island, mainIsland, out var closest))
		{
			return closest;
		}
		return GetTriangleCentroid(tri, island[0]);
	}

	private static bool TryGetClosestVertexBetweenIslands(NavMeshTriangulation tri, List<int> a, List<int> b, out Vector3 closest)
	{
		float num = float.PositiveInfinity;
		Vector3 vector = default(Vector3);
		List<Vector3> triangleVertices = GetTriangleVertices(tri, a);
		List<Vector3> triangleVertices2 = GetTriangleVertices(tri, b);
		for (int i = 0; i < triangleVertices.Count; i++)
		{
			Vector3 vector2 = triangleVertices[i];
			for (int j = 0; j < triangleVertices2.Count; j++)
			{
				float sqrMagnitude = (vector2 - triangleVertices2[j]).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					vector = vector2;
					if (num <= 0f)
					{
						closest = vector;
						return true;
					}
				}
			}
		}
		if (num < float.PositiveInfinity)
		{
			closest = vector;
			return true;
		}
		closest = default(Vector3);
		return false;
	}

	private static List<Vector3> GetTriangleVertices(NavMeshTriangulation tri, List<int> tris)
	{
		List<Vector3> list = new List<Vector3>(tris.Count * 3);
		for (int i = 0; i < tris.Count; i++)
		{
			int num = tris[i] * 3;
			list.Add(tri.vertices[tri.indices[num]]);
			list.Add(tri.vertices[tri.indices[num + 1]]);
			list.Add(tri.vertices[tri.indices[num + 2]]);
		}
		return list;
	}

	private static Vector3 GetTriangleCentroid(NavMeshTriangulation tri, int triIndex)
	{
		int num = triIndex * 3;
		Vector3 vector = tri.vertices[tri.indices[num]];
		Vector3 vector2 = tri.vertices[tri.indices[num + 1]];
		Vector3 vector3 = tri.vertices[tri.indices[num + 2]];
		return (vector + vector2 + vector3) / 3f;
	}

	private static List<int> GetMainIsland(List<List<int>> islands)
	{
		List<int> list = null;
		for (int i = 0; i < islands.Count; i++)
		{
			List<int> list2 = islands[i];
			if (list == null || list2.Count > list.Count)
			{
				list = list2;
			}
		}
		return list;
	}

	private static List<List<int>> FindIslands(NavMeshTriangulation tri)
	{
		int[] array = BuildWeldedVertexMap(tri.vertices, 0.05f);
		Dictionary<(int, int), List<int>> dictionary = BuildEdgeToTriangles(array, tri.indices);
		int num = tri.indices.Length / 3;
		HashSet<int> hashSet = new HashSet<int>();
		List<List<int>> list = new List<List<int>>();
		for (int i = 0; i < num; i++)
		{
			if (!hashSet.Add(i))
			{
				continue;
			}
			List<int> list2 = new List<int>();
			Stack<int> stack = new Stack<int>();
			stack.Push(i);
			while (stack.Count > 0)
			{
				int num2 = stack.Pop();
				list2.Add(num2);
				int num3 = num2 * 3;
				for (int j = 0; j < 3; j++)
				{
					int num4 = array[tri.indices[num3 + j]];
					int num5 = array[tri.indices[num3 + (j + 1) % 3]];
					if (num4 > num5)
					{
						int num6 = num4;
						num4 = num5;
						num5 = num6;
					}
					if (!dictionary.TryGetValue((num4, num5), out var value))
					{
						continue;
					}
					for (int k = 0; k < value.Count; k++)
					{
						int item = value[k];
						if (hashSet.Add(item))
						{
							stack.Push(item);
						}
					}
				}
			}
			list.Add(list2);
		}
		return list;
	}

	private static int[] BuildWeldedVertexMap(Vector3[] vertices, float epsilon)
	{
		Dictionary<(int, int, int), int> dictionary = new Dictionary<(int, int, int), int>();
		int[] array = new int[vertices.Length];
		int num = 0;
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 vector = vertices[i];
			(int, int, int) key = (Mathf.RoundToInt(vector.x / epsilon), Mathf.RoundToInt(vector.y / epsilon), Mathf.RoundToInt(vector.z / epsilon));
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = (dictionary[key] = num++);
			}
			array[i] = value;
		}
		return array;
	}

	private static Dictionary<(int, int), List<int>> BuildEdgeToTriangles(int[] vertexMap, int[] indices)
	{
		Dictionary<(int, int), List<int>> dictionary = new Dictionary<(int, int), List<int>>();
		for (int i = 0; i < indices.Length; i += 3)
		{
			int item = i / 3;
			for (int j = 0; j < 3; j++)
			{
				int num = vertexMap[indices[i + j]];
				int num2 = vertexMap[indices[i + (j + 1) % 3]];
				if (num > num2)
				{
					int num3 = num;
					num = num2;
					num2 = num3;
				}
				if (!dictionary.TryGetValue((num, num2), out var value))
				{
					value = new List<int>();
					dictionary[(num, num2)] = value;
				}
				value.Add(item);
			}
		}
		return dictionary;
	}

	private static bool IsPathConnected(Vector3 a, Vector3 b, int areaMask)
	{
		if (!NavMesh.SamplePosition(a, out var hit, 2f, areaMask))
		{
			return false;
		}
		if (!NavMesh.SamplePosition(b, out var hit2, 2f, areaMask))
		{
			return false;
		}
		NavMeshPath navMeshPath = new NavMeshPath();
		if (NavMesh.CalculatePath(hit.position, hit2.position, areaMask, navMeshPath))
		{
			return navMeshPath.status == NavMeshPathStatus.PathComplete;
		}
		return false;
	}

	private static Bounds ComputeIslandBounds(NavMeshTriangulation tri, List<int> islandTris)
	{
		bool flag = false;
		Bounds result = new Bounds(Vector3.zero, Vector3.zero);
		for (int i = 0; i < islandTris.Count; i++)
		{
			int num = islandTris[i] * 3;
			for (int j = 0; j < 3; j++)
			{
				Vector3 vector = tri.vertices[tri.indices[num + j]];
				if (!flag)
				{
					result = new Bounds(vector, Vector3.zero);
					flag = true;
				}
				else
				{
					result.Encapsulate(vector);
				}
			}
		}
		return result;
	}

	private static void DrawBoundsWire(Bounds b, Color color, float duration)
	{
		Vector3 center = b.center;
		Vector3 extents = b.extents;
		Vector3 start = center + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z);
		Vector3 vector = center + new Vector3(0f - extents.x, 0f - extents.y, extents.z);
		Vector3 vector2 = center + new Vector3(0f - extents.x, extents.y, 0f - extents.z);
		Vector3 end = center + new Vector3(0f - extents.x, extents.y, extents.z);
		Vector3 vector3 = center + new Vector3(extents.x, 0f - extents.y, 0f - extents.z);
		Vector3 end2 = center + new Vector3(extents.x, 0f - extents.y, extents.z);
		Vector3 end3 = center + new Vector3(extents.x, extents.y, 0f - extents.z);
		Vector3 start2 = center + new Vector3(extents.x, extents.y, extents.z);
		Debug.DrawLine(start, vector, color, duration);
		Debug.DrawLine(start, vector2, color, duration);
		Debug.DrawLine(start, vector3, color, duration);
		Debug.DrawLine(start2, end2, color, duration);
		Debug.DrawLine(start2, end3, color, duration);
		Debug.DrawLine(start2, end, color, duration);
		Debug.DrawLine(vector, end, color, duration);
		Debug.DrawLine(vector, end2, color, duration);
		Debug.DrawLine(vector2, end, color, duration);
		Debug.DrawLine(vector2, end3, color, duration);
		Debug.DrawLine(vector3, end2, color, duration);
		Debug.DrawLine(vector3, end3, color, duration);
	}
}
