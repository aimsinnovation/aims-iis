using Aims.Sdk;
using System;
using System.Collections.Generic;
using Aims.IISAgent.NodeRefCreators;

namespace Aims.IISAgent.TopologyCollectors
{
	internal class ServerTopologyCollector : ITopologyCollector
	{
		private readonly INodeRefCreator<object> _serverNodeRefCreator;

		public ServerTopologyCollector(INodeRefCreator<object> serverNodeRefCreator)
		{
			_serverNodeRefCreator = serverNodeRefCreator;
		}

		public IEnumerable<Topology> Collect()
		{
			var serverRef = _serverNodeRefCreator.CreateNodeRefFromObj(null);
			return new[]
			{
				new Topology{
					Node = new Node
					{
						NodeRef = serverRef,
						Name = serverRef.Parts[AgentConstants.NodeRefPart.MachineName],
						Status = AgentConstants.Status.Running,
						CreationTime = DateTimeOffset.UtcNow,
						ModificationTime = DateTimeOffset.UtcNow,
						Properties = new Dictionary<string, string>(),
					},
					Links = new Link[0]
				}
			};
		}
	}
}