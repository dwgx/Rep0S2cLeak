using UnityEngine;

public class PlayerExpressionsUI : SemiUI
{
	public static PlayerExpressionsUI instance;

	public GameObject PlayerAvatarMenu;

	public PlayerExpression playerExpression;

	public PlayerAvatarVisuals playerAvatarVisuals;

	[Space]
	public AnimationCurve shrinkCurve;

	public RectTransform rectShrink;

	private CanvasGroup canvasGroup;

	private float shrinkLerp;

	private bool shrinkActive;

	private float shrinkTimer;

	private Vector3 shrinkScalePrevious;

	private float canvasGroupAlphaPrevious;

	private void Awake()
	{
		instance = this;
	}

	protected override void Start()
	{
		base.Start();
		uiText = null;
		canvasGroup = GetComponent<CanvasGroup>();
	}

	protected override void Update()
	{
		if (!LevelGenerator.Instance.Generated || SemiFunc.MenuLevel())
		{
			return;
		}
		base.Update();
		Hide();
		if (isHidden)
		{
			ShrinkReset();
			if (PlayerAvatarMenu.activeSelf)
			{
				PlayerAvatarMenu.SetActive(value: false);
			}
		}
		else if (!PlayerAvatarMenu.activeSelf)
		{
			PlayerAvatarMenu.SetActive(value: true);
		}
		if (shrinkTimer > 0f)
		{
			if (!shrinkActive)
			{
				shrinkActive = true;
				shrinkLerp = 0f;
				shrinkScalePrevious = rectShrink.localScale;
				canvasGroupAlphaPrevious = canvasGroup.alpha;
			}
			shrinkTimer -= Time.deltaTime;
			if (shrinkLerp < 1f)
			{
				shrinkLerp += Time.deltaTime * 2f;
				rectShrink.localScale = Vector3.LerpUnclamped(shrinkScalePrevious, Vector3.one, shrinkCurve.Evaluate(shrinkLerp));
				canvasGroup.alpha = Mathf.LerpUnclamped(canvasGroupAlphaPrevious, 1f, shrinkCurve.Evaluate(shrinkLerp));
			}
		}
		else
		{
			if (shrinkActive)
			{
				shrinkActive = false;
				shrinkLerp = 0f;
				shrinkScalePrevious = rectShrink.localScale;
				canvasGroupAlphaPrevious = canvasGroup.alpha;
			}
			if (shrinkLerp < 1f)
			{
				shrinkLerp += Time.deltaTime * 0.5f;
				rectShrink.localScale = Vector3.LerpUnclamped(shrinkScalePrevious, Vector3.one * 0.7f, shrinkCurve.Evaluate(shrinkLerp));
				canvasGroup.alpha = Mathf.LerpUnclamped(canvasGroupAlphaPrevious, 0.35f, shrinkCurve.Evaluate(shrinkLerp));
			}
		}
	}

	public void ShrinkReset()
	{
		shrinkTimer = 5f;
	}
}
