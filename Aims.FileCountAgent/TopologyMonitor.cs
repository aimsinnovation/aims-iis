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
	public class TopologyMonitor : MonitorBase<Topology>
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

		protected override Topology[] Collect()
		{
			using(var iisManager = new ServerManager())
			{
				Dictionary<string, NodeRef> pools = iisManager.ApplicationPools
					.Select(CreateNodeFromAppPool)
					.ToDictionary(nr => nr.Name, nr => nr.NodeRef);
				List<Topology> topologyList = new List<Topology>();
				topologyList.AddRange(iisManager.Sites
					.Select(site => new Topology
					{
						Node = CreateNodeFromSite(site),
						Links = site.Applications
							.Select(pool => new Link
							{
								From = pools[pool.ApplicationPoolName],
								To = CreateNodeRefFromSite(site),
								LinkType = LinkType.Hierarchy
							})
							.ToArray()
					})
					);
				topologyList.AddRange(iisManager.ApplicationPools
					.Select(pool => new Topology
						{
							Node = CreateNodeFromAppPool(pool),
							Links = new Link[0]
						}));
				return topologyList.ToArray();
			}
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
	    private static Node CreateNodeFromSite(Site site)
	    {
		    return new Node
		    {
			    NodeRef = CreateNodeRefFromSite(site),
			    Name = site.Name,
			    Status = MapStatus[site.State]
		    };
		}

	    private static Node CreateNodeFromAppPool(ApplicationPool pool)
	    {
		    return new Node
		    {
			    NodeRef = CreateNodeRefNodeFromAppPool(pool),
			    Name = pool.Name,
			    Status = MapStatus[pool.State],
		    };
	    }

	    private static NodeRef CreateNodeRefFromSite(Site site)
	    {
		    return new NodeRef
		    {
			    NodeType = AgentConstants.NodeType.Site,
			    Parts = new Dictionary<string, string>
			    {
				    {AgentConstants.NodeRefPart.Id, site.Id.ToString()}
			    }
		    };
	    }

	    private static NodeRef CreateNodeRefNodeFromAppPool(ApplicationPool pool)
	    {
		    return new NodeRef
		    {
			    NodeType = AgentConstants.NodeType.AppPool,
			    Parts = new Dictionary<string, string> {{AgentConstants.NodeRefPart.Id, pool.Name}}
		    };
	    }

	}
}