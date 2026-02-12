using UnityEngine;
using UnityEngine.UI;

public class OverchargeRoundBarUI : MonoBehaviour
{
	public Image roundBar;

	public Image roundBarBG;

	public Image overchargeWarningIcon;

	public GameObject roundBarObject;

	internal float roundBarFillAmount;

	private SpringFloat roundBarScale;

	private float scaleTarget;

	private float warningTimer;

	private float flashColorTime;

	private float flashColorTimer;

	private Color flashColor;

	private Color originalColor;

	public Sound soundOverchargeWarning;

	private float showTimer;

	private bool show;

	private CanvasGroup canvasGroup;

	public AnimationCurve ringFillCurve;

	private void Start()
	{
		roundBarScale = new SpringFloat();
		roundBarScale.speed = 40f;
		roundBarScale.damping = 0.5f;
		originalColor = roundBar.color;
		overchargeWarningIcon.enabled = false;
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private void Show()
	{
		showTimer = 0.2f;
	}

	private void Update()
	{
		if ((bool)WorldSpaceUIParent.instance)
		{
			canvasGroup.alpha = WorldSpaceUIParent.instance.canvasGroup.alpha;
		}
		if (showTimer <= 0f && show)
		{
			show = false;
		}
		if (showTimer > 0f)
		{
			show = true;
			showTimer -= Time.deltaTime;
		}
		if (show)
		{
			scaleTarget = 1f;
			if (!roundBar.enabled)
			{
				roundBarScale.springVelocity = 50f;
				roundBarScale.speed = 40f;
				roundBarScale.damping = 0.5f;
				roundBar.enabled = true;
				roundBarBG.enabled = true;
			}
		}
		else if (scaleTarget != 0f)
		{
			roundBarScale.speed = 50f;
			roundBarScale.damping = 0.9f;
			roundBarScale.springVelocity = 50f;
			scaleTarget = 0f;
		}
		if (!show && roundBarObject.transform.localScale.x <= 0f && roundBar.enabled)
		{
			roundBar.enabled = false;
			roundBarBG.enabled = false;
			overchargeWarningIcon.enabled = false;
			roundBarObject.transform.localScale = Vector3.zero;
			roundBarScale.springVelocity = 0f;
			roundBarScale.lastPosition = 0f;
		}
		FlashColorLogic();
		float num = Mathf.Ceil(PhysGrabber.instance.physGrabBeamOverChargeFloat * 100f);
		if (num > 0f && GameDirector.instance.currentState == GameDirector.gameState.Main)
		{
			Show();
		}
		roundBarFillAmount = ringFillCurve.Evaluate(num / 100f);
		roundBar.fillAmount = Mathf.Lerp(roundBar.fillAmount, roundBarFillAmount, Time.deltaTime * 5f);
		float num2 = SemiFunc.SpringFloatGet(roundBarScale, scaleTarget);
		roundBarObject.transform.localScale = new Vector3(num2, num2, num2);
		if (PhysGrabber.instance.grabbed && (bool)PhysGrabber.instance.grabbedPhysGrabObject && PhysGrabber.instance.grabbedPhysGrabObject.isEnemy)
		{
			if (num > 70f)
			{
				warningTimer += Time.deltaTime * 2f;
			}
			if (num > 80f)
			{
				warningTimer += Time.deltaTime * 2f;
			}
			if (num > 90f)
			{
				warningTimer += Time.deltaTime * 2f;
			}
			if (num > 95f)
			{
				warningTimer += Time.deltaTime * 2f;
			}
		}
		else if (num > 80f)
		{
			warningTimer += Time.deltaTime * 2f;
		}
		if (warningTimer > 1f)
		{
			Color color = new Color(1f, 0.6f, 0f);
			soundOverchargeWarning.Play(PlayerAvatar.instance.transform.position);
			FlashColor(color, 0.2f);
			roundBarScale.springVelocity = 100f;
			warningTimer = 0f;
		}
	}

	private void FlashColor(Color color, float time)
	{
		flashColor = color;
		flashColorTime = time;
		flashColorTimer = time;
	}

	private void FlashColorLogic()
	{
		if (flashColorTimer > 0f)
		{
			overchargeWarningIcon.enabled = true;
			roundBar.color = Color.Lerp(flashColor, originalColor, flashColorTimer / flashColorTime);
			overchargeWarningIcon.color = Color.Lerp(flashColor, originalColor, flashColorTimer / flashColorTime);
			flashColorTimer -= Time.deltaTime;
		}
		else
		{
			overchargeWarningIcon.enabled = false;
			overchargeWarningIcon.color = originalColor;
			roundBar.color = originalColor;
		}
	}
}
