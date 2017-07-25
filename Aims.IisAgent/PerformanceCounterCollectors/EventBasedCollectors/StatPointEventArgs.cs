using System;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors.EventBasedCollectors
{
	public class StatPointEventArgs : EventArgs
	{
		public StatPointEventArgs(StatPoint point)
		{
			Point = point;
		}

		public StatPoint Point { get; private set; }
	}
}