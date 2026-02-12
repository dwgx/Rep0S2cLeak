using UnityEngine;

public class TextureScroller : MonoBehaviour
{
	public float scrollSpeed = 0.5f;

	private Renderer rend;

	private Vector2 savedOffset;

	private TruckLandscapeScroller truck;

	private void Start()
	{
		rend = GetComponent<Renderer>();
		savedOffset = rend.material.mainTextureOffset;
		truck = GetComponentInParent<TruckLandscapeScroller>();
		if (truck != null)
		{
			scrollSpeed *= truck.truckSpeed;
		}
	}

	private void Update()
	{
		float x = Mathf.Repeat(Time.time * scrollSpeed, 1f);
		Vector2 mainTextureOffset = new Vector2(x, savedOffset.y);
		rend.material.mainTextureOffset = mainTextureOffset;
	}

	private void OnDisable()
	{
		rend.material.mainTextureOffset = savedOffset;
	}
}
