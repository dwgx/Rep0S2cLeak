namespace Audial.Utils;

public class CombFilter : BufferedComponent
{
	public CombFilter(float delayLength, float gain)
		: base(delayLength, gain)
	{
	}

	public new float ProcessSample(int channel, float sample)
	{
		return base.ProcessSample(channel, sample);
	}
}
