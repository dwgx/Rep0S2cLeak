using System.Collections;
using UnityEngine;

public class DirtFinderMapDoorTarget : MonoBehaviour
{
	public Transform Target;

	public Transform HingeTransform;

	public MapLayer Layer;

	private void Start()
	{
		StartCoroutine(Logic());
	}

	public IEnumerator Logic()
	{
		while ((bool)Target && Target.gameObject.activeSelf)
		{
			if (Map.Instance.Active)
			{
				Map.Instance.DoorUpdate(HingeTransform, Target.transform, Layer);
			}
			yield return new WaitForSeconds(0.1f);
		}
		Object.Destroy(base.gameObject);
	}
}
