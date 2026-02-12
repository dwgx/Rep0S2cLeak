using UnityEngine;

public class UraniumScript : MonoBehaviour
{
	public enum UraniumIntensity
	{
		Big,
		Small,
		None
	}

	public bool instant;

	[Space]
	public Sound uraniumSoundLoop;

	public Sound uraniumCloudSound;

	public GameObject geigerLoopObject;

	private PhysGrabObject physGrabObject;

	private ParticleSystem uraniumCloudParticles;

	public HurtCollider hurtCollider;

	public UraniumIntensity uraniumIntensityOption;

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		uraniumCloudParticles = GetComponentInChildren<ParticleSystem>();
	}

	private void Update()
	{
		if (instant)
		{
			Cloud();
			Object.Destroy(base.gameObject);
			return;
		}
		uraniumSoundLoop.PlayLoop(physGrabObject.grabbed, 5f, 5f);
		if (physGrabObject.grabbedLocal && uraniumIntensityOption == UraniumIntensity.Big)
		{
			PostProcessing.Instance.VignetteOverride(new Color(0f, 0.6f, 0f), 1f, 0.5f, 0.1f, 2f, 0.1f, base.gameObject);
		}
		else if (physGrabObject.grabbedLocal && uraniumIntensityOption == UraniumIntensity.Small)
		{
			PostProcessing.Instance.VignetteOverride(new Color(0f, 0.6f, 0f), 0.5f, 0.5f, 0.33f, 2f, 0.1f, base.gameObject);
		}
	}

	public void Cloud()
	{
		GameObject obj = Object.Instantiate(geigerLoopObject, base.transform.position, Quaternion.identity);
		if (instant)
		{
			uraniumCloudSound.Play(base.transform.position);
		}
		else
		{
			uraniumCloudSound.Play(physGrabObject.centerPoint);
		}
		uraniumCloudParticles.transform.parent = null;
		hurtCollider.transform.parent = null;
		uraniumCloudParticles.Play();
		Object.Destroy(uraniumCloudParticles.gameObject, 10f);
		hurtCollider.gameObject.SetActive(value: true);
		Object.Destroy(obj, 7f);
	}
}
