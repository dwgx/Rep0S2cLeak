using UnityEngine;

public class MapLayer : MonoBehaviour
{
	public int layer;

	internal Vector3 positionStart;

	private void Start()
	{
		positionStart = base.transform.position;
	}
}
