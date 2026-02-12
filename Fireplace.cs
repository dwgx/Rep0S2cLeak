using UnityEngine;

public class Fireplace : MonoBehaviour
{
	public bool isLit;

	public bool isCornerFireplace;

	public Vector3 offsetOverride = new Vector3(0f, 0f, 0f);

	public GameObject fire;

	private Vector3 fireOffset = new Vector3(0f, 0.028f, 0.249f);

	private void Awake()
	{
		if (isLit)
		{
			GameObject gameObject = Object.Instantiate(fire, base.transform.position, base.transform.rotation);
			gameObject.transform.parent = base.transform;
			gameObject.transform.localRotation = Quaternion.identity;
			if (isCornerFireplace)
			{
				fireOffset = new Vector3(0.824f, 0.028f, 0.824f);
				gameObject.transform.localRotation *= Quaternion.Euler(0f, 45f, 0f);
			}
			if (offsetOverride != Vector3.zero)
			{
				fireOffset = offsetOverride;
			}
			gameObject.transform.localPosition = fireOffset;
		}
	}
}
