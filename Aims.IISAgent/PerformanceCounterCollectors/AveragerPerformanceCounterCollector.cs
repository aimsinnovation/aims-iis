using System;
using System.Linq;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors
{
	//Calculate average per sec
	public class AveragerPerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private readonly IBasePerformanceCounterCollector _basePerformanceCounterCollector;
		private DateTime _lastCallTime;

		public AveragerPerformanceCounterCollector(IBasePerformanceCounterCollector basePerformanceCounterCollector)
		{
			if (basePerformanceCounterCollector == null)
				throw new ArgumentNullException(nameof(basePerformanceCounterCollector));

			_basePerformanceCounterCollector = basePerformanceCounterCollector;
			_lastCallTime = DateTime.UtcNow;
		}

		public StatPoint[] Collect()
		{
			var nowTime = DateTime.UtcNow;
			double collectPeriod = Math.Max(1, (nowTime - _lastCallTime).TotalSeconds);
			_lastCallTime = nowTime;

			return _basePerformanceCounterCollector.Collect()
				.Select(sp =>
				{
					sp.Value /= collectPeriod;
					return sp;
				})
				.ToArray();
		}
	}
}