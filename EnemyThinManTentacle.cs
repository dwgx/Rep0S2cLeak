using UnityEngine;

public class EnemyThinManTentacle : MonoBehaviour
{
	public AnimationCurve wiggleCurve;

	private float rotationLerp = 1f;

	private float rotationLerpSpeed;

	private Quaternion rotationStartPos;

	private Quaternion rotationEndPos;

	private float scaleLerp = 1f;

	private float scaleLerpSpeed;

	private Vector3 scaleStartPos;

	private Vector3 scaleEndPos;

	private void Update()
	{
		if (rotationLerp >= 1f)
		{
			rotationStartPos = base.transform.localRotation;
			rotationEndPos = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
			rotationLerpSpeed = Random.Range(1f, 2f);
			rotationLerp = 0f;
		}
		else
		{
			rotationLerp += Time.deltaTime * rotationLerpSpeed;
			base.transform.localRotation = Quaternion.Lerp(rotationStartPos, rotationEndPos, wiggleCurve.Evaluate(rotationLerp));
		}
		if (scaleLerp >= 1f)
		{
			scaleStartPos = base.transform.localScale;
			scaleEndPos = new Vector3(Random.Range(0.8f, 1.2f), Random.Range(0.8f, 1.2f), Random.Range(0.8f, 1.2f));
			scaleLerpSpeed = Random.Range(2f, 4f);
			scaleLerp = 0f;
		}
		else
		{
			scaleLerp += Time.deltaTime * scaleLerpSpeed;
			base.transform.localScale = Vector3.Lerp(scaleStartPos, scaleEndPos, wiggleCurve.Evaluate(scaleLerp));
		}
	}
}
