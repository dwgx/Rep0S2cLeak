using UnityEngine;

public class ValuableDiscover : MonoBehaviour
{
	public static ValuableDiscover instance;

	public GameObject graphicPrefab;

	public RectTransform canvasRect;

	private CanvasGroup canvasGroup;

	private float hideTimer;

	internal float hideAlpha = 1f;

	private void Awake()
	{
		instance = this;
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		float num = 1f;
		if (hideTimer > 0f)
		{
			num = 0f;
			hideTimer -= Time.deltaTime;
		}
		hideAlpha = Mathf.Lerp(hideAlpha, num, Time.deltaTime * 20f);
		canvasGroup.alpha = hideAlpha;
	}

	public void New(PhysGrabObject _target, ValuableDiscoverGraphic.State _state)
	{
		ValuableDiscoverGraphic component = Object.Instantiate(graphicPrefab, base.transform).GetComponent<ValuableDiscoverGraphic>();
		component.target = _target;
		if (_state == ValuableDiscoverGraphic.State.Reminder)
		{
			component.ReminderSetup();
		}
		if (_state == ValuableDiscoverGraphic.State.Bad)
		{
			component.BadSetup();
		}
	}

	public void Hide()
	{
		hideTimer = 0.1f;
	}
}
