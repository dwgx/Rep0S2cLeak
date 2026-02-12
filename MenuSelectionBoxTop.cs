using UnityEngine;
using UnityEngine.UI;

public class MenuSelectionBoxTop : MonoBehaviour
{
	private RectTransform rectTransform;

	private RawImage rawImage;

	private bool fadeDone;

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		rawImage = GetComponentInChildren<RawImage>();
	}

	private void Update()
	{
		MenuSelectionBox activeSelectionBox = MenuManager.instance.activeSelectionBox;
		if ((bool)activeSelectionBox)
		{
			rectTransform.localPosition = activeSelectionBox.rectTransform.position - base.transform.parent.position;
			base.transform.localScale = activeSelectionBox.rectTransform.localScale;
			rawImage.color = activeSelectionBox.rawImage.color * 1.5f;
			fadeDone = false;
		}
		else if (!fadeDone)
		{
			rawImage.color = new Color(1f, 1f, 1f, rawImage.color.a - Time.deltaTime);
			if (rawImage.color.a <= 0f)
			{
				fadeDone = true;
			}
		}
	}
}
