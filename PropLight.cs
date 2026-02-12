using UnityEngine;

public class PropLight : MonoBehaviour
{
	public bool levelLight = true;

	internal bool turnedOff;

	[Range(0f, 2f)]
	public float lightRangeMultiplier = 1f;

	internal Light lightComponent;

	internal float originalIntensity;

	internal Behaviour halo;

	internal bool hasHalo;

	private void Awake()
	{
		lightComponent = GetComponent<Light>();
		originalIntensity = lightComponent.intensity;
		halo = GetComponent("Halo") as Behaviour;
		if ((bool)halo)
		{
			hasHalo = true;
		}
	}

	private void Start()
	{
		if (LevelGenerator.Instance.Generated)
		{
			SemiFunc.LightAdd(this);
		}
	}
}
