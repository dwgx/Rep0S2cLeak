using UnityEngine;

public class RemoveSphere : MonoBehaviour
{
	private void Start()
	{
		Object.Destroy(base.gameObject);
	}
}
