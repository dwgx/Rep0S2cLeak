using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScanlines : MonoBehaviour
{
	private TextMeshProUGUI parentText;

	private float originalAlpha;

	private Image image;

	private float changeColorTimer;

	private void Start()
	{
		image = GetComponent<Image>();
		originalAlpha = image.color.a;
		parentText = GetComponentInParent<TextMeshProUGUI>();
	}

	private void Update()
	{
		if ((bool)parentText)
		{
			if (changeColorTimer <= 0f)
			{
				Color color = parentText.color;
				image.color = new Color(color.r, color.g, color.b, originalAlpha);
				changeColorTimer = 0.03f;
			}
			else
			{
				changeColorTimer -= Time.deltaTime;
			}
		}
	}
}
