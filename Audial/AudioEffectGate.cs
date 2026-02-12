using Audial.Utils;
using UnityEngine;

namespace Audial;

public class AudioEffectGate : MonoBehaviour
{
	[SerializeField]
	private Envelope envelope;

	private float sampleRate;

	[SerializeField]
	private float _inputGain = 1f;

	[SerializeField]
	private float _threshold = 0.247f;

	[SerializeField]
	private float _attack;

	[SerializeField]
	public float _release = 0.75f;

	[SerializeField]
	private float _outputGain = 1f;

	private float env;

	public float InputGain
	{
		get
		{
			return _inputGain;
		}
		set
		{
			if (value != _inputGain)
			{
				_inputGain = Mathf.Clamp(value, 0f, 3f);
			}
		}
	}

	public float Threshold
	{
		get
		{
			return _threshold;
		}
		set
		{
			if (value != _threshold)
			{
				_threshold = Mathf.Clamp(value, 0f, 1f);
			}
		}
	}

	public float Attack
	{
		get
		{
			return _attack;
		}
		set
		{
			if (value != _attack)
			{
				_attack = Mathf.Clamp(value, 0f, 1f);
				envelope.Attack = _attack;
			}
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
			if (value != _release)
			{
				_release = Mathf.Clamp(value, 0f, 1f);
				envelope.Release = _release;
			}
		}
	}

	public float OutputGain
	{
		get
		{
			return _outputGain;
		}
		set
		{
			if (value != _outputGain)
			{
				_outputGain = Mathf.Clamp(value, 0f, 5f);
			}
		}
	}

	private void OnEnable()
	{
		sampleRate = AudioSettings.outputSampleRate;
		envelope = new Envelope(Attack, Release);
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		for (int i = 0; i < data.Length; i += channels)
		{
			data[i] *= InputGain;
			data[i + 1] *= InputGain;
			float sample = Mathf.Sqrt(data[i] * data[i] + data[i + 1] * data[i + 1]);
			float num = envelope.ProcessSample(sample);
			float num2 = 1f;
			if (num < Threshold)
			{
				num2 = Mathf.Pow(num / 4f, 4f);
			}
			data[i] *= num2 * OutputGain;
			data[i + 1] *= num2 * OutputGain;
		}
	}
}
