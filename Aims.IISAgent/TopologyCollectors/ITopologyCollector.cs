using System.Collections.Generic;

namespace Aims.IISAgent.TopologyCollectors
{
	interface ITopologyCollector
	{
		IEnumerable<Topology> Collect();
	}
}
