using TMPro;
using UnityEngine;

public class WorldSpaceUITTS : WorldSpaceUIChild
{
	internal TextMeshProUGUI text;

	internal float wordTime;

	internal TTSVoice ttsVoice;

	internal Transform followTransform;

	internal PlayerAvatar playerAvatar;

	internal Vector3 offsetPosition;

	private float flashTimer = 0.1f;

	private Color textColor = Color.yellow;

	private Color textColorTarget = Color.white;

	private bool flashDone;

	public AnimationCurve curveIntro;

	private float curveLerp;

	internal Vector3 followPosition;

	private float alphaCheckTimer;

	private float textAlphaTarget;

	private float textAlpha;

	private Camera cameraMain;

	private void Awake()
	{
		text = GetComponent<TextMeshProUGUI>();
		text.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
		cameraMain = Camera.main;
	}

	protected override void Update()
	{
		base.Update();
		if (alphaCheckTimer <= 0f)
		{
			textAlphaTarget = 1f;
			alphaCheckTimer = 0.1f;
			if (!SpectateCamera.instance || !SpectateCamera.instance.CheckState(SpectateCamera.State.Death))
			{
				float num = 5f;
				float num2 = 20f;
				float num3 = Vector3.Distance(cameraMain.transform.position, worldPosition);
				if (num3 > num)
				{
					num3 = Mathf.Clamp(num3, num, num2);
					textAlphaTarget = 1f - (num3 - num) / (num2 - num);
				}
				if ((bool)ttsVoice && ttsVoice.playerAvatar.voiceChat.lowPassLogicTTS.LowPass)
				{
					textAlphaTarget *= 0.5f;
				}
			}
		}
		else
		{
			alphaCheckTimer -= Time.deltaTime;
		}
		if (!followTransform || !ttsVoice || !ttsVoice.isSpeaking || !playerAvatar || (playerAvatar.isDisabled && !playerAvatar.playerDeathHead.spectated))
		{
			textAlphaTarget = 0f;
			if (textAlpha < 0.01f)
			{
				Object.Destroy(base.gameObject);
				return;
			}
		}
		textAlpha = Mathf.Lerp(textAlpha, textAlphaTarget, 30f * Time.deltaTime);
		if (!flashDone)
		{
			flashTimer -= Time.deltaTime;
			if (flashTimer <= 0f)
			{
				if (textColor != textColorTarget)
				{
					textColor = Color.Lerp(textColor, textColorTarget, 20f * Time.deltaTime);
				}
				else
				{
					flashDone = true;
				}
			}
		}
		text.color = new Color(textColor.r, textColor.g, textColor.b, textAlpha);
		if ((bool)followTransform)
		{
			followPosition = Vector3.Lerp(followPosition, followTransform.position + offsetPosition, 10f * Time.deltaTime);
		}
		worldPosition = followPosition + curveIntro.Evaluate(curveLerp) * Vector3.up * 0.025f;
		curveLerp += Time.deltaTime * 4f;
		curveLerp = Mathf.Clamp01(curveLerp);
		if ((bool)ttsVoice && ttsVoice.currentWordTime != wordTime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
