using UnityEngine;
using UnityEngine.UI;

public class ValuableDiscoverGraphic : MonoBehaviour
{
	public enum State
	{
		Discover,
		Reminder,
		Bad
	}

	public PhysGrabObject target;

	private State state;

	private Camera mainCamera;

	private RectTransform canvasRect;

	private Vector3[] screenSpaceCorners;

	private bool hidden = true;

	private bool first = true;

	[Space]
	public Color colorDiscoverCorner;

	public Color colorDiscoverMiddle;

	[Space]
	public Color ColorReminderCorner;

	public Color ColorReminderMiddle;

	[Space]
	public Color ColorBadCorner;

	public Color ColorBadMiddle;

	[Space]
	public Sound sound;

	[Space]
	public AnimationCurve introCurve;

	public float introSpeed;

	public AnimationCurve outroCurve;

	public float outroSpeed;

	public float waitTime;

	private float waitTimer;

	private float animLerp;

	[Space]
	public RectTransform middle;

	private Vector2 middleTarget;

	private Vector2 middleTargetNew;

	private Vector2 middleTargetSize;

	private Vector2 middleTargetSizeNew;

	public RectTransform topLeft;

	private Vector2 topLeftTarget;

	private Vector2 topLeftTargetNew;

	public RectTransform topRight;

	private Vector2 topRightTarget;

	private Vector2 topRightTargetNew;

	public RectTransform botLeft;

	private Vector2 botLeftTarget;

	private Vector2 botLeftTargetNew;

	public RectTransform botRight;

	private Vector2 botRightTarget;

	private Vector2 botRightTargetNew;

	private CanvasGroup canvasGroup;

