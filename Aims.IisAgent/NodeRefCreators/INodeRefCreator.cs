using Aims.Sdk;

namespace Aims.IisAgent.NodeRefCreators
{
	public interface INodeRefCreator<in TBaseObject>
	{
		NodeRef CreateNodeRefFromObj(TBaseObject obj);
		NodeRef CreateFromInstanceName(string instanceName);
	}
}