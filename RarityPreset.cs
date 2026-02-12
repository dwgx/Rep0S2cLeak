using UnityEngine;

[CreateAssetMenu(fileName = "Rarity - _____", menuName = "Other/Rarity Preset", order = 0)]
public class RarityPreset : ScriptableObject
{
	[Range(0f, 100f)]
	public float chance;
}
