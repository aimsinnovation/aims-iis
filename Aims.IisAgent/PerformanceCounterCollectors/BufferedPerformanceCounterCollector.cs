using System;
using System.Collections.Generic;
using System.Linq;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors
{
	public class BufferedPerformanceCounterCollector : IBasePerformanceCounterCollector
	{
		private readonly Dictionary<NodeRef, Queue<StatPoint>> _buffer;
		private readonly Func<StatPoint, StatPoint, StatPoint> _agregator;

		public BufferedPerformanceCounterCollector(Func<StatPoint, StatPoint, StatPoint> agregator, IEventBasedCollector collector)
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

		public StatPoint[] Collect()
		{
			var answer = _buffer
				.Select<KeyValuePair<NodeRef, Queue<StatPoint>>, StatPoint>
					(pair => pair.Value.Aggregate(_agregator))
				.ToArray();
			_buffer.Clear();
			return answer;
		}
	}
}