using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aims.Sdk;

namespace Aims.FileCountAgent
{
    public partial class StatisticsMonitor : MonitorBase<StatPoint>
    {
        private readonly EnvironmentApi _api;
        private readonly EventLog _eventLog;

        public StatisticsMonitor(EnvironmentApi api, IEnumerable<NodeRef> nodeRefs, EventLog eventLog)
            : base((int)TimeSpan.FromMinutes(1).TotalMilliseconds, true)
        {
            _api = api;
            _eventLog = eventLog;

            Start();
        }

        protected override StatPoint[] Collect()
        {
            return new StatPoint[0];// _watchers.Select(w => w.Collect()).ToArray();
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