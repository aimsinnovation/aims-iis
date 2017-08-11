using System;
using System.Threading;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector.EventBasedCollectors
{
	public class TimerSource : IEventSource<StatPoint>, IDisposable
	{
		private readonly IBasePerformanceCounterCollector _performanceCounterCollector;

		private readonly Timer _timer;

		public TimerSource(IBasePerformanceCounterCollector performanceCounterCollector, TimeSpan period)
		{
			if (performanceCounterCollector == null)
				throw new ArgumentNullException(nameof(performanceCounterCollector));
			if (period == null)
				throw new ArgumentNullException(nameof(period));
			_performanceCounterCollector = performanceCounterCollector;
			_timer = new Timer(SendPoint, new AutoResetEvent(false), TimeSpan.FromSeconds(1), period);
		}

		private void SendPoint(object state)
		{
			if (EventOccured == null) return;
			var values = _performanceCounterCollector.Collect();
			foreach (var value in values)
			{
				EventOccured.Invoke(this, new GenericEventArgs<StatPoint>(value));
			}
		}

		public void Dispose()
		{
			_timer.Dispose();
		}

		public event EventHandler<GenericEventArgs<StatPoint>> EventOccured;
	}
}