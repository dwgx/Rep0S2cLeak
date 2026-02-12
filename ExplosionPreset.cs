using UnityEngine;

[CreateAssetMenu(fileName = "Effect Presets", menuName = "Effect Presets/Explosion Preset", order = 1)]
public class ExplosionPreset : ScriptableObject
{
	[Header("Settings")]
	[Space(5f)]
	public float explosionForceMultiplier = 1f;

	[Space(20f)]
	[Header("Colors")]
	[Space(5f)]
	public Gradient explosionColors;

	public Gradient smokeColors;

	public Gradient lightColor;

	[Space(20f)]
	[Header("Sounds")]
	[Space(5f)]
	public Sound explosionSoundSmall;

	public Sound explosionSoundSmallGlobal;

	public Sound explosionSoundMedium;

	public Sound explosionSoundMediumGlobal;

	public Sound explosionSoundBig;

	public Sound explosionSoundBigGlobal;
}
