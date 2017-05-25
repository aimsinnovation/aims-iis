using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aims.IisAgent;
using Aims.IisAgent.NodeRefCreators;
using Aims.Sdk;
using Microsoft.Web.Administration;

namespace Aims.IisAgent
{
	public class TopologyMonitor : MonitorBase<Topology>
    {
        private readonly EnvironmentApi _api;
        private readonly EventLog _eventLog;

		private readonly INodeRefCreator<Site> _siteNodeRefCreator = new SiteNodeRefCreator();
		private readonly AppPoolNodeRefCreator _appPoolNodeRefCreator = new AppPoolNodeRefCreator();
		private readonly ServerNodeRefCreator _serverNodeRefCreator = new ServerNodeRefCreator();

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
				List<Topology> topologyList = new List<Topology>();
				topologyList.AddRange(iisManager.Sites
					.Select(site => new Topology
					{
						Node = CreateNodeFromSite(site),
						Links = site.Applications
							.Select(pool => new Link
							{
								From = _appPoolNodeRefCreator.CreateNodeRefFromObj(pool),
								To = _siteNodeRefCreator.CreateNodeRefFromObj(site),
								LinkType = LinkType.Hierarchy
							})
							.ToArray()
					})
					);
				topologyList.AddRange(iisManager.ApplicationPools
					.Select(pool => new Topology
						{
							Node = CreateNodeFromAppPool(pool),
							Links = new Link[]
							{
								new Link
								{
									From = _serverNodeRefCreator.CreateNodeRefFromObj(null),
									To = _appPoolNodeRefCreator.CreateNodeRefFromObj(pool),
									LinkType = LinkType.Hierarchy
								}
							}
						}));
				topologyList.Add(new Topology
				{
					Node = new Node
					{
						NodeRef = _serverNodeRefCreator.CreateNodeRefFromObj(null),
						Name = _serverNodeRefCreator.Name,
						Status = AgentConstants.Status.Running,
						CreationTime = DateTimeOffset.UtcNow,
						ModificationTime = DateTimeOffset.UtcNow,
						Properties = new Dictionary<string, string>(),
					},
					Links = new Link[0]
				});
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

	    private Node CreateNodeFromSite(Site site)
	    {
		    return new Node
		    {
			    NodeRef = _siteNodeRefCreator.CreateNodeRefFromObj(site),
			    Name = site.Name,
			    Status = MapStatus[site.State],
				CreationTime = DateTimeOffset.UtcNow,
			    ModificationTime = DateTimeOffset.UtcNow,
			    Properties = new Dictionary<string, string>(),
			};
		}

	    private Node CreateNodeFromAppPool(ApplicationPool pool)
	    {
		    return new Node
		    {
			    NodeRef = _appPoolNodeRefCreator.CreateNodeRefFromObj(pool),
			    Name = pool.Name,
			    Status = MapStatus[pool.State],
			    CreationTime = DateTimeOffset.UtcNow,
				ModificationTime = DateTimeOffset.UtcNow,
				Properties = new Dictionary<string, string>(),
			};
	    }
	}
}