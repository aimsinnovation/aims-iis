using Aims.Sdk;

namespace Aims.IISAgent.NodeRefCreators
{
	public interface INodeRefCreator
	{
		NodeRef CreateFromInstanceName(string instanceName);
	}
}