using UnityEngine;

public class LineBetweenTwoPoints : MonoBehaviour
{
	public GameObject lineBetweenTwoPoints;

	public bool hasSpheres = true;

	private GameObject line;

	private GameObject sphere1;

	private GameObject sphere2;

	public Color lineColor = Color.white;

	private LineRenderer lineRenderer;

	private Vector3 pointA;

	private Vector3 pointB;

	private float lineRenderLifetime;

	private void Start()
	{
		line = lineBetweenTwoPoints.transform.Find("Line").gameObject;
		sphere1 = lineBetweenTwoPoints.transform.Find("Sphere1").gameObject;
		sphere2 = lineBetweenTwoPoints.transform.Find("Sphere2").gameObject;
		ItemDrone component = GetComponent<ItemDrone>();
		if ((bool)component)
		{
			lineColor = component.beamColor;
		}
		lineRenderer = line.GetComponent<LineRenderer>();
		lineRenderer.positionCount = 2;
		lineRenderer.enabled = false;
		if ((bool)sphere1)
		{
			sphere1.GetComponent<MeshRenderer>().enabled = false;
		}
		if ((bool)sphere2)
		{
			sphere2.GetComponent<MeshRenderer>().enabled = false;
		}
		lineRenderer.material.SetColor("_EmissionColor", lineColor);
		lineRenderer.material.SetColor("_Color", lineColor);
		lineRenderer.material.SetColor("_AlbedoColor", lineColor);
		if ((bool)sphere1)
		{
			sphere1.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", lineColor);
			sphere1.GetComponent<MeshRenderer>().material.SetColor("_Color", lineColor);
			sphere1.GetComponent<MeshRenderer>().material.SetColor("_AlbedoColor", lineColor);
		}
		if ((bool)sphere2)
		{
			sphere2.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", lineColor);
			sphere2.GetComponent<MeshRenderer>().material.SetColor("_Color", lineColor);
			sphere2.GetComponent<MeshRenderer>().material.SetColor("_AlbedoColor", lineColor);
		}
		if (!hasSpheres)
		{
			Object.Destroy(sphere1);
			Object.Destroy(sphere2);
		}
	}

	private void Update()
	{
		float num = 2f;
		lineRenderer.material.mainTextureOffset = new Vector2(Time.time * num, 0f);
		if ((bool)sphere1)
		{
			sphere1.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(Time.time * num, 0f);
		}
		if ((bool)sphere2)
		{
			sphere2.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(Time.time * num, 0f);
		}
		float num2 = 0.05f;
		float num3 = 0.025f;
		lineRenderer.startWidth = num2 + Mathf.Sin(Time.time * 5f) * num3;
		lineRenderer.endWidth = num2 + Mathf.Cos(Time.time * 5f) * num3;
		if (!lineRenderer.enabled)
		{
			return;
		}
		if (lineRenderLifetime <= 0f)
		{
			lineRenderer.enabled = false;
			lineRenderer.gameObject.SetActive(value: false);
			if ((bool)sphere1)
			{
				sphere1.GetComponent<MeshRenderer>().enabled = false;
			}
			if ((bool)sphere2)
			{
				sphere2.GetComponent<MeshRenderer>().enabled = false;
			}
		}
		else
		{
			lineRenderLifetime -= Time.deltaTime;
		}
	}

	public void DrawLine(Vector3 point1, Vector3 point2)
	{
		lineRenderer.gameObject.SetActive(value: true);
		lineRenderer.enabled = true;
		if ((bool)sphere1)
		{
			sphere1.GetComponent<MeshRenderer>().enabled = true;
			sphere1.transform.position = point1;
		}
		if ((bool)sphere2)
		{
			sphere2.GetComponent<MeshRenderer>().enabled = true;
			sphere2.transform.position = point2;
		}
		float num = 2f;
		if ((bool)sphere1)
		{
			sphere1.transform.localScale = new Vector3(lineRenderer.startWidth * num, lineRenderer.startWidth * num, lineRenderer.startWidth * num);
		}
		if ((bool)sphere2)
		{
			sphere2.transform.localScale = new Vector3(lineRenderer.endWidth * num, lineRenderer.endWidth * num, lineRenderer.endWidth * num);
		}
		lineRenderer.SetPosition(0, point1);
		lineRenderer.SetPosition(1, point2);
		lineRenderLifetime = 0.01f;
	}
}
