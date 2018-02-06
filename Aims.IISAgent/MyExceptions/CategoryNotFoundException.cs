using System;

namespace Aims.IISAgent.MyExceptions
{
	internal class CategoryNotFoundException : Exception
	{
		private const string MessageFormatString = "Category with name {0} not found";

		public CategoryNotFoundException(string categoryName)
			: base(string.Format(MessageFormatString, categoryName))
		{ }
	}
}