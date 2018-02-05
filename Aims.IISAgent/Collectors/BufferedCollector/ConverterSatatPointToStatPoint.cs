using Aims.Sdk;

namespace Aims.IISAgent.Collectors.BufferedCollector
{
	internal class ConverterSatatPointToStatPoint : IConverterToStatPoint<StatPoint>
	{
		public StatPoint ConvertPoint(StatPoint item)
		{
			return item;
		}
	}
}