using System.Collections;
using UnityEngine;

public class DirtFinderMapEnemy : MonoBehaviour
{
	public Transform Parent;

	public IEnumerator Logic()
	{
		while (Parent != null && Parent.gameObject.activeSelf)
		{
			if (Map.Instance.Active)
			{
				Map.Instance.EnemyPositionSet(base.transform, Parent.transform);
			}
			yield return new WaitForSeconds(0.1f);
		}
		Object.Destroy(base.gameObject);
	}
}
