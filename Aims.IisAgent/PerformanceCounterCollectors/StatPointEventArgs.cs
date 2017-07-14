using System;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors
{
	public class StatPointEventArgs : EventArgs
	{
		public StatPoint Point { get; private set; }

		public StatPointEventArgs(StatPoint point)
		{
			Point = point;
		}
	}
}