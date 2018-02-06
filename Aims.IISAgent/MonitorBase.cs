using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Aims.IISAgent.Loggers;

namespace Aims.IISAgent
{
	public abstract class MonitorBase<T> : IDisposable
	{
		private readonly int _intervalMilliseconds;

		private bool _isRunning = false;

		private readonly ILogger _logger;

		protected MonitorBase(int intervalMilliseconds, ILogger logger)
		{
			_intervalMilliseconds = intervalMilliseconds;
			_logger = logger;
		}

		public virtual void Dispose()
		{
			_isRunning = false;
		}

		protected abstract T[] Collect();

		protected abstract void Send(T[] items);

		public void Start()
		{
			var thread = new Thread(Run) { IsBackground = true, CurrentCulture = CultureInfo.InvariantCulture };
			_isRunning = true;
			thread.Start();
		}

		private void Run()
		{
			var stopwatch = new Stopwatch();
			while (_isRunning)
			{
				stopwatch.Restart();

				try
				{
					T[] items = Collect();
					if (items.Length > 0)
					{
						Send(items);
					}
				}
				catch (Exception e)
				{
					_logger.WriteError(e.ToString());
				}

				long timeout = _intervalMilliseconds - stopwatch.ElapsedMilliseconds;
				Thread.Sleep(timeout > 0 ? (int)timeout : 0);
			}
		}
	}
}