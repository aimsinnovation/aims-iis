using System;
using System.Linq;
using Aims.IISAgent.NodeRefCreators;
using Aims.IISAgent.PerformanceCounterCollectors.MyPerformanceCounters;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors
{
	public class MyPerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private readonly IMyPerformanceCounter _performanceCounter;

		private readonly INodeRefCreator _nodeRefCreator;

		private readonly string _statType;

		public MyPerformanceCounterCollector(IMyPerformanceCounter performanceCounter, INodeRefCreator nodeRefCreator, string statType)
		{
			_performanceCounter = performanceCounter;
			_nodeRefCreator = nodeRefCreator;
			_statType = statType;
		}

		public StatPoint[] Collect()
		{
			return _performanceCounter
				.GetValues()
				.Select(value => new StatPoint
				{
					NodeRef = _nodeRefCreator.CreateFromInstanceName(value.InstanceName),
					Time = DateTimeOffset.UtcNow,
					Value = value.Value, //TODO think about it, but 9 9 9...
					StatType = _statType
				})
				.ToArray();
		}
	}
}