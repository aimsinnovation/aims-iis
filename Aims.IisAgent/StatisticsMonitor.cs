using System;
using System.Collections.Generic;
using System.Linq;
using Aims.IISAgent.Module.Loggers;
using Aims.IISAgent.Module.Pipes;
using Aims.IISAgent.NodeRefCreators;
using Aims.IISAgent.PerformanceCounterCollectors;
using Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector;
using Aims.IISAgent.Pipes;
using Aims.Sdk;

namespace Aims.IISAgent
{
	public class StatisticsMonitor : MonitorBase<StatPoint>
	{
		private const string CategoryNameAspDotNet = "ASP.NET";
		private const string CategoryNameW3Svc = "W3SVC_W3WP";
		private const string CategoryNameWebService = "Web Service";

		private readonly EnvironmentApi _api;
		private readonly IBasePerformanceCounterCollector[] _collectors;
		private readonly ILogger _eventLog;

		public StatisticsMonitor(EnvironmentApi api, ILogger eventLog, TimeSpan collectTimeSpan, IEnumerable<Func<IBasePerformanceCounterCollector>> counterCreators = null)
			: base((int)collectTimeSpan.TotalMilliseconds)
		{
			_api = api;
			_eventLog = eventLog;
			_collectors = Initialize(counterCreators == null ? GetCounters(eventLog) : counterCreators)
					.ToArray();
		}

		protected override StatPoint[] Collect()
		{
			return _collectors
				.SelectMany(c =>
				{
					try
					{
						return c.Collect();
					}
					catch (Exception ex)
					{
						if (Config.VerboseLog)
						{
							_eventLog.WriteError(String.Format("An error occurred while trying to collect stat points: {0}", ex));
						}
						return new StatPoint[0];
					}
				}
				)
				.ToArray();
		}

		protected override void Send(StatPoint[] items)
		{
			try
			{
				_api.StatPoints.Send(items);
			}
			catch (Exception ex)
			{
				if (Config.VerboseLog)
				{
					_eventLog.WriteError(String.Format("An error occurred while trying to send stat points: {0}", ex));
				}
			}
		}

		private static StatPoint[] Agregator(Queue<StatPoint> queue)
		{
			var answer = queue.Aggregate((point1, point2) => new StatPoint
			{
				NodeRef = Equals(point1.NodeRef, point2.NodeRef)
					? point1.NodeRef
					: throw new InvalidOperationException("try to agregate different points"),
				StatType = Equals(point1.StatType, point2.StatType)
					? point1.StatType
					: throw new InvalidOperationException("try to agregate different points"),
				Time = point1.Time > point2.Time ? point1.Time : point2.Time,
				Value = point1.Value + point2.Value
			});
			answer.Value /= queue.Count();
			return new[] { answer };
		}

		private static IEnumerable<Func<IBasePerformanceCounterCollector>> GetCounters(ILogger logger)
		{
			var appPoolNodeRefCreator = new AppPoolNodeRefCreator();
			var serverNodeRefCreator = new ServerNodeRefCreator();
			var siteNodeRefCreator = new SiteNodeRefCreator();
			var tracker = new MessageTracker(1000, logger);

			yield return () => new DifferencePerformanceCounterCollector(
				new MultiInstancePerformanceCounterCollector(
					CategoryNameW3Svc, "Total HTTP Requests Served",
					AgentConstants.StatType.Requests,
					appPoolNodeRefCreator));

			//yield return () => new MultiInstancePerformanceCounterCollector(
			//	CategoryNameW3Svc, "Total Threads",
			//	AgentConstants.StatType.TotalThreads,
			//	appPoolNodeRefCreator);

			//yield return () => new NoInstancePerformanceCounterCollector(
			//	CategoryNameAspDotNet, "Requests Queued",
			//	AgentConstants.StatType.RequestQueued,
			//	serverNodeRefCreator);

			//yield return () => new DifferencePerformanceCounterCollector(
			//	new MultiInstancePerformanceCounterCollector(
			//		CategoryNameWebService, "Total Get Requests",
			//		AgentConstants.StatType.GetRequests,
			//		siteNodeRefCreator));

			//yield return () => new DifferencePerformanceCounterCollector(
			//	new MultiInstancePerformanceCounterCollector(
			//		CategoryNameWebService, "Total Post Requests",
			//		AgentConstants.StatType.PostRequests,
			//		siteNodeRefCreator));

			//yield return () => new DifferencePerformanceCounterCollector(
			//	new MultiInstancePerformanceCounterCollector(
			//		CategoryNameWebService, "Total Bytes Sent",
			//		AgentConstants.StatType.BytesSent,
			//		siteNodeRefCreator));

			//yield return () => new DifferencePerformanceCounterCollector(
			//	new MultiInstancePerformanceCounterCollector(
			//		CategoryNameWebService, "Total Bytes Received",
			//		AgentConstants.StatType.BytesReceived,
			//		siteNodeRefCreator));

			//yield return () => new BufferedCollector(new AvgCollectionAgregator(),
			//	new TimerBasedCollector(
			//		new MultiInstancePerformanceCounterCollector(
			//			CategoryNameWebService, "Current Connections",
			//			AgentConstants.StatType.ActiveConnections,
			//			siteNodeRefCreator),
			//		TimeSpan.FromSeconds(1)));

			//yield return () => new MultiInstancePerformanceCounterCollector(
			//	CategoryNameW3Svc, "Active Requests",
			//	AgentConstants.StatType.ActiveRequests,
			//	appPoolNodeRefCreator);

			yield return () =>
			new BufferedCollector<Message>(
				Agregator, tracker,
				new MessageConverterToStatPoint(siteNodeRefCreator));

			tracker.Start();//TODO этот код выполнится?
		}

		private IEnumerable<IBasePerformanceCounterCollector> Initialize(IEnumerable<Func<IBasePerformanceCounterCollector>> creators)
		{
			foreach (var creator in creators)
			{
				IBasePerformanceCounterCollector collector = null;
				try
				{
					collector = creator();
				}
				catch (Exception ex)
				{
					if (Config.VerboseLog)
					{
						_eventLog.WriteError(string.Format("An error occurred while trying to create PerformanceCounterCollector: {0}", ex));
					}
				}

				if (collector != null)
					yield return collector;
			}
		}
	}
}