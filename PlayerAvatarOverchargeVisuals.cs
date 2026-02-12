using UnityEngine;

public class PlayerAvatarOverchargeVisuals : MonoBehaviour
{
	private Light overchargeLight;

	private ParticleSystem overchargeParticles;

	private PhysGrabber physGrabber;

	private Transform physGrabBeamOrigin;

	private PlayerAvatar playerAvatar;

	public Sound soundOverchargeLoop;

	public AnimationCurve overchargeIntensityCurve;

	private void Start()
	{
		physGrabber = GetComponent<PhysGrabber>();
		overchargeLight = GetComponentInChildren<Light>();
		overchargeParticles = GetComponentInChildren<ParticleSystem>();
		PhysGrabBeam componentInParent = GetComponentInParent<PhysGrabBeam>();
		physGrabber = componentInParent.playerAvatar.GetComponent<PhysGrabber>();
		if (!physGrabber.isLocal)
		{
			physGrabBeamOrigin = componentInParent.PhysGrabPointOriginClient;
		}
		else
		{
			physGrabBeamOrigin = componentInParent.PhysGrabPointOrigin;
		}
		playerAvatar = componentInParent.playerAvatar;
		base.transform.parent = playerAvatar.transform.parent;
	}

	private void Update()
	{
		base.transform.position = physGrabBeamOrigin.position;
		if (physGrabber.isLocal)
		{
			base.transform.position += playerAvatar.localCamera.transform.forward * 0.5f;
		}
		if (playerAvatar.isTumbling)
		{
			base.transform.position = playerAvatar.playerAvatarVisuals.transform.position;
		}
		float num = (float)(int)physGrabber.physGrabBeamOverCharge / 2f;
		num /= 100f;
		if (playerAvatar.isDisabled)
		{
			num = 0f;
		}
		bool playing = num > 0f;
		float num2 = overchargeIntensityCurve.Evaluate(num);
		soundOverchargeLoop.LoopVolumeCurrent = 0.5f * num2;
		soundOverchargeLoop.PlayLoop(playing, 0.5f, 0.5f, 1f + 2f * num2);
		if (num > 0f)
		{
			if (!overchargeLight.enabled)
			{
				overchargeParticles.Play();
				overchargeLight.enabled = true;
			}
			float num3 = num * Mathf.Sin(Time.time * (10f + 20f * num2));
			overchargeLight.intensity = 8f * num2 + num3;
			overchargeParticles.emissionRate = num2 * 50f;
			overchargeParticles.transform.localScale = new Vector3(1f, 1f, 1f) * (0.1f + 0.8f * num2);
		}
		else if (overchargeLight.enabled)
		{
			overchargeParticles.Stop();
			overchargeLight.enabled = false;
		}
	}
}