	private void Start()
	{
		canvasRect = ValuableDiscover.instance.canvasRect;
		mainCamera = Camera.main;
		waitTimer = waitTime;
		if (state == State.Reminder)
		{
			waitTimer = waitTime * 0.5f;
		}
		if (state == State.Bad)
		{
			waitTimer = waitTime * 3f;
		}
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		if ((bool)target)
		{
			bool flag = true;
			Bounds bigBounds = new Bounds(target.centerPoint, Vector3.zero);
			MeshRenderer[] componentsInChildren = target.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				bigBounds.Encapsulate(meshRenderer.bounds);
			}
			if (SemiFunc.OnScreen(bigBounds.center, 0.5f, 0.5f))
			{
				Rect rect = RendererBoundsInScreenSpace(bigBounds);
				if (rect.width > 2f || rect.height > 2f)
				{
					topLeftTargetNew = rect.center;
					topRightTargetNew = rect.center;
					botLeftTargetNew = rect.center;
					botRightTargetNew = rect.center;
					middleTargetNew = rect.center;
					middleTargetSizeNew = new Vector2(0f, 0f);
				}
				else
				{
					topLeftTargetNew = GetScreenPosition(new Vector3(rect.xMin, rect.yMax, 0f));
					topRightTargetNew = GetScreenPosition(new Vector3(rect.xMax, rect.yMax, 0f));
					botLeftTargetNew = GetScreenPosition(new Vector3(rect.xMin, rect.yMin, 0f));
					botRightTargetNew = GetScreenPosition(new Vector3(rect.xMax, rect.yMin, 0f));
					middleTargetNew = GetScreenPosition(rect.center);
					middleTargetSizeNew = new Vector2(rect.width * 1.9f + 0.025f, rect.height + 0.025f);
				}
			}
			else
			{
				flag = false;
			}
			if (flag)
			{
				if (first)
				{
					if (state == State.Reminder)
					{
						sound.Play(target.centerPoint, 0.3f);
					}
					else
					{
						sound.Play(target.centerPoint);
					}
					middle.gameObject.SetActive(value: true);
					topLeft.gameObject.SetActive(value: true);
					topRight.gameObject.SetActive(value: true);
					botLeft.gameObject.SetActive(value: true);
					botRight.gameObject.SetActive(value: true);
					first = false;
				}
				if (hidden)
				{
					middleTarget = middleTargetNew;
					middleTargetSize = middleTargetSizeNew;
					topLeftTarget = topLeftTargetNew;
					topRightTarget = topRightTargetNew;
					botLeftTarget = botLeftTargetNew;
					botRightTarget = botRightTargetNew;
					hidden = false;
				}
				middleTarget = Vector2.Lerp(middleTarget, middleTargetNew, 50f * Time.deltaTime);
				middleTargetSize = Vector2.Lerp(middleTargetSize, middleTargetSizeNew, 50f * Time.deltaTime);
				topLeftTarget = Vector2.Lerp(topLeftTarget, topLeftTargetNew, 50f * Time.deltaTime);
				topRightTarget = Vector2.Lerp(topRightTarget, topRightTargetNew, 50f * Time.deltaTime);
				botLeftTarget = Vector2.Lerp(botLeftTarget, botLeftTargetNew, 50f * Time.deltaTime);
				botRightTarget = Vector2.Lerp(botRightTarget, botRightTargetNew, 50f * Time.deltaTime);
				canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, 50f * Time.deltaTime);
			}
			else
			{
				hidden = true;
				topLeftTarget = middleTarget;
				topRightTarget = middleTarget;
				botLeftTarget = middleTarget;
				botRightTarget = middleTarget;
				middleTargetSize = Vector2.zero;
				canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, 50f * Time.deltaTime);
			}
		}
		else
		{
			waitTimer = 0f;
		}
		middle.anchoredPosition = middleTarget;
		if (waitTimer > 0f)
		{
			animLerp = Mathf.Clamp01(animLerp + introSpeed * Time.deltaTime);
			middle.sizeDelta = Vector2.LerpUnclamped(Vector2.zero, middleTargetSize, introCurve.Evaluate(animLerp));
			topLeft.anchoredPosition = Vector2.LerpUnclamped(middleTarget, topLeftTarget, introCurve.Evaluate(animLerp));
			topRight.anchoredPosition = Vector2.LerpUnclamped(middleTarget, topRightTarget, introCurve.Evaluate(animLerp));
			botLeft.anchoredPosition = Vector2.LerpUnclamped(middleTarget, botLeftTarget, introCurve.Evaluate(animLerp));
			botRight.anchoredPosition = Vector2.LerpUnclamped(middleTarget, botRightTarget, introCurve.Evaluate(animLerp));
			if (animLerp >= 1f)
			{
				waitTimer -= Time.deltaTime;
				if (waitTimer <= 0f)
				{
					animLerp = 0f;
				}
			}
		}
		else
		{
			animLerp = Mathf.Clamp01(animLerp + outroSpeed * Time.deltaTime);
			middle.sizeDelta = Vector2.LerpUnclamped(middleTargetSize, Vector2.zero, outroCurve.Evaluate(animLerp));
			topLeft.anchoredPosition = Vector2.LerpUnclamped(topLeftTarget, middleTarget, outroCurve.Evaluate(animLerp));
			topRight.anchoredPosition = Vector2.LerpUnclamped(topRightTarget, middleTarget, outroCurve.Evaluate(animLerp));
			botLeft.anchoredPosition = Vector2.LerpUnclamped(botLeftTarget, middleTarget, outroCurve.Evaluate(animLerp));
			botRight.anchoredPosition = Vector2.LerpUnclamped(botRightTarget, middleTarget, outroCurve.Evaluate(animLerp));
			if (animLerp >= 1f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void ReminderSetup()
	{
		state = State.Reminder;
		middle.GetComponent<Image>().color = ColorReminderMiddle;
		topLeft.GetComponent<Image>().color = ColorReminderCorner;
		topRight.GetComponent<Image>().color = ColorReminderCorner;
		botLeft.GetComponent<Image>().color = ColorReminderCorner;
		botRight.GetComponent<Image>().color = ColorReminderCorner;
	}

	public void BadSetup()
	{
		state = State.Bad;
		middle.GetComponent<Image>().color = ColorBadMiddle;
		topLeft.GetComponent<Image>().color = ColorBadCorner;
		topRight.GetComponent<Image>().color = ColorBadCorner;
		botLeft.GetComponent<Image>().color = ColorBadCorner;
		botRight.GetComponent<Image>().color = ColorBadCorner;
	}

	private Vector3 GetScreenPosition(Vector3 _position)
	{
		return new Vector3(_position.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f, _position.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f, _position.z) / SemiFunc.UIMulti();
	}

	private Rect RendererBoundsInScreenSpace(Bounds bigBounds)
	{
		if (screenSpaceCorners == null)
		{
			screenSpaceCorners = new Vector3[8];
		}
		screenSpaceCorners[0] = mainCamera.WorldToViewportPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
		screenSpaceCorners[1] = mainCamera.WorldToViewportPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));
		screenSpaceCorners[2] = mainCamera.WorldToViewportPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
		screenSpaceCorners[3] = mainCamera.WorldToViewportPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));
		screenSpaceCorners[4] = mainCamera.WorldToViewportPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
		screenSpaceCorners[5] = mainCamera.WorldToViewportPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));
		screenSpaceCorners[6] = mainCamera.WorldToViewportPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
		screenSpaceCorners[7] = mainCamera.WorldToViewportPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));
		float x = screenSpaceCorners[0].x;
		float y = screenSpaceCorners[0].y;
		float x2 = screenSpaceCorners[0].x;
		float y2 = screenSpaceCorners[0].y;
		for (int i = 1; i < 8; i++)
		{
			if (screenSpaceCorners[i].x < x)
			{
				x = screenSpaceCorners[i].x;
			}
			if (screenSpaceCorners[i].y < y)
			{
				y = screenSpaceCorners[i].y;
			}
			if (screenSpaceCorners[i].x > x2)
			{
				x2 = screenSpaceCorners[i].x;
			}
			if (screenSpaceCorners[i].y > y2)
			{
				y2 = screenSpaceCorners[i].y;
			}
		}
		return Rect.MinMaxRect(x, y, x2, y2);
	}
}
