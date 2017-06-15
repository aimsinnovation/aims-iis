using Aims.Sdk;

namespace Aims.IISAgent
{
	public interface IBasePerformanceCounterCollector
	{
		 StatPoint[] Collect();//TODO use IEnumerable<>
	}
}