using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Aims.IISAgent.Loggers;
using Aims.IISAgent.MyExceptions;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors
{
	public class ReIniterPerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private readonly Func<IBasePerformanceCounterCollector> _initializer;
		private IBasePerformanceCounterCollector _performanceCounter;
		private readonly ILogger _logger;
		private static Timer _flushTimer = new Timer(FlushCache, new AutoResetEvent(false), TimeSpan.FromSeconds(1), TimeSpan.FromHours(1));
		private static readonly object MutexWarningIsLogged = new object();
		private static bool _warningIsLogged = false;

		public ReIniterPerformanceCounterCollector(Func<IBasePerformanceCounterCollector> initializer, ILogger logger)
		{
			if (initializer == null)
				throw new ArgumentNullException(nameof(initializer));
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));
			_initializer = initializer;
			_logger = logger;
		}

		public StatPoint[] Collect()
		{
			try
			{
				_performanceCounter = _initializer();
			}
			catch (CategoryNotFoundException e)
			{
				lock (MutexWarningIsLogged)
				{
					if (_warningIsLogged) return new StatPoint[0];
					_warningIsLogged = true;
				}
				_logger.WriteWarning(e.ToString());
				return new StatPoint[0];
			}
			try
			{
				return _performanceCounter.Collect();
			}
			catch (Exception e)
			{
				_performanceCounter = null;
				_logger.WriteWarning(e.ToString());
				return new StatPoint[0];
			}
		}

		public static void FlushCache(object state)
		{
			try
			{
				lock (MutexWarningIsLogged)
				{
					_warningIsLogged = false;
				}
				var assembly = Assembly.GetAssembly(typeof(PerformanceCounterCategory));
				var type = assembly.GetType("System.Diagnostics.PerformanceCounterLib");
				var method = type.GetMethod("CloseAllTables", BindingFlags.NonPublic | BindingFlags.Static);
				method.Invoke(null, null);
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}