using UnityEngine;

public class SledgehammerBob : MonoBehaviour
{
	public CameraBob CameraBob;

	[Space]
	public float SpringFreqPosZ = 15f;

	public float SpringDampingPosZ = 0.5f;

	private float TargetPosZ;

	private float CurrentPosZ;

	private float VelocityPosZ;

	private SpringUtils.tDampedSpringMotionParams SpringParamsPosZ = new SpringUtils.tDampedSpringMotionParams();

	[Space]
	public float SpringFreqPosY = 15f;

	public float SpringDampingPosY = 0.5f;

	private float TargetPosY;

	private float CurrentPosY;

	private float VelocityPosY;

	private SpringUtils.tDampedSpringMotionParams SpringParamsPosY = new SpringUtils.tDampedSpringMotionParams();

	private void Start()
	{
		CameraBob = GameDirector.instance.CameraBob;
	}

	private void Update()
	{
		TargetPosY = CameraBob.transform.localRotation.z * -500f;
		SpringUtils.CalcDampedSpringMotionParams(ref SpringParamsPosY, Time.deltaTime, SpringFreqPosY, SpringDampingPosY);
		SpringUtils.UpdateDampedSpringMotion(ref CurrentPosY, ref VelocityPosY, TargetPosY, in SpringParamsPosY);
		TargetPosZ = CameraBob.transform.localPosition.y * -0.5f;
		SpringUtils.CalcDampedSpringMotionParams(ref SpringParamsPosZ, Time.deltaTime, SpringFreqPosZ, SpringDampingPosZ);
		SpringUtils.UpdateDampedSpringMotion(ref CurrentPosZ, ref VelocityPosZ, TargetPosZ, in SpringParamsPosZ);
		base.transform.localRotation = Quaternion.Euler(0f - CurrentPosY, 0f, CurrentPosY);
		base.transform.localPosition = new Vector3(0f, 0f, CameraBob.transform.localPosition.y + CurrentPosZ);
	}
}
