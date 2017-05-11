using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Aims.Sdk;

namespace Aims.FileCountAgent
{
	class AppPoolPerformanceCounterCollector : PerformanceCounterCollector
	{
		public AppPoolPerformanceCounterCollector(string categogyName, string counterName, string statType)
			: base(categogyName, counterName, statType)
		{
		}

		public override StatPoint[] Collect()
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
					})
					.ToArray();
			}
			finally
			{
				foreach(var counter in counters)
				{
					counter.Dispose();
				}
			}
		}

		private static NodeRef MapInstanceName(string instanceName)
		{
			Regex expression = new Regex("(_.+)");
			MatchCollection matches = expression.Matches(instanceName);
			var appPoolName = matches[0].Value.Substring(1);
			return new NodeRef
			{
				NodeType = AgentConstants.NodeType.AppPool,
				Parts = new Dictionary<string, string>
				{
					{AgentConstants.NodeRefPart.InstanceName, appPoolName}
				}
			};
		}
	}
}