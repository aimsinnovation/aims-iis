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
			double collectPeriod = (DateTime.UtcNow - _lastCallTime).TotalSeconds;//TODO check value
			if (Math.Abs(collectPeriod) < 1.0)
				collectPeriod = 1.0;
			_lastCallTime = DateTime.UtcNow;
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