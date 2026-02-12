using UnityEngine;
using UnityEngine.UI;

public class Blink : MonoBehaviour
{
	public Image targetImage;

	public float blinkTime;

	private float blinkTimer;

	private void Update()
	{
		if (blinkTimer <= 0f)
		{
			if (targetImage.enabled)
			{
				targetImage.enabled = false;
			}
			else
			{
				targetImage.enabled = true;
			}
			blinkTimer = blinkTime;
		}
		else
		{
			blinkTimer -= Time.deltaTime;
		}
	}
}
