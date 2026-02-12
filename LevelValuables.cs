using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Valuables - _____", menuName = "Level/Level Valuables Preset", order = 2)]
public class LevelValuables : ScriptableObject
{
	public List<PrefabRef> tiny;

	public List<PrefabRef> small;

	public List<PrefabRef> medium;

	public List<PrefabRef> big;

	public List<PrefabRef> wide;

	public List<PrefabRef> tall;

	public List<PrefabRef> veryTall;
}
