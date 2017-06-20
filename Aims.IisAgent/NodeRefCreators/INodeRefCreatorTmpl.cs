using Aims.Sdk;

namespace Aims.IISAgent.NodeRefCreators
{
	public interface INodeRefCreator<in TBaseObject>
	{
		NodeRef CreateNodeRefFromObj(TBaseObject obj);
	}
}