using UnityEngine;

public class LevitationSphereEffect : MonoBehaviour
{
	private MeshRenderer meshRenderer;

	private Light lightSphere;

	private LevitationSphere _levitationSphere;

	private float originalScale;

	private Color originalMaterialColor;

	private void Start()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		lightSphere = GetComponentInChildren<Light>();
		_levitationSphere = GetComponentInParent<LevitationSphere>();
		originalScale = base.transform.localScale.x;
		originalMaterialColor = meshRenderer.material.color;
	}

	private void Update()
	{
		if ((bool)_levitationSphere && (_levitationSphere.state == LevitationSphere.State.levitate || _levitationSphere.state == LevitationSphere.State.start))
		{
			PulseEffect();
		}
	}

	private void PulseEffect()
	{
		if (base.transform.parent.transform.localScale == Vector3.zero)
		{
			return;
		}
		base.transform.localScale += new Vector3(1f, 1f, 1f) * Time.deltaTime * 2f;
		Color color = meshRenderer.material.color;
		if (base.transform.localScale.magnitude > 10f)
		{
			color.a -= 1f * Time.deltaTime;
			if ((bool)lightSphere)
			{
				lightSphere.intensity = 4f * color.a;
			}
		}
		meshRenderer.material.color = color;
		if ((bool)lightSphere)
		{
			lightSphere.range = base.transform.localScale.x * 2.8f;
		}
		meshRenderer.material.mainTextureOffset += new Vector2(0.1f, 0.1f) * Time.deltaTime;
		if (color.a <= 0f)
		{
			if ((bool)lightSphere)
			{
				lightSphere.intensity = 4f;
			}
			if ((bool)lightSphere)
			{
				lightSphere.range = 0f;
			}
			base.transform.localScale = Vector3.zero;
			color.a = 1f;
			meshRenderer.material.color = color;
		}
	}
}
