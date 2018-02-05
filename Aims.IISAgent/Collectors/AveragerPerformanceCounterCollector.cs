using System;
using System.Linq;
using Aims.Sdk;

namespace Aims.IISAgent.Collectors
{
	//Calculate average per sec
	public class AveragerPerformanceCounterCollector : ICollector
	{
		private readonly ICollector _collector;
		private DateTime _lastCallTime;

		public AveragerPerformanceCounterCollector(ICollector collector)
		{
			if (collector == null)
				throw new ArgumentNullException(nameof(collector));

			_collector = collector;
			_lastCallTime = DateTime.UtcNow;
		}

		public StatPoint[] Collect()
		{
			var nowTime = DateTime.UtcNow;
			double collectPeriod = Math.Max(1, (nowTime - _lastCallTime).TotalSeconds);
			_lastCallTime = nowTime;

			return _collector.Collect()
				.Select(sp =>
				{
					sp.Value /= collectPeriod;
					return sp;
				})
				.ToArray();
		}
	}
}