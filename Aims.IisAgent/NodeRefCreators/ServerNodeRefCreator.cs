using System;
using System.Collections.Generic;
using Aims.Sdk;
using Environment = System.Environment;

namespace Aims.IISAgent.NodeRefCreators
{
	public class ServerNodeRefCreator : INodeRefCreator<object>
	{
		public NodeRef CreateNodeRefFromObj(object obj)
		{
			if(obj != null)
				throw new ArgumentException(nameof(obj));
			return new NodeRef
			{
				NodeType = AgentConstants.NodeType.Server,
				Parts = new Dictionary<string, string>
				{
					{AgentConstants.NodeRefPart.MachineName, Environment.MachineName}
				}
			};
		}

		public NodeRef CreateFromInstanceName(string instanceName)
		{
			return CreateNodeRefFromObj(null);
		}

		public string Name => Environment.MachineName;
	}
}