using UnityEngine;

public class EnemyHeadEyeTilt : MonoBehaviour
{
	public Transform Follow;

	private void Update()
	{
		base.transform.localRotation = Follow.localRotation;
	}
}
