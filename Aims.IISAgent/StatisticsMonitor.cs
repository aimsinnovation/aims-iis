using System;
using System.Collections.Generic;
using System.Linq;
using Aims.IISAgent.Collectors;
using Aims.IISAgent.Collectors.BufferedCollector;
using Aims.IISAgent.Collectors.BufferedCollector.EventBasedCollectors;
using Aims.IISAgent.Loggers;
using Aims.IISAgent.NodeRefCreators;
using Aims.IISAgent.Pipes;
using Aims.Sdk;
using MessageConverterToStatPoint = Aims.IISAgent.Collectors.BufferedCollector.MessageConverterToStatPoint;

namespace Aims.IISAgent
{
	public class StatisticsMonitor : MonitorBase<StatPoint>
	{
		private const string CategoryNameAspDotNet = "ASP.NET";
		private const string CategoryNameW3Svc = "W3SVC_W3WP";
		private const string CategoryNameWebService = "Web Service";

		private readonly EnvironmentApi _api;
		private readonly ICollector[] _collectors;
		private readonly ILogger _log;

		public StatisticsMonitor(EnvironmentApi api, ILogger log, TimeSpan collectTimeSpan, IEnumerable<Func<ICollector>> counterCreators = null)
			: base((int)collectTimeSpan.TotalMilliseconds, log)
		{
			_api = api;
			_log = log;
			_collectors = Initialize(counterCreators == null ? GetCounters(log) : counterCreators)
					.ToArray();
		}

		protected override StatPoint[] Collect()
		{
			return _collectors
				.SelectMany(c =>
				{
					try
					{
						return c.Collect();
					}
					catch (Exception ex)
					{
						_log.WriteError(String.Format("An error occurred while trying to collect stat points: {0}", ex));
						return new StatPoint[0];
					}
				}
				)
				.ToArray();
		}

		protected override void Send(StatPoint[] items)
		{
			try
			{
				_api.StatPoints.Send(items);
			}
			catch (Exception ex)
			{
				_log.WriteError(String.Format("An error occurred while trying to send stat points: {0}", ex));
			}
		}

		private static IAgregator Summator()
		{
			return new MemorylessSummator();
		}

		private static IAgregator Avarager()
		{
			return new MemorylessAvarager();
		}

		private static IEnumerable<Func<ICollector>> GetCounters(ILogger logger)
		{
			var appPoolNodeRefCreator = new AppPoolNodeRefCreator();
			var serverNodeRefCreator = new ServerNodeRefCreator();
			var siteNodeRefCreator = new SiteNodeRefCreator();
			var tracker = new MessageTracker(1000, logger);
			TimeSpan smallTickPeriod = TimeSpan.FromSeconds(2);
			TimeSpan hugeTickPeriod = TimeSpan.FromHours(1);
			var stTost = new ConverterSatatPointToStatPoint();

			yield return
				() => new DifferenceCollector(
					new ReIniterCollector(
						() => new MultiInstancePerformanceCounterCollector(
							CategoryNameW3Svc, "Total HTTP Requests Served",
							AgentConstants.StatType.Requests,
							appPoolNodeRefCreator),
						hugeTickPeriod,
						logger,
						PerformanceCounterFlush.FlushCache));

			yield return
				() => new ReIniterCollector(
					() => new MultiInstancePerformanceCounterCollector(
						CategoryNameW3Svc, "Total Threads",
						AgentConstants.StatType.TotalThreads,
						appPoolNodeRefCreator),
					hugeTickPeriod,
					logger,
					PerformanceCounterFlush.FlushCache);

			yield return () =>
				new BufferedCollector<StatPoint>(
					Avarager,
					new TimerSource(
						new ReIniterCollector(
							() => new NoInstancePerformanceCounterCollector(
								CategoryNameAspDotNet, "Requests Queued",
								AgentConstants.StatType.RequestQueued,
								serverNodeRefCreator),
							hugeTickPeriod,
							logger,
							PerformanceCounterFlush.FlushCache),
						smallTickPeriod),
					stTost);

			yield return () => new DifferenceCollector(
				new ReIniterCollector(
					() => new MultiInstancePerformanceCounterCollector(
						CategoryNameWebService, "Total Get Requests",
						AgentConstants.StatType.GetRequests,
						siteNodeRefCreator),
					hugeTickPeriod,
					logger,
					PerformanceCounterFlush.FlushCache));

			yield return () => new DifferenceCollector(
				new ReIniterCollector(
					() => new MultiInstancePerformanceCounterCollector(
						CategoryNameWebService, "Total Post Requests",
						AgentConstants.StatType.PostRequests,
						siteNodeRefCreator),
					hugeTickPeriod,
					logger,
					PerformanceCounterFlush.FlushCache));

			yield return
				() => new DifferenceCollector(
					new ReIniterCollector(
						() => new MultiInstancePerformanceCounterCollector(
							CategoryNameWebService, "Total Bytes Sent",
							AgentConstants.StatType.BytesSent,
							siteNodeRefCreator),
					hugeTickPeriod,
					logger,
					PerformanceCounterFlush.FlushCache));

			yield return
				() => new DifferenceCollector(
					new ReIniterCollector(
						() => new MultiInstancePerformanceCounterCollector(
							CategoryNameWebService, "Total Bytes Received",
							AgentConstants.StatType.BytesReceived,
							siteNodeRefCreator),
						hugeTickPeriod,
						logger,
						PerformanceCounterFlush.FlushCache));

			yield return
				() => new BufferedCollector<StatPoint>(
					Avarager,
					new TimerSource(
						new ReIniterCollector(
							() => new MultiInstancePerformanceCounterCollector(
								CategoryNameWebService, "Current Connections",
								AgentConstants.StatType.ActiveConnections,
								siteNodeRefCreator),
							hugeTickPeriod,
							logger,
							PerformanceCounterFlush.FlushCache),
						TimeSpan.FromSeconds(1)),
					stTost);

			yield return
				() => new ReIniterCollector(
					() => new MultiInstancePerformanceCounterCollector(
						CategoryNameW3Svc, "Active Requests",
						AgentConstants.StatType.ActiveRequests,
						appPoolNodeRefCreator),
					hugeTickPeriod,
					logger,
					PerformanceCounterFlush.FlushCache);

			yield return () =>
				new BufferedCollector<Message>(
					Summator, tracker, new MessageConverterToStatPoint(siteNodeRefCreator));

			tracker.Start();
		}

		private IEnumerable<ICollector> Initialize(IEnumerable<Func<ICollector>> creators)
		{
			foreach (var creator in creators)
			{
				ICollector collector = null;
				try
				{
					collector = creator();
				}
				catch (Exception ex)
				{
					_log.WriteError(string.Format("An error occurred while trying to create PerformanceCounterCollector: {0}", ex));
				}

				if (collector != null)
					yield return collector;
			}
		}
	}
}