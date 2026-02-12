using UnityEngine;

public class ToolBackAway : MonoBehaviour
{
	public bool Active;

	public Transform ParentTransform;

	private LayerMask Mask;

	private float RaycastTime = 0.1f;

	private float RaycastTimer;

	public float Length = 1f;

	private float LengthHit;

	private float BackAwayAmount;

	public float BackAwayAmountMax;

	private Vector3 StartPosition;

	public float springFreq = 15f;

	public float springDamping = 0.5f;

	private float target;

	private float current;

	private float velocity;

	private SpringUtils.tDampedSpringMotionParams springParams = new SpringUtils.tDampedSpringMotionParams();

	private void Start()
	{
		StartPosition = base.transform.localPosition;
		Mask = SemiFunc.LayerMaskGetVisionObstruct();
	}

	private void FixedUpdate()
	{
		if (!Active)
		{
			return;
		}
		if (RaycastTimer <= 0f)
		{
			RaycastTimer = RaycastTime;
			LengthHit = Length;
			RaycastHit[] array = Physics.RaycastAll(ParentTransform.position, ParentTransform.forward, Length, Mask);
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit = array[i];
				if ((!raycastHit.transform.CompareTag("Player") || !raycastHit.transform.GetComponent<PlayerController>()) && raycastHit.distance < LengthHit)
				{
					LengthHit = raycastHit.distance;
				}
			}
		}
		else
		{
			RaycastTimer -= Time.fixedDeltaTime;
		}
	}

	private void Update()
	{
		if (Active)
		{
			BackAwayAmount = Mathf.Max(0f - BackAwayAmountMax, LengthHit - Length);
		}
		else
		{
			BackAwayAmount = 0f;
		}
		SpringUtils.CalcDampedSpringMotionParams(ref springParams, Time.deltaTime, springFreq, springDamping);
		SpringUtils.UpdateDampedSpringMotion(ref current, ref velocity, BackAwayAmount, in springParams);
		base.transform.localPosition = new Vector3(StartPosition.x, StartPosition.y, StartPosition.z + current);
	}
}
