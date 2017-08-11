using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector
{
	internal class ConverterSatatPointToStatPoint : IConverterToStatPoint<StatPoint>
	{
		public StatPoint ConvertPoint(StatPoint item)
		{
			return item;
		}
	}
}