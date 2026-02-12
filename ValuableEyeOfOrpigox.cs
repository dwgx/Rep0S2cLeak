using UnityEngine;

public class ValuableEyeOfOrpigox : MonoBehaviour
{
	public Transform eye;

	public Sound slimyEyeLoop;

	[Header("Rotation Threshold Settings")]
	public float rotationThreshold = 0.1f;

	public float maxRotationForVolume = 10f;

	private PhysGrabObject physGrabObject;

	private float pitchMultiplier;

	private Quaternion previousEyeRotation;

	private bool eyeLoopPlaying;

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		previousEyeRotation = eye.rotation;
	}

	private void Update()
	{
		slimyEyeLoop.PlayLoop(eyeLoopPlaying, 10f, 1f, pitchMultiplier);
		if (physGrabObject.playerGrabbing.Count <= 0)
		{
			eyeLoopPlaying = false;
			return;
		}
		PlayerAvatar component = physGrabObject.playerGrabbing[0].GetComponent<PlayerAvatar>();
		Vector3 vector = ((!SemiFunc.IsMultiplayer()) ? component.localCamera.transform.position : component.playerAvatarVisuals.headLookAtTransform.position);
		Quaternion quaternion = Quaternion.LookRotation((vector - eye.position).normalized);
		float magnitude = physGrabObject.rbAngularVelocity.magnitude;
		float num = Mathf.Lerp(10f, 30f, Mathf.Clamp01(magnitude / 5f));
		eye.rotation = Quaternion.Slerp(eye.rotation, quaternion, Time.deltaTime * num);
		float num2 = Quaternion.Angle(previousEyeRotation, eye.rotation);
		previousEyeRotation = eye.rotation;
		if (num2 > rotationThreshold)
		{
			eyeLoopPlaying = true;
		}
		else
		{
			eyeLoopPlaying = false;
		}
		pitchMultiplier = Mathf.Clamp(num2, 0.75f, 1.5f);
	}
}
