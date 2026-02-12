using UnityEngine;

public class ItemGrenadeShockwave : MonoBehaviour
{
	public GameObject shockwavePrefab;

	public void Explosion()
	{
		Object.Instantiate(shockwavePrefab, base.transform.position, Quaternion.identity);
	}
}
