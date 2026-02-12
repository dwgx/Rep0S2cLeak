using System;
using UnityEngine;

namespace Audial.Utils;

[Serializable]
public class Envelope
{
	private EnvelopeState envelopeState;

	private float env;

	private float _attack;

	public float attackCoeff;

	private float _release;

	private float releaseCoeff;

	private float sustain = 1f;

	private float sampleRate;

	public float Attack
	{
		get
		{
			return _attack;
		}
		set
		{
			_attack = value;
			attackCoeff = Mathf.Exp(-1f / (Settings.SampleRate * _attack));
		}
	}

	public float Release
	{
		get
		{
			return _release;
		}
		set
		{
			_release = value;
			releaseCoeff = Mathf.Exp(-1f / (Settings.SampleRate * _release));
		}
	}

	public Envelope(float attack, float release)
	{
		Attack = attack;
		Release = release;
	}

	public float ProcessSample(float sample)
	{
		float num = ((sample > env) ? attackCoeff : releaseCoeff);
		env = (1f - num) * sample + num * env;
		return env;
	}
}
