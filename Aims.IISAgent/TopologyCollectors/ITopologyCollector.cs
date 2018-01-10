using System.Collections.Generic;

namespace Aims.IISAgent.TopologyCollectors
{
	public interface ITopologyCollector
	{
		IEnumerable<Topology> Collect();
	}
}