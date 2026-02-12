using UnityEngine;

public class RigidbodyFollow : MonoBehaviour
{
	public Transform Target;

	public bool Scale;

	private Rigidbody Rigidbody;

	private void Start()
	{
		Rigidbody = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		Rigidbody.position = Target.position;
		Rigidbody.rotation = Target.rotation;
		if (Scale)
		{
			base.transform.localScale = Target.localScale;
		}
	}
}
