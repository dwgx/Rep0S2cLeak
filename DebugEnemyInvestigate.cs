using UnityEngine;

public class DebugEnemyInvestigate : MonoBehaviour
{
	public AnimationCurve animationCurve;

	private float lerp;

	private float alpha = 1f;

	internal float radius = 1f;

	private float radiusCurrent = 2f;

	private void Update()
	{
		lerp += Time.deltaTime;
		radiusCurrent = Mathf.Lerp(0f, radius, animationCurve.Evaluate(lerp));
		if (lerp >= 1f)
		{
			alpha -= Time.deltaTime;
			if (alpha <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	private void OnDrawGizmos()
	{
		base.transform.eulerAngles = Vector3.zero;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = new Color(1f, 1f, 1f, alpha);
		Gizmos.DrawWireSphere(Vector3.zero, 0.1f);
		Gizmos.color = new Color(1f, 0.62f, 0f, 0.23f * alpha);
		Gizmos.DrawSphere(Vector3.zero, radiusCurrent);
		Gizmos.color = new Color(1f, 0.62f, 0f, alpha);
		Gizmos.DrawWireSphere(Vector3.zero, radiusCurrent);
		base.transform.localEulerAngles = new Vector3(45f, 0f, 0f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireSphere(Vector3.zero, radiusCurrent);
		base.transform.localEulerAngles = new Vector3(0f, 45f, 0f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireSphere(Vector3.zero, radiusCurrent);
		base.transform.localEulerAngles = new Vector3(0f, 0f, 45f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireSphere(Vector3.zero, radiusCurrent);
	}
}
