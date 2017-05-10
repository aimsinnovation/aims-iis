using System;
using System.Diagnostics;
using System.Linq;
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
			Category = PerformanceCounterCategory
				.GetCategories()
				.Single(s => s.CategoryName.Equals(categogyName, StringComparison.InvariantCultureIgnoreCase));

			CounterName = counterName;
			StatType = statType;
		}

		public abstract StatPoint[] Collect();
	}
}