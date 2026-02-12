using UnityEngine;

[CreateAssetMenu(fileName = "Material - _____", menuName = "Other/Material Preset", order = 1)]
public class MaterialPreset : ScriptableObject
{
	public string Name;

	public Materials.Type Type;

	[Space]
	[Header("Impact")]
	public Sound ImpactLight;

	public Sound ImpactMedium;

	public Sound ImpactHeavy;

	[Space]
	[Header("Rare Impact")]
	public Sound RareImpactLight;

	public int RareImpactLightMin;

	public int RareImpactLightMax;

	[HideInInspector]
	public float RareImpactLightCurrent;

	[Space]
	public Sound RareImpactMedium;

	public int RareImpactMediumMin;

	public int RareImpactMediumMax;

	[HideInInspector]
	public float RareImpactMediumCurrent;

	[Space]
	public Sound RareImpactHeavy;

	public int RareImpactHeavyMin;

	public int RareImpactHeavyMax;

	[HideInInspector]
	public float RareImpactHeavyCurrent;

	[Space]
	[Header("Footstep")]
	public Sound FootstepLight;

	public Sound FootstepMedium;

	public Sound FootstepHeavy;

	[Space]
	[Header("Rare Footstep")]
	public Sound RareFootstepLight;

	public int RareFootstepLightMin;

	public int RareFootstepLightMax;

	[HideInInspector]
	public float RareFootstepLightCurrent;

	[Space]
	public Sound RareFootstepMedium;

	public int RareFootstepMediumMin;

	public int RareFootstepMediumMax;

	[HideInInspector]
	public float RareFootstepMediumCurrent;

	[Space]
	public Sound RareFootstepHeavy;

	public int RareFootstepHeavyMin;

	public int RareFootstepHeavyMax;

	[HideInInspector]
	public float RareFootstepHeavyCurrent;

	[Space]
	[Header("Slide")]
	public Sound SlideOneShot;

	public Sound SlideLoop;

	[Space]
	[Header("Prefabs")]
	public GameObject FootstepPrefab;

	public GameObject ImpactPrefab;

	private void Start()
	{
		Setup();
	}

	public void Setup()
	{
		RareImpactLightCurrent = Random.Range(RareImpactLightMin, RareImpactLightMax);
		RareImpactMediumCurrent = Random.Range(RareImpactMediumMin, RareImpactMediumMax);
		RareImpactHeavyCurrent = Random.Range(RareImpactHeavyMin, RareImpactHeavyMax);
		RareFootstepLightCurrent = Random.Range(RareFootstepLightMin, RareFootstepLightMax);
		RareFootstepMediumCurrent = Random.Range(RareFootstepMediumMin, RareFootstepMediumMax);
		RareFootstepHeavyCurrent = Random.Range(RareFootstepHeavyMin, RareFootstepHeavyMax);
	}

	private void OnValidate()
	{
		if (!SemiFunc.OnValidateCheck())
		{
			ImpactLight.Type = AudioManager.AudioType.MaterialImpact;
			ImpactMedium.Type = AudioManager.AudioType.MaterialImpact;
			ImpactHeavy.Type = AudioManager.AudioType.MaterialImpact;
			RareImpactLight.Type = AudioManager.AudioType.MaterialImpact;
			RareImpactMedium.Type = AudioManager.AudioType.MaterialImpact;
			RareImpactHeavy.Type = AudioManager.AudioType.MaterialImpact;
			FootstepLight.Type = AudioManager.AudioType.Footstep;
			FootstepMedium.Type = AudioManager.AudioType.Footstep;
			FootstepHeavy.Type = AudioManager.AudioType.Footstep;
			RareFootstepLight.Type = AudioManager.AudioType.Footstep;
			RareFootstepMedium.Type = AudioManager.AudioType.Footstep;
			RareFootstepHeavy.Type = AudioManager.AudioType.Footstep;
			SlideOneShot.Type = AudioManager.AudioType.MaterialImpact;
			SlideLoop.Type = AudioManager.AudioType.MaterialImpact;
		}
	}
}
