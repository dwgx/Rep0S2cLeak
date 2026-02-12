using UnityEngine;

public class EnemyHeadGrabberLocalCamera : MonoBehaviour
{
	private float activeTimer;

	public SpringQuaternion spring;

	public Transform springTransform;

	[Space]
	public Transform scaleTransform;

	public AnimationCurve scaleCurve;

	private float scaleLerp;

	private void Awake()
	{
		Active();
		scaleTransform.localScale = Vector3.one;
	}

	private void Update()
	{
		if (scaleLerp < 1f)
		{
			scaleLerp += Time.deltaTime * 1f;
			scaleTransform.localScale = Vector3.one * scaleCurve.Evaluate(scaleLerp);
		}
		springTransform.rotation = SemiFunc.SpringQuaternionGet(spring, base.transform.rotation);
		Vector3 localEulerAngles = springTransform.localEulerAngles;
		if (springTransform.localEulerAngles.x > 180f)
		{
			localEulerAngles.x -= 360f;
		}
		localEulerAngles.x = Mathf.Clamp(localEulerAngles.x, -360f, 15f);
		if (springTransform.localEulerAngles.z > 180f)
		{
			localEulerAngles.z -= 360f;
		}
		localEulerAngles.z = Mathf.Clamp(localEulerAngles.z, -10f, 10f);
		Quaternion localRotation = springTransform.localRotation;
		springTransform.localEulerAngles = localEulerAngles;
		Quaternion localRotation2 = springTransform.localRotation;
		springTransform.localRotation = Quaternion.Slerp(localRotation, localRotation2, 200f * Time.deltaTime);
		activeTimer -= Time.deltaTime;
		if (activeTimer <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Active()
	{
		activeTimer = 0.25f;
	}

	public void ActiveReset()
	{
		activeTimer = 0f;
	}
}
