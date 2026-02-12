using UnityEngine;

public class CameraJump : MonoBehaviour
{
	public static CameraJump instance;

	internal bool jumpActive;

	public AnimationCurve jumpCurve;

	public float jumpSpeed = 1f;

	private float jumpLerp;

	public Vector3 jumpPosition;

	public Vector3 jumpRotation;

	[Space]
	private bool landActive;

	public AnimationCurve landCurve;

	public float landSpeed = 1f;

	private float landLerp;

	public Vector3 landPosition;

	public Vector3 landRotation;

	private void Awake()
	{
		instance = this;
	}

	public void Jump()
	{
		GameDirector.instance.CameraImpact.Shake(1f, 0.05f);
		GameDirector.instance.CameraShake.Shake(2f, 0.1f);
		jumpActive = true;
		jumpLerp = 0f;
	}

	public void Land()
	{
		if (!landActive)
		{
			GameDirector.instance.CameraImpact.Shake(1f, 0.05f);
			GameDirector.instance.CameraShake.Shake(2f, 0.1f);
			landActive = true;
			landLerp = 0f;
		}
	}

	private void Update()
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		if (jumpActive)
		{
			if (jumpLerp >= 1f)
			{
				jumpActive = false;
				jumpLerp = 0f;
			}
			else
			{
				zero += Vector3.LerpUnclamped(Vector3.zero, jumpPosition, jumpCurve.Evaluate(jumpLerp));
				zero2 += Vector3.LerpUnclamped(Vector3.zero, jumpRotation, jumpCurve.Evaluate(jumpLerp));
				jumpLerp += jumpSpeed * Time.deltaTime;
			}
		}
		if (landActive)
		{
			if (landLerp >= 1f)
			{
				landActive = false;
				landLerp = 0f;
			}
			else
			{
				zero += Vector3.LerpUnclamped(Vector3.zero, landPosition, landCurve.Evaluate(landLerp));
				zero2 += Vector3.LerpUnclamped(Vector3.zero, landRotation, landCurve.Evaluate(landLerp));
				landLerp += landSpeed * Time.deltaTime;
			}
		}
		zero *= GameplayManager.instance.cameraAnimation;
		zero2 *= GameplayManager.instance.cameraAnimation;
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, zero, 30f * Time.deltaTime);
		Quaternion localRotation = base.transform.localRotation;
		base.transform.localEulerAngles = zero2;
		Quaternion localRotation2 = base.transform.localRotation;
		base.transform.localRotation = localRotation;
		base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, localRotation2, 30f * Time.deltaTime);
	}
}
