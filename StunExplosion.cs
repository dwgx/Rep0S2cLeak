using UnityEngine;

public class StunExplosion : MonoBehaviour
{
	public Light light;

	public AnimationCurve lightCurve;

	public AnimationCurve lightCurvePhotosensitive;

	private float lightEval;

	private float removeTimer;

	private HurtCollider hurtCollider;

	public ItemGrenade itemGrenade;

	public GameObject stunParticles;

	private void Start()
	{
		hurtCollider = GetComponentInChildren<HurtCollider>();
		if (SemiFunc.Photosensitivity())
		{
			stunParticles.SetActive(value: false);
		}
	}

	public void StunExplosionReset()
	{
		removeTimer = 0f;
		lightEval = 0f;
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if ((bool)light)
		{
			if (lightEval < 1f)
			{
				float num = lightCurve.Evaluate(lightEval);
				if (SemiFunc.Photosensitivity())
				{
					num = lightCurvePhotosensitive.Evaluate(lightEval);
				}
				light.intensity = 10f * num;
				lightEval += 0.2f * Time.deltaTime;
			}
			else
			{
				light.intensity = 0f;
			}
		}
		if (removeTimer > 0.5f)
		{
			hurtCollider.gameObject.SetActive(value: false);
		}
		else
		{
			hurtCollider.gameObject.SetActive(value: true);
		}
		removeTimer += Time.deltaTime;
		if (removeTimer >= 20f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
