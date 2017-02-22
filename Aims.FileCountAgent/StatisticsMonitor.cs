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
        private readonly Watcher[] _watchers;

        public StatisticsMonitor(EnvironmentApi api, IEnumerable<NodeRef> nodeRefs, EventLog eventLog)
            : base((int)TimeSpan.FromMinutes(1).TotalMilliseconds, true)
        {
            _api = api;
            _eventLog = eventLog;
            _watchers = nodeRefs.Select(r => new Watcher(r)).ToArray();

            Start();
        }

        public override void Dispose()
        {
            foreach (IDisposable disposable in _watchers)
            {
                disposable.Dispose();
            }
            base.Dispose();
        }

        protected override StatPoint[] Collect()
        {
            return _watchers.Select(w => w.Collect()).ToArray();
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