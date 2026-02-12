using UnityEngine;
using UnityEngine.UI;

public class RewindTexture : MonoBehaviour
{
	public float scrollSpeed = 0.5f;

	private RawImage rawImage;

	private void Start()
	{
		rawImage = GetComponent<RawImage>();
	}

	private void Update()
	{
		float num = Mathf.Repeat(Time.time * scrollSpeed, 1f);
		Rect uvRect = rawImage.uvRect;
		uvRect.x = num;
		uvRect.y = num;
		rawImage.uvRect = uvRect;
	}
}
