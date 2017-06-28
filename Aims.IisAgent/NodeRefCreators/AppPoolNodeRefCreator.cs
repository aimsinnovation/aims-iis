using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aims.Sdk;
using Microsoft.Web.Administration;

namespace Aims.IISAgent.NodeRefCreators
{
	public class AppPoolNodeRefCreator : INodeRefCreator, INodeRefCreator<ApplicationPool>, INodeRefCreator<Application>
	{
		public NodeRef CreateFromInstanceName(string instanceName)
		{
			Regex expression = new Regex("(_.+)");
			MatchCollection matches = expression.Matches(instanceName);
			var appPoolName = matches[0].Value.Substring(1);
			return new NodeRef
			{
				NodeType = AgentConstants.NodeType.AppPool,
				Parts = new Dictionary<string, string>
				{
					{AgentConstants.NodeRefPart.InstanceName, appPoolName}
				}
			};
		}

		public NodeRef CreateNodeRefFromObj(ApplicationPool obj)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			return new NodeRef
			{
				NodeType = AgentConstants.NodeType.AppPool,
				Parts = new Dictionary<string, string>
				{
					{ AgentConstants.NodeRefPart.InstanceName, obj.Name }
				}
			};
		}

		public NodeRef CreateNodeRefFromObj(Application obj)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			return new NodeRef
			{
				NodeType = AgentConstants.NodeType.AppPool,
				Parts = new Dictionary<string, string>
				{
					{ AgentConstants.NodeRefPart.InstanceName, obj.ApplicationPoolName }
				}
			};
		}
	}
}