using UnityEngine;
using UnityEngine.Audio;

public class MixerEffects : MonoBehaviour
{
	public AudioMixer mixer;

	public CameraTilt camTilt;

	[Space]
	public float DistortionTiltMultiplier = 0.001f;

	public float DistortionTiltMax = 0.2f;

	public float DistortionTiltSpeed = 1f;

	private float DistortionTilt;

	private float DistortionDefault;

	[Space]
	public float LowpassTiltMultiplier = 10f;

	public float LowpassTiltMax = 1000f;

	public float LowpassTiltSpeed = 1f;

	private float LowpassTilt;

	private float LowpassDefault;

	[Space]
	public float PitchTiltMultiplier = 0.001f;

	public float PitchTiltMax = 0.1f;

	private void Start()
	{
	}

	private void Update()
	{
		mixer.SetFloat("Pitch", 1f - Mathf.Clamp(Mathf.Abs(camTilt.tiltZresult * PitchTiltMultiplier), 0f, PitchTiltMax));
	}
}
