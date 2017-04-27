using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Aims.Sdk;

using Microsoft.Web.Administration;

namespace Aims.FileCountAgent
{
    public class TopologyMonitor : MonitorBase<Node>
    {
        private readonly EnvironmentApi _api;
        private readonly EventLog _eventLog;
        private readonly NodeRef[] _nodeRefs;

		public TopologyMonitor(EnvironmentApi api, NodeRef[] nodeRefs, EventLog eventLog)
            : base((int)TimeSpan.FromMinutes(5).TotalMilliseconds, true)
        {
            _api = api;
            _nodeRefs = nodeRefs;
            _eventLog = eventLog;

			
		}

		protected override Node[] Collect()
		{
			var iisManager = new ServerManager();
			return iisManager.Sites
				.Select(s => new Node
				{
					NodeRef = new NodeRef
					{
						NodeType = AgentConstants.NodeType.Site,
						Parts = new Dictionary<string, string>{{AgentConstants.NodeRefPart.Id, s.Id.ToString()}}
					},
					Name = s.Name,
					Status = AgentConstants.Status.Undefined,

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