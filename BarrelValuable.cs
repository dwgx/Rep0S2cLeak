using UnityEngine;

public class BarrelValuable : Trap
{
	private ParticleScriptExplosion particleScriptExplosion;

	private int HitCount;

	private int MaxHitCount = 3;

	public Transform Center;

	protected override void Start()
	{
		base.Start();
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
	}

	public void Explode()
	{
		particleScriptExplosion.Spawn(Center.position, 1f, 50, 100);
	}

	public void PotentialExplode()
	{
		if (isLocal)
		{
			if (HitCount >= MaxHitCount - 1)
			{
				Explode();
			}
			else
			{
				HitCount++;
			}
		}
	}
}
