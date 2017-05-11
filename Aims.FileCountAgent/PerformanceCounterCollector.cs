using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Aims.Sdk;

namespace Aims.FileCountAgent
{
	public abstract class PerformanceCounterCollector
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
		}

		public abstract StatPoint[] Collect();
	}
}