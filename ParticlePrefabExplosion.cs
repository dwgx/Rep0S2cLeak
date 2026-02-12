using UnityEngine;

public class ParticlePrefabExplosion : MonoBehaviour
{
	public ParticleSystem particleFire;

	public ParticleSystem particleSmoke;

	public Light light;

	public AnimationCurve lightIntensityCurve;

	private float lightIntensityCurveProgress;

	public Gradient lightColorOverTime;

	internal float explosionSize = 1f;

	internal float particleSizeMultiplier = 1f;

	internal int explosionDamage;

	internal int explosionDamageEnemy;

	private float smokeNullCheckTimer;

	[HideInInspector]
	public float forceMultiplier = 1f;

	public bool onlyParticleEffect;

	internal bool SkipHurtColliderSetup;

	public HurtCollider HurtCollider;

	private bool HurtColliderActive = true;

	private bool HurtColliderFirstSetup = true;

	private bool HurtColliderSecondSetup = true;

	private float HurtColliderTimer;

	private void Start()
	{
		float num = explosionSize * particleSizeMultiplier;
		bool flag = false;
		if (num <= 0.25f)
		{
			flag = true;
		}
		ParticleSystem.MainModule main = particleFire.main;
		float startSpeedMultiplier = main.startSpeedMultiplier;
		float startLifetimeMultiplier = main.startLifetimeMultiplier;
		main.startSpeedMultiplier = startSpeedMultiplier * num;
		main.startLifetimeMultiplier = startLifetimeMultiplier * num;
		main.startLifetimeMultiplier = Mathf.Max(main.startLifetimeMultiplier, 0.2f);
		main.startSizeMultiplier = Mathf.Max(main.startSizeMultiplier, 0.1f);
		if (flag)
		{
			main.startSpeedMultiplier *= 0.5f;
			main.startLifetimeMultiplier *= 0.5f;
			main.startSizeMultiplier *= 0.8f;
		}
		particleFire.Play();
		ParticleSystem.MainModule main2 = particleSmoke.main;
		startSpeedMultiplier = main2.startSpeedMultiplier;
		startLifetimeMultiplier = main2.startLifetimeMultiplier;
		main2.startSpeedMultiplier = startSpeedMultiplier * num;
		main2.startLifetimeMultiplier = startLifetimeMultiplier * num;
		main2.startLifetimeMultiplier = Mathf.Max(main2.startLifetimeMultiplier * 1.2f, 2f);
		main2.startSizeMultiplier = Mathf.Max(main.startSizeMultiplier * 1.2f, 0.1f);
		particleSmoke.Play();
		light.enabled = true;
	}

	private void Update()
	{
		if (!onlyParticleEffect && HurtColliderActive)
		{
			HurtColliderTimer += Time.deltaTime;
			if (HurtColliderFirstSetup)
			{
				if (!SkipHurtColliderSetup)
				{
					HurtCollider.playerDamage = explosionDamage;
					HurtCollider.enemyDamage = explosionDamageEnemy;
					HurtCollider.physHitForce = (float)explosionDamage * 0.5f;
					if (explosionDamage >= 50)
					{
						HurtCollider.physImpact = HurtCollider.BreakImpact.Heavy;
						HurtCollider.physHingeDestroy = true;
					}
					else if (explosionDamage >= 15)
					{
						HurtCollider.physImpact = HurtCollider.BreakImpact.Medium;
					}
					else
					{
						HurtCollider.physImpact = HurtCollider.BreakImpact.Light;
					}
				}
				HurtCollider.gameObject.SetActive(value: true);
				HurtCollider.transform.localScale = new Vector3(explosionSize, explosionSize, explosionSize);
				HurtColliderFirstSetup = false;
			}
			if (HurtColliderSecondSetup && HurtColliderTimer > 0.2f)
			{
				HurtCollider.playerDamage = 0;
				HurtCollider.playerHitForce *= 0.25f;
				HurtCollider.physHitForce *= 0.25f;
				if (HurtCollider.physImpact > HurtCollider.BreakImpact.None)
				{
					HurtCollider.physImpact--;
				}
				HurtCollider.transform.localScale = new Vector3(explosionSize * 2f, explosionSize * 2f, explosionSize * 2f);
				HurtColliderSecondSetup = false;
			}
			if (HurtColliderTimer > 0.5f)
			{
				HurtCollider.gameObject.SetActive(value: false);
				HurtColliderActive = false;
			}
		}
		smokeNullCheckTimer += Time.deltaTime;
		if (smokeNullCheckTimer > 1f)
		{
			if (!particleSmoke)
			{
				Object.Destroy(base.gameObject);
			}
			smokeNullCheckTimer = 0f;
		}
		if (light.enabled)
		{
			float num = explosionSize;
			num = Mathf.Max(num, 0.8f);
			lightIntensityCurveProgress += 0.5f * Time.deltaTime;
			light.intensity = 10f * num * lightIntensityCurve.Evaluate(lightIntensityCurveProgress);
			light.range = 10f * num * lightIntensityCurve.Evaluate(lightIntensityCurveProgress);
			light.color = lightColorOverTime.Evaluate(lightIntensityCurveProgress);
			if (lightIntensityCurveProgress > lightIntensityCurve.keys[lightIntensityCurve.length - 1].time)
			{
				light.enabled = false;
				lightIntensityCurveProgress = 0f;
			}
		}
	}
}
