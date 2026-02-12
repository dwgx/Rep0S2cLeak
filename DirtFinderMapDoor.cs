using System.Collections;
using UnityEngine;

public class DirtFinderMapDoor : MonoBehaviour
{
	public Transform Target;

	public GameObject DoorPrefab;

	public PhysGrabHinge Hinge;

	private GameObject MapObject;

	public void Start()
	{
		Hinge = GetComponent<PhysGrabHinge>();
		StartCoroutine(Logic());
	}

	private IEnumerator Logic()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		MapObject = Map.Instance.AddDoor(this, DoorPrefab);
		while (!Hinge.broken)
		{
			yield return new WaitForSeconds(1f);
		}
		Object.Destroy(MapObject);
	}
}
