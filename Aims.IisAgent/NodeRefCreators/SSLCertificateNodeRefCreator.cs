using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Aims.Sdk;

namespace Aims.IISAgent.NodeRefCreators
{
	internal class SslCertificateNodeRefCreator : INodeRefCreator<X509Certificate2>
	{
		public NodeRef CreateNodeRefFromObj(X509Certificate2 obj)
		{
			return new NodeRef
			{
				NodeType = AgentConstants.NodeType.SslCert,
				Parts = new Dictionary<string, string>
				{
					{"ssl-certificate-name", obj.FriendlyName}
				}
			};
		}
	}
}