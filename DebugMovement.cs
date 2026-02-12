using UnityEngine;

public class DebugMovement : MonoBehaviour
{
	public float speed = 1f;

	public float leftRight = 1f;

	public float upDown = 1f;

	private Vector3 startPos;

	private void Start()
	{
		startPos = base.transform.position;
	}

	private void Update()
	{
		base.transform.position = startPos + new Vector3(Mathf.Sin(Time.time * speed), Mathf.Cos(Time.time * 0.5f * speed), Mathf.Cos(Time.time * 0.25f * speed));
	}
}
