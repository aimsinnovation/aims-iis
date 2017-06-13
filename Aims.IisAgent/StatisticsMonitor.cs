using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Aims.IISAgent.NodeRefCreators;
using Aims.Sdk;
using System = Aims.Sdk.System;

namespace Aims.IISAgent
{
	public class StatisticsMonitor : MonitorBase<StatPoint>
	{

	    private const string CategoryNameW3Svc = "W3SVC_W3WP";
	    private const string CategoryNameAspDotNet = "ASP.NET";
	    private const string CategoryNameWebService = "Web Service";

		List<Func<IBasePerformanceCounterCollector>> creators = new List<Func<IBasePerformanceCounterCollector>>
		{
				() => new DifferencePerformanceCounterCollector(
					new MultiInstancePerformanceCounterCollector(
						CategoryNameW3Svc, "Total HTTP Requests Served",
						AgentConstants.StatType.Requests,
						new AppPoolNodeRefCreator())),
				() => new MultiInstancePerformanceCounterCollector(
					CategoryNameW3Svc, "Total Threads",
					AgentConstants.StatType.TotalThreads,
					new AppPoolNodeRefCreator()),

				() => new NoInstancePerformanceCounterCollector(
					CategoryNameAspDotNet, "Requests Queued",
					AgentConstants.StatType.RequestQueued,
					new ServerNodeRefCreator()),

				() => new DifferencePerformanceCounterCollector(
					new MultiInstancePerformanceCounterCollector(
						CategoryNameWebService, "Total Get Requests",
						AgentConstants.StatType.GetRequests,
						new SiteNodeRefCreator())),
				() => new DifferencePerformanceCounterCollector(
					new MultiInstancePerformanceCounterCollector(
						CategoryNameWebService, "Total Post Requests",
						AgentConstants.StatType.PostRequests,
						new SiteNodeRefCreator())),
				() => new DifferencePerformanceCounterCollector(
					new MultiInstancePerformanceCounterCollector(
						CategoryNameWebService, "Total Bytes Sent",
						AgentConstants.StatType.BytesSent,
						new SiteNodeRefCreator())),
				() => new DifferencePerformanceCounterCollector(
					new MultiInstancePerformanceCounterCollector(
						CategoryNameWebService, "Total Bytes Received",
						AgentConstants.StatType.BytesReceived,
						new SiteNodeRefCreator())),
				() => new MultiInstancePerformanceCounterCollector(
					CategoryNameWebService, "Current Connections",
					AgentConstants.StatType.ActiveConnections,
					new SiteNodeRefCreator()),

				() => new MultiInstancePerformanceCounterCollector(
						CategoryNameW3Svc, "Active Requests",
						AgentConstants.StatType.ActiveRequests,
						new AppPoolNodeRefCreator()),
		};

		private readonly EnvironmentApi _api;
        private readonly EventLog _eventLog;

	    private readonly IBasePerformanceCounterCollector[] _collectors;

		public StatisticsMonitor(EnvironmentApi api, EventLog eventLog, TimeSpan collectTimeSpan)
            : base((int)collectTimeSpan.TotalMilliseconds, true)
        {
            _api = api;
            _eventLog = eventLog;
			_collectors = Initialize(eventLog, creators)
					.ToArray();
		}

        protected override StatPoint[] Collect()
        {
			return _collectors
				.SelectMany(c => {
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