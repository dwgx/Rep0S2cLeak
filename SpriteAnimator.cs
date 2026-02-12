using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;

	public List<Sprite> animationSprites;

	public int framesPerSecond = 12;

	private int currentSpriteIndex;

	private bool isAnimating = true;

	private float secondsPerFrame;

	private void Start()
	{
		if (spriteRenderer == null)
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}
		secondsPerFrame = 1f / (float)framesPerSecond;
		StartCoroutine(AnimateSprite());
	}

	private IEnumerator AnimateSprite()
	{
		while (isAnimating)
		{
			spriteRenderer.sprite = animationSprites[currentSpriteIndex];
			currentSpriteIndex = (currentSpriteIndex + 1) % animationSprites.Count;
			yield return new WaitForSeconds(secondsPerFrame);
		}
	}

	private void OnDisable()
	{
		isAnimating = false;
	}
}
