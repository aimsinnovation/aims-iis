using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Aims.Sdk;
using Environment = System.Environment;

namespace Aims.IisAgent
{
	class ServerPerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private readonly string _counterName;
		private readonly string _statType;
		private readonly PerformanceCounterCategory _category;

		public ServerPerformanceCounterCollector(string categogyName, string counterName, string statType)
		{
			_counterName = counterName;
			_statType = statType;
			_category = PerformanceCounterCategory
				.GetCategories()
				.SingleOrDefault(category => category.CategoryName.Equals(categogyName,
					StringComparison.InvariantCultureIgnoreCase));
			//if(_category == null)
			//{
			//	File.AppendAllText(@"C:\log.log", categogyName);
			//}
		}

		public StatPoint[] Collect()
		{
			if (_category == null)
				return new StatPoint[0];
			using (var counter = new PerformanceCounter(_category.CategoryName, _counterName))
			{
				return new StatPoint[]
				{
					new StatPoint
					{
						NodeRef = InstanceName(),
						StatType = _statType,
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
				NodeType = AgentConstants.NodeType.Server,
				Parts = new Dictionary<string, string>
				{
					{AgentConstants.NodeRefPart.MachineName, Environment.MachineName}
				}
			};
		}
	}
}