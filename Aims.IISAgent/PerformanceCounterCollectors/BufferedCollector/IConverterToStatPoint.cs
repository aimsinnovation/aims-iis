using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector
{
	public interface IConverterToStatPoint<in T>
	{
		StatPoint ConvertPoint(T item);
	}
}