using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Aims.Sdk;
using System = Aims.Sdk.System;

namespace Aims.IisAgent
{
	public class StatisticsMonitor : MonitorBase<StatPoint>
    {
	    private const string CategoryNameW3Svc = "W3SVC_W3WP";
	    private const string CategoryNameAspDotNet = "ASP.NET";
	    private const string CategoryNameWebService = "Web Service";

		private readonly EnvironmentApi _api;
        private readonly EventLog _eventLog;

	    private readonly PerformanceCounterCollector[] _collectors;

		public StatisticsMonitor(EnvironmentApi api, EventLog eventLog)
            : base((int)TimeSpan.FromMinutes(1).TotalMilliseconds, true)
        {
            _api = api;
            _eventLog = eventLog;

			_collectors = new PerformanceCounterCollector[]
	        {
				new AppPoolPerformanceCounterCollector(CategoryNameW3Svc, "Requests / Sec", AgentConstants.StatType.RequestsPerSec),
				new AppPoolPerformanceCounterCollector(CategoryNameW3Svc, "Total Threads", AgentConstants.StatType.TotalThreads), 
				new AppPoolPerformanceCounterCollector(CategoryNameW3Svc, "WebSocket Active Requests", AgentConstants.StatType.ActiveRequests),
				new ServerPerformanceCounterCollector(CategoryNameAspDotNet, "Requests Queued", AgentConstants.StatType.RequestQueued),
		        new SitePerformanceCounterCollector(CategoryNameWebService, "Get Requests/sec", AgentConstants.StatType.GetRequests),
		        new SitePerformanceCounterCollector(CategoryNameWebService, "Post Requests/sec", AgentConstants.StatType.PostRequests),
		        new SitePerformanceCounterCollector(CategoryNameWebService, "Bytes Sent/sec", AgentConstants.StatType.BytesSentPerSec),
		        new SitePerformanceCounterCollector(CategoryNameWebService, "Bytes Received/sec", AgentConstants.StatType.PostRequests),
			};
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