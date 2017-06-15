using Aims.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aims.IISAgent.NodeRefCreators;

namespace Aims.IISAgent.TopologyCollectors
{
	class ServerTopologyCollector : ITopologyCollector
	{
		private ServerNodeRefCreator _serverNodeRefCreator = new ServerNodeRefCreator();

		public Topology[] Collect()
		{
			return new Topology[]
			{
				new Topology{
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
				}
			};
		}
	}
}
