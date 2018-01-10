using System;
using System.Collections.Generic;
using System.Linq;
using Aims.IISAgent.NodeRefCreators;
using Aims.Sdk;
using Microsoft.Web.Administration;

namespace Aims.IISAgent.TopologyCollectors
{
	internal class AppPoolTopologyCollector : ITopologyCollector
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

		private readonly INodeRefCreator<ApplicationPool> _appPoolNodeRefCreator;
		private readonly INodeRefCreator<object> _serverNodeRefCreator;

		public AppPoolTopologyCollector(INodeRefCreator<ApplicationPool> appPoolNodeRefCreator, INodeRefCreator<object> serverNodeRefCreator)
		{
			_appPoolNodeRefCreator = appPoolNodeRefCreator;
			_serverNodeRefCreator = serverNodeRefCreator;
		}

		public IEnumerable<Topology> Collect()
		{
			using (var iisManager = new ServerManager())
			{
				return iisManager.ApplicationPools
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
					})
					.ToArray();
			}
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