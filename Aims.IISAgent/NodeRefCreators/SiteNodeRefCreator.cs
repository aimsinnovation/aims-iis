﻿using System;
using System.Collections.Generic;
using Aims.Sdk;
using Microsoft.Web.Administration;
using static Aims.IISAgent.AgentConstants;

namespace Aims.IISAgent.NodeRefCreators
{
	public class SiteNodeRefCreator : INodeRefCreator, INodeRefCreator<Site>
	{
		//Instance name is like site name
		public NodeRef CreateFromInstanceName(string instanceName)
		{
			return new NodeRef
			{
				NodeType = NodeType.Site,
				Parts = new Dictionary<string, string>
				{
					{NodeRefPart.Id, instanceName}
				}
			};
		}

		public NodeRef CreateNodeRefFromObj(Site obj)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));
			return new NodeRef
			{
				NodeType = NodeType.Site,
				Parts = new Dictionary<string, string>
				{
					{NodeRefPart.Id, obj.Name}
				}
			};
		}
	}
}