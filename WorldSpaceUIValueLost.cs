using TMPro;
using UnityEngine;

public class WorldSpaceUIValueLost : WorldSpaceUIChild
{
	internal float timer;

	private float flashTimer = 0.2f;

	private Vector3 scale;

	private TextMeshProUGUI text;

	private Color textColor;

	private float shakeXAmount;

	private float shakeYAmount;

	private float floatY;

	private float shakeTimerX;

	private float shakeXTarget;

	private float shakeX;

	private float shakeTimerY;

	private float shakeYTarget;

	private float shakeY;

	public AnimationCurve curveIntro;

	public AnimationCurve curveOutro;

	private float curveLerp;

	internal int value;

	protected override void Start()
	{
		base.Start();
		shakeXAmount = 0.005f;
		shakeYAmount = 0.005f;
		timer = 3f;
		text = GetComponent<TextMeshProUGUI>();
		textColor = text.color;
		text.color = Color.white;
		text.text = "-$" + SemiFunc.DollarGetString(value);
		scale = base.transform.localScale;
		if (value < 1000)
		{
			scale *= 0.75f;
			base.transform.localScale = scale;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (text.color != textColor)
		{
			flashTimer -= Time.deltaTime;
			if (flashTimer <= 0f && text.color != textColor)
			{
				text.color = Color.Lerp(text.color, textColor, 20f * Time.deltaTime);
				shakeX = Mathf.Lerp(shakeX, 0f, 20f * Time.deltaTime);
				shakeY = Mathf.Lerp(shakeY, 0f, 20f * Time.deltaTime);
			}
			else
			{
				if (shakeTimerX <= 0f)
				{
					shakeXTarget = Random.Range(0f - shakeXAmount, shakeXAmount);
					shakeTimerX = Random.Range(0.008f, 0.015f);
				}
				else
				{
					shakeTimerX -= Time.deltaTime;
					shakeX = Mathf.Lerp(shakeX, shakeXTarget, 50f * Time.deltaTime);
				}
				if (shakeTimerX <= 0f)
				{
					shakeYTarget = Random.Range(0f - shakeYAmount, shakeYAmount);
					shakeTimerX = Random.Range(0.008f, 0.015f);
				}
				else
				{
					shakeTimerX -= Time.deltaTime;
					shakeY = Mathf.Lerp(shakeY, shakeYTarget, 50f * Time.deltaTime);
				}
			}
		}
		floatY += 0.02f * Time.deltaTime;
		positionOffset = new Vector3(shakeX, shakeY + floatY, 0f);
		timer -= Time.deltaTime;
		if (timer > 0f)
		{
			curveLerp += 10f * Time.deltaTime;
			curveLerp = Mathf.Clamp01(curveLerp);
			base.transform.localScale = scale * curveIntro.Evaluate(curveLerp);
			return;
		}
		curveLerp -= 5f * Time.deltaTime;
		curveLerp = Mathf.Clamp01(curveLerp);
		base.transform.localScale = scale * curveOutro.Evaluate(curveLerp);
		if (curveLerp <= 0f)
		{
			WorldSpaceUIParent.instance.valueLostList.Remove(this);
			Object.Destroy(base.gameObject);
		}
	}
}
