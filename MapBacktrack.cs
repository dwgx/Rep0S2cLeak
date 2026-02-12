using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MapBacktrack : MonoBehaviour
{
	public GameObject pointPrefab;

	private List<MapBacktrackPoint> points = new List<MapBacktrackPoint>();

	[Space]
	public int amount;

	public float spacing;

	public float pointWait;

	public float resetWait;

	private int currentPoint;

	private Vector3 currentPointPosition;

	private int currentPointCorner;

	private Vector3 truckDestination;

	private NavMeshPath path;

	private void Start()
	{
		path = new NavMeshPath();
		for (int i = 0; i < amount; i++)
		{
			GameObject gameObject = Object.Instantiate(pointPrefab, base.transform);
			points.Add(gameObject.GetComponent<MapBacktrackPoint>());
			gameObject.transform.name = $"Point {i}";
		}
		StartCoroutine(Backtrack());
	}

	private IEnumerator Backtrack()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.5f);
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			if ((bool)levelPathPoint.Room && levelPathPoint.Room.Truck)
			{
				truckDestination = levelPathPoint.transform.position;
				break;
			}
		}
		while (true)
		{
			Vector3 lastNavmeshPosition = PlayerController.instance.playerAvatarScript.LastNavmeshPosition;
			Vector3 targetPosition = lastNavmeshPosition;
			if (RoundDirector.instance.allExtractionPointsCompleted)
			{
				targetPosition = truckDestination;
			}
			else if ((bool)RoundDirector.instance.extractionPointCurrent)
			{
				targetPosition = RoundDirector.instance.extractionPointCurrent.transform.position;
			}
			bool flag = false;
			if (Map.Instance.Active)
			{
				MapLayer layerParent = Map.Instance.GetLayerParent(lastNavmeshPosition.y + 1f);
				MapLayer layerParent2 = Map.Instance.GetLayerParent(targetPosition.y + 1f);
				if (layerParent.layer == layerParent2.layer)
				{
					flag = true;
				}
			}
			if (!Map.Instance.Active || (flag && Vector3.Distance(lastNavmeshPosition, targetPosition) < 10f))
			{
				yield return new WaitForSeconds(0.25f);
				continue;
			}
			NavMesh.CalculatePath(lastNavmeshPosition, targetPosition, -1, path);
			currentPoint = 0;
			currentPointPosition = lastNavmeshPosition;
			currentPointCorner = 0;
			while (currentPoint < points.Count)
			{
				bool flag2 = false;
				float num = spacing;
				while (!flag2 && currentPointCorner < path.corners.Length)
				{
					float num2 = Vector3.Distance(currentPointPosition, path.corners[currentPointCorner]);
					if (num2 < num)
					{
						currentPointPosition = path.corners[currentPointCorner];
						num -= num2;
						currentPointCorner++;
						continue;
					}
					currentPointPosition = Vector3.Lerp(currentPointPosition, path.corners[currentPointCorner], num / num2);
					if (Map.Instance.GetLayerParent(currentPointPosition.y + 1f).layer == Map.Instance.PlayerLayer)
					{
						points[currentPoint].Show(_sameLayer: true);
					}
					else
					{
						points[currentPoint].Show(_sameLayer: false);
					}
					Vector3 vector = new Vector3(currentPointPosition.x, 0f, currentPointPosition.z);
					points[currentPoint].transform.position = vector * Map.Instance.Scale + Map.Instance.OverLayerParent.position;
					currentPoint++;
					flag2 = true;
				}
				if (currentPointCorner >= path.corners.Length)
				{
					currentPoint = points.Count;
				}
				yield return new WaitForSeconds(pointWait);
			}
			foreach (MapBacktrackPoint _point in points)
			{
				while (_point.animating)
				{
					yield return new WaitForSeconds(0.05f);
				}
			}
			yield return new WaitForSeconds(pointWait);
		}
	}
}
