using System.Collections;
using UnityEngine;

public class DirtFinderMapScanlines : MonoBehaviour
{
	public float Speed;

	public float MaxZ;

	private void OnEnable()
	{
		StartCoroutine(Logic());
	}

	private IEnumerator Logic()
	{
		while (true)
		{
			base.transform.localPosition += new Vector3(0f, 0f, Speed);
			if (base.transform.localPosition.z < MaxZ)
			{
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, 0f);
				base.transform.localPosition += new Vector3(0f, 0f, Speed);
			}
			yield return new WaitForSeconds(0.1f);
		}
	}
}
