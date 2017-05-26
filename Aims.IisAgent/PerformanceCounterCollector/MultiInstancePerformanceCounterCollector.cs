using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Aims.IISAgent.NodeRefCreators;
using Aims.Sdk;

namespace Aims.IISAgent
{
	public class MultiInstancePerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private readonly string _counterName;
		private readonly string _statType;
		private readonly PerformanceCounterCategory _category;
		private readonly INodeRefCreator _nodeRefCreator;

		public MultiInstancePerformanceCounterCollector(string categogyName, string counterName, string statType,
			INodeRefCreator nodeRefCreator)
		{
			if (nodeRefCreator == null)
				throw new ArgumentNullException(nameof(nodeRefCreator));
			_counterName = counterName;
			_statType = statType;
			_nodeRefCreator = nodeRefCreator;
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