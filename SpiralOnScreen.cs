using UnityEngine;

public class SpiralOnScreen : MonoBehaviour
{
	private float activeTimer;

	public Transform spiral;

	public SpriteRenderer spiralSpriteRenderer;

	public float rotationSpeed = 1f;

	public float fadeInSpeed = 1f;

	public float fadeOutSpeed = 1f;

	private void Start()
	{
		MakeTransparent();
	}

	private void Update()
	{
		if (activeTimer > 0f)
		{
			activeTimer -= Time.deltaTime;
			FadeIn();
			spiral.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
			return;
		}
		FadeOut();
		if (spiralSpriteRenderer.material.color.a <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Active()
	{
		activeTimer = 0.05f;
	}

	private void FadeIn()
	{
		Color color = spiralSpriteRenderer.material.color;
		color.a = Mathf.MoveTowards(color.a, 1f, Time.deltaTime * fadeInSpeed);
		spiralSpriteRenderer.material.color = color;
	}

	private void FadeOut()
	{
		Color color = spiralSpriteRenderer.material.color;
		color.a = Mathf.MoveTowards(color.a, 0f, Time.deltaTime * fadeOutSpeed);
		spiralSpriteRenderer.material.color = color;
	}

	private void MakeTransparent()
	{
		Color color = spiralSpriteRenderer.material.color;
		color.a = 0f;
		spiralSpriteRenderer.material.color = color;
	}
}
