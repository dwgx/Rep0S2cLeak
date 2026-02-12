using UnityEngine;

[CreateAssetMenu(fileName = "Constant Music - _____", menuName = "Level/Constant Music Preset", order = 3)]
public class ConstantMusicAsset : ScriptableObject
{
	public AudioClip clip;

	public float volume = 1f;
}
