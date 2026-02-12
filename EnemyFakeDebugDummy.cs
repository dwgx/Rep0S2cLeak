using UnityEngine;

public class EnemyFakeDebugDummy : MonoBehaviour
{
	private Vector3 startPosition;

	private float timePassed;

	public float rotationSpeed = 50f;

	public float figure8Speed = 1f;

	public float figure8Width = 3f;

	public float figure8Height = 2f;

	private void Start()
	{
		startPosition = base.transform.position;
	}

	private void Update()
	{
		timePassed += Time.deltaTime * figure8Speed;
		float x = Mathf.Sin(timePassed) * figure8Width;
		float y = Mathf.Sin(timePassed * 2f) * figure8Height;
		base.transform.position = startPosition + new Vector3(x, y, 0f);
		base.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
		base.transform.Rotate(Vector3.left, rotationSpeed * Time.deltaTime);
	}
}
