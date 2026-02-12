using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Music - _____", menuName = "Level/Level Music Preset", order = 3)]
public class LevelMusicAsset : ScriptableObject
{
	public List<LevelMusic.LevelMusicTrack> tracks;
}
