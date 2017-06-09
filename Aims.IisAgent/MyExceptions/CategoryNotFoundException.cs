using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aims.IISAgent.MyExceptions
{
	class CategoryNotFoundException : Exception
	{
		private const string MessageFormatString = "Category with name {0} not found";
		public CategoryNotFoundException(string categoryName)
			:base(string.Format(MessageFormatString, categoryName))
		{}
	}
}