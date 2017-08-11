using System;
using System.Diagnostics;
using System.Linq;
using Aims.IISAgent.Loggers;
using Aims.IISAgent.NodeRefCreators;
using Aims.Sdk;
using Aims.IISAgent.TopologyCollectors;

namespace Aims.IISAgent
{
	public class TopologyMonitor : MonitorBase<Topology>
	{
		private readonly EnvironmentApi _api;
		private readonly ILogger _log;

		private readonly ITopologyCollector[] _toplogyCollectors;

		public TopologyMonitor(EnvironmentApi api, ILogger log, TimeSpan period)
			: base((int)period.TotalMilliseconds, log)
		{
			_api = api;
			_log = log;

			var serverNodeRefCreator = new ServerNodeRefCreator();
			var appPoolNodeRefCreator = new AppPoolNodeRefCreator();
			var siteNodeRefCreator = new SiteNodeRefCreator();

			_toplogyCollectors = new ITopologyCollector[]
			{
				new AppPoolTopologyCollector(appPoolNodeRefCreator, serverNodeRefCreator),
				new SiteTopologyCollector(siteNodeRefCreator, appPoolNodeRefCreator, _log),
				new ServerTopologyCollector(serverNodeRefCreator),
				new SslCertificateTopologyCollector(siteNodeRefCreator,
					new SslCertificateNodeRefCreator(),
					TimeSpan.FromDays(Config.SslCertFirstWarning),
					TimeSpan.FromDays(Config.SslCertSecondWarning)),
			};
		}

		protected override Topology[] Collect()
		{
			return _toplogyCollectors
				.SelectMany(tc => tc.Collect())
				.ToArray();
		}

		protected override void Send(Topology[] items)
		{
			try
			{
				_api.Nodes.Send(items
					.Select(item => item.Node)
					.ToArray());
				_api.Links.Send(items
					.SelectMany(item => item.Links)
					.ToArray());
			}
			catch (Exception ex)
			{
				_log.WriteError(String.Format("An error occurred while trying to send topology: {0}", ex));
			}
		}
	}
}