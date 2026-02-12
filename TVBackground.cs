using UnityEngine;

public class TVBackground : MonoBehaviour
{
	public Vector2 scrollSpeed = new Vector2(0.5f, 0.5f);

	private Vector2 offset;

	private Material material;

	private void Start()
	{
		material = GetComponent<Renderer>().material;
	}

	private void Update()
	{
		offset.x = Time.time * scrollSpeed.x;
		offset.y = Time.time * scrollSpeed.y;
		material.SetTextureOffset("_MainTex", offset);
	}
}
