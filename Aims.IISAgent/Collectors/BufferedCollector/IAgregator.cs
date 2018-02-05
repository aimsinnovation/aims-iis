using Aims.Sdk;

namespace Aims.IISAgent.Collectors.BufferedCollector
{
	public interface IAgregator
	{
		void Add(StatPoint point);

		StatPoint Get();
	}
}