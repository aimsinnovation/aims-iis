using System;
using System.Collections.Generic;
using Aims.Sdk;
using Microsoft.Web.Administration;

namespace Aims.IISAgent.NodeRefCreators
{
	public class SiteNodeRefCreator : INodeRefCreator, INodeRefCreator<Site>
	{
		public NodeRef CreateNodeRefFromObj(Site obj)
		{
			if(obj == null)
				throw new ArgumentNullException(nameof(obj));
			return new NodeRef
			{
				NodeType = AgentConstants.NodeType.Site,
				Parts = new Dictionary<string, string>
				{
					{AgentConstants.NodeRefPart.Id, obj.Name}
				}
			};
		}

		//Instance name is like site name
		public NodeRef CreateFromInstanceName(string instanceName)
		{
			return new NodeRef
			{
				NodeType = AgentConstants.NodeType.Site,
				Parts = new Dictionary<string, string>
				{
					{AgentConstants.NodeRefPart.Id, instanceName}
				}
			};
		}
	}
}