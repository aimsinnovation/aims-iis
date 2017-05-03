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

	    private static readonly Dictionary<ObjectState, string> MapStatus =
		    new Dictionary<ObjectState, string>
		    {
			    { ObjectState.Started, AgentConstants.Status.Running},
			    { ObjectState.Starting, AgentConstants.Status.Running},
			    { ObjectState.Stopped, AgentConstants.Status.Stopped},
			    { ObjectState.Stopping, AgentConstants.Status.Stopped},
			    { ObjectState.Unknown, AgentConstants.Status.Undefined}
		    };

		public TopologyMonitor(EnvironmentApi api, EventLog eventLog)
            : base((int)TimeSpan.FromMinutes(5).TotalMilliseconds, true)
        {
            _api = api;
            _eventLog = eventLog;
		}

		protected override Node[] Collect()
		{
			using(var iisManager = new ServerManager())
			{
				return CollectNodes(iisManager.Sites, site => new Node
				{
					NodeRef = new NodeRef
					{
						NodeType = AgentConstants.NodeType.Site,
						Parts = new Dictionary<string, string>
						{
							{ AgentConstants.NodeRefPart.Id, site.Id.ToString() }
						}
					},
					Name = site.Name,
					Status = MapStatus[site.State]
				}).Union(CollectNodes(iisManager.ApplicationPools, pool => new Node
					{
						NodeRef = new NodeRef
						{
							NodeType = AgentConstants.NodeType.Site,
							Parts = new Dictionary<string, string> { { AgentConstants.NodeRefPart.Id, pool.Name.ToString() } }
						},
						Name = pool.Name,
						Status = MapStatus[pool.State],
					}))
					.ToArray();
			}
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

	    private static Node[] CollectNodes<TTopologyNode>(IEnumerable<TTopologyNode> nodes, Func<TTopologyNode, Node> mapTopologyNode)
	    {
		    return nodes.Select(mapTopologyNode).ToArray();
	    }
	}
}