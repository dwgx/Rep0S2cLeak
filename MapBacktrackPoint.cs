using System.Collections;
using UnityEngine;

public class MapBacktrackPoint : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;

	public AnimationCurve curve;

	public float speed;

	private float lerp;

	public bool animating;

	private void Awake()
	{
		base.transform.localScale = Vector3.zero;
	}

	public void Show(bool _sameLayer)
	{
		Color color = spriteRenderer.color;
		if (_sameLayer)
		{
			color.a = 1f;
		}
		else
		{
			color.a = 0.2f;
		}
		spriteRenderer.color = color;
		StopCoroutine(Animate());
		StartCoroutine(Animate());
	}

	private IEnumerator Animate()
	{
		animating = true;
		lerp = 0f;
		while (true)
		{
			lerp += Time.deltaTime * speed;
			base.transform.localScale = Vector3.one * curve.Evaluate(lerp);
			if (!(lerp < 1f))
			{
				break;
			}
			yield return new WaitForSeconds(0.05f);
		}
		animating = false;
	}
}
