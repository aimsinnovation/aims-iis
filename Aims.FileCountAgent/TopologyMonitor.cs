using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aims.Sdk;

namespace Aims.FileCountAgent
{
    public class TopologyMonitor : MonitorBase<Node>
    {
        private readonly EnvironmentApi _api;
        private readonly EventLog _eventLog;
        private readonly NodeRef[] _nodeRefs;

        public TopologyMonitor(EnvironmentApi api, NodeRef[] nodeRefs, EventLog eventLog)
            : base((int)TimeSpan.FromMinutes(5).TotalMilliseconds)
        {
            _api = api;
            _nodeRefs = nodeRefs;
            _eventLog = eventLog;
        }

        protected override Node[] Collect()
        {
            return _nodeRefs
                .Select(r => new Node
                {
                    NodeRef = r,
                    Name = r.Parts[AgentConstants.NodeRefPart.Path],
                    ModificationTime = DateTimeOffset.Now,
                    Status = AgentConstants.Status.Undefined,
                    Properties = new Dictionary<string, string>(),
                })
                .ToArray();
        }

        protected override void Send(Node[] items)
        {
            try
            {
                _api.Nodes.Send(items);
            }
            catch (Exception ex)
            {
                if (Config.VerboseLog)
                {
                    _eventLog.WriteEntry(String.Format("An error occurred while trying to send topology: {0}", ex),
                        EventLogEntryType.Error);
                }
            }
        }
    }
}