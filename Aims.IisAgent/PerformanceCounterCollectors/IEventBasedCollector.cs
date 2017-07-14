using System;

namespace Aims.IISAgent.PerformanceCounterCollectors
{
	public interface IEventBasedCollector
	{
		event EventHandler<StatPointEventArgs> StatPointRecieved;
	}
}