using UnityEngine;

public class PlayerAvatarEyelids : MonoBehaviour
{
	public enum Eye
	{
		Left,
		Right
	}

	public Eye eye;

	public Transform eyelidUpperScale;

	public Transform eyelidUpperRotation;

	public Transform eyelidUpper;

	public Transform eyelidLowerScale;

	public Transform eyelidLowerRotation;

	public Transform eyelidLower;

	internal float eyelidUpperClosedPercentage;

	internal float eyelidLowerClosedPercentage;

	private SpringFloat springUpper = new SpringFloat();

	private SpringFloat springUpperScale = new SpringFloat();

	private SpringFloat springUpperRotation = new SpringFloat();

	private SpringFloat springLower = new SpringFloat();

	private SpringFloat springLowerScale = new SpringFloat();

	private SpringFloat springLowerRotation = new SpringFloat();

	private PlayerAvatarVisuals playerVisuals;

	private Transform parentTransform;

	private void Start()
	{
		springUpper.damping = 0.01f;
		springUpper.speed = 40f;
		playerVisuals = GetComponentInParent<PlayerAvatarVisuals>();
		parentTransform = playerVisuals.transform;
		springLower.damping = 0.01f;
		springLower.speed = 40f;
	}

	private void Update()
	{
		Quaternion localRotation = base.transform.localRotation;
		base.transform.localRotation = Quaternion.LookRotation(base.transform.forward, parentTransform.up);
		base.transform.localRotation = Quaternion.Euler(localRotation.x, localRotation.y, base.transform.localRotation.eulerAngles.z);
	}
}
