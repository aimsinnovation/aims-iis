using Aims.Sdk;

namespace Aims.IisAgent.NodeRefCreators
{
	public interface INodeRefCreator
	{
		NodeRef CreateFromInstanceName(string instanceName);
	}
}