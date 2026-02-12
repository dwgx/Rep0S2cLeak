using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy - _____", menuName = "Other/Enemy Setup", order = 0)]
public class EnemySetup : ScriptableObject
{
	public List<PrefabRef> spawnObjects;

	public bool levelsCompletedCondition;

	[Range(0f, 10f)]
	public int levelsCompletedMin;

	[Range(0f, 10f)]
	public int levelsCompletedMax = 10;

	[Space]
	public RarityPreset rarityPreset;

	[Space]
	public int runsPlayed;
}
