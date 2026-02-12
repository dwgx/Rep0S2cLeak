using TMPro;
using UnityEngine;

public class WorldSpaceUIValue : WorldSpaceUIChild
{
	public static WorldSpaceUIValue instance;

	private float showTimer;

	private Vector3 scale;

	private int value;

	private TextMeshProUGUI text;

	private Vector3 newWorldPosition;

	private Vector3 offset;

	private PhysGrabObject currentPhysGrabObject;

	public AnimationCurve curveIntro;

	public AnimationCurve curveOutro;

	private float curveLerp;

	[Space]
	public Color colorValue;

	public Color colorCost;

	[Space]
	public float textSizeValue;

	public float textSizeCost;

	private void Awake()
	{
		positionOffset = new Vector3(0f, -0.05f, 0f);
		instance = this;
		scale = base.transform.localScale;
		text = GetComponent<TextMeshProUGUI>();
	}

	protected override void Update()
	{
		base.Update();
		worldPosition = Vector3.Lerp(worldPosition, newWorldPosition, 50f * Time.deltaTime);
		if ((bool)currentPhysGrabObject)
		{
			newWorldPosition = currentPhysGrabObject.centerPoint + offset;
		}
		if (showTimer > 0f)
		{
			showTimer -= Time.deltaTime;
			curveLerp += 10f * Time.deltaTime;
			curveLerp = Mathf.Clamp01(curveLerp);
			base.transform.localScale = scale * curveIntro.Evaluate(curveLerp);
			return;
		}
		curveLerp -= 10f * Time.deltaTime;
		curveLerp = Mathf.Clamp01(curveLerp);
		base.transform.localScale = scale * curveOutro.Evaluate(curveLerp);
		if (curveLerp <= 0f)
		{
			currentPhysGrabObject = null;
		}
	}

	public void Show(PhysGrabObject _grabObject, int _value, bool _cost, Vector3 _offset)
	{
		if ((bool)currentPhysGrabObject && !(currentPhysGrabObject == _grabObject))
		{
			return;
		}
		value = _value;
		if (_cost)
		{
			text.text = "-$" + SemiFunc.DollarGetString(value) + "K";
			text.fontSize = textSizeCost;
		}
		else
		{
			text.text = "$" + SemiFunc.DollarGetString(value);
			text.fontSize = textSizeValue;
		}
		showTimer = 0.1f;
		if (!currentPhysGrabObject)
		{
			offset = _offset;
			currentPhysGrabObject = _grabObject;
			newWorldPosition = currentPhysGrabObject.centerPoint + offset - Vector3.up * 0.1f;
			worldPosition = newWorldPosition;
			if (_cost)
			{
				text.color = colorCost;
			}
			else
			{
				text.color = colorValue;
			}
		}
	}
}
