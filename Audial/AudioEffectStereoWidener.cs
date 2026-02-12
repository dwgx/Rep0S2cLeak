using UnityEngine;

namespace Audial;

public class AudioEffectStereoWidener : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 2f)]
	private float _width = 1.3f;

	public float Width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = Mathf.Clamp(value, 0f, 2f);
		}
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (channels >= 2)
		{
			float num = Width * 0.5f;
			for (int i = 0; i < data.Length; i += channels)
			{
				float num2 = (data[i] + data[i + 1]) * 0.5f;
				float num3 = (data[i] - data[i + 1]) * num;
				data[i] = num2 + num3;
				data[i + 1] = num2 - num3;
			}
		}
	}
}
