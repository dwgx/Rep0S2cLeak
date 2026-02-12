using UnityEngine;

namespace Audial;

public class AudioEffectDelay : MonoBehaviour
{
	private float sampleRate;

	private float[,] delayBuffer;

	private int index;

	[SerializeField]
	private float _BPM = 120f;

	[SerializeField]
	private int _delayCount = 3;

	[SerializeField]
	private int _delayUnit = 8;

	[SerializeField]
	private float _dryWet = 0.5f;

	[SerializeField]
	private float _decayLength = 0.25f;

	[SerializeField]
	private float _pan;

	public bool PingPong;

	private float delayLength;

	private int delaySamples;

	private float output;

	public float BPM
	{
		get
		{
			return _BPM;
		}
		set
		{
			_BPM = Mathf.Clamp(value, 40f, 300f);
			ChangeDelay();
		}
	}

	public int DelayCount
	{
		get
		{
			return _delayCount;
		}
		set
		{
			_delayCount = Mathf.Clamp(value, 1, 8);
			ChangeDelay();
		}
	}

	public int DelayUnit
	{
		get
		{
			return _delayUnit;
		}
		set
		{
			_delayUnit = Mathf.Clamp(value, 1, 32);
			ChangeDelay();
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
			_dryWet = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public float DecayLength
	{
		get
		{
			return _decayLength;
		}
		set
		{
			_decayLength = Mathf.Clamp(value, 0.1f, 1f);
		}
	}

	public float Pan
	{
		get
		{
			return _pan;
		}
		set
		{
			_pan = Mathf.Clamp(value, -1f, 1f);
		}
	}

	private void Awake()
	{
		sampleRate = AudioSettings.outputSampleRate;
		ChangeDelay();
	}

	private void ChangeDelay()
	{
		delayLength = (float)DelayCount * (240f / BPM) / (float)DelayUnit;
		delaySamples = (int)(delayLength * sampleRate);
		delayBuffer = new float[2, delaySamples];
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (delayBuffer == null)
		{
			ChangeDelay();
		}
		float[] array = new float[channels];
		float[] array2 = new float[2] { 1f, 1f };
		if (Pan > 0f)
		{
			array2[0] = 1f - Mathf.Abs(Pan);
		}
		else if (Pan < 0f)
		{
			array2[1] = 1f - Mathf.Abs(Pan);
		}
		for (int i = 0; i < data.Length; i += channels)
		{
			index %= delaySamples;
			if (PingPong)
			{
				for (int j = 0; j < channels; j++)
				{
					array[j] = delayBuffer[j, index];
					delayBuffer[j, index] = 0f;
				}
				for (int k = 0; k < channels; k++)
				{
					float num = data[i + k];
					float num2 = array[(k + 1) % channels];
					output = num * (1f - DryWet) + num2 * DryWet;
					data[i + k] = output;
					delayBuffer[k, index] += num2 * DecayLength;
					delayBuffer[(k + 1) % channels, index] += num * array2[k];
				}
			}
			else
			{
				for (int l = 0; l < channels; l++)
				{
					array[l] = delayBuffer[l, index];
					delayBuffer[l, index] = 0f;
					float num3 = data[i + l];
					float num4 = array[l];
					output = num3 * (1f - DryWet) + num4 * DryWet;
					data[i + l] = output;
					delayBuffer[l, index] += num4 * DecayLength;
					delayBuffer[l, index] += num3 * array2[l];
				}
			}
			index++;
		}
	}
}
