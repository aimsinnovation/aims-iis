using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aims.Sdk;
using Environment = System.Environment;

namespace Aims.FileCountAgent
{
	class NoInstancePerformanceCounterCollector : PerformanceCounterCollector
	{
		public NoInstancePerformanceCounterCollector(string categogyName, string counterName, string statType) 
			: base(categogyName, counterName, statType)
		{
		}

		public override StatPoint[] Collect()
		{
			using (var counter = new PerformanceCounter(Category.CategoryName, CounterName))
			{
				return new StatPoint[]
				{
					new StatPoint
					{
						NodeRef = InstanceName(),
						StatType = StatType,
						Time = DateTimeOffset.UtcNow,
						Value = counter.NextValue(),
					}
				};
			}
		}

		private static NodeRef InstanceName()
		{
			return new NodeRef
			{
				NodeType = AgentConstants.NodeType.Machine,
				Parts = new Dictionary<string, string>
				{
					{AgentConstants.NodeRefPart.MachineName, Environment.MachineName}
				}
			};
		}
	}
}