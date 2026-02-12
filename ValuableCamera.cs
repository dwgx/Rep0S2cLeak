using System.Collections.Generic;
using UnityEngine;

public class ValuableCamera : MonoBehaviour
{
	public Sound soundFlash;

	public List<MeshRenderer> meshRenderers;

	private float emmissionTimer;

	private Transform stunExplosion;

	private void Start()
	{
		stunExplosion = GetComponentInChildren<StunExplosion>().transform;
		stunExplosion.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (!(emmissionTimer > 0f))
		{
			return;
		}
		emmissionTimer -= Time.deltaTime;
		if (!(emmissionTimer <= 0f))
		{
			return;
		}
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			meshRenderer.material.DisableKeyword("_EMISSION");
		}
	}

	public void Explosion()
	{
		soundFlash.Play(base.transform.position);
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			meshRenderer.material.EnableKeyword("_EMISSION");
			emmissionTimer = 0.2f;
		}
		GameObject obj = Object.Instantiate(stunExplosion.gameObject, base.transform.position, base.transform.rotation);
		obj.transform.parent = null;
		obj.SetActive(value: true);
	}
}
