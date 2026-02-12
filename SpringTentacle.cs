using UnityEngine;

public class SpringTentacle : MonoBehaviour
{
	public SpringQuaternion springStart;

	public Transform springStartTarget;

	public Transform springStartSource;

	[Space]
	public SpringQuaternion springMid;

	public Transform springMidTarget;

	public Transform springMidSource;

	[Space]
	public SpringQuaternion springEnd;

	public Transform springEndTarget;

	public Transform springEndSource;

	private float offsetX;

	private float offsetY;

	private void Start()
	{
		offsetX = Random.Range(0f, 100f);
		offsetY = Random.Range(0f, 100f);
	}

	private void Update()
	{
		springStartTarget.transform.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 5f + offsetX) * 5f, Mathf.Sin(Time.time * 5f + offsetY) * 10f, 0f);
		springStartSource.rotation = SemiFunc.SpringQuaternionGet(springStart, springStartTarget.transform.rotation);
		springMidSource.rotation = SemiFunc.SpringQuaternionGet(springMid, springMidTarget.transform.rotation);
		springEndSource.rotation = SemiFunc.SpringQuaternionGet(springEnd, springEndTarget.transform.rotation);
	}
}
