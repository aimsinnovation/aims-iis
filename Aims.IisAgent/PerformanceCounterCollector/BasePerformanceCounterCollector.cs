using Aims.Sdk;

namespace Aims.IisAgent
{
	public interface IBasePerformanceCounterCollector
	{
		 StatPoint[] Collect();//TODO use IEnumerable<>
	}
}