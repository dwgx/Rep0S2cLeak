using System;
using UnityEngine;

namespace Audial.Utils;

public class BufferedComponent
{
	public float[,] buffer;

	private float loopTime;

	public float delayLength;

	public float decayLength;

	public int bufferLength;

	public float gain;

	public int readIndex;

	public int writeIndex;

	public int channelCount = 1;

	private float sampleRate;

	[SerializeField]
	private int _offset;

	public float DelayLength
	{
		get
		{
			return delayLength;
		}
		set
		{
			delayLength = value;
			Offset = (int)(delayLength * (Settings.SampleRate / 1000f));
		}
	}

	public int Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = (int)Mathf.Lerp(_offset, value, 0.8f);
		}
	}

	public BufferedComponent(float delayLength, float gain)
	{
		DelayLength = delayLength;
		this.gain = gain;
		bufferLength = (int)Settings.SampleRate * 10;
		buffer = new float[channelCount, bufferLength];
	}

	public void SetGainByDecayTime(float decayLength)
	{
		gain = Mathf.Pow(0.001f, delayLength / decayLength);
	}

	public float ProcessSample(int channel, float sample)
	{
		if (channel >= channelCount)
		{
			channelCount = channel + 1;
			buffer = new float[channelCount, bufferLength];
		}
		readIndex = ((Offset > writeIndex) ? (bufferLength + writeIndex - Offset) : (writeIndex - Offset));
		float num = buffer[channel, readIndex];
		buffer[channel, writeIndex] = sample + num * gain;
		return num;
	}

	public float ProcessSample(float sample)
	{
		readIndex = ((Offset > writeIndex) ? (bufferLength + writeIndex - Offset) : (writeIndex - Offset));
		float num = buffer[0, readIndex];
		buffer[0, writeIndex] = sample + num * gain;
		return num;
	}

	public void MoveIndex()
	{
		writeIndex = (writeIndex + 1) % bufferLength;
	}

	public void Reset()
	{
		for (int i = 0; i < buffer.Length; i++)
		{
			Array.Clear(buffer, 0, buffer.Length);
		}
		readIndex = 0;
	}
}
