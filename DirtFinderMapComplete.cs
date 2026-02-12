using TMPro;
using UnityEngine;

public class DirtFinderMapComplete : MonoBehaviour
{
	public SpriteRenderer FlashRenderer;

	public AnimationCurve FlashCurve;

	public float FlashSpeed;

	private float FlashLerp;

	[Space]
	public TextMeshPro TextTop;

	public TextMeshPro TextBot;

	private float TextDilate;

	private float TextDilateWait;

	private float TextDilateIncrease = 1f;

	[Space]
	public float CompleteTime;

	private void Start()
	{
		GameDirector.instance.CameraImpact.Shake(1f, 0.25f);
		GameDirector.instance.CameraShake.Shake(1f, 0.25f);
	}

	private void Update()
	{
		if (FlashLerp < 1f)
		{
			FlashLerp += FlashSpeed * Time.deltaTime;
			FlashRenderer.color = Color.Lerp(FlashRenderer.color, new Color(255f, 255f, 255f, 0f), FlashCurve.Evaluate(FlashLerp));
			if (FlashLerp >= 1f)
			{
				FlashLerp = 1f;
				FlashRenderer.transform.gameObject.SetActive(value: false);
			}
		}
		TextDilate += 5f * TextDilateIncrease * Time.deltaTime;
		if (TextDilate >= 1f)
		{
			TextDilate = 1f;
			if (TextDilateWait > 0f)
			{
				TextDilateWait -= Time.deltaTime;
			}
			else
			{
				TextDilateWait = 0.5f;
				TextDilateIncrease = -1f;
			}
		}
		else if (TextDilate <= -1f)
		{
			TextDilate = -1f;
			if (TextDilateWait > 0f)
			{
				TextDilateWait -= Time.deltaTime;
			}
			else
			{
				TextDilateWait = 0.5f;
				TextDilateIncrease = 1f;
			}
		}
		TextTop.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, TextDilate);
		TextBot.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, TextDilate);
		CompleteTime -= Time.deltaTime;
		if (CompleteTime <= 0f)
		{
			CompleteTime = 0f;
			base.gameObject.SetActive(value: false);
		}
	}
}
