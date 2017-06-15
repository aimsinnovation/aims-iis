using Aims.Sdk;
using Microsoft.Web.Administration;
using Aims.IISAgent.NodeRefCreators;

namespace Aims.IISAgent.TopologyCollectors
{
	interface ITopologyCollector
	{
		Topology[] Collect();
	}
}
