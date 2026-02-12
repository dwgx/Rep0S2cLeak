using System;
using UnityEngine;

[Serializable]
public class LevelAmbienceBreaker
{
	[HideInInspector]
	public string name;

	public AudioClip sound;

	[Range(0f, 1f)]
	public float volume = 0.5f;

	internal LevelAmbience ambience;

	public void Test()
	{
		if (Application.isPlaying)
		{
			AmbienceBreakers.instance.LiveTest(ambience, this);
		}
	}
}
