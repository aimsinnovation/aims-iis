using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Aims.IisAgent.NodeRefCreators;
using Aims.Sdk;
using System = Aims.Sdk.System;

namespace Aims.IisAgent
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
		//		StatType = AgentConstants.StatType.RequestsPerSec,
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
		//		StatType = AgentConstants.StatType.RequestsPerSec,
		//		NodeRefCreator = new AppPoolNodeRefCreator()
		//	},
		//};

		private readonly EnvironmentApi _api;
        private readonly EventLog _eventLog;

	    private readonly List<IBasePerformanceCounterCollector> _collectors;

		public StatisticsMonitor(EnvironmentApi api, EventLog eventLog, TimeSpan collectTimeSpan)
            : base((int)collectTimeSpan.TotalMilliseconds, true)
        {
            _api = api;
            _eventLog = eventLog;
	        _collectors = new List<IBasePerformanceCounterCollector>
	        {
				new AveragerPerformanceCounterCollector(
			        new DifferencePerformanceCounterCollector(
				        new PerformanceCounterCollector(
							CategoryNameW3Svc, "Total HTTP Requests Served", 
							AgentConstants.StatType.RequestsPerSec,
							new AppPoolNodeRefCreator()))),
		        new PerformanceCounterCollector(
			        CategoryNameW3Svc, "Total Threads", 
					AgentConstants.StatType.TotalThreads,
			        new AppPoolNodeRefCreator()),
		        new PerformanceCounterCollector(
			        CategoryNameW3Svc, "WebSocket Active Requests", 
					AgentConstants.StatType.ActiveRequests,
			        new AppPoolNodeRefCreator()),
					
		   //     new ServerPerformanceCounterCollector(
					//CategoryNameAspDotNet, "Requests Queued", 
					//AgentConstants.StatType.RequestQueued),
				
		        new AveragerPerformanceCounterCollector(
			        new DifferencePerformanceCounterCollector(
						new PerformanceCounterCollector(
							CategoryNameWebService, "Total Get Requests",
							AgentConstants.StatType.GetRequests,
							new SiteNodeRefCreator()))),
				
				new AveragerPerformanceCounterCollector(
			        new DifferencePerformanceCounterCollector(
						new PerformanceCounterCollector(
							CategoryNameWebService, "Total Post Requests", 
							AgentConstants.StatType.PostRequests,
							new SiteNodeRefCreator()))),
				new AveragerPerformanceCounterCollector(
					new DifferencePerformanceCounterCollector(
						new PerformanceCounterCollector(
							CategoryNameWebService, "Total Bytes Sent", 
							AgentConstants.StatType.BytesSentPerSec,
							new SiteNodeRefCreator()))),
				new AveragerPerformanceCounterCollector(
					new DifferencePerformanceCounterCollector(
						new PerformanceCounterCollector(
							CategoryNameWebService, "Total Bytes Received", 
							AgentConstants.StatType.BytesReceivedPerSec,
							new SiteNodeRefCreator()))),
				
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