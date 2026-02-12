using System.Collections.Generic;
using UnityEngine;

public class EnemyValuable : MonoBehaviour
{
	private PhysGrabObjectImpactDetector impactDetector;

	private float indestructibleTimer = 5f;

	public MeshRenderer outerMeshRenderer;

	private Material outerMaterial;

	private int fresnelPowerIndex;

	private int fresnelColorIndex;

	[Space]
	private float fresnelPowerDefault;

	public float fresnelPowerIndestructible;

	[Space]
	private Color fresnelColorDefault;

	public Color fresnelColorIndestructible;

	[Space]
	public AnimationCurve indestructibleCurve;

	private float indestructibleLerp;

	[Space]
	public Transform innerTransform;

	public Transform speckSmallTransform;

	public Transform speckBigTransform;

	[Space]
	public List<ParticleSystem> particleSystems;

	public GameObject enemyValuableExplosion;

	private bool hasExplosion;

	private void Start()
	{
		impactDetector = GetComponentInChildren<PhysGrabObjectImpactDetector>();
		impactDetector.indestructibleSpawnTimer = 0.1f;
		outerMaterial = outerMeshRenderer.material;
		fresnelPowerIndex = Shader.PropertyToID("_FresnelPower");
		fresnelColorIndex = Shader.PropertyToID("_FresnelColor");
		fresnelPowerDefault = outerMaterial.GetFloat(fresnelPowerIndex);
		outerMaterial.SetFloat(fresnelPowerIndex, fresnelPowerIndestructible);
		fresnelColorDefault = outerMaterial.GetColor(fresnelColorIndex);
		outerMaterial.SetColor(fresnelColorIndex, fresnelColorIndestructible);
		if (SemiFunc.MoonLevel() >= 4)
		{
			hasExplosion = true;
		}
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
		else if (indestructibleLerp < 1f)
		{
			float value = Mathf.Lerp(fresnelPowerIndestructible, fresnelPowerDefault, indestructibleCurve.Evaluate(indestructibleLerp));
			outerMaterial.SetFloat(fresnelPowerIndex, value);
			Color value2 = Color.Lerp(fresnelColorIndestructible, fresnelColorDefault, indestructibleCurve.Evaluate(indestructibleLerp));
			outerMaterial.SetColor(fresnelColorIndex, value2);
			indestructibleLerp += 2f * Time.deltaTime;
		}
		innerTransform.Rotate(base.transform.up * 60f * Time.deltaTime);
		speckSmallTransform.Rotate(base.transform.up * 100f * Time.deltaTime);
		speckBigTransform.Rotate(-base.transform.up * 20f * Time.deltaTime);
	}

	public void Destroy()
	{
		impactDetector.DestroyObject();
	}

	public void DestroyImpulse()
	{
		if (hasExplosion)
		{
			enemyValuableExplosion.transform.parent = null;
			enemyValuableExplosion.SetActive(value: true);
		}
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			particleSystem.gameObject.SetActive(value: true);
			particleSystem.transform.parent = null;
			ParticleSystem.MainModule main = particleSystem.main;
			main.stopAction = ParticleSystemStopAction.Destroy;
		}
	}
}
