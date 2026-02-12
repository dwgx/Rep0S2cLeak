using System.Collections;
using UnityEngine;

public class ItemGrenadeExplosive : MonoBehaviour
{
	private ParticleScriptExplosion particleScriptExplosion;

	private void Start()
	{
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		if (SemiFunc.RunIsShop() && SemiFunc.IsMasterClientOrSingleplayer())
		{
			ItemToggle component = GetComponent<ItemToggle>();
			if (ShopManager.instance.isThief)
			{
				StartCoroutine(ThiefLaunch());
				component.ToggleItem(toggle: true);
				GetComponent<ItemGrenade>().isSpawnedGrenade = true;
			}
		}
	}

	private IEnumerator ThiefLaunch()
	{
		yield return new WaitForSeconds(0.2f);
		Rigidbody component = GetComponent<Rigidbody>();
		Vector3 forward = ShopManager.instance.extractionPoint.forward;
		forward += Vector3.up * Random.Range(0.1f, 0.5f);
		forward += ShopManager.instance.extractionPoint.right * Random.Range(-0.5f, 0.5f);
		component.AddForce(forward * Random.Range(3, 7), ForceMode.Impulse);
	}

	public void Explosion()
	{
		particleScriptExplosion.Spawn(base.transform.position, 1.2f, 75, 160, 4f);
	}
}
