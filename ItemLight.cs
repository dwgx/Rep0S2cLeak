using System.Collections.Generic;
using UnityEngine;

public class ItemLight : MonoBehaviour
{
	public bool alwaysActive;

	public Light itemLight;

	private float lightIntensityOriginal;

	private float lightRangeOriginal;

	private bool showLight = true;

	private PhysGrabObject physGrabObject;

	private bool culledLight;

	public AnimationCurve lightIntensityCurve;

	private float animationCurveEval;

	public List<MeshRenderer> meshRenderers;

	private float fresnelScaleOriginal;

	private ItemEquippable itemEquippable;

	private void Start()
	{
		physGrabObject = GetComponentInParent<PhysGrabObject>();
		itemEquippable = GetComponentInParent<ItemEquippable>();
		lightIntensityOriginal = itemLight.intensity;
		lightRangeOriginal = itemLight.range;
		itemLight.intensity = 0f;
		itemLight.range = 0f;
		itemLight.enabled = false;
		if (meshRenderers.Count > 0)
		{
			foreach (MeshRenderer meshRenderer in meshRenderers)
			{
				if ((bool)meshRenderer && meshRenderer.gameObject.activeSelf && (bool)meshRenderer && meshRenderer.gameObject.activeSelf)
				{
					Material material = meshRenderer.material;
					fresnelScaleOriginal = material.GetFloat("_FresnelScale");
					break;
				}
			}
		}
		if (alwaysActive)
		{
			itemLight.enabled = true;
			showLight = true;
			itemLight.intensity = lightIntensityOriginal;
			itemLight.range = lightRangeOriginal;
		}
	}

	private void SetAllFresnel(float _value)
	{
		if (meshRenderers.Count <= 0)
		{
			return;
		}
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			if ((bool)meshRenderer && meshRenderer.gameObject.activeSelf)
			{
				meshRenderer.material.SetFloat("_FresnelScale", _value);
			}
		}
	}

	private void Update()
	{
		if (showLight)
		{
			if (!itemLight.enabled)
			{
				itemLight.intensity = 0f;
				itemLight.range = 0f;
				animationCurveEval = 0f;
				itemLight.enabled = true;
			}
			if (itemLight.intensity < lightIntensityOriginal - 0.01f)
			{
				animationCurveEval += Time.deltaTime * 0.05f;
				float t = lightIntensityCurve.Evaluate(animationCurveEval);
				if (meshRenderers.Count > 0)
				{
					foreach (MeshRenderer meshRenderer in meshRenderers)
					{
						if ((bool)meshRenderer && meshRenderer.gameObject.activeSelf)
						{
							Material material = meshRenderer.material;
							float num = material.GetFloat("_FresnelScale");
							material.SetFloat("_FresnelScale", Mathf.Lerp(num, fresnelScaleOriginal, t));
						}
					}
				}
				itemLight.intensity = Mathf.Lerp(itemLight.intensity, lightIntensityOriginal, t);
				itemLight.range = Mathf.Lerp(itemLight.range, lightRangeOriginal, t);
			}
		}
		else if (itemLight.enabled)
		{
			animationCurveEval += Time.deltaTime * 1f;
			float t2 = lightIntensityCurve.Evaluate(animationCurveEval);
			itemLight.intensity = Mathf.Lerp(itemLight.intensity, 0f, t2);
			itemLight.range = Mathf.Lerp(itemLight.range, 0f, t2);
			if (meshRenderers.Count > 0)
			{
				foreach (MeshRenderer meshRenderer2 in meshRenderers)
				{
					if ((bool)meshRenderer2 && meshRenderer2.gameObject.activeSelf)
					{
						Material material2 = meshRenderer2.material;
						float num2 = material2.GetFloat("_FresnelScale");
						material2.SetFloat("_FresnelScale", Mathf.Lerp(num2, 0f, t2));
					}
				}
			}
			if (itemLight.intensity < 0.01f)
			{
				animationCurveEval = 0f;
				itemLight.intensity = 0f;
				itemLight.range = 0f;
				if (meshRenderers.Count > 0)
				{
					foreach (MeshRenderer meshRenderer3 in meshRenderers)
					{
						if ((bool)meshRenderer3 && meshRenderer3.gameObject.activeSelf)
						{
							meshRenderer3.material.SetFloat("_FresnelScale", 0f);
						}
					}
				}
				itemLight.enabled = false;
			}
		}
		if (!SemiFunc.FPSImpulse1())
		{
			return;
		}
		if ((bool)SemiFunc.PlayerGetNearestTransformWithinRange(16f, base.transform.position))
		{
			culledLight = false;
		}
		else
		{
			culledLight = true;
		}
		if (!alwaysActive)
		{
			if (!culledLight)
			{
				if (!physGrabObject.grabbed)
				{
					if (!showLight)
					{
						itemLight.enabled = true;
						showLight = true;
					}
				}
				else
				{
					showLight = false;
				}
			}
			else
			{
				showLight = false;
			}
		}
		else if (culledLight)
		{
			showLight = false;
		}
		else
		{
			showLight = true;
		}
		if ((bool)itemEquippable && itemEquippable.IsEquipped())
		{
			showLight = false;
		}
	}
}
