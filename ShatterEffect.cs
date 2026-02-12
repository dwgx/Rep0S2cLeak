using UnityEngine;

public class ShatterEffect : MonoBehaviour
{
	public ParticleSystem partSystem;

	public ParticleSystem particleSystemSmoke;

	[Range(0f, 100f)]
	public float particleAmountMultiplier = 50f;

	public Gradient particleColors;

	public Transform ParticleEmissionBox;

	[Space]
	public Sound ShatterSound;

	private ParticleSystem.MainModule mainModule;

	private ParticleSystem.EmissionModule emissionModule;

	private ParticleSystem.ShapeModule shapeModule;

	private ParticleSystem.ShapeModule shapeModuleSmoke;

	private ParticleSystem.MainModule mainModuleSmoke;

	private void Start()
	{
		mainModule = partSystem.main;
		emissionModule = partSystem.emission;
		shapeModule = partSystem.shape;
		mainModuleSmoke = particleSystemSmoke.main;
		shapeModuleSmoke = particleSystemSmoke.shape;
		SetupParticleSystem();
	}

	private void SetupParticleSystem()
	{
		emissionModule.rateOverTimeMultiplier = partSystem.emission.rateOverTimeMultiplier * (particleAmountMultiplier / 100f);
		shapeModule.scale = ParticleEmissionBox.localScale;
		shapeModuleSmoke.scale = ParticleEmissionBox.localScale;
	}

	public void SpawnParticles(Vector3 direction)
	{
		partSystem.transform.rotation = Quaternion.LookRotation(direction);
		particleSystemSmoke.transform.rotation = Quaternion.LookRotation(direction);
		partSystem.transform.position = ParticleEmissionBox.position;
		shapeModule.scale = ParticleEmissionBox.localScale;
		mainModule.startColor = particleColors;
		partSystem.Play();
		partSystem.transform.SetParent(null);
		particleSystemSmoke.Play();
		particleSystemSmoke.transform.SetParent(null);
		ShatterSound.Play(base.transform.position);
		Object.Destroy(base.gameObject);
	}
}
