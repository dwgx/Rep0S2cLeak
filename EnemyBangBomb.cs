using UnityEngine;

public class EnemyBangBomb : MonoBehaviour
{
	public SpringQuaternion spring;

	public Transform source;

	public Transform target;

	private void Update()
	{
		source.rotation = SemiFunc.SpringQuaternionGet(spring, target.rotation);
	}
}
