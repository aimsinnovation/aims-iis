using Aims.Sdk;

namespace Aims.IISAgent.NodeRefCreators
{
	public interface INodeRefCreator<in TBaseObject> : INodeRefCreator
	{
		NodeRef CreateNodeRefFromObj(TBaseObject obj);
	}
}