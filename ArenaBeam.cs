using UnityEngine;

public class ArenaBeam : MonoBehaviour
{
	public Transform lineTarget;

	private LineRenderer lineRenderer;

	private PhysGrabObject physGrabObject;

	internal bool outro;

	private void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.widthMultiplier = 0f;
	}

	private void Update()
	{
		if ((bool)lineTarget)
		{
			Vector3 position = base.transform.position;
			Vector3 position2 = lineTarget.position;
			Vector3[] array = new Vector3[20];
			for (int i = 0; i < 20; i++)
			{
				float num = (float)i / 19f;
				array[i] = Vector3.Lerp(position, position2, num);
				float num2 = 1f - Mathf.Abs(num - 0.3f) * 2f;
				float num3 = 1f;
				array[i] += Vector3.right * Mathf.Sin(Time.time * (30f * num3) + (float)i) * 0.05f * num2;
				array[i] += Vector3.forward * Mathf.Cos(Time.time * (30f * num3) + (float)i) * 0.05f * num2;
			}
			lineRenderer.material.mainTextureOffset = new Vector2(Time.time * 2f, 0f);
			lineRenderer.positionCount = 20;
			lineRenderer.SetPositions(array);
		}
		else
		{
			outro = true;
		}
		if (!outro)
		{
			if (lineRenderer.widthMultiplier < 0.195f)
			{
				lineRenderer.widthMultiplier = Mathf.Lerp(lineRenderer.widthMultiplier, 0.2f, Time.deltaTime * 2f);
			}
			else
			{
				lineRenderer.widthMultiplier = 0.2f;
			}
		}
		else if (lineRenderer.widthMultiplier > 0.005f)
		{
			lineRenderer.widthMultiplier = Mathf.Lerp(lineRenderer.widthMultiplier, 0f, Time.deltaTime * 2f);
		}
		else
		{
			lineRenderer.widthMultiplier = 0f;
			base.transform.parent.gameObject.SetActive(value: false);
		}
	}
}
