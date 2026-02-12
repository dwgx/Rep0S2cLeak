using UnityEngine;

public class SquirtCode : MonoBehaviour
{
	public Transform SquirtPlane1;

	public Transform SquirtPlane2;

	private Vector3 SquirtPlane1OriginalScale;

	private Vector3 SquirtPlane2OriginalScale;

	private Vector3 mainOriginalScale;

	private void Start()
	{
		SquirtPlane1OriginalScale = SquirtPlane1.localScale;
		SquirtPlane2OriginalScale = SquirtPlane2.localScale;
		mainOriginalScale = base.transform.localScale;
	}

	private void Update()
	{
		base.transform.Rotate(Vector3.right * Time.deltaTime * 800f);
		SquirtPlane1.localScale = new Vector3(SquirtPlane1OriginalScale.x, SquirtPlane1OriginalScale.y, SquirtPlane1OriginalScale.z + Mathf.Sin(Time.time * 50f) * 0.15f);
		SquirtPlane2.localScale = new Vector3(SquirtPlane1OriginalScale.x, SquirtPlane1OriginalScale.y, SquirtPlane1OriginalScale.z + Mathf.Sin(Time.time * 50f + 50f) * 0.15f);
		base.transform.localScale = new Vector3(mainOriginalScale.x + Mathf.Sin(Time.time * 50f) * 0.15f, mainOriginalScale.y, mainOriginalScale.z);
	}
}
