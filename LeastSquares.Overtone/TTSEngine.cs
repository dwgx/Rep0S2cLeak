using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Overtone.Scripts;
using UnityEngine;

namespace LeastSquares.Overtone;

public class TTSEngine : MonoBehaviour
{
	private IntPtr _context;

	private readonly object _lock = new object();

	public bool Loaded { get; private set; }

	public bool Disposed { get; private set; }

	private void Awake()
	{
		lock (_lock)
		{
			_context = TTSNative.OvertoneStart();
			Loaded = true;
		}
	}

	public async Task<AudioClip> Speak(string text, TTSVoiceNative voice)
	{
		SpeechUnit[] array = SSMLPreprocessor.Preprocess(text);
		List<float> samples = new List<float>();
		TTSResult tTSResult = null;
		SpeechUnit[] array2 = array;
		foreach (SpeechUnit unit in array2)
		{
			tTSResult = await SpeakSamples(unit, voice);
			samples.AddRange(tTSResult.Samples);
		}
		return MakeClip(text, new TTSResult
		{
			Samples = samples.ToArray(),
			Channels = tTSResult.Channels,
			SampleRate = tTSResult.SampleRate
		});
	}

	private AudioClip MakeClip(string name, TTSResult result)
	{
		AudioClip audioClip = AudioClip.Create(name ?? string.Empty, result.Samples.Length, (int)result.Channels, (int)result.SampleRate, stream: false);
		audioClip.SetData(result.Samples, 0);
		return audioClip;
	}

	public async Task<TTSResult> SpeakSamples(SpeechUnit unit, TTSVoiceNative voice)
	{
		TaskCompletionSource<TTSResult> tcs = new TaskCompletionSource<TTSResult>();
		float[] samples = null;
		FixedString textPtr = new FixedString(unit.Text);
		try
		{
			TTSNative.OvertoneResult result = new TTSNative.OvertoneResult
			{
				Channels = 0u
			};
			await Task.Run(delegate
			{
				lock (_lock)
				{
					try
					{
						voice.AcquireReaderLock();
						if (Disposed || voice.Disposed)
						{
							samples = Array.Empty<float>();
							Debug.LogWarning("Couldn't process TTS. TTSEngine or TTSVoiceNative has been disposed.");
						}
						else
						{
							result = TTSNative.OvertoneText2Audio(_context, textPtr.Address, voice.Pointer);
							samples = PtrToSamples(result.Samples, result.LengthSamples);
							voice.ReleaseReaderLock();
						}
					}
					catch (Exception ex)
					{
						Debug.LogError("Error while processing TTS: " + ex.Message);
						tcs.SetException(ex);
					}
				}
			});
			tcs.SetResult(new TTSResult
			{
				Channels = result.Channels,
				SampleRate = result.SampleRate,
				Samples = samples
			});
			TTSNative.OvertoneFreeResult(result);
			return await tcs.Task;
		}
		finally
		{
			if (textPtr != null)
			{
				((IDisposable)textPtr).Dispose();
			}
		}
	}

	private float[] PtrToSamples(IntPtr int16Buffer, uint samplesLength)
	{
		float[] array = new float[samplesLength];
		short[] array2 = new short[samplesLength];
		Marshal.Copy(int16Buffer, array2, 0, (int)samplesLength);
		for (int i = 0; i < samplesLength; i++)
		{
			array[i] = (float)array2[i] / 32767f;
		}
		return array;
	}

	private void OnDestroy()
	{
		Dispose();
	}

	private void Dispose()
	{
		lock (_lock)
		{
			Disposed = true;
			if (_context != IntPtr.Zero)
			{
				TTSNative.OvertoneFree(_context);
			}
			Debug.Log("Successfully cleaned up TTS Engine");
		}
	}
}
