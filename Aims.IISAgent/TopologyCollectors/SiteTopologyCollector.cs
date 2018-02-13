using System;
using System.Collections.Generic;
using System.Linq;
using Aims.IISAgent.NodeRefCreators;
using Aims.Sdk;
using Microsoft.Web.Administration;

namespace Aims.IISAgent.TopologyCollectors
{
	internal class SiteTopologyCollector : ITopologyCollector
	{
		private static readonly Dictionary<ObjectState, string> MapStatus =
			new Dictionary<ObjectState, string>
			{
						{ ObjectState.Started, AgentConstants.Status.Running},
						{ ObjectState.Starting, AgentConstants.Status.Running},
						{ ObjectState.Stopped, AgentConstants.Status.Stopped},
						{ ObjectState.Stopping, AgentConstants.Status.Stopped},
						{ ObjectState.Unknown, AgentConstants.Status.Undefined}
			};

		private readonly INodeRefCreator<Application> _appPoolNodeRefCreator;
		private readonly INodeRefCreator<Site> _siteNodeRefCreator;

		public SiteTopologyCollector(INodeRefCreator<Site> siteNodeRefCreator, INodeRefCreator<Application> appPoolNodeRefCreator)
		{
			_siteNodeRefCreator = siteNodeRefCreator;
			_appPoolNodeRefCreator = appPoolNodeRefCreator;
		}

		public IEnumerable<Topology> Collect()
		{
			using (var iisManager = new ServerManager())
			{
				return iisManager.Sites
					.Select(site =>
					{
						try
						{
							return new Topology
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
							};
						}
						catch (Exception)
						{
							return null;//ok, skip. May be it is error in applicationHost.config? But other sites may be ok.
						}
					})
					.Where(t => t != null)
					.ToArray();
			}
		}

		private Node CreateNodeFromSite(Site site)
		{
			string status;
			try
			{
				status = MapStatus[site.State];//Possible site.State == null or some brokens value
			}
			catch (Exception)
			{
				status = AgentConstants.Status.Undefined;
			}
			return new Node
			{
				NodeRef = _siteNodeRefCreator.CreateNodeRefFromObj(site),
				Name = site.Name,
				Status = status,
				CreationTime = DateTimeOffset.UtcNow,
				ModificationTime = DateTimeOffset.UtcNow,
				Properties = new Dictionary<string, string>(),
			};
		}
	}
}