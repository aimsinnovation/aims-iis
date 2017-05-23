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
			Dictionary<NodeRef, StatPoint> answer = new Dictionary<NodeRef, StatPoint>();
			foreach(var statPoint in _basePerformanceCounterCollector.Collect())
			{
				try
				{
					answer.Add(statPoint.NodeRef, statPoint);
				}
				catch(ArgumentException)
				{
					//fix multi instance
					answer[statPoint.NodeRef].Value += statPoint.Value;
				}
			}
			foreach(var keyValuePair in answer)
			{
				try
				{
					keyValuePair.Value.Value -= _prewValues[keyValuePair.Value.NodeRef].Value;
				}
				catch(KeyNotFoundException)
				{
				}
			}
			_prewValues = answer;
			return answer
				.Select(sp => sp.Value)
				.ToArray();
		}
	}
}