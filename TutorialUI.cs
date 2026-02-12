using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialUI : SemiUI
{
	public TextMeshProUGUI Text;

	public Transform progressBar;

	public AnimationCurve scaleInCurve;

	public static TutorialUI instance;

	private string messagePrev = "prev";

	private Color bigMessageColor = Color.white;

	private Color bigMessageFlashColor = Color.white;

	private float messageTimer;

	private float progressBarTarget;

	internal float progressBarCurrent;

	[HideInInspector]
	public float animationCurveEval;

	public VideoPlayer videoPlayer;

	public VideoClip staticVideo;

	public VideoClip nextVideo;

	private string nextText;

	private float bigVideoTimer = 5f;

	public Transform videoTransform;

	public RawImage videoImage;

	public TextMeshProUGUI dummyText;

	public TextMeshProUGUI dummyTextExclamation;

	public Transform dummyTextTransform;

	private float dummyTextAnimationEval;

	private float dummyTextTimer = 30f;

	private string currentDummyText;

	private float hideAllTimer;

	private bool scaleDown;

	protected override void Start()
	{
		uiText = Text;
		base.Start();
		instance = this;
		videoPlayer.clip = staticVideo;
		dummyTextTransform.gameObject.SetActive(value: false);
		dummyTextTimer = 30f;
		videoTransform.gameObject.SetActive(value: false);
		videoPlayer.gameObject.SetActive(value: false);
		base.transform.localScale = new Vector3(0f, 1f, 1f);
	}

	public void TutorialText(string message)
	{
		if (!(messageTimer > 0f))
		{
			messageTimer = 0.2f;
			if (message != messagePrev)
			{
				Text.text = message;
				SemiUISpringShakeY(20f, 10f, 0.3f);
				SemiUISpringScale(0.4f, 5f, 0.2f);
				messagePrev = message;
			}
		}
	}

	public void SetPage(VideoClip video, string text, string dummyTextString, bool transition = true)
	{
		SemiUISpringShakeY(10f, 8f, 0.5f);
		if (transition)
		{
			Text.text = "Good job! <sprite name=creepycrying>";
			videoPlayer.clip = staticVideo;
			nextVideo = video;
			nextText = text;
		}
		else
		{
			Text.text = text;
			videoPlayer.clip = video;
			nextVideo = video;
			nextText = text;
		}
		videoPlayer.Play();
		videoTransform.transform.localScale = new Vector3(1f, 1f, 1f);
		videoImage.color = new Color(1f, 1f, 1f, 1f);
		bigVideoTimer = 7f;
		currentDummyText = dummyTextString;
		dummyText.text = dummyTextString;
		dummyTextTimer = 30f;
		dummyTextAnimationEval = 0f;
		dummyTextTransform.gameObject.SetActive(value: false);
		StartCoroutine(SwitchPage());
	}

	public void SetTipPage(VideoClip video, string text, bool _scaleDown)
	{
		videoTransform.gameObject.SetActive(value: true);
		videoPlayer.gameObject.SetActive(value: true);
		videoPlayer.clip = video;
		Text.text = text;
		videoPlayer.time = 0.0;
		videoPlayer.Play();
		videoTransform.transform.localScale = new Vector3(1f, 1f, 1f);
		videoImage.color = new Color(1f, 1f, 1f, 1f);
		bigVideoTimer = 6f;
		currentDummyText = "";
		dummyText.text = "";
		dummyTextTimer = 30f;
		dummyTextAnimationEval = 0f;
		dummyTextTransform.gameObject.SetActive(value: false);
		scaleDown = _scaleDown;
	}

	private IEnumerator SwitchPage()
	{
		yield return new WaitForSeconds(2f);
		if (videoPlayer.clip != nextVideo)
		{
			SemiUISpringShakeY(10f, 8f, 0.5f);
		}
		videoPlayer.clip = nextVideo;
		videoPlayer.Play();
		Text.text = nextText;
	}

	protected override void Update()
	{
		base.Update();
		if (hideTimer > 0f && showTimer <= 0f)
		{
			if (hideAllTimer > 0f)
			{
				hideAllTimer -= Time.deltaTime;
				return;
			}
			dummyTextTransform.gameObject.SetActive(value: false);
			dummyTextTimer = 30f;
			videoTransform.gameObject.SetActive(value: false);
			videoPlayer.gameObject.SetActive(value: false);
			return;
		}
		videoTransform.gameObject.SetActive(value: true);
		videoPlayer.gameObject.SetActive(value: true);
		hideAllTimer = 2f;
		if (dummyTextTimer <= 0f)
		{
			if (currentDummyText != "" && !dummyTextTransform.gameObject.activeSelf)
			{
				dummyTextTransform.gameObject.SetActive(value: true);
				dummyText.text = currentDummyText;
				dummyTextAnimationEval = 0f;
				bigVideoTimer = 1f;
			}
			if (dummyTextAnimationEval < 1f)
			{
				dummyTextAnimationEval += Time.deltaTime * 3f;
				dummyTextAnimationEval = Mathf.Clamp01(dummyTextAnimationEval);
				float t = scaleInCurve.Evaluate(dummyTextAnimationEval);
				dummyTextTransform.localPosition = new Vector3(dummyTextTransform.localPosition.x, Mathf.LerpUnclamped(-20f, 20f, t), dummyTextTransform.localPosition.z);
			}
		}
		else
		{
			dummyTextTimer -= Time.deltaTime;
		}
		if (!SemiFunc.RunIsTutorial())
		{
			if (bigVideoTimer > 0f)
			{
				if (scaleDown)
				{
					bigVideoTimer -= Time.deltaTime;
				}
				float num = 1f;
				videoTransform.transform.localScale = new Vector3(Mathf.Lerp(videoTransform.transform.localScale.x, num, Time.deltaTime * 20f), Mathf.Lerp(videoTransform.transform.localScale.y, num, Time.deltaTime * 20f), Mathf.Lerp(videoTransform.transform.localScale.z, num, Time.deltaTime * 20f));
				float num2 = 1f;
				videoImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(videoImage.color.a, num2, Time.deltaTime * 20f));
			}
			else
			{
				float num3 = 0.7f;
				videoTransform.transform.localScale = new Vector3(Mathf.Lerp(videoTransform.transform.localScale.x, num3, Time.deltaTime * 20f), Mathf.Lerp(videoTransform.transform.localScale.y, num3, Time.deltaTime * 20f), Mathf.Lerp(videoTransform.transform.localScale.z, num3, Time.deltaTime * 20f));
				float num4 = 0.5f;
				videoImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(videoImage.color.a, num4, Time.deltaTime * 20f));
			}
		}
		progressBarTarget = TutorialDirector.instance.tutorialProgress;
		animationCurveEval += Time.deltaTime * 3f;
		animationCurveEval = Mathf.Clamp(animationCurveEval, 0f, 1f);
		float y = scaleInCurve.Evaluate(animationCurveEval);
		base.transform.localScale = new Vector3(1f, y, 1f);
		progressBarCurrent = progressBar.localScale.x;
		progressBar.localScale = new Vector3(Mathf.Lerp(progressBar.localScale.x, progressBarTarget, Time.deltaTime * 20f), 1f, 1f);
		if (currentDummyText == "" || dummyTextTimer > 0f)
		{
			dummyTextTransform.gameObject.SetActive(value: false);
		}
	}
}
