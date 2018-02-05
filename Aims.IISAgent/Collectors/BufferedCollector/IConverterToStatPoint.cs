using Aims.Sdk;

namespace Aims.IISAgent.Collectors.BufferedCollector
{
	public interface IConverterToStatPoint<in T>
	{
		StatPoint ConvertPoint(T item);
	}
}