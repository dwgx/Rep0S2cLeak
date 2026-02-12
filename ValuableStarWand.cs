using UnityEngine;

public class ValuableStarWand : MonoBehaviour
{
	public ParticleSystem particle;

	private PropLight propLight;

	private float initialLightIntensity = 1f;

	private float targetLightIntensity;

	private Rigidbody rb;

	private PhysGrabObject grabObject;

	public Sound sparkles;

	private bool soundPlayed;

	private void Awake()
	{
		propLight = GetComponentInChildren<PropLight>();
		rb = GetComponent<Rigidbody>();
		grabObject = GetComponent<PhysGrabObject>();
	}

	private void Update()
	{
		if (rb.velocity.magnitude > 0.45f)
		{
			if (grabObject.impactDetector.inCart && grabObject.playerGrabbing.Count <= 0)
			{
				return;
			}
			if (particle.isStopped)
			{
				particle.Play(withChildren: true);
				if (!soundPlayed)
				{
					sparkles.Play(base.transform.position);
					soundPlayed = true;
				}
			}
			targetLightIntensity = initialLightIntensity;
		}
		else
		{
			if (particle.isPlaying)
			{
				particle.Stop(withChildren: true);
				soundPlayed = false;
			}
			targetLightIntensity = 0f;
		}
		if (propLight.lightComponent.intensity != targetLightIntensity)
		{
			propLight.lightComponent.intensity = Mathf.Lerp(propLight.lightComponent.intensity, targetLightIntensity, Time.deltaTime * 5f);
		}
	}
}
