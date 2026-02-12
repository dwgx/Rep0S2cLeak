using UnityEngine;

public class VaseExplosionTest : MonoBehaviour
{
	public Transform Center;

	private ParticleScriptExplosion particleScriptExplosion;

	private void Start()
	{
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
	}

	public void Explosion()
	{
		particleScriptExplosion.Spawn(Center.position, 1f, 10, 10);
	}
}
