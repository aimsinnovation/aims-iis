using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Aims.Sdk;
using System = Aims.Sdk.System;

namespace Aims.FileCountAgent
{
	public partial class StatisticsMonitor : MonitorBase<StatPoint>
    {
        private readonly EnvironmentApi _api;
        private readonly EventLog _eventLog;

	    private readonly PerformanceCounterCollector[] _collectors;

		public StatisticsMonitor(EnvironmentApi api, EventLog eventLog)
            : base((int)TimeSpan.FromMinutes(1).TotalMilliseconds, true)
        {
            _api = api;
            _eventLog = eventLog;
			
	        const string categoryName = "W3SVC_W3WP";
			_collectors = new[]
	        {
		        new PerformanceCounterCollector(categoryName, "Requests / Sec", "aims.iis.requests-per-sec", MapInstanceName),
		        new PerformanceCounterCollector("ASP.NET", "Requests Queued", "aims.iis.requests-per-sec", MapInstanceName),
	        };
        }

	    private NodeRef MapInstanceName(string instanceName)
	    {
			Regex expression = new Regex("(_.+)");//it's not ass, it's regular expression
		    MatchCollection matches = expression.Matches(instanceName);
		    var appPoolName = matches[0].Value.Substring(1);
			return new NodeRef
		    {
			    NodeType = AgentConstants.NodeType.AppPool,
				Parts = new Dictionary<string, string>
				{
					{AgentConstants.NodeRefPart.InstanceName, appPoolName}
				}
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