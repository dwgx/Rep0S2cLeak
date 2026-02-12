using System.Collections;
using Photon.Pun;
using UnityEngine;

public class ItemGrenadeHuman : MonoBehaviour
{
	private ParticleScriptExplosion particleScriptExplosion;

	private ItemToggle itemToggle;

	private ItemGrenade itemGrenade;

	private PhotonView photonView;

	private PhysGrabObject physGrabObject;

	private Rigidbody rb;

	public Sound soundExplosion;

	public Sound soundExplosionGlobal;

	private void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		itemToggle = GetComponent<ItemToggle>();
		itemGrenade = GetComponent<ItemGrenade>();
		photonView = GetComponent<PhotonView>();
		physGrabObject = GetComponent<PhysGrabObject>();
		rb = GetComponent<Rigidbody>();
	}

	public void Spawn()
	{
		StartCoroutine(LateSpawn());
		itemGrenade.isSpawnedGrenade = true;
	}

	private IEnumerator LateSpawn()
	{
		while (!physGrabObject.spawned || rb.isKinematic)
		{
			yield return null;
		}
		itemToggle.ToggleItem(toggle: true);
		itemGrenade.tickTime = Random.Range(1.5f, 3f);
		Vector3 vector = Quaternion.Euler(Random.Range(-45, 45), Random.Range(-180, 180), 0f) * Vector3.forward;
		rb.AddForce(vector * Random.Range(5, 10), ForceMode.Impulse);
		rb.AddTorque(Random.insideUnitSphere * Random.Range(5f, 10f), ForceMode.Impulse);
		itemGrenade.isSpawnedGrenade = true;
	}

	public void Explosion()
	{
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		particleScriptExplosion.Spawn(base.transform.position, 0.8f, 50, 100, 2f, onlyParticleEffect: false, disableSound: true);
		soundExplosion.Play(base.transform.position);
		soundExplosionGlobal.Play(base.transform.position);
	}
}
