namespace Aims.IISAgent.PerformanceCounterCollectors.MyPerformanceCounters
{
	public interface IMyPerformanceCounter
	{
		MyPerformanceCounterValue[] GetValues();
	}
}