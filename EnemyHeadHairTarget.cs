using UnityEngine;

public class EnemyHeadHairTarget : MonoBehaviour
{
	public Transform Parent;

	private void Start()
	{
		base.transform.parent = Parent;
	}
}
