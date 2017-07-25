using System;
using System.Threading;

namespace Aims.IISAgent.PerformanceCounterCollectors.EventBasedCollectors
{
	public class TimerBasedCollector : IEventBasedCollector, IDisposable
	{
		private readonly IBasePerformanceCounterCollector _performanceCounterCollector;

		private readonly Timer _timer;

		public event EventHandler<StatPointEventArgs> StatPointRecieved;

		public TimerBasedCollector(IBasePerformanceCounterCollector performanceCounterCollector, TimeSpan period)
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
			if (StatPointRecieved == null) return;
			var values = _performanceCounterCollector.Collect();
			foreach (var value in values)
			{
				StatPointRecieved.Invoke(this, new StatPointEventArgs(value));
			}
		}

		public void Dispose()
		{
			_timer.Dispose();
		}
	}
}