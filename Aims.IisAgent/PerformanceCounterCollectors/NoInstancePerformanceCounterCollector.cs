using System;
using System.Diagnostics;
using System.Linq;
using Aims.IISAgent.NodeRefCreators;
using Aims.Sdk;

namespace Aims.IISAgent
{
	internal class NoInstancePerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private readonly PerformanceCounterCategory _category;
		private readonly string _counterName;
		private readonly INodeRefCreator _nodeRefCreator;
		private readonly string _statType;

		public NoInstancePerformanceCounterCollector(string categogyName, string counterName, string statType,
			INodeRefCreator nodeRefCreator)
		{
			if (nodeRefCreator == null)
				throw new ArgumentNullException();

			_nodeRefCreator = nodeRefCreator;
			_counterName = counterName;
			_statType = statType;
			_category = PerformanceCounterCategory
				.GetCategories()
				.SingleOrDefault(category => category.CategoryName.Equals(categogyName,
					StringComparison.InvariantCultureIgnoreCase));
			if (_category == null)
			{
				throw new MyExceptions.CategoryNotFoundException(categogyName);
			}
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
						NodeRef = _nodeRefCreator.CreateFromInstanceName(null),
						StatType = _statType,
						Time = DateTimeOffset.UtcNow,
						Value = counter.NextValue(),
					}
				};
			}
		}
	}
}