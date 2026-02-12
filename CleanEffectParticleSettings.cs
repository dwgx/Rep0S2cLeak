using UnityEngine;

public class CleanEffectParticleSettings : MonoBehaviour
{
	public ParticleSystem gleamParticles;

	public GameObject cleanEffectParticleRadius;

	public GameObject cleanEffectParticleSize;

	public GameObject cleanEffectParticleAmount;

	private void Start()
	{
		UpdateParticleProperties();
	}

	private void UpdateParticleProperties()
	{
		float num = cleanEffectParticleSize.transform.localScale.x * 10f / 4f;
		float x = cleanEffectParticleRadius.transform.localScale.x;
		float num2 = cleanEffectParticleAmount.transform.localScale.x * 100f / 4f;
		ParticleSystem.MainModule main = gleamParticles.main;
		float startSizeMultiplier = main.startSizeMultiplier;
		main.startSizeMultiplier = startSizeMultiplier * num;
		ParticleSystem.ShapeModule shape = gleamParticles.shape;
		shape.radius = x / 2f;
		ParticleSystem.EmissionModule emission = gleamParticles.emission;
		float rateOverTimeMultiplier = emission.rateOverTimeMultiplier;
		emission.rateOverTimeMultiplier = rateOverTimeMultiplier * num2;
		Object.Destroy(cleanEffectParticleSize);
		Object.Destroy(cleanEffectParticleRadius);
		Object.Destroy(cleanEffectParticleAmount);
	}
}
