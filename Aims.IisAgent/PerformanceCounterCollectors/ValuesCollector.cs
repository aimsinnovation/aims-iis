using System;
using System.Linq;
using Aims.IISAgent.NodeRefCreators;
using Aims.IISAgent.PerformanceCounterCollectors.ValuesProviders;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors
{
	public class ValuesCollector : IBasePerformanceCounterCollector
	{
		private readonly IValuesProvider _performanceCounter;

		private readonly INodeRefCreator _nodeRefCreator;

		private readonly string _statType;

		public ValuesCollector(IValuesProvider performanceCounter, INodeRefCreator nodeRefCreator, string statType)
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
					Value = value.Value,
					StatType = _statType
				})
				.ToArray();
		}
	}
}