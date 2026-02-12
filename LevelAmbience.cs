using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Ambience - _____", menuName = "Level/Level Ambience", order = 2)]
public class LevelAmbience : ScriptableObject
{
	public AudioClip loopClip;

	[Range(0f, 1f)]
	public float loopVolume = 0.5f;

	[Space(20f)]
	public List<LevelAmbienceBreaker> breakers;

	private void OnValidate()
	{
		if (SemiFunc.OnValidateCheck())
		{
			return;
		}
		foreach (LevelAmbienceBreaker breaker in breakers)
		{
			if ((bool)breaker.sound)
			{
				breaker.name = breaker.sound.name.ToUpper();
			}
			breaker.ambience = this;
		}
		if (Application.isPlaying && (bool)LevelGenerator.Instance && LevelGenerator.Instance.Generated)
		{
			AmbienceLoop.instance.LiveUpdate();
		}
	}
}
