using System;

namespace Aims.IISAgent.PerformanceCounterCollectors.EventBasedCollectors
{
	public interface IEventSource<T>
	{
		event EventHandler<GenericEventArgs<T>> EventOccured;
	}
}