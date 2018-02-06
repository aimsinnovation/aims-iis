using System;

namespace Aims.IISAgent.MyExceptions
{
	internal class InstanceNotFoundException : Exception
	{
		private const string MessageFormatString = "Instance not found for category {0} for counter {1}";

		public InstanceNotFoundException(string categoryName, string counterName, Exception innerException)
				: base(string.Format(MessageFormatString, categoryName, counterName), innerException)
		{ }
	}
}