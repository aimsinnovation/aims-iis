using System;
using System.Collections.Generic;
using System.Linq;

namespace Aims.IISAgent.TopologyCollectors
{
	public class FunnelTopologyCollector : ITopologyCollector
	{
		private readonly List<ITopologyCollector> _collectors;

		public FunnelTopologyCollector(params ITopologyCollector[] collectors)
		{
			if (collectors == null) throw new ArgumentNullException(nameof(collectors));
			_collectors = new List<ITopologyCollector>();
			foreach (var collector in collectors)
				_collectors.Add(collector);
		}

		public IEnumerable<Topology> Collect()
		{
			return _collectors.SelectMany(c => c.Collect());
		}
	}
}