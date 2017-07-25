using System;

namespace Aims.IISAgent.PerformanceCounterCollectors.EventBasedCollectors
{
	public interface IEventBasedCollector
	{
		event EventHandler<StatPointEventArgs> StatPointRecieved;
	}
}