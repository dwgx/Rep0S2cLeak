using System.Collections;
using UnityEngine;

public class DirtFinderMapWall : MonoBehaviour
{
	public enum WallType
	{
		Wall_1x1,
		Door_1x1,
		Door_1x2,
		Door_Blocked,
		Door_1x1_Diagonal,
		Wall_1x05,
		Wall_1x025,
		Wall_1x1_Diagonal,
		Wall_1x05_Diagonal,
		Wall_1x025_Diagonal,
		Door_1x05_Diagonal,
		Door_1x1_Wizard,
		Door_Blocked_Wizard,
		Stairs,
		Door_1x05,
		Door_1x1_Arctic,
		Door_Blocked_Arctic,
		Wall_1x1_Curve,
		Wall_1x05_Curve,
		Door_1x1_Museum
	}

	public WallType Type;

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
		Map.Instance.AddWall(this);
	}
}
