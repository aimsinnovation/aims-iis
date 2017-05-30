using System;
using System.Diagnostics;
using Aims.Sdk;

namespace Aims.IISAgent
{
    public class Agent : IDisposable
    {
        private readonly TopologyMonitor _topologyMonitor;
        private readonly StatisticsMonitor _statisticsMonitor;

        public Agent(Uri apiAddress, Guid environmentId, string token, EventLog eventLog)
        {
            var api = new Api(apiAddress, token)
                .ForEnvironment(environmentId);

            _statisticsMonitor = new StatisticsMonitor(api, eventLog, TimeSpan.FromSeconds(Config.StatisticCollectPeriod));
            _topologyMonitor = new TopologyMonitor(api, eventLog, TimeSpan.FromSeconds(Config.TopologyUpdatePeriod));

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