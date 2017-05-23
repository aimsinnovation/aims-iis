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
		private readonly double _collectPeriod;

		private Dictionary<NodeRef, StatPoint> _prewValues;

		//передавайте имя счетчика общей
		public SitePerformanceCounterCollector(string categogyName, string counterTotalStatisticName, string statType, TimeSpan collectPeriod) 
			: base(categogyName, counterTotalStatisticName, statType)
		{
			_collectPeriod = collectPeriod.Seconds;
			_prewValues = new Dictionary<NodeRef, StatPoint>();
		}

		public override StatPoint[] Collect()
		{
			var newStatPoints = GetStatPoints();
			Dictionary<NodeRef, StatPoint> answer = new Dictionary<NodeRef, StatPoint>(); 
			foreach (var statPoint in newStatPoints)
			{
				answer.Add(statPoint.NodeRef, statPoint);
				try
				{
					answer[statPoint.NodeRef].Value = statPoint.Value - _prewValues[statPoint.NodeRef].Value;
				}
				catch (KeyNotFoundException e)
				{
				}
			}
			_prewValues = answer;
			return answer
				.Select(sp =>
					new StatPoint
					{
						NodeRef = sp.Value.NodeRef,
						StatType = sp.Value.StatType,
						Time = sp.Value.Time,
						Value = sp.Value.Value / _collectPeriod
					}
				)
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