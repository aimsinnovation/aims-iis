using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Aims.IisAgent.NodeRefCreators;
using Aims.Sdk;

namespace Aims.IisAgent
{
	public class PerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private readonly string _counterName;
		private readonly string _statType;
		private readonly PerformanceCounterCategory _category;
		private readonly INodeRefCreator _nodeRefCreator;

		public PerformanceCounterCollector(string categogyName, string counterName, string statType,
			INodeRefCreator nodeRefCreator)
		{
			if (nodeRefCreator == null)
				throw new ArgumentNullException(nameof(nodeRefCreator));
			_counterName = counterName;
			_statType = statType;
			_nodeRefCreator = nodeRefCreator;
			_category = PerformanceCounterCategory
				.GetCategories()
				.Single(category => category.CategoryName.Equals(categogyName,
					StringComparison.InvariantCultureIgnoreCase));
			//if (Category == null)
			//{
			//	File.AppendAllText(@"C:\log.log", categogyName);
			//}
		}

		public StatPoint[] Collect()
		{
			PerformanceCounter[] counters = _category.GetInstanceNames()
				.Select(instanceName => new PerformanceCounter(_category.CategoryName, _counterName, instanceName))
				.ToArray();
			try
			{
				return counters
					.Select(c => new StatPoint
					{
						NodeRef = _nodeRefCreator.CreateFromInstanceName(c.InstanceName),
						StatType = _statType,
						Time = DateTimeOffset.UtcNow,
						Value = c.NextValue(),
					})
					.ToArray();
			}
			finally
			{
				foreach (var counter in counters)
				{
					counter.Dispose();
				}
			}
		}
	}
}