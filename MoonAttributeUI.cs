using TMPro;
using UnityEngine;

public class MoonAttributeUI : MonoBehaviour
{
	public TextMeshProUGUI text;

	public RectTransform rect;

	public RectTransform animRect;

	public RectTransform flairRightPosition;

	public RectTransform flairRight;

	public RectTransform flairLeftPosition;

	public RectTransform flairLeft;

	public AnimationCurve introCurve;

	private float introLerp;

	public AnimationCurve flairCurve;

	private float flairLerp;

	private void Update()
	{
		if (introLerp < 1f)
		{
			introLerp += Time.deltaTime * 3f;
			animRect.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0f, -3f), Vector2.zero, introCurve.Evaluate(introLerp));
			if (introLerp > 1f)
			{
				SetFlair();
			}
		}
		if (introLerp > 0.5f && flairLerp < 1f)
		{
			if (flairLerp == 0f)
			{
				SetFlair();
			}
			flairLerp += Time.deltaTime * 3f;
			flairRight.anchoredPosition = Vector2.LerpUnclamped(new Vector2(-20f, 0f), Vector2.zero, flairCurve.Evaluate(flairLerp));
			flairLeft.anchoredPosition = Vector2.LerpUnclamped(new Vector2(20f, 0f), Vector2.zero, flairCurve.Evaluate(flairLerp));
		}
	}

	public void SetFlair()
	{
		float num = 7f;
		flairRightPosition.gameObject.SetActive(value: true);
		flairLeftPosition.gameObject.SetActive(value: true);
		flairRightPosition.anchoredPosition = new Vector2(text.renderedWidth / 2f + num, 1f);
		flairLeftPosition.anchoredPosition = new Vector2((0f - text.renderedWidth) / 2f - num, 1f);
	}
}
