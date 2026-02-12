using UnityEngine;

public class PlayerReviveEffects : MonoBehaviour
{
	private bool triggered;

	public PlayerAvatar PlayerAvatar;

	public Transform enableTransform;

	[Space]
	public Light reviveLight;

	private float reviveLightIntensityDefault;

	public ParticleSystem impactParticle;

	public ParticleSystem swirlParticle;

	[Space]
	public Sound reviveSound;

	private void Start()
	{
		reviveLightIntensityDefault = reviveLight.intensity;
	}

	private void Update()
	{
		if (triggered)
		{
			reviveLight.intensity = Mathf.Lerp(reviveLight.intensity, 0f, Time.deltaTime * 1f);
			if (impactParticle.isStopped && swirlParticle.isStopped && reviveLight.intensity < 0.01f)
			{
				triggered = false;
				reviveLight.intensity = reviveLightIntensityDefault;
				enableTransform.gameObject.SetActive(value: false);
			}
		}
	}

	public void Trigger()
	{
		base.transform.position = PlayerAvatar.playerDeathHead.physGrabObject.centerPoint;
		if (SemiFunc.RunIsTutorial())
		{
			base.transform.position = PlayerAvatar.instance.transform.position;
		}
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		triggered = true;
		enableTransform.gameObject.SetActive(value: true);
		reviveSound.Play(base.transform.position);
	}
}
