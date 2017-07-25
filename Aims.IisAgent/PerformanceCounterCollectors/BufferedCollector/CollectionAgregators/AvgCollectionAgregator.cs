using System;
using System.Collections.Generic;
using System.Linq;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector.CollectionAgregators
{
	public class AvgCollectionAgregator : ICollectionAgregator
	{
		public StatPoint[] AgregateStatPoints(Queue<StatPoint> statPoints)
		{
			var answer = statPoints.Aggregate((point1, point2) => new StatPoint
			{
				NodeRef = Equals(point1.NodeRef, point2.NodeRef)
					? point1.NodeRef
					: throw new InvalidOperationException("try to agregate different points"),
				StatType = Equals(point1.StatType, point2.StatType)
					? point1.StatType
					: throw new InvalidOperationException("try to agregate different points"),
				Time = point1.Time > point2.Time ? point1.Time : point2.Time,
				Value = point1.Value + point2.Value
			});
			answer.Value /= statPoints.Count();
			return new[] { answer };
		}
	}
}