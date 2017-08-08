using System;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors
{
	public interface IBasePerformanceCounterCollector
	{
		StatPoint[] Collect();
	}
}