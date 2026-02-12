using UnityEngine;

public class SurplusValuable : MonoBehaviour
{
	private PhysGrabObjectImpactDetector impactDetector;

	private float indestructibleTimer = 3f;

	public float coinMultiplier = 1f;

	public ParticleSystem coinParticles;

	public Sound spawnSound;

	private void Start()
	{
		impactDetector = GetComponentInChildren<PhysGrabObjectImpactDetector>();
		impactDetector.indestructibleSpawnTimer = 0.1f;
		coinParticles.Emit((int)(30f * coinMultiplier));
		spawnSound.Play(base.transform.position);
	}

	private void Update()
	{
		if (indestructibleTimer > 0f)
		{
			indestructibleTimer -= Time.deltaTime;
			if (indestructibleTimer <= 0f)
			{
				impactDetector.destroyDisable = false;
			}
		}
	}

	public void BreakLight()
	{
		coinParticles.Emit((int)(3f * coinMultiplier));
	}

	public void BreakMedium()
	{
		coinParticles.Emit((int)(5f * coinMultiplier));
	}

	public void BreakHeavy()
	{
		coinParticles.Emit((int)(10f * coinMultiplier));
	}

	public void DestroyImpulse()
	{
		coinParticles.Emit((int)(20f * coinMultiplier));
		coinParticles.transform.parent = null;
		ParticleSystem.MainModule main = coinParticles.main;
		main.stopAction = ParticleSystemStopAction.Destroy;
	}
}
