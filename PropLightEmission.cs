using UnityEngine;

public class PropLightEmission : MonoBehaviour
{
	public bool levelLight = true;

	internal bool turnedOff;

	internal Renderer meshRenderer;

	internal Color originalEmission;

	internal Material material;

	private void Awake()
	{
		meshRenderer = GetComponent<Renderer>();
		material = meshRenderer.material;
		originalEmission = material.GetColor("_EmissionColor");
	}
}
