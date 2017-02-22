using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aims.Sdk;

namespace Aims.FileCountAgent
{
    public class Agent : IDisposable
    {
        private readonly TopologyMonitor _topologyMonitor;
        private readonly StatisticsMonitor _statisticsMonitor;

        public Agent(Uri apiAddress, Guid environmentId, string token, EventLog eventLog)
        {
            var api = new Api(apiAddress, token)
                .ForEnvironment(environmentId);
            var nodeRefs = Config.FilePaths
                .Select(p => new NodeRef
                {
                    NodeType = AgentConstants.NodeType.Path,
                    Parts = new Dictionary<string, string> { { "path", p } },
                })
                .ToArray();

            _statisticsMonitor = new StatisticsMonitor(api, nodeRefs, eventLog);
            _topologyMonitor = new TopologyMonitor(api, nodeRefs, eventLog);
        }

        public void Dispose()
        {
            _statisticsMonitor.Dispose();
            _topologyMonitor.Dispose();
        }
    }
}