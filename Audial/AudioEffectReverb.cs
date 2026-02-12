using Audial.Utils;
using UnityEngine;

namespace Audial;

public class AudioEffectReverb : MonoBehaviour
{
	[SerializeField]
	private float _reverbTime = 1.55f;

	[SerializeField]
	private float _reverbGain = 1f;

	[SerializeField]
	private float _dryWet = 0.16f;

	private CombFilter[] combFilters;

	private AllPassFilter[] allPassFilters;

	public float ReverbTime
	{
		get
		{
			return _reverbTime;
		}
		set
		{
			_reverbTime = Mathf.Clamp(value, 0.5f, 10f);
			Callibrate();
		}
	}

	public float ReverbGain
	{
		get
		{
			return _reverbGain;
		}
		set
		{
			_reverbGain = Mathf.Clamp(value, 0.5f, 5f);
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

	private void Awake()
	{
		Settings.SampleRate = AudioSettings.outputSampleRate;
		Initialize();
	}

	private void Initialize()
	{
		combFilters = new CombFilter[4];
		combFilters[0] = new CombFilter(29.7f, 1f);
		combFilters[1] = new CombFilter(37.1f, 1f);
		combFilters[2] = new CombFilter(41.1f, 1f);
		combFilters[3] = new CombFilter(43.7f, 1f);
		Callibrate();
		allPassFilters = new AllPassFilter[2];
		allPassFilters[0] = new AllPassFilter(5f, 1f);
		allPassFilters[0].SetGainByDecayTime(1.683f);
		allPassFilters[1] = new AllPassFilter(1.7f, 1f);
		allPassFilters[1].SetGainByDecayTime(2.232f);
	}

	private void Callibrate()
	{
		if (combFilters != null)
		{
			for (int i = 0; i < combFilters.Length; i++)
			{
				combFilters[i].SetGainByDecayTime(ReverbTime * 1000f);
			}
		}
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (combFilters == null || allPassFilters == null)
		{
			Initialize();
		}
		for (int i = 0; i < data.Length; i += channels)
		{
			for (int j = 0; j < channels; j++)
			{
				float num = data[i + j] * ReverbGain;
				for (int k = 0; k < combFilters.Length; k++)
				{
					num += combFilters[k].ProcessSample(j, data[i + j]);
				}
				num /= (float)combFilters.Length;
				float num2 = num / (float)combFilters.Length;
				for (int l = 0; l < allPassFilters.Length; l++)
				{
					num2 += allPassFilters[l].ProcessSample(j, num);
				}
				data[i + j] = data[i + j] * (1f - DryWet) + num2 * ReverbGain / (float)allPassFilters.Length * DryWet;
			}
			for (int m = 0; m < combFilters.Length; m++)
			{
				combFilters[m].MoveIndex();
			}
			for (int n = 0; n < allPassFilters.Length; n++)
			{
				allPassFilters[n].MoveIndex();
			}
		}
	}
}
