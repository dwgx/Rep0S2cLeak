using UnityEngine;

public class ItemMineExplosive : MonoBehaviour
{
	private ParticleScriptExplosion particleScriptExplosion;

	private void Start()
	{
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
	}

	public void OnTriggered()
	{
		particleScriptExplosion.Spawn(base.transform.position, 1.2f, 75, 200, 4f);
	}
}
