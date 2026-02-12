using UnityEngine;

public class CameraCrouchRotation : MonoBehaviour
{
	public CameraCrouchPosition CameraCrouchPosition;

	[Space]
	public float Rotation;

	public float RotationSpeed;

	public AnimationCurve RotationCurveIntro;

	public AnimationCurve RotationCurveOutro;

	[HideInInspector]
	public float RotationLerp;

	private void Start()
	{
		RotationLerp = 1f;
	}

	private void Update()
	{
		RotationLerp += Time.deltaTime * RotationSpeed;
		RotationLerp = Mathf.Clamp01(RotationLerp);
		float num = 0f;
		num = ((!CameraCrouchPosition.Active) ? (RotationCurveOutro.Evaluate(RotationLerp) * Rotation) : (RotationCurveIntro.Evaluate(RotationLerp) * Rotation));
		num *= GameplayManager.instance.cameraAnimation;
		base.transform.localRotation = Quaternion.Euler(num, 0f, 0f);
	}
}
