using UnityEngine;

[CreateAssetMenu(fileName = "Durability ", menuName = "Phys Object/Durability Preset", order = 1)]
public class Durability : ScriptableObject
{
	[Range(0f, 100f)]
	public float fragility = 100f;

	[Range(0f, 100f)]
	public float durability = 100f;
}
