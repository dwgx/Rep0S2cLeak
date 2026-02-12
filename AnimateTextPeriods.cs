using System.Collections;
using TMPro;
using UnityEngine;

public class AnimateTextPeriods : MonoBehaviour
{
	private TextMeshProUGUI textMesh;

	private string textString;

	private Coroutine animateCoroutine;

	private void Awake()
	{
		textMesh = GetComponent<TextMeshProUGUI>();
		textString = textMesh.text;
	}

	private void OnEnable()
	{
		animateCoroutine = StartCoroutine(AnimateDots());
	}

	private IEnumerator AnimateDots()
	{
		while (true)
		{
			textMesh.text = textString + "...".Substring(0, Mathf.FloorToInt(Time.unscaledTime * 8f % 4f));
			yield return null;
		}
	}

	private void OnDisable()
	{
		if (animateCoroutine != null)
		{
			StopCoroutine(animateCoroutine);
		}
	}
}
