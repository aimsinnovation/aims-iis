using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Aims.Sdk;

namespace Aims.IisAgent
{
	public class AppPoolDiffPerformanceCounterCollector : PerformanceCounterCollector
	{
		private DateTime _lastCallTime;

		private Dictionary<NodeRef, StatPoint> _prewValues;

		public AppPoolDiffPerformanceCounterCollector(string categogyName, string counterName, string statType)
			: base(categogyName, counterName, statType)
		{
			_lastCallTime = DateTime.UtcNow;
			_prewValues = new Dictionary<NodeRef, StatPoint>();
		}

		public override StatPoint[] Collect()
		{
			double collectPeriod = (DateTime.UtcNow - _lastCallTime).TotalSeconds;
			_lastCallTime = DateTime.UtcNow;
			var newStatPoints = GetStatPoints();
			Dictionary<NodeRef, StatPoint> answer = new Dictionary<NodeRef, StatPoint>();
			foreach (var statPoint in newStatPoints)
			{
				try
				{
					answer.Add(statPoint.NodeRef, statPoint);
				}
				catch (ArgumentException e)
				{
					//fix multiinstance per 1 app pool
					answer[statPoint.NodeRef].Value += statPoint.Value;
				}
			}
			foreach (var statPoint in answer)
			{
				try
				{
					answer[statPoint.Value.NodeRef].Value -= _prewValues[statPoint.Value.NodeRef].Value;
				}
				catch(KeyNotFoundException e)
				{
					//answer[statPoint.Value.NodeRef].Value *= collectPeriod;
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
						Value = sp.Value.Value / collectPeriod
					}
				)
				.ToArray();
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