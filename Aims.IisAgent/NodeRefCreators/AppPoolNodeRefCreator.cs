using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aims.Sdk;
using Microsoft.Web.Administration;

namespace Aims.IisAgent.NodeRefCreators
{
	public class AppPoolNodeRefCreator:INodeRefCreator<ApplicationPool>
	{
		public NodeRef CreateNodeRefFromObj(ApplicationPool obj)
		{
			return new NodeRef
			{
				NodeType = AgentConstants.NodeType.AppPool,
				Parts = new Dictionary<string, string>
				{
					{ AgentConstants.NodeRefPart.InstanceName, obj.Name }
				}
			};
		}

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
	}
}