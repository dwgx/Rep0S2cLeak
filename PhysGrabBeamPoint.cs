using UnityEngine;

public class PhysGrabBeamPoint : MonoBehaviour
{
	public float tileSpeedX = 0.5f;

	public float tileSpeedY = 0.5f;

	public float textureJitterSpeed = 10f;

	public float sphereJitterSpeed = 10f;

	private Vector3 originalScale;

	public Material originalMaterial;

	public Material greenScreenMaterial;

	private void Start()
	{
		originalScale = base.transform.localScale;
		originalMaterial = GetComponent<Renderer>().material;
	}

	private void OnEnable()
	{
		if ((bool)VideoGreenScreen.instance)
		{
			GetComponent<Renderer>().material = greenScreenMaterial;
			{
				foreach (Transform item in base.transform)
				{
					item.GetComponent<Renderer>().material = greenScreenMaterial;
				}
				return;
			}
		}
		GetComponent<Renderer>().material = originalMaterial;
		foreach (Transform item2 in base.transform)
		{
			item2.GetComponent<Renderer>().material = originalMaterial;
		}
	}

	private void Update()
	{
		float num = Time.time * tileSpeedX;
		float num2 = Time.time * tileSpeedY;
		GetComponent<Renderer>().material.mainTextureOffset = new Vector2(num, num2);
		float num3 = Mathf.Sin(Time.time * textureJitterSpeed) * 0.1f;
		GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f + num3, 1f + num3);
		foreach (Transform item in base.transform)
		{
			item.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0f - num, 0f - num2);
			item.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f - num3, 1f - num3);
		}
		float num4 = Mathf.Sin(Time.time * sphereJitterSpeed * 1.5f) * (originalScale.x * 0.3f);
		base.transform.localScale = (originalScale + new Vector3(num4, num4, num4) * 0.35f) * 0.5f;
	}
}
