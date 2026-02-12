using UnityEngine;

public class ItemGrenadeStun : MonoBehaviour
{
	public Sound soundExplosion;

	public Sound soundTinnitus;

	private Transform stunExplosion;

	private ItemGrenade itemGrenade;

	private void Start()
	{
		stunExplosion = GetComponentInChildren<StunExplosion>().transform;
		stunExplosion.gameObject.SetActive(value: false);
		itemGrenade = GetComponent<ItemGrenade>();
	}

	public void Explosion()
	{
		soundExplosion.Play(base.transform.position);
		soundTinnitus.Play(base.transform.position);
		GameObject obj = Object.Instantiate(stunExplosion.gameObject, base.transform.position, base.transform.rotation);
		obj.transform.parent = null;
		obj.SetActive(value: true);
		obj.GetComponent<StunExplosion>().itemGrenade = itemGrenade;
	}
}
