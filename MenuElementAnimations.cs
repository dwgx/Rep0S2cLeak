using UnityEngine;

public class MenuElementAnimations : MonoBehaviour
{
	private SpringFloat springFloatScale;

	private SpringFloat springFloatPosX;

	private SpringFloat springFloatPosY;

	private SpringFloat springFloatRotation;

	private RectTransform rectTransform;

	private Vector2 initialPosition;

	private float initialScale;

	private float initialRotation;

	public bool forceMiddlePivot = true;

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		if (forceMiddlePivot)
		{
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
		}
		initialPosition = rectTransform.anchoredPosition;
		initialScale = rectTransform.localScale.x;
		initialRotation = rectTransform.localEulerAngles.z;
		springFloatPosX = new SpringFloat();
		springFloatPosY = new SpringFloat();
		springFloatScale = new SpringFloat();
		springFloatRotation = new SpringFloat();
		springFloatPosX.lastPosition = initialPosition.x;
		springFloatPosY.lastPosition = initialPosition.y;
		springFloatScale.lastPosition = initialScale;
		springFloatRotation.lastPosition = initialRotation;
	}

	private void Update()
	{
		float x = SemiFunc.SpringFloatGet(springFloatPosX, initialPosition.x);
		float y = SemiFunc.SpringFloatGet(springFloatPosY, initialPosition.y);
		float num = SemiFunc.SpringFloatGet(springFloatScale, initialScale);
		float z = SemiFunc.SpringFloatGet(springFloatRotation, initialRotation);
		rectTransform.anchoredPosition = new Vector2(x, y);
		rectTransform.localScale = new Vector3(num, num, 1f);
		rectTransform.localEulerAngles = new Vector3(0f, 0f, z);
	}

	public void UIAniNewInitialPosition(Vector2 newPos)
	{
		initialPosition = newPos;
	}

	public void UIAniNudgeX(float nudgeForce = 10f, float dampen = 0.2f, float springStrengthMultiplier = 1f)
	{
		springFloatPosX.damping = dampen;
		springFloatPosX.springVelocity = nudgeForce * 100f;
		springFloatPosX.speed = nudgeForce * 5f * springStrengthMultiplier;
	}

	public void UIAniNudgeY(float nudgeForce = 10f, float dampen = 0.2f, float springStrengthMultiplier = 1f)
	{
		springFloatPosY.damping = dampen;
		springFloatPosY.springVelocity = nudgeForce * 100f;
		springFloatPosY.speed = nudgeForce * 5f * springStrengthMultiplier;
	}

	public void UIAniScale(float scaleForce = 2f, float dampen = 0.2f, float springStrengthMultiplier = 1f)
	{
		springFloatScale.damping = dampen;
		springFloatScale.springVelocity = scaleForce * 1f;
		springFloatScale.speed = scaleForce * 15f * springStrengthMultiplier;
	}

	public void UIAniRotate(float rotateForce = 2f, float dampen = 0.2f, float springStrengthMultiplier = 1f)
	{
		springFloatRotation.damping = dampen;
		springFloatRotation.springVelocity = rotateForce * 100f;
		springFloatRotation.speed = rotateForce * 15f * springStrengthMultiplier;
	}
}
