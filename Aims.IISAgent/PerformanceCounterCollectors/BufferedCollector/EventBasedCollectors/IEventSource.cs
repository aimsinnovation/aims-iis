using System;

namespace Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector.EventBasedCollectors
{
	public interface IEventSource<T>
	{
		event EventHandler<GenericEventArgs<T>> EventOccured;
	}
}