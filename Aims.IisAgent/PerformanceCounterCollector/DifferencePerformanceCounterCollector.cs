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
			Dictionary<NodeRef, StatPoint> newValues = new Dictionary<NodeRef, StatPoint>();
			foreach(var statPoint in _basePerformanceCounterCollector.Collect())
			{
				try
				{
					newValues.Add(statPoint.NodeRef, statPoint);
				}
				catch(ArgumentException)
				{
					//fix multi instance
					newValues[statPoint.NodeRef].Value += statPoint.Value;
				}
			}
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
					statPoint.Value -= _prewValues[keyValuePair.Key].Value;
				}
				catch (KeyNotFoundException)
				{
					statPoint.Value = 0.0;
				}
				finally
				{
					answer.Add(statPoint);
				}
			}
			_prewValues = newValues;
			return answer
				.ToArray();
		}
	}
}