using UnityEngine;

public class DirtFinderBob : MonoBehaviour
{
	public CameraBob CameraBob;

	public PlayerAvatar PlayerAvatar;

	[Space]
	public float PosZMultiplier = 1f;

	public float SpringFreqPosZ = 15f;

	public float SpringDampingPosZ = 0.5f;

	private float TargetPosZ;

	private float CurrentPosZ;

	private float VelocityPosZ;

	private SpringUtils.tDampedSpringMotionParams SpringParamsPosZ = new SpringUtils.tDampedSpringMotionParams();

	[Space]
	public float PosYMultiplier = 1f;

	public float SpringFreqPosY = 15f;

	public float SpringDampingPosY = 0.5f;

	private float TargetPosY;

	private float CurrentPosY;

	private float VelocityPosY;

	private SpringUtils.tDampedSpringMotionParams SpringParamsPosY = new SpringUtils.tDampedSpringMotionParams();

	private void Start()
	{
		CameraBob = CameraBob.Instance;
	}

	private void Update()
	{
		if (!GameManager.Multiplayer() || PlayerAvatar.isLocal)
		{
			TargetPosY = CameraBob.transform.localRotation.y * PosYMultiplier;
			SpringUtils.CalcDampedSpringMotionParams(ref SpringParamsPosY, Time.deltaTime, SpringFreqPosY, SpringDampingPosY);
			SpringUtils.UpdateDampedSpringMotion(ref CurrentPosY, ref VelocityPosY, TargetPosY, in SpringParamsPosY);
			TargetPosZ = CameraBob.transform.localRotation.z * PosZMultiplier;
			SpringUtils.CalcDampedSpringMotionParams(ref SpringParamsPosZ, Time.deltaTime, SpringFreqPosZ, SpringDampingPosZ);
			SpringUtils.UpdateDampedSpringMotion(ref CurrentPosZ, ref VelocityPosZ, TargetPosZ, in SpringParamsPosZ);
			base.transform.localPosition = new Vector3(0f, CurrentPosY * 0.0025f + CameraJump.instance.transform.localPosition.y, 0f);
			base.transform.localRotation = Quaternion.Euler(CameraJump.instance.transform.localRotation.eulerAngles.x * 2f, 0f, 0f);
		}
	}
}
