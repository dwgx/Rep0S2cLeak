using UnityEngine;

public class EnemyShadowScreenVeinEffect : MonoBehaviour
{
	private float activeTimer;

	public SpriteRenderer veinSpriteRenderer;

	public SpriteRenderer handSpriteRenderer;

	public float fadeInSpeed = 1f;

	public float fadeOutSpeed = 1.5f;

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
			return;
		}
		FadeOut();
		if (veinSpriteRenderer.material.color.a <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Active()
	{
		activeTimer = 0.4f;
	}

	private void FadeIn()
	{
		Color color = veinSpriteRenderer.material.color;
		color.a = Mathf.MoveTowards(color.a, 1f, Time.deltaTime * fadeInSpeed);
		veinSpriteRenderer.material.color = color;
		handSpriteRenderer.material.color = color;
	}

	private void FadeOut()
	{
		Color color = veinSpriteRenderer.material.color;
		color.a = Mathf.MoveTowards(color.a, 0f, Time.deltaTime * fadeOutSpeed);
		veinSpriteRenderer.material.color = color;
		handSpriteRenderer.material.color = color;
	}

	private void MakeTransparent()
	{
		Color color = veinSpriteRenderer.material.color;
		color.a = 0f;
		veinSpriteRenderer.material.color = color;
		handSpriteRenderer.material.color = color;
	}
}
