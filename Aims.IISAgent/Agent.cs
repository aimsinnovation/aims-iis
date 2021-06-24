using System;
using System.Diagnostics;
using Aims.IISAgent.Loggers;
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
			var logger = new FileLogger();
			_statisticsMonitor = new StatisticsMonitor(api, logger, TimeSpan.FromSeconds(Config.StatisticCollectPeriod));
			_topologyMonitor = new TopologyMonitor(api, logger, TimeSpan.FromSeconds(Config.TopologyUpdatePeriod));

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