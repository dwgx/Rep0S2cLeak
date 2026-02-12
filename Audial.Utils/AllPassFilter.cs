namespace Audial.Utils;

public class AllPassFilter : BufferedComponent
{
	public AllPassFilter(float delayLength, float gain)
		: base(delayLength, gain)
	{
	}

	public new float ProcessSample(int channel, float sample)
	{
		return base.ProcessSample(channel, sample) - gain * sample;
	}
}
