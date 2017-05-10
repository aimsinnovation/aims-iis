using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Aims.Sdk;
using System = Aims.Sdk.System;

namespace Aims.FileCountAgent
{
	public class StatisticsMonitor : MonitorBase<StatPoint>
    {
	    private const string CategoryNameW3Svc = "W3SVC_W3WP";
	    private const string CategoryNameAspDotNet = "ASP.NET";

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
				new InstancePerformanceCounterCollector(CategoryNameW3Svc, "Requests / Sec", AgentConstants.StatType.RequestsPerSec), 
				new InstancePerformanceCounterCollector(CategoryNameW3Svc, "Total Threads", AgentConstants.StatType.TotalThreads), 
				new InstancePerformanceCounterCollector(CategoryNameW3Svc, "WebSocket Active Requests", AgentConstants.StatType.ActiveRequest),
		        //new InstancePerformanceCounterCollector("ASP.NET Applications", "Sessions Active", AgentConstants.StatType.ActiveRequest),
				new NoInstancePerformanceCounterCollector(CategoryNameAspDotNet, "Requests Queued", AgentConstants.StatType.RequestQueued), 
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