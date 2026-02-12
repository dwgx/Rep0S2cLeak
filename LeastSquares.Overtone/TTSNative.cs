using System;
using System.Runtime.InteropServices;

namespace LeastSquares.Overtone;

public static class TTSNative
{
	public struct OvertoneResult
	{
		public uint Channels;

		public uint SampleRate;

		public uint LengthSamples;

		public IntPtr Samples;
	}

	private const string OvertoneLibrary = "overtone";

	[DllImport("overtone", CallingConvention = CallingConvention.Cdecl, EntryPoint = "overtone_start")]
	public static extern IntPtr OvertoneStart();

	[DllImport("overtone", CallingConvention = CallingConvention.Cdecl, EntryPoint = "overtone_text_2_audio")]
	public static extern OvertoneResult OvertoneText2Audio(IntPtr ctx, IntPtr text, IntPtr voice);

	[DllImport("overtone", CallingConvention = CallingConvention.Cdecl, EntryPoint = "overtone_load_voice")]
	public static extern IntPtr OvertoneLoadVoice(IntPtr configBuffer, uint configBufferSize, IntPtr modelBuffer, uint modelBufferSize);

	[DllImport("overtone", CallingConvention = CallingConvention.Cdecl, EntryPoint = "overtone_set_speaker_id")]
	public static extern void OvertoneSetSpeakerId(IntPtr voice, long speakerId);

	[DllImport("overtone", CallingConvention = CallingConvention.Cdecl, EntryPoint = "overtone_free_voice")]
	public static extern void OvertoneFreeVoice(IntPtr voice);

	[DllImport("overtone", CallingConvention = CallingConvention.Cdecl, EntryPoint = "overtone_free_result")]
	public static extern void OvertoneFreeResult(OvertoneResult result);

	[DllImport("overtone", CallingConvention = CallingConvention.Cdecl, EntryPoint = "overtone_free")]
	public static extern void OvertoneFree(IntPtr ctx);
}
