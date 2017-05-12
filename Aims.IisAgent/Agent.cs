using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aims.IisAgent;
using Aims.Sdk;

namespace Aims.IisAgent
{
    public class Agent : IDisposable
    {
        private readonly TopologyMonitor _topologyMonitor;
        private readonly StatisticsMonitor _statisticsMonitor;

        public Agent(Uri apiAddress, Guid environmentId, string token, EventLog eventLog)
        {
			//TODO What is it?
            var api = new Api(apiAddress, token)
                .ForEnvironment(environmentId);


            _statisticsMonitor = new StatisticsMonitor(api, eventLog);
            _topologyMonitor = new TopologyMonitor(api, eventLog);

			_statisticsMonitor.Start();
			_topologyMonitor.Start();
        }

        public void Dispose()
        {
            _statisticsMonitor.Dispose();
            _topologyMonitor.Dispose();
        }
    }
}