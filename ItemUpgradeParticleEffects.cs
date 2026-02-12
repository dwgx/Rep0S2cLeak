using UnityEngine;

public class ItemUpgradeParticleEffects : MonoBehaviour
{
	private float destroyTimer;

	private void Update()
	{
		destroyTimer += Time.deltaTime;
		if (destroyTimer > 5f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
