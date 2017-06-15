using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aims.IISAgent;
using Aims.IISAgent.NodeRefCreators;
using Aims.Sdk;
using Microsoft.Web.Administration;
using Aims.IISAgent.TopologyCollectors;

namespace Aims.IISAgent
{
	public class TopologyMonitor : MonitorBase<Topology>
    {
        private readonly EnvironmentApi _api;
        private readonly EventLog _eventLog;

		private readonly ServerNodeRefCreator _serverNodeRefCreator = new ServerNodeRefCreator();

		ITopologyCollector[] _toplogyCollectors = new ITopologyCollector[]
			{
				new AppPoolTopologyCollector(),
				new SiteTopologyCollector(),
				new ServerTopologyCollector(),
			};

		public TopologyMonitor(EnvironmentApi api, EventLog eventLog, TimeSpan period)
            : base((int)period.TotalMilliseconds, true)
        {
            _api = api;
            _eventLog = eventLog;
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
                if (Config.VerboseLog)
                {
                    _eventLog.WriteEntry(String.Format("An error occurred while trying to send topology: {0}", ex),
                        EventLogEntryType.Error);
                }
            }
        }


	}
}