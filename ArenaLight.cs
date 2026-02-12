using UnityEngine;

public class ArenaLight : MonoBehaviour
{
	internal MeshRenderer meshRenderer;

	internal Light arenaLight;

	private float lightIntensity = 0.5f;

	private void Start()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		arenaLight = GetComponentInChildren<Light>();
		lightIntensity = arenaLight.intensity;
	}

	private void Update()
	{
		if (arenaLight.enabled)
		{
			if (arenaLight.intensity > lightIntensity)
			{
				arenaLight.intensity = Mathf.Lerp(arenaLight.intensity, lightIntensity, Time.deltaTime * 2f);
				Color color = new Color(0.3f, 0f, 0f);
				meshRenderer.material.SetColor("_EmissionColor", Color.Lerp(meshRenderer.material.GetColor("_EmissionColor"), color, Time.deltaTime * 2f));
			}
			else
			{
				arenaLight.intensity = lightIntensity;
			}
		}
	}

	public void TurnOnArenaWarningLight()
	{
		meshRenderer.material.SetColor("_EmissionColor", Color.red);
		arenaLight.enabled = true;
	}

	public void PulsateLight()
	{
		arenaLight.intensity = lightIntensity * 2f;
		meshRenderer.material.SetColor("_EmissionColor", Color.red);
	}
}
