using System;
using System.Collections.Generic;
using System.Linq;
using Aims.Sdk;

namespace Aims.IisAgent
{
	public class DifferencePerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private Dictionary<NodeRef, StatPoint> _prewValues;

		private readonly IBasePerformanceCounterCollector _basePerformanceCounterCollector;

		public DifferencePerformanceCounterCollector(IBasePerformanceCounterCollector basePerformanceCounterCollector)
		{
			if (basePerformanceCounterCollector == null)
				throw new ArgumentNullException(nameof(basePerformanceCounterCollector));
			_basePerformanceCounterCollector = basePerformanceCounterCollector;
			_prewValues = new Dictionary<NodeRef, StatPoint>();
		}

		public StatPoint[] Collect()
		{
			Dictionary<NodeRef, StatPoint> newValues = CollectFromBase();
			var answer = MakeDifference(newValues, _prewValues);
			_prewValues = newValues;
			return answer
				.ToArray();
		}

		private Dictionary<NodeRef, StatPoint> CollectFromBase()
		{
			Dictionary<NodeRef, StatPoint> collectedValues = new Dictionary<NodeRef, StatPoint>();
			foreach(var statPoint in _basePerformanceCounterCollector.Collect())
			{
				try
				{
					collectedValues.Add(statPoint.NodeRef, statPoint);
				}
				catch(ArgumentException)
				{
					//fix multi instance
					collectedValues[statPoint.NodeRef].Value += statPoint.Value;
				}
			}
			return collectedValues;
		}

		private static IEnumerable<StatPoint> MakeDifference(IDictionary<NodeRef, StatPoint> newValues, IDictionary<NodeRef, StatPoint>  prewValues)
		{
			List<StatPoint> answer = new List<StatPoint>(newValues.Count);
			foreach(var keyValuePair in newValues)
			{
				StatPoint statPoint = new StatPoint
				{
					NodeRef = keyValuePair.Key,
					StatType = keyValuePair.Value.StatType,
					Time = keyValuePair.Value.Time,
					Value = keyValuePair.Value.Value,
				};
				try
				{
					statPoint.Value -= prewValues[keyValuePair.Key].Value;
				}
				catch(KeyNotFoundException)
				{
					statPoint.Value = 0.0;
				}
				answer.Add(statPoint);
			}
			return answer;
		}
	}
}