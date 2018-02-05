using System;
using System.Threading;
using Aims.IISAgent.Loggers;
using Aims.Sdk;

namespace Aims.IISAgent.Collectors
{
	public class ReIniterCollector : ICollector
	{
		private readonly Func<ICollector> _initializer;
		private ICollector _performanceCounter;
		private readonly ILogger _logger;
		private readonly Action _flushAction;
		private Timer _flushTimer;
		private readonly object _mutex = new object();
		private bool _errorOccurred = false;

		public ReIniterCollector(Func<ICollector> initializer, TimeSpan reinitSpan, ILogger logger, Action flushAction = null)
		{
			if (initializer == null)
				throw new ArgumentNullException(nameof(initializer));
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));
			_initializer = initializer;
			_logger = logger;
			_flushAction = flushAction;
			_flushTimer = new Timer(ReinitAction, new AutoResetEvent(false), reinitSpan, reinitSpan);
		}

		public StatPoint[] Collect()
		{
			try
			{
				lock (_mutex)
				{
					if (_errorOccurred)
						return new StatPoint[0];
					if (_performanceCounter == null)
						_performanceCounter = _initializer();
					return _performanceCounter.Collect();
				}
			}
			catch (Exception e)
			{
				lock (_mutex)
				{
					if (!_errorOccurred)
					{
						_logger.WriteWarning(e.ToString());
						_errorOccurred = true;
						_performanceCounter = null;
					}
				}
				return new StatPoint[0];
			}
		}

		private void ReinitAction(object state)
		{
			try
			{
				_flushAction?.Invoke();
				lock (_mutex)
				{
					if (_errorOccurred)
						_errorOccurred = false;
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}