using Aims.Sdk;
using System;
using System.Linq;

namespace Aims.IisAgent
{
	//Calculate average per sec 
	public class AveragerPerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private DateTime _lastCallTime;

		private readonly IBasePerformanceCounterCollector _basePerformanceCounterCollector;

		public AveragerPerformanceCounterCollector(IBasePerformanceCounterCollector basePerformanceCounterCollector)
		{
			if(basePerformanceCounterCollector == null)
				throw new ArgumentNullException(nameof(basePerformanceCounterCollector));
			_basePerformanceCounterCollector = basePerformanceCounterCollector;
			_lastCallTime = DateTime.UtcNow;
		}

		public StatPoint[] Collect()
		{
			var nowTime = DateTime.UtcNow;
			double collectPeriod = (nowTime - _lastCallTime).TotalSeconds;
			if (Math.Abs(collectPeriod) < 1.0)
				collectPeriod = 1.0;
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