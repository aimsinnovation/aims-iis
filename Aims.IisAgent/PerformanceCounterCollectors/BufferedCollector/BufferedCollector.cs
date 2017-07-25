using System;
using System.Collections.Generic;
using Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector.CollectionAgregators;
using Aims.IISAgent.PerformanceCounterCollectors.EventBasedCollectors;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector
{
	public class BufferedCollector : IBasePerformanceCounterCollector
	{
		private readonly Dictionary<NodeRef, Queue<StatPoint>> _buffer;
		private readonly ICollectionAgregator _agregator;

		public BufferedCollector(ICollectionAgregator agregator, IEventBasedCollector collector)
		{
			if (agregator == null)
				throw new NullReferenceException(nameof(agregator));
			if (collector == null)
				throw new NullReferenceException(nameof(collector));
			_agregator = agregator;
			collector.StatPointRecieved += AddStatPoint;
			_buffer = new Dictionary<NodeRef, Queue<StatPoint>>();
		}

		private void AddStatPoint(object sender, StatPointEventArgs e)
		{
			var statPoint = e.Point;
			lock (_buffer)
			{
				Queue<StatPoint> sequnce;
				if (_buffer.TryGetValue(statPoint.NodeRef, out sequnce))
					sequnce.Enqueue(statPoint);
				else
					_buffer.Add(statPoint.NodeRef, new Queue<StatPoint>(
						new[]
						{
							statPoint
						}
					));
			}
		}

		public StatPoint[] Collect()
		{
			List<StatPoint> answer = new List<StatPoint>();
			lock (_buffer)
			{
				foreach (var pair in _buffer)
					answer.AddRange(_agregator.AgregateStatPoints(pair.Value));
				_buffer.Clear();
			}
			return answer.ToArray();
		}
	}
}