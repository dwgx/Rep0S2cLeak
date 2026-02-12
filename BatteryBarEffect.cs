using UnityEngine;
using UnityEngine.UI;

public class BatteryBarEffect : MonoBehaviour
{
	private float parentSize;

	public bool barLossEffect = true;

	public AnimationCurve barAnimationCurve;

	public AnimationCurve barFadeOutCurve;

	public AnimationCurve whiteFlashCurve;

	private float curveProgress;

	private RawImage barImage;

	internal Color barColor;

	private void Start()
	{
		parentSize = base.transform.parent.transform.localScale.x;
		barImage = GetComponent<RawImage>();
		if (barLossEffect)
		{
			barColor = Color.red;
		}
		RectTransform component = GetComponent<RectTransform>();
		RectTransform component2 = base.transform.parent.GetComponent<RectTransform>();
		component.sizeDelta = component2.sizeDelta;
	}

	private void Update()
	{
		if (barLossEffect)
		{
			float num = barAnimationCurve.Evaluate(curveProgress);
			float num2 = barFadeOutCurve.Evaluate(curveProgress);
			float t = whiteFlashCurve.Evaluate(curveProgress);
			base.transform.localPosition = new Vector3(0f, 5f * parentSize * num, 0f);
			Color color = Color.Lerp(barColor, Color.white, t);
			barImage.color = new Color(color.r, color.g, color.b, num2);
			curveProgress += Time.deltaTime * 5f;
			if (curveProgress >= 0.99f)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			float num3 = barAnimationCurve.Evaluate(curveProgress);
			float num4 = barFadeOutCurve.Evaluate(curveProgress);
			float t2 = whiteFlashCurve.Evaluate(curveProgress);
			base.transform.localPosition = new Vector3(0f, (0f - 5f * parentSize) * num3, 0f);
			Color color2 = Color.Lerp(barColor, Color.white, t2);
			barImage.color = new Color(color2.r, color2.g, color2.b, num4);
			curveProgress += Time.deltaTime * 5f;
			if (curveProgress >= 0.99f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
