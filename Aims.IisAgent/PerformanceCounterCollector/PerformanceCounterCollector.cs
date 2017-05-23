using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Aims.Sdk;

namespace Aims.IisAgent
{
	public abstract class PerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		protected readonly string CounterName;
		protected readonly string StatType;
		protected readonly PerformanceCounterCategory Category;

		protected PerformanceCounterCollector(string categogyName, string counterName, string statType)
		{
			CounterName = counterName;
			StatType = statType;
			Category = PerformanceCounterCategory
				.GetCategories()
				.Single(category => category.CategoryName.Equals(categogyName, 
				StringComparison.InvariantCultureIgnoreCase));
			//if (Category == null)
			//{
			//	File.AppendAllText(@"C:\log.log", categogyName);
			//}
		}

		public abstract StatPoint[] Collect();
	}
}