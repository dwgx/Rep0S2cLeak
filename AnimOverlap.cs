using UnityEngine;

public class AnimOverlap : MonoBehaviour
{
	public Transform targetFollow;

	private float previousX;

	private float previousY;

	private Quaternion targetAngle;

	[Header("Rotation X")]
	public float springFreqRotX = 15f;

	public float springDampingRotX = 0.5f;

	private float targetRotX;

	private float currentRotX;

	private float velocityRotX;

	private SpringUtils.tDampedSpringMotionParams springParamsRotX = new SpringUtils.tDampedSpringMotionParams();

	[Header("Rotation Y")]
	public float springFreqRotY = 15f;

	public float springDampingRotY = 0.5f;

	private float targetRotY;

	private float currentRotY;

	private float velocityRotY;

	private SpringUtils.tDampedSpringMotionParams springParamsRotY = new SpringUtils.tDampedSpringMotionParams();

	[Header("Rotation Z")]
	public float springFreqRotZ = 15f;

	public float springDampingRotZ = 0.5f;

	private float targetRotZ;

	private float currentRotZ;

	private float velocityRotZ;

	private SpringUtils.tDampedSpringMotionParams springParamsRotZ = new SpringUtils.tDampedSpringMotionParams();

	private float velocity;

	private void Update()
	{
	}
}
