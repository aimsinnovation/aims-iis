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

	    private readonly List<PerformanceCounterCollector> _collectors;

		public StatisticsMonitor(EnvironmentApi api, EventLog eventLog, TimeSpan collectTimeSpan)
            : base((int)collectTimeSpan.TotalMilliseconds, true)
        {
            _api = api;
            _eventLog = eventLog;
			_collectors = new List<PerformanceCounterCollector>();

			try
	        {
		        _collectors.AddRange(
			        new PerformanceCounterCollector[]
			        {
				        new AppPoolDiffPerformanceCounterCollector(CategoryNameW3Svc, "Total HTTP Requests Served",
					        AgentConstants.StatType.RequestsPerSec),
				        new AppPoolPerformanceCounterCollector(CategoryNameW3Svc, "Total Threads",
					        AgentConstants.StatType.TotalThreads),
				        new AppPoolPerformanceCounterCollector(CategoryNameW3Svc, "WebSocket Active Requests",
					        AgentConstants.StatType.ActiveRequests),
			        });
			}
	        catch (Exception ex)
	        {
		        if(Config.VerboseLog)
		        {
			        _eventLog.WriteEntry(String.Format(
				        "An error occurred while trying to create PerformanceCounterCollector: {0}", ex));
		        }
	        }

	        try
	        {
		        _collectors.AddRange(
			        new PerformanceCounterCollector[]
			        {
				        new ServerPerformanceCounterCollector(CategoryNameAspDotNet, "Requests Queued", AgentConstants.StatType.RequestQueued),
			        });
	        }
	        catch(Exception ex)
	        {
		        if(Config.VerboseLog)
		        {
			        _eventLog.WriteEntry(String.Format(
				        "An error occurred while trying to create PerformanceCounterCollector: {0}", ex));
		        }

	        }

	        try
	        {
		        _collectors.AddRange(
			        new PerformanceCounterCollector[]
			        {
				        new SitePerformanceCounterCollector(CategoryNameWebService, "Total Get Requests", AgentConstants.StatType.GetRequests, collectTimeSpan),
				        new SitePerformanceCounterCollector(CategoryNameWebService, "Total Post Requests", AgentConstants.StatType.PostRequests, collectTimeSpan),
				        new SitePerformanceCounterCollector(CategoryNameWebService, "Total Bytes Sent", AgentConstants.StatType.BytesSentPerSec, collectTimeSpan),
				        new SitePerformanceCounterCollector(CategoryNameWebService, "Total Bytes Received", AgentConstants.StatType.BytesReceivedPerSec, collectTimeSpan),
			        });
	        }
	        catch(Exception ex)
	        {
		        if(Config.VerboseLog)
		        {
			        _eventLog.WriteEntry(String.Format(
				        "An error occurred while trying to create PerformanceCounterCollector: {0}", ex));
		        }

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