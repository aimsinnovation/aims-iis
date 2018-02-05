using System;
using System.Diagnostics;
using System.Reflection;

namespace Aims.IISAgent.PerformanceCounterCollectors
{
	public static class PerformanceCounterFlush
	{
		private static DateTime _lastSync = DateTime.UtcNow;
		private static readonly object Sync = new object();
		private static readonly TimeSpan MinFlushSpan = TimeSpan.FromMinutes(10);

		public static void FlushCache()
		{
			try
			{
				lock (Sync)//must use static field for sync to prevent plural flush
				{
					if (DateTime.UtcNow - _lastSync > MinFlushSpan)
					{
						_lastSync = DateTime.UtcNow;

						var assembly = Assembly.GetAssembly(typeof(PerformanceCounterCategory));
						var type = assembly.GetType("System.Diagnostics.PerformanceCounterLib");
						var method = type.GetMethod("CloseAllTables", BindingFlags.NonPublic | BindingFlags.Static);
						method.Invoke(null, null);
					}
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}