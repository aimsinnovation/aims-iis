using System;

namespace Aims.IISAgent.Collectors.BufferedCollector.EventBasedCollectors
{
	public interface IEventSource<T>
	{
		event EventHandler<GenericEventArgs<T>> EventOccured;
	}
}