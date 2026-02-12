using UnityEngine;

public class ValuableLevitationPotion : MonoBehaviour
{
	public GameObject levitationSphere;

	public void ActivateSphere()
	{
		Object.Instantiate(levitationSphere, base.transform.position, Quaternion.identity);
	}
}
