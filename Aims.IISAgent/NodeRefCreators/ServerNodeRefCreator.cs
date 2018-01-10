using System;
using System.Collections.Generic;
using Aims.Sdk;
using static Aims.IISAgent.AgentConstants;
using Environment = System.Environment;

namespace Aims.IISAgent.NodeRefCreators
{
	public class ServerNodeRefCreator : INodeRefCreator, INodeRefCreator<object>
	{
		public NodeRef CreateNodeRefFromObj(object obj)
		{
			if (obj != null)
				throw new ArgumentException(nameof(obj));

			return new NodeRef
			{
				NodeType = NodeType.Server,
				Parts = new Dictionary<string, string>
				{
					{NodeRefPart.MachineName, Environment.MachineName}
				}
			};
		}

		public NodeRef CreateFromInstanceName(string instanceName)
		{
			return CreateNodeRefFromObj(null);
		}
	}
}