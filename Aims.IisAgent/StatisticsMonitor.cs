using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aims.IISAgent.NodeRefCreators;
using Aims.IISAgent.PerformanceCounterCollectors;
using Aims.Sdk;

namespace Aims.IISAgent
{
	public class StatisticsMonitor : MonitorBase<StatPoint>
	{
		private const string CategoryNameAspDotNet = "ASP.NET";
		private const string CategoryNameW3Svc = "W3SVC_W3WP";
		private const string CategoryNameWebService = "Web Service";

		private readonly EnvironmentApi _api;
		private readonly IBasePerformanceCounterCollector[] _collectors;
		private readonly EventLog _eventLog;

		public StatisticsMonitor(EnvironmentApi api, EventLog eventLog, TimeSpan collectTimeSpan)
			: base((int)collectTimeSpan.TotalMilliseconds)
		{
			_api = api;
			_eventLog = eventLog;
			_collectors = Initialize(eventLog, GetCounters())
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
						if (Config.VerboseLog)
						{
							_eventLog.WriteEntry(String.Format("An error occurred while trying to collect stat points: {0}", ex),
								EventLogEntryType.Error);
						}
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
				if (Config.VerboseLog)
				{
					_eventLog.WriteEntry(String.Format("An error occurred while trying to send stat points: {0}", ex),
						EventLogEntryType.Error);
				}
			}
		}

		private IEnumerable<Func<IBasePerformanceCounterCollector>> GetCounters()
		{
			var appPoolNodeRefCreator = new AppPoolNodeRefCreator();
			var serverNodeRefCreator = new ServerNodeRefCreator();
			var siteNodeRefCreator = new SiteNodeRefCreator();

			yield return () => new DifferencePerformanceCounterCollector(
				new MultiInstancePerformanceCounterCollector(
					CategoryNameW3Svc, "Total HTTP Requests Served",
					AgentConstants.StatType.Requests,
					appPoolNodeRefCreator));

			yield return () => new MultiInstancePerformanceCounterCollector(
				CategoryNameW3Svc, "Total Threads",
				AgentConstants.StatType.TotalThreads,
				appPoolNodeRefCreator);

			yield return () => new NoInstancePerformanceCounterCollector(
				CategoryNameAspDotNet, "Requests Queued",
				AgentConstants.StatType.RequestQueued,
				serverNodeRefCreator);

			yield return () => new DifferencePerformanceCounterCollector(
				new MultiInstancePerformanceCounterCollector(
					CategoryNameWebService, "Total Get Requests",
					AgentConstants.StatType.GetRequests,
					siteNodeRefCreator));

			yield return () => new DifferencePerformanceCounterCollector(
				new MultiInstancePerformanceCounterCollector(
					CategoryNameWebService, "Total Post Requests",
					AgentConstants.StatType.PostRequests,
					siteNodeRefCreator));

			yield return () => new DifferencePerformanceCounterCollector(
				new MultiInstancePerformanceCounterCollector(
					CategoryNameWebService, "Total Bytes Sent",
					AgentConstants.StatType.BytesSent,
					siteNodeRefCreator));

			yield return () => new DifferencePerformanceCounterCollector(
				new MultiInstancePerformanceCounterCollector(
					CategoryNameWebService, "Total Bytes Received",
					AgentConstants.StatType.BytesReceived,
					siteNodeRefCreator));

			yield return () => new MultiInstancePerformanceCounterCollector(
				CategoryNameWebService, "Current Connections",
				AgentConstants.StatType.ActiveConnections,
				siteNodeRefCreator);

			yield return () => new MultiInstancePerformanceCounterCollector(
				CategoryNameW3Svc, "Active Requests",
				AgentConstants.StatType.ActiveRequests,
				appPoolNodeRefCreator);
		}

		private IEnumerable<IBasePerformanceCounterCollector> Initialize(EventLog log, IEnumerable<Func<IBasePerformanceCounterCollector>> creators)
		{
			foreach (var creator in creators)
			{
				IBasePerformanceCounterCollector collector = null;
				try
				{
					collector = creator();
				}
				catch (Exception ex)
				{
					if (Config.VerboseLog)
					{
						_eventLog.WriteEntry(String.Format("An error occurred while trying to create PerformanceCounterCollector: {0}", ex),
							EventLogEntryType.Error);
					}
				}

				if (collector != null)
					yield return collector;
			}
		}
	}
}