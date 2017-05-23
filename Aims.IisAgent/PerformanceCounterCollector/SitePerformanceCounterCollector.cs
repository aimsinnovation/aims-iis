using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Aims.Sdk;

namespace Aims.IisAgent
{
	class SitePerformanceCounterCollector : PerformanceCounterCollector
	{

		public SitePerformanceCounterCollector(string categogyName, string counterTotalStatisticName, string statType) 
			: base(categogyName, counterTotalStatisticName, statType)
		{
		}

		public override StatPoint[] Collect()
		{
			var newStatPoints = GetStatPoints();
			return newStatPoints
				.ToArray();
		}

		private static NodeRef MapInstanceName(string instanceName)
		{
			return new NodeRef
			{
				NodeType = AgentConstants.NodeType.Site,
				Parts = new Dictionary<string, string>
				{
					{AgentConstants.NodeRefPart.Id, instanceName}
				}
			};
		}

		private IEnumerable<StatPoint> GetStatPoints()
		{
			PerformanceCounter[] counters = Category.GetInstanceNames()
				.Select(instanceName => new PerformanceCounter(Category.CategoryName, CounterName, instanceName))
				.ToArray();
			try
			{
				return counters
					.Select(c => new StatPoint
					{
						NodeRef = MapInstanceName(c.InstanceName),
						StatType = StatType,
						Time = DateTimeOffset.UtcNow,
						Value = c.NextValue(),
					});
			}
			finally
			{
				foreach(var counter in counters)
				{
					counter.Dispose();
				}
			}
		}
	}
}