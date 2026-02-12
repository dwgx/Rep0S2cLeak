using UnityEngine;

public class DeathPitSaveEffect : MonoBehaviour
{
	private float time = 3f;

	internal float timeCurrent;

	private ParticleSystem[] particles;

	public Transform glowTransform;

	public ParticleSystem electricityParticle;

	private float electricityRateDefault;

	private float electricityRate;

	public Sound loopSound;

	private float loopVolumeDefault;

	internal PhysGrabObject physGrabObject;

	internal float deathPitForceTimer;

	private void Start()
	{
		timeCurrent = time;
		particles = GetComponentsInChildren<ParticleSystem>();
		electricityRateDefault = electricityParticle.emission.rateOverTime.constant;
		electricityRate = electricityRateDefault;
		loopVolumeDefault = loopSound.Volume;
	}

	private void Update()
	{
		if (deathPitForceTimer > 0f)
		{
			deathPitForceTimer -= Time.deltaTime;
		}
		if (timeCurrent > 0f)
		{
			loopSound.PlayLoop(playing: true, 5f, 5f);
			timeCurrent -= Time.deltaTime;
			ParticleSystem.EmissionModule emission = electricityParticle.emission;
			electricityRate -= 25f * Time.deltaTime;
			emission.rateOverTime = new ParticleSystem.MinMaxCurve(electricityRate);
			loopSound.LoopVolume -= 0.2f * Time.deltaTime;
			return;
		}
		loopSound.PlayLoop(playing: false, 5f, 1f);
		ParticleSystem[] array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop(withChildren: true);
		}
		array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].isPlaying)
			{
				return;
			}
		}
		Object.Destroy(base.gameObject);
	}

	private void OnDisable()
	{
		timeCurrent = 0f;
	}

	public void Setup(PhysGrabObject _physGrabObject)
	{
		physGrabObject = _physGrabObject;
		float value = (physGrabObject.boundingBox.x + physGrabObject.boundingBox.y + physGrabObject.boundingBox.z) / 3f;
		value = Mathf.Clamp(value, 0.2f, 1.3f);
		glowTransform.localScale *= value / 1f;
	}

	public void Reset()
	{
		timeCurrent = time;
		electricityRate = electricityRateDefault;
		loopSound.LoopVolume = loopVolumeDefault;
	}
}
