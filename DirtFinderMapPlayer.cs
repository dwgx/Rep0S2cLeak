using System.Collections;
using UnityEngine;

public class DirtFinderMapPlayer : MonoBehaviour
{
	public static DirtFinderMapPlayer Instance;

	private Transform PlayerTransform;

	private Vector3 StartOffset;

	private void Awake()
	{
		Instance = this;
		StartOffset = base.transform.position;
	}

	private void OnEnable()
	{
		PlayerTransform = null;
		StartCoroutine(FindPlayer());
	}

	private IEnumerator FindPlayer()
	{
		yield return new WaitForSeconds(0.1f);
		while (!PlayerTransform)
		{
			if ((bool)PlayerController.instance)
			{
				PlayerTransform = PlayerController.instance.transform;
				StartCoroutine(Logic());
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator Logic()
	{
		while (true)
		{
			base.transform.position = PlayerTransform.transform.position * Map.Instance.Scale + Map.Instance.OverLayerParent.position + StartOffset;
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, 0f, base.transform.localPosition.z);
			base.transform.rotation = PlayerTransform.rotation;
			MapLayer layerParent = Map.Instance.GetLayerParent(PlayerTransform.position.y + 0.01f);
			Map.Instance.PlayerLayer = layerParent.layer;
			yield return new WaitForSeconds(0.1f);
		}
	}
}
