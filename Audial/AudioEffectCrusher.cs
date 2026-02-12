using Audial.Utils;
using UnityEngine;

namespace Audial;

public class AudioEffectCrusher : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private int _bitDepth = 8;

	private long m;

	[SerializeField]
	[Range(0.001f, 1f)]
	private float _sampleRate = 0.1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _dryWet = 1f;

	private float[] y;

	private float cnt;

	private LFO lfo;

	public int BitDepth
	{
		get
		{
			return _bitDepth;
		}
		set
		{
			if (value != _bitDepth)
			{
				_bitDepth = Mathf.Clamp(value, 1, 32);
				Callibrate();
			}
		}
	}

	public float SampleRate
	{
		get
		{
			return _sampleRate;
		}
		set
		{
			if (value != _sampleRate)
			{
				_sampleRate = Mathf.Clamp(value, 0.001f, 1f);
			}
		}
	}

	public float DryWet
	{
		get
		{
			return _dryWet;
		}
		set
		{
			if (value != _dryWet)
			{
				_dryWet = Mathf.Clamp(value, 0f, 1f);
			}
		}
	}

	private void Awake()
	{
		y = new float[2];
		Callibrate();
	}

	private void Callibrate()
	{
		m = 1 << _bitDepth - 1;
		m = ((m < 0) ? int.MaxValue : m);
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		for (int i = 0; i < data.Length; i += channels)
		{
			cnt += SampleRate;
			if (cnt >= 1f)
			{
				cnt -= 1f;
				for (int j = 0; j < channels; j++)
				{
					y[j] = (float)(long)(data[i + j] * (float)m) / (float)m;
				}
			}
			for (int k = 0; k < channels; k++)
			{
				float num = y[k];
				data[i + k] = data[i + k] * (1f - DryWet) + num * DryWet;
			}
		}
	}
}
