using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OverchargeUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static OverchargeUI instance;

	public TextMeshProUGUI textOverchargeMax;

	private float warningTimer;

	public Sound soundOverchargeWarning;

	public Image roundBar;

	public OverchargeRoundBarUI overchargeRoundBarUI;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
	}

	protected override void Update()
	{
		base.Update();
		if (!PlayerAvatar.instance.isDisabled)
		{
			if (!PhysGrabber.instance || ((bool)PhysGrabber.instance && PhysGrabber.instance.physGrabBeamOverChargeFloat <= 0f))
			{
				Hide();
			}
		}
		else
		{
			Hide();
		}
		if (!isHidden)
		{
			float num = Mathf.Ceil(PhysGrabber.instance.physGrabBeamOverChargeFloat * 100f);
			if (!roundBar.enabled)
			{
				roundBar.enabled = true;
			}
			roundBar.fillAmount = num / 100f;
			Text.text = num.ToString();
			textOverchargeMax.text = "<b><color=red>/</color></b>" + Mathf.Ceil(100f);
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
			else if (num > 90f)
			{
				warningTimer += Time.deltaTime * 2f;
			}
			if (warningTimer > 1f)
			{
				Color color = new Color(1f, 0.6f, 0f);
				soundOverchargeWarning.Play(PlayerAvatar.instance.transform.position);
				SemiUITextFlashColor(color, 0.05f);
				SemiUISpringScale(1.1f, 0.25f, 0.05f);
				warningTimer = 0f;
			}
		}
		else if (roundBar.enabled)
		{
			roundBar.enabled = false;
		}
	}
}
