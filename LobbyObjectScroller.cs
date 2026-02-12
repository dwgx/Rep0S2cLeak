using UnityEngine;

public class LobbyObjectScroller : MonoBehaviour
{
	public float scrollSpeed = 12f;

	public float maxDistanceX = 80f;

	private float offsetX = -22f;

	private TruckLandscapeScroller truck;

	private void Start()
	{
		truck = GetComponentInParent<TruckLandscapeScroller>();
		if (truck != null)
		{
			scrollSpeed *= truck.truckSpeed;
		}
	}

	private void Update()
	{
		base.transform.position += Vector3.right * scrollSpeed * Time.deltaTime;
		if (base.transform.position.x > maxDistanceX + offsetX)
		{
			base.transform.position = new Vector3(0f - maxDistanceX + offsetX, base.transform.position.y, base.transform.position.z);
		}
	}
}
