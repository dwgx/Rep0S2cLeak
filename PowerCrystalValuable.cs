using UnityEngine;

public class PowerCrystalValuable : MonoBehaviour
{
	private ParticleScriptExplosion particleScriptExplosion;

	public Transform Center;

	public GameObject Crystal;

	public AnimationCurve GlowCurve;

	public PropLight CrystalLight;

	private bool GlowActive;

	private float GlowLerp;

	private float GlowIntensity;

	private void Start()
	{
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
	}

	private void Update()
	{
		if (GlowActive)
		{
			GlowLerp += 8f * Time.deltaTime;
		}
		if (GlowLerp >= 1f)
		{
			GlowLerp = 0f;
			GlowActive = false;
		}
		CrystalLight.lightComponent.intensity = 3f + GlowCurve.Evaluate(GlowLerp) * GlowIntensity;
	}

	public void Explode()
	{
		SemiFunc.LightRemove(CrystalLight);
		particleScriptExplosion.Spawn(Center.position, 1f, 50, 50);
	}

	public void GlowDim()
	{
		GlowIntensity = 3f;
		GlowActive = true;
	}

	public void GlowMed()
	{
		GlowIntensity = 5f;
		GlowActive = true;
	}

	public void GlowStrong()
	{
		GlowIntensity = 10f;
		GlowActive = true;
	}
}
