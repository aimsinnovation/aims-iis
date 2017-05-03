using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aims.Sdk;

namespace Aims.FileCountAgent
{
	public class PerformanceCounterCollector
	{
		private readonly string _counterName;
		private readonly string _statType;
		private readonly Func<string, NodeRef> _instanceMapper;
		private readonly PerformanceCounterCategory _category;

		public PerformanceCounterCollector(string categogyName, string counterName, string statType, Func<string, NodeRef> instanceMapper)
		{
			_category = PerformanceCounterCategory
				.GetCategories()
				.Single(s => s.CategoryName.Equals(categogyName, StringComparison.InvariantCultureIgnoreCase));

			_counterName = counterName;
			_statType = statType;
			_instanceMapper = instanceMapper;
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
						NodeRef = _instanceMapper(c.InstanceName),
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