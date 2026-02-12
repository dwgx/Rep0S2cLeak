using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatteryVisualLogic : MonoBehaviour
{
	public HorizontalLayoutGroup batteryBarContainerGroup;

	public GameObject batteryBarPrefab;

	public int batteryBars = 3;

	public Transform batteryBarContainer;

	public bool inUI;

	public bool cameraTurn = true;

	public Transform batteryBarCharge;

	public Transform batteryBarDrain;

	internal List<GameObject> bars = new List<GameObject>();

	internal float batteryPercent = 100f;

	internal int currentBars = 3;

	public ItemBattery itemBattery;

	private ItemBattery itemBatteryPrev;

	public RawImage batteryBorderShadow;

	public RawImage batteryBorderMain;

	public RawImage batteryBackground;

	public RawImage batteryCharge;

	public RawImage batteryDrain;

	public GameObject batteryOutVisual;

	public GameObject batteryChargeNeededVisual;

	private float batteryDrainFullXScale = 0.93f;

	private float batteryChargeFullXScale = 1f;

	private Color batteryColorMain = new Color(1f, 1f, 0f, 1f);

	private Color batteryColorBackground = new Color(0.1f, 0.1f, 0.1f, 1f);

	private Color batteryColorShadow = new Color(0.1f, 0.1f, 0.1f, 0.5f);

	private Color batteryColorWarning = new Color(1f, 0f, 0f, 1f);

	private Color batteryColorCharge = new Color(0f, 1f, 0f, 1f);

	private Color batteryColorDrain = new Color(1f, 0.2f, 0f, 1f);

	private SpringFloat springScale = new SpringFloat();

	private SpringFloat springRotation = new SpringFloat();

	private SpringVector3 springPosition = new SpringVector3();

	public GameObject barLossEffectPrefab;

	internal float targetScale = 1f;

	private float targetRotation;

	private Vector3 targetPosition = Vector3.zero;

	private float targetScaleOriginal = 1f;

	private float targetRotationOriginal;

	private Vector3 targetPositionOriginal = Vector3.zero;

	private float overrideTimerBatteryOutWarning;

	private bool batteryIsOutWarning;

	private float overrideTimerBatteryDrain;

	private bool batteryIsDraining;

	private float overrideTimerBatteryCharge;

	private bool batteryIsCharging;

	private float overrideTimerChargeNeeded;

	private bool batteryChargeNeeded;

	private float chargeAnimationProgress;

	private float drainAnimationProgress;

	private float warningAnimationProgress;

	private float chargeNeededAnimationProgress;

	internal bool doOutro;

	private void Awake()
	{
		targetPosition = base.transform.localPosition;
		targetScale = base.transform.localScale.x;
		targetScaleOriginal = targetScale;
		targetRotationOriginal = targetRotation;
		targetPositionOriginal = targetPosition;
	}

	public void HideForCameras()
	{
		if (!inUI)
		{
			base.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			}
		}
	}

	private void Start()
	{
		springScale = new SpringFloat();
		springScale.damping = 0.4f;
		springScale.speed = 30f;
		springRotation = new SpringFloat();
		springRotation.damping = 0.3f;
		springRotation.speed = 40f;
		springPosition = new SpringVector3();
		springPosition.damping = 0.35f;
		springPosition.speed = 30f;
		springPosition.lastPosition = targetPosition;
		SetSpacing();
		BatteryBarsSet();
		batteryBarCharge.gameObject.SetActive(value: false);
		batteryBarDrain.gameObject.SetActive(value: false);
		batteryDrainFullXScale = batteryBarDrain.localScale.x;
		batteryChargeFullXScale = batteryBarCharge.localScale.x;
	}

	public void VisibleForCamerasAgain()
	{
		if (!inUI)
		{
			base.gameObject.layer = LayerMask.NameToLayer("Default");
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("Default");
			}
		}
	}

	private void SetAllMainColors()
	{
		batteryBorderMain.color = batteryColorMain;
		batteryBackground.color = batteryColorBackground;
		batteryBorderShadow.color = batteryColorShadow;
		batteryCharge.color = batteryColorCharge;
		batteryDrain.color = batteryColorDrain;
		foreach (GameObject bar in bars)
		{
			bar.GetComponent<RawImage>().color = batteryColorMain;
		}
	}

	private void BatteryVisualBounce()
	{
		springPosition.springVelocity = new Vector3(0f, 500f * base.transform.localScale.x, 0f);
		springRotation.springVelocity = 1000f;
	}

	public void OverrideBatteryOutWarning(float _time)
	{
		overrideTimerBatteryOutWarning = _time;
		batteryIsOutWarning = true;
	}

	public void OverrideChargeNeeded(float _time)
	{
		overrideTimerChargeNeeded = _time;
		batteryChargeNeeded = true;
	}

	public void OverrideBatteryDrain(float _time)
	{
		if (!batteryIsDraining)
		{
			drainAnimationProgress = 0f;
			if (!batteryDrain.gameObject.activeSelf)
			{
				batteryDrain.gameObject.SetActive(value: true);
			}
			BatteryVisualBounce();
		}
		overrideTimerBatteryDrain = _time;
		batteryIsDraining = true;
	}

	public void OverrideBatteryCharge(float _time)
	{
		if (!batteryIsCharging)
		{
			chargeAnimationProgress = 0f;
			if (!batteryCharge.gameObject.activeSelf)
			{
				batteryCharge.gameObject.SetActive(value: true);
			}
			BatteryVisualBounce();
		}
		overrideTimerBatteryCharge = _time;
		batteryIsCharging = true;
	}

	private void OverrideBatteryOutWarningTimer()
	{
		if (overrideTimerBatteryOutWarning > 0f)
		{
			overrideTimerBatteryOutWarning -= Time.deltaTime;
			return;
		}
		if (batteryIsOutWarning)
		{
			BatteryColorMainReset();
		}
		if (batteryOutVisual.activeSelf)
		{
			batteryOutVisual.SetActive(value: false);
		}
		batteryIsOutWarning = false;
	}

	private void OverrideChargeNeededTimer()
	{
		if (overrideTimerChargeNeeded > 0f)
		{
			overrideTimerChargeNeeded -= Time.deltaTime;
			return;
		}
		if (batteryChargeNeededVisual.activeSelf)
		{
			batteryChargeNeededVisual.SetActive(value: false);
		}
		batteryChargeNeeded = false;
	}

	private void OverrideBatteryDrainTimer()
	{
		if (overrideTimerBatteryDrain > 0f)
		{
			overrideTimerBatteryDrain -= Time.deltaTime;
			return;
		}
		if (batteryDrain.gameObject.activeSelf)
		{
			batteryDrain.gameObject.SetActive(value: false);
		}
		batteryIsDraining = false;
	}

	private void OverrideBatteryChargeTimer()
	{
		if (overrideTimerBatteryCharge > 0f)
		{
			overrideTimerBatteryCharge -= Time.deltaTime;
			return;
		}
		if (batteryCharge.gameObject.activeSelf)
		{
			batteryCharge.gameObject.SetActive(value: false);
		}
		batteryIsCharging = false;
	}

	private void OverrideTimers()
	{
		OverrideBatteryOutWarningTimer();
		OverrideBatteryDrainTimer();
		OverrideBatteryChargeTimer();
		OverrideChargeNeededTimer();
	}

	private void BatteryDrainVisuals()
	{
		if (batteryIsDraining)
		{
			float t = drainAnimationProgress;
			batteryBarDrain.localScale = new Vector3(Mathf.Lerp(batteryDrainFullXScale, 0f, t), batteryBarDrain.localScale.y, batteryBarDrain.localScale.z);
			if (drainAnimationProgress >= 1f)
			{
				drainAnimationProgress = 0f;
				springPosition.springVelocity = new Vector3(-250f * base.transform.localScale.x, 0f, 0f);
				springRotation.springVelocity = -50f;
			}
			drainAnimationProgress += Time.deltaTime * 5f;
		}
	}

	private void BatteryChargeVisuals()
	{
		if (batteryIsCharging)
		{
			float t = chargeAnimationProgress;
			batteryBarCharge.localScale = new Vector3(Mathf.Lerp(0f, batteryChargeFullXScale, t), batteryBarCharge.localScale.y, batteryBarCharge.localScale.z);
			if (chargeAnimationProgress >= 1f)
			{
				chargeAnimationProgress = 0f;
				springPosition.springVelocity = new Vector3(250f * base.transform.localScale.x, 0f, 0f);
				springRotation.springVelocity = 50f;
			}
			chargeAnimationProgress += Time.deltaTime * 5f;
		}
	}

	private void BatteryOutWarningVisuals()
	{
		if (!batteryIsOutWarning)
		{
			return;
		}
		float t = warningAnimationProgress;
		batteryBorderMain.color = Color.Lerp(batteryColorMain, batteryColorWarning, t);
		if (warningAnimationProgress > 0.5f && batteryOutVisual.activeSelf)
		{
			batteryOutVisual.SetActive(value: false);
		}
		if (warningAnimationProgress >= 1f)
		{
			if (!batteryOutVisual.activeSelf)
			{
				batteryOutVisual.SetActive(value: true);
			}
			warningAnimationProgress = 0f;
		}
		warningAnimationProgress += Time.deltaTime * 2f;
	}

	private void BatteryChargeNeededVisuals()
	{
		if (!batteryChargeNeeded)
		{
			return;
		}
		if (chargeNeededAnimationProgress > 0.5f && batteryChargeNeededVisual.activeSelf)
		{
			batteryChargeNeededVisual.SetActive(value: false);
		}
		if (chargeNeededAnimationProgress >= 1f)
		{
			if (!batteryChargeNeededVisual.activeSelf)
			{
				batteryChargeNeededVisual.SetActive(value: true);
			}
			chargeNeededAnimationProgress = 0f;
		}
		chargeNeededAnimationProgress += Time.deltaTime * 0.5f;
	}

	private void BatteryVisuals()
	{
		BatteryDrainVisuals();
		BatteryChargeVisuals();
		BatteryOutWarningVisuals();
		BatteryChargeNeededVisuals();
	}

	private void Update()
	{
		if (itemBatteryPrev != itemBattery)
		{
			BatteryBarsSet();
			itemBatteryPrev = itemBattery;
		}
		base.transform.localPosition = SemiFunc.SpringVector3Get(springPosition, targetPosition);
		float num = SemiFunc.SpringFloatGet(springScale, targetScale);
		base.transform.localScale = new Vector3(num, num, num);
		float z = SemiFunc.SpringFloatGet(springRotation, targetRotation);
		base.transform.localRotation = Quaternion.Euler(0f, 0f, z);
		if (doOutro && num <= 0f)
		{
			ResetOutro();
			base.gameObject.SetActive(value: false);
		}
		OverrideTimers();
		BatteryVisuals();
	}

	public void ResetOutro()
	{
		targetScale = targetScaleOriginal;
		targetRotation = targetRotationOriginal;
		doOutro = false;
	}

	private void OnEnable()
	{
		base.transform.localScale = Vector3.zero;
		springScale.lastPosition = 0f;
		base.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
		springRotation.lastPosition = -90f;
		springPosition.lastPosition = new Vector3(targetPosition.x, targetPosition.y - 0.5f, targetPosition.z);
		base.transform.localPosition = new Vector3(targetPosition.x, targetPosition.y - 0.5f, targetPosition.z);
		SetSpacing();
		SetBatteryColor();
		doOutro = false;
	}

	private void SetBatteryColor()
	{
		if ((bool)itemBattery)
		{
			if (itemBattery.currentBars > 0)
			{
				BatteryColorMainReset();
			}
			else
			{
				BatteryColorMainSet(Color.red);
			}
		}
	}

	public void BatteryBarsSet()
	{
		SetAllMainColors();
		int num = 6;
		if ((bool)itemBattery)
		{
			num = itemBattery.batteryBars;
		}
		foreach (GameObject bar in bars)
		{
			Object.Destroy(bar);
		}
		bars.Clear();
		batteryBars = num;
		for (int i = 0; i < num; i++)
		{
			GameObject item = Object.Instantiate(batteryBarPrefab, batteryBarContainer);
			bars.Add(item);
		}
		if ((bool)itemBattery)
		{
			int num2 = itemBattery.currentBars;
			for (int j = 0; j < bars.Count; j++)
			{
				if (j >= num2)
				{
					bars[j].GetComponent<RawImage>().color = new Color(1f, 1f, 1f, 0f);
				}
			}
			SetBatteryColor();
			currentBars = num2;
		}
		if ((bool)itemBattery)
		{
			for (int k = 0; k < bars.Count; k++)
			{
				if (k >= itemBattery.batteryLifeInt)
				{
					bars[k].GetComponent<RawImage>().color = new Color(1f, 1f, 1f, 0f);
				}
				else
				{
					bars[k].GetComponent<RawImage>().color = batteryColorMain;
				}
			}
		}
		SetSpacing();
	}

	private void SetSpacing()
	{
		batteryBarContainerGroup.spacing = 12f / (float)batteryBars;
		if (batteryBars <= 8)
		{
			batteryBarContainerGroup.spacing = 2f;
		}
		if (batteryBars > 8)
		{
			batteryBarContainerGroup.spacing = 1f;
		}
	}

	public void BatteryBarsUpdate(int _setToBars = -1, bool _forceUpdate = false)
	{
		if (!itemBattery && _setToBars == -1)
		{
			return;
		}
		int num = _setToBars;
		if ((bool)itemBattery)
		{
			num = itemBattery.currentBars;
		}
		if (!inUI && !_forceUpdate && BatteryUI.instance.batteryVisualLogic.itemBattery == itemBattery)
		{
			BatteryUI.instance.batteryVisualLogic.BatteryBarsUpdate(num, _forceUpdate: true);
		}
		SetBatteryColor();
		currentBars = num;
		for (int i = 0; i < bars.Count; i++)
		{
			if (i >= num)
			{
				if (bars[i].GetComponent<RawImage>().color.a != 0f)
				{
					BatteryVisualBounce();
					Object.Instantiate(barLossEffectPrefab, bars[i].transform).transform.localPosition = new Vector3(0f, 0f, 0f);
				}
				bars[i].GetComponent<RawImage>().color = new Color(1f, 1f, 1f, 0f);
				continue;
			}
			if (bars[i].GetComponent<RawImage>().color.a == 0f)
			{
				BatteryVisualBounce();
				GameObject obj = Object.Instantiate(barLossEffectPrefab, bars[i].transform);
				obj.transform.localPosition = new Vector3(0f, 0f, 0f);
				BatteryBarEffect component = obj.GetComponent<BatteryBarEffect>();
				component.barLossEffect = false;
				component.barColor = Color.green;
			}
			bars[i].GetComponent<RawImage>().color = batteryColorMain;
		}
	}

	public void BatteryColorMainSet(Color _color)
	{
		batteryBorderMain.color = _color;
		foreach (GameObject bar in bars)
		{
			if (bar.GetComponent<RawImage>().color.a > 0f)
			{
				bar.GetComponent<RawImage>().color = _color;
			}
		}
	}

	public void BatteryColorMainReset()
	{
		batteryBorderMain.color = itemBattery.batteryColorMedium;
		batteryColorMain = itemBattery.batteryColor;
		foreach (GameObject bar in bars)
		{
			if (bar.GetComponent<RawImage>().color.a > 0f)
			{
				bar.GetComponent<RawImage>().color = batteryColorMain;
			}
		}
	}

	public void HideCurrentBar(bool _hide, Color _blinkColor)
	{
		if ((bool)itemBattery)
		{
			int num = itemBattery.currentBars;
			if (num > 0)
			{
				bars[num - 1].GetComponent<RawImage>().color = (_hide ? new Color(_blinkColor.r, _blinkColor.g, _blinkColor.b, 0f) : _blinkColor);
				return;
			}
			_ = batteryColorMain;
			bars[0].GetComponent<RawImage>().color = (_hide ? new Color(_blinkColor.r, _blinkColor.g, _blinkColor.b, 0f) : _blinkColor);
		}
	}

	public void BatteryOutro()
	{
		if (!doOutro)
		{
			springScale.springVelocity = 0.1f;
			springRotation.springVelocity = -1000f;
			doOutro = true;
			targetScale = 0f;
		}
	}
}
