using UnityEngine;

public class AnimScaleFlicker : MonoBehaviour
{
	public AnimationCurve animCurve;

	public float amountMult = 1f;

	public float speedMult = 1f;

	[Header("Scale X")]
	public float xAmountMin;

	public float xAmountMax;

	public float xSpeedMin;

	public float xSpeedMax;

	private float xInit;

	private float xOld;

	private float xNew;

	private float xSpeed;

	private float xLerp = 1f;

	[Header("Scale Y")]
	public float yAmountMin;

	public float yAmountMax;

	public float ySpeedMin;

	public float ySpeedMax;

	private float yInit;

	private float yOld;

	private float yNew;

	private float ySpeed;

	private float yLerp = 1f;

	[Header("Scale Z")]
	public float zAmountMin;

	public float zAmountMax;

	public float zSpeedMin;

	public float zSpeedMax;

	private float zInit;

	private float zOld;

	private float zNew;

	private float zSpeed;

	private float zLerp = 1f;

	private void Start()
	{
		xInit = base.transform.localScale.x;
		yInit = base.transform.localScale.y;
		zInit = base.transform.localScale.z;
	}

	private void Update()
	{
		float num = Mathf.LerpUnclamped(xOld, xNew, animCurve.Evaluate(xLerp));
		xLerp += xSpeed * speedMult * Time.deltaTime;
		if (xLerp >= 1f)
		{
			xOld = xNew;
			xNew = Random.Range(xAmountMin, xAmountMax);
			xSpeed = Random.Range(xSpeedMin, xSpeedMax);
			xLerp = 0f;
		}
		float num2 = Mathf.LerpUnclamped(yOld, yNew, animCurve.Evaluate(yLerp));
		yLerp += ySpeed * speedMult * Time.deltaTime;
		if (yLerp >= 1f)
		{
			yOld = yNew;
			yNew = Random.Range(yAmountMin, yAmountMax);
			ySpeed = Random.Range(ySpeedMin, ySpeedMax);
			yLerp = 0f;
		}
		float num3 = Mathf.LerpUnclamped(zOld, zNew, animCurve.Evaluate(zLerp));
		zLerp += zSpeed * speedMult * Time.deltaTime;
		if (zLerp >= 1f)
		{
			zOld = zNew;
			zNew = Random.Range(zAmountMin, zAmountMax);
			zSpeed = Random.Range(zSpeedMin, zSpeedMax);
			zLerp = 0f;
		}
		base.transform.localScale = new Vector3(xInit + num * amountMult, yInit + num2 * amountMult, zInit + num3 * amountMult);
	}
}
