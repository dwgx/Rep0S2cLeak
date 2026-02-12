using UnityEngine;

public class ItemShockwave : MonoBehaviour
{
	public MeshRenderer meshRenderer;

	private float startScale = 1f;

	private bool finalScale;

	private Light lightShockwave;

	public ParticleSystem particleSystemWave;

	public ParticleSystem particleSystemSparks;

	public ParticleSystem particleSystemLightning;

	private HurtCollider hurtCollider;

	public Sound soundExplosion;

	public Sound soundExplosionGlobal;

	private void Start()
	{
		startScale = base.transform.localScale.x;
		lightShockwave = GetComponentInChildren<Light>();
		hurtCollider = GetComponentInChildren<HurtCollider>();
		meshRenderer.material.color = Color.white;
		base.transform.localScale = Vector3.zero;
		soundExplosion.Play(base.transform.position);
		soundExplosionGlobal.Play(base.transform.position);
		particleSystemSparks.Play();
		particleSystemLightning.Play();
		GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(20f, 3f, 8f, base.transform.position, 0.1f);
	}

	private void Update()
	{
		base.transform.Rotate(Vector3.up, 100f * Time.deltaTime);
		if (base.transform.localScale.x < startScale)
		{
			base.transform.localScale += Vector3.one * Time.deltaTime * 20f;
			lightShockwave.intensity = Mathf.Lerp(4f, 35f, Mathf.InverseLerp(0f, startScale, base.transform.localScale.x));
			lightShockwave.range = base.transform.localScale.x * 3f;
			return;
		}
		if (!finalScale)
		{
			base.transform.localScale = Vector3.one * startScale;
			hurtCollider.gameObject.SetActive(value: false);
			finalScale = true;
			return;
		}
		float num = Mathf.Lerp(base.transform.localScale.x, startScale * 1.2f, Time.deltaTime * 2f);
		base.transform.localScale = Vector3.one * num;
		float num2 = Mathf.InverseLerp(startScale, startScale * 1.2f, num);
		Color color = meshRenderer.material.color;
		color.a = Mathf.Lerp(1f, 0f, num2);
		meshRenderer.material.color = color;
		lightShockwave.intensity = Mathf.Lerp(35f, 0f, num2);
		if (num2 > 0.998f)
		{
			if ((bool)particleSystemSparks)
			{
				particleSystemSparks.transform.parent = null;
			}
			Object.Destroy(base.gameObject);
		}
	}
}
