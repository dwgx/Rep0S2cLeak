using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level - _____", menuName = "Level/Level Preset", order = 0)]
public class Level : ScriptableObject
{
	public string ResourcePath = "";

	[Space]
	public string NarrativeName = "";

	[Space]
	public int ModuleAmount = 6;

	public int PassageMaxAmount = 2;

	[Space]
	public bool HasEnemies = true;

	[Space]
	public GameObject ConnectObject;

	public GameObject BlockObject;

	public List<PrefabRef> StartRooms;

	[Space]
	public Sprite LoadingGraphic01;

	public Sprite LoadingGraphic02;

	public Sprite LoadingGraphic03;

	[Header("Difficulty 1")]
	public List<PrefabRef> ModulesNormal1;

	public List<PrefabRef> ModulesPassage1;

	public List<PrefabRef> ModulesDeadEnd1;

	public List<PrefabRef> ModulesExtraction1;

	[Space]
	[Header("Difficulty 2")]
	public List<PrefabRef> ModulesNormal2;

	public List<PrefabRef> ModulesPassage2;

	public List<PrefabRef> ModulesDeadEnd2;

	public List<PrefabRef> ModulesExtraction2;

	[Space]
	[Header("Difficulty 3")]
	public List<PrefabRef> ModulesNormal3;

	public List<PrefabRef> ModulesPassage3;

	public List<PrefabRef> ModulesDeadEnd3;

	public List<PrefabRef> ModulesExtraction3;

	public List<LevelValuables> ValuablePresets;

	public LevelMusicAsset MusicPreset;

	public ConstantMusicAsset ConstantMusicPreset;

	public List<LevelAmbience> AmbiencePresets;

	[Header("Fog")]
	public Color FogColor = Color.black;

	public float FogStartDistance;

	public float FogEndDistance = 15f;

	[Space(20f)]
	[Header("Environment")]
	public Color AmbientColor = new Color(0f, 0f, 0.2f);

	public Color AmbientColorAdaptation = new Color(0.06f, 0.06f, 0.39f);

	[Space(20f)]
	[Header("Color")]
	public float ColorTemperature;

	public Color ColorFilter = Color.white;

	[Space(20f)]
	[Header("Bloom")]
	public float BloomIntensity = 10f;

	public float BloomThreshold = 0.9f;

	[Space(20f)]
	[Header("Vignette")]
	public Color VignetteColor = new Color(0.02f, 0f, 0.22f, 0f);

	[Range(0f, 1f)]
	public float VignetteIntensity = 0.4f;

	[Range(0f, 1f)]
	public float VignetteSmoothness = 0.7f;

	public string DiscordIcon;

	public void OnValidate()
	{
		if (!SemiFunc.OnValidateCheck() && Application.isPlaying && LevelGenerator.Instance != null && LevelGenerator.Instance.Generated)
		{
			EnvironmentDirector.Instance.Setup();
			PostProcessing.Instance.Setup();
		}
	}
}
