using UnityEngine;

public class MenuLoadingGraphics : MonoBehaviour
{
	private CanvasGroup loadingCanvasGroup;

	public RectTransform loadingCircle;

	public RectTransform loadingHourglass;

	public AnimationCurve hourglassCurve;

	private float hourglassLerp;

	private bool loadingActive = true;

	private bool loadingDone;

	private void Start()
	{
		loadingCanvasGroup = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		if (!loadingActive)
		{
			return;
		}
		if (loadingDone)
		{
			loadingCanvasGroup.alpha -= Time.deltaTime * 5f;
			if (loadingCanvasGroup.alpha <= 0.01f)
			{
				loadingCanvasGroup.alpha = 0f;
				loadingCanvasGroup.gameObject.SetActive(value: false);
				loadingActive = false;
			}
		}
		else
		{
			loadingCanvasGroup.alpha += Time.deltaTime * 5f;
		}
		loadingCircle.Rotate(new Vector3(0f, 0f, (0f - Time.deltaTime) * 360f));
		hourglassLerp += Time.deltaTime * 1f;
		if (hourglassLerp > 1f)
		{
			float pitch = MenuManager.instance.soundHover.Pitch;
			MenuManager.instance.soundHover.Pitch = 0.3f;
			MenuManager.instance.soundHover.Play(Vector3.zero, 0.5f);
			MenuManager.instance.soundHover.Pitch = pitch;
			hourglassLerp = 0f;
		}
		loadingHourglass.eulerAngles = new Vector3(0f, 0f, Mathf.LerpUnclamped(90f, 0f, hourglassCurve.Evaluate(hourglassLerp)));
	}

	public void Reset()
	{
		loadingActive = true;
		loadingDone = false;
		loadingCanvasGroup.gameObject.SetActive(value: true);
	}

	public void SetDone()
	{
		loadingDone = true;
	}
}
