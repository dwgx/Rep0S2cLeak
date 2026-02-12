using System.Collections;
using UnityEngine;

public class MapModule : MonoBehaviour
{
	public Module module;

	public AnimationCurve curve;

	public float speed;

	private float curveLerp;

	private bool animating;

	public Transform graphic;

	public void Hide()
	{
		if (!animating)
		{
			animating = true;
			StartCoroutine(HideAnimation());
		}
	}

	private void Update()
	{
		if (Map.Instance.Active)
		{
			graphic.transform.rotation = DirtFinderMapPlayer.Instance.transform.rotation;
			graphic.transform.rotation = Quaternion.Euler(new Vector3(90f, graphic.transform.rotation.eulerAngles.y, graphic.transform.rotation.eulerAngles.z));
		}
	}

	private IEnumerator HideAnimation()
	{
		while (curveLerp < 1f)
		{
			curveLerp += speed * Time.deltaTime;
			base.transform.localScale = Vector3.one * curve.Evaluate(curveLerp);
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}
}
