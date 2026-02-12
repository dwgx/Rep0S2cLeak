using System;
using System.Collections;
using UnityEngine;

public class RandomRotateAndScale : MonoBehaviour
{
	[Space]
	[Header("Spawn")]
	public AnimationCurve spawnScaleCurve;

	public float spawnAnimationLength = 1f;

	[Space]
	[Header("Time before despawn")]
	public float durationBeforeDespawn = 5f;

	[Space]
	[Header("Despawn")]
	public AnimationCurve despawnScaleCurve;

	public float despawnAnimationLength = 1f;

	[Space]
	[Header("Scale")]
	public float scaleMultiplier = 1f;

	private void Start()
	{
		RotateObjectAndChildren();
		StartCoroutine(ScaleAnimation(spawnScaleCurve, spawnAnimationLength, delegate
		{
			StartCoroutine(WaitAndDespawn(durationBeforeDespawn));
		}));
		base.transform.position += Vector3.up * 0.02f;
		float maxDistance = 0.1f;
		if (Physics.Raycast(new Ray(base.transform.position, -Vector3.up), out var hitInfo, maxDistance))
		{
			base.transform.position = hitInfo.point + Vector3.up * 0.0001f;
		}
	}

	private void RotateObjectAndChildren()
	{
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		base.transform.localRotation = Quaternion.Euler(localEulerAngles.x + 90f, localEulerAngles.y, UnityEngine.Random.Range(0, 360));
		foreach (Transform item in base.transform)
		{
			Vector3 localEulerAngles2 = item.localEulerAngles;
			item.localRotation = Quaternion.Euler(localEulerAngles2.x, localEulerAngles2.y, UnityEngine.Random.Range(0, 360));
		}
	}

	private IEnumerator ScaleAnimation(AnimationCurve curve, float animationLength, Action onComplete)
	{
		float elapsedTime = 0f;
		while (elapsedTime < animationLength)
		{
			elapsedTime += Time.deltaTime;
			float time = elapsedTime / animationLength;
			float num = curve.Evaluate(time) * scaleMultiplier;
			base.transform.localScale = new Vector3(num, num, num);
			yield return null;
		}
		onComplete?.Invoke();
	}

	private IEnumerator WaitAndDespawn(float duration)
	{
		yield return new WaitForSeconds(duration);
		StartCoroutine(ScaleAnimation(despawnScaleCurve, despawnAnimationLength, delegate
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}));
	}
}
