using Aims.Sdk;

namespace Aims.IisAgent.NodeRefCreators
{
	public interface INodeRefCreator<in TBaseObject> : INodeRefCreator
	{
		NodeRef CreateNodeRefFromObj(TBaseObject obj);
	}
}