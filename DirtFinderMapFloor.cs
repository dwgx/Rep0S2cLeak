using System.Collections;
using UnityEngine;

public class DirtFinderMapFloor : MonoBehaviour
{
	public enum FloorType
	{
		Floor_1x1,
		Floor_1x1_Diagonal,
		Floor_1x05,
		Floor_1x025,
		Floor_1x05_Diagonal,
		Floor_1x025_Diagonal,
		Truck_Floor,
		Truck_Wall,
		Used_Floor,
		Used_Wall,
		Inactive_Floor,
		Inactive_Wall,
		Floor_1x1_Curve,
		Floor_1x1_Curve_Inverted,
		Floor_1x05_Curve,
		Floor_1x05_Curve_Inverted
	}

	public FloorType Type;

	internal MapObject MapObject;

	private void Start()
	{
		StartCoroutine(Add());
	}

	private IEnumerator Add()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		Map.Instance.AddFloor(this);
	}
}
