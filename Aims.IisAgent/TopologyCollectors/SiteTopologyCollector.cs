using Aims.IISAgent.NodeRefCreators;
using Aims.Sdk;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aims.IISAgent.TopologyCollectors
{
	class SiteTopologyCollector : ITopologyCollector
	{
		private readonly INodeRefCreator<Site> _siteNodeRefCreator = new SiteNodeRefCreator();
		private readonly AppPoolNodeRefCreator _appPoolNodeRefCreator = new AppPoolNodeRefCreator();

		private static readonly Dictionary<ObjectState, string> MapStatus =
			new Dictionary<ObjectState, string>
			{
						{ ObjectState.Started, AgentConstants.Status.Running},
						{ ObjectState.Starting, AgentConstants.Status.Running},
						{ ObjectState.Stopped, AgentConstants.Status.Stopped},
						{ ObjectState.Stopping, AgentConstants.Status.Stopped},
						{ ObjectState.Unknown, AgentConstants.Status.Undefined}
			};


		public Topology[] Collect()
		{
			using (var iisManager = new ServerManager())
			{
				return iisManager.Sites
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
					.ToArray();
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
	}
}
