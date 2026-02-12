using UnityEngine;
using UnityEngine.UI;

public class HurtVignette : MonoBehaviour
{
	public Image image;

	public RectTransform rectTransform;

	[Space]
	public Color activeColor;

	public Color inactiveColor;

	[Space]
	public float activeScale;

	public float inactiveScale;

	[Space]
	public AnimationCurve introCurve;

	public AnimationCurve outroCurve;

	private float lerp;

	public static HurtVignette instance;

	private void Start()
	{
		instance = this;
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated || SemiFunc.MenuLevel())
		{
			return;
		}
		if (!PlayerController.instance.playerAvatarScript.isDisabled && (float)PlayerController.instance.playerAvatarScript.playerHealth.health < 10f)
		{
			if (lerp < 1f)
			{
				lerp += 1f * Time.deltaTime;
				rectTransform.localScale = Vector3.one * Mathf.Lerp(inactiveScale, activeScale, introCurve.Evaluate(lerp));
				image.color = Color.Lerp(inactiveColor, activeColor, introCurve.Evaluate(lerp));
			}
		}
		else if (lerp > 0f)
		{
			lerp -= 1f * Time.deltaTime;
			rectTransform.localScale = Vector3.one * Mathf.Lerp(inactiveScale, activeScale, outroCurve.Evaluate(lerp));
			image.color = Color.Lerp(inactiveColor, activeColor, outroCurve.Evaluate(lerp));
		}
	}
}
