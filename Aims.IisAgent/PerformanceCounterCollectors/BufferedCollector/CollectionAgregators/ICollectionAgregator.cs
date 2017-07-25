using System.Collections.Generic;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector.CollectionAgregators
{
	public interface ICollectionAgregator
	{
		StatPoint[] AgregateStatPoints(Queue<StatPoint> statPoints);
	}
}