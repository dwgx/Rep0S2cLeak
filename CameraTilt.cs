using UnityEngine;

public class CameraTilt : MonoBehaviour
{
	public static CameraTilt Instance;

	public float tiltZ = 250f;

	public float tiltZMax = 10f;

	[Space]
	public float tiltX = 250f;

	public float tiltXMax = 10f;

	[Space]
	public float strafeAmount = 1f;

	public float CrouchMultiplier = 1f;

	private float Amount = 1f;

	private float AmountCurrent = 1f;

	private float previousX;

	private float previousY;

	private Quaternion targetAngle;

	[HideInInspector]
	public float tiltXresult;

	[HideInInspector]
	public float tiltZresult;

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (SemiFunc.MenuLevel())
		{
			base.transform.localRotation = Quaternion.identity;
			return;
		}
		if (PlayerController.instance.Crouching)
		{
			AmountCurrent = Mathf.Lerp(AmountCurrent, Amount * CrouchMultiplier, Time.deltaTime * 5f);
		}
		else
		{
			AmountCurrent = Mathf.Lerp(AmountCurrent, Amount, Time.deltaTime * 5f);
		}
		float num = SemiFunc.InputMovementX();
		if (GameDirector.instance.DisableInput || (bool)SpectateCamera.instance || PlayerController.instance.InputDisableTimer > 0f)
		{
			num = 0f;
		}
		if (base.transform.rotation.x != previousX && base.transform.rotation.y != previousY)
		{
			if (Mathf.Abs(base.transform.rotation.eulerAngles.y - previousY) < 180f && Mathf.Abs(base.transform.rotation.eulerAngles.x - previousX) < 180f)
			{
				tiltXresult = (previousX - base.transform.rotation.eulerAngles.x) / Time.deltaTime * tiltX;
				tiltXresult = Mathf.Clamp(tiltXresult, 0f - tiltXMax, tiltXMax);
				tiltZresult = (base.transform.rotation.eulerAngles.y - previousY) / Time.deltaTime * tiltZ + num * strafeAmount;
				tiltZresult = Mathf.Clamp(tiltZresult, 0f - tiltZMax, tiltZMax);
				float num2 = 1f;
				if ((bool)SpectateCamera.instance)
				{
					num2 = 0.1f;
				}
				num2 *= GameplayManager.instance.cameraAnimation;
				targetAngle = Quaternion.Euler(tiltXresult * AmountCurrent * num2, 0f, tiltZresult * AmountCurrent * num2);
			}
			previousX = base.transform.rotation.eulerAngles.x;
			previousY = base.transform.rotation.eulerAngles.y;
		}
		float num3 = 3f;
		if (targetAngle == Quaternion.identity)
		{
			num3 = 10f;
		}
		base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, targetAngle, num3 * Time.deltaTime);
	}
}
