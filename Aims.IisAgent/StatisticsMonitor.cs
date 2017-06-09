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
		//public struct CounterSpecification
		//{
		//	public string CounterName { get; set; }
		//	public string StatType { get; set; }
		//	public INodeRefCreator NodeRefCreator { get; set; }
		//}

	    private const string CategoryNameW3Svc = "W3SVC_W3WP";
	    private const string CategoryNameAspDotNet = "ASP.NET";
	    private const string CategoryNameWebService = "Web Service";

		//private readonly CounterSpecification[] _categoryW3Svc = new CounterSpecification[]
		//{
		//	new CounterSpecification
		//	{
		//		CounterName = "Total HTTP Requests Served",
		//		StatType = AgentConstants.StatType.Requests,
		//		NodeRefCreator = new AppPoolNodeRefCreator()
		//	},
		//	new CounterSpecification
		//	{
		//		CounterName = "Total Threads",
		//		StatType = AgentConstants.StatType.TotalThreads,
		//		NodeRefCreator = new AppPoolNodeRefCreator()
		//	},
		//	new CounterSpecification
		//	{
		//		CounterName = "Total HTTP Requests Served",
		//		StatType = AgentConstants.StatType.Requests,
		//		NodeRefCreator = new AppPoolNodeRefCreator()
		//	},
		//};

		private readonly EnvironmentApi _api;
        private readonly EventLog _eventLog;

	    private readonly IBasePerformanceCounterCollector[] _collectors;

		public StatisticsMonitor(EnvironmentApi api, EventLog eventLog, TimeSpan collectTimeSpan)
            : base((int)collectTimeSpan.TotalMilliseconds, true)
        {
            _api = api;
            _eventLog = eventLog;
			_collectors = Initialize(eventLog,
				() => new DifferencePerformanceCounterCollector(
						new MultiInstancePerformanceCounterCollector(
							CategoryNameW3Svc, "Total HTTP Requests Served",
							AgentConstants.StatType.Requests,
							new AppPoolNodeRefCreator())),
				() => new MultiInstancePerformanceCounterCollector(
					CategoryNameW3Svc, "Total Threads",
					AgentConstants.StatType.TotalThreads,
					new AppPoolNodeRefCreator()),
				() => new MultiInstancePerformanceCounterCollector(
					CategoryNameW3Svc, "WebSocket Active Requests",
					AgentConstants.StatType.ActiveRequests,
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
					new SiteNodeRefCreator()))
					.ToArray();
		}

		private IEnumerable<IBasePerformanceCounterCollector> Initialize(EventLog log, params Func<IBasePerformanceCounterCollector>[] creators)
		{
			foreach(var creator in creators)
			{
				IBasePerformanceCounterCollector collector = null;
				try
				{
					 collector = creator();
				}
				catch(Exception ex)
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

        protected override StatPoint[] Collect()
        {
	        return _collectors
				.SelectMany(c => c.Collect())
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
	}
}