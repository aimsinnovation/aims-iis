using Aims.Sdk;

namespace Aims.IISAgent.Collectors
{
	public interface ICollector
	{
		StatPoint[] Collect();
	}
}