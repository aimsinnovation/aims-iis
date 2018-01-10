using System;
using System.Collections.Generic;
using System.Linq;
using Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector.EventBasedCollectors;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector
{
	public class BufferedCollector<T> : IBasePerformanceCounterCollector
	{
		private readonly Dictionary<NodeRef, Queue<StatPoint>> _buffer;
		private readonly Func<Queue<StatPoint>, StatPoint[]> _agregator;
		private readonly IConverterToStatPoint<T> _converter;

		public BufferedCollector(Func<Queue<StatPoint>, StatPoint[]> agregator, IEventSource<T> collector, IConverterToStatPoint<T> converter)
		{
			if (agregator == null)
				throw new NullReferenceException(nameof(agregator));
			if (collector == null)
				throw new NullReferenceException(nameof(collector));
			if (converter == null)
				throw new NullReferenceException(nameof(converter));
			_agregator = agregator;
			_converter = converter;
			collector.EventOccured += AddStatPoint;
			_buffer = new Dictionary<NodeRef, Queue<StatPoint>>();
		}

		private void AddStatPoint(object sender, GenericEventArgs<T> e)
		{
			var statPoint = _converter.ConvertPoint(e.Item);
			if (statPoint == null) return;
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
				{
					Dictionary<string, Queue<StatPoint>> statTypeSort = new Dictionary<string, Queue<StatPoint>>();
					foreach (var point in pair.Value)
					{
						Queue<StatPoint> sequnce;
						if (statTypeSort.TryGetValue(point.StatType, out sequnce))
							sequnce.Enqueue(point);
						else
							statTypeSort.Add(point.StatType, new Queue<StatPoint>(
								new[]
								{
									point
								}
							));
					}
					foreach (var seq in statTypeSort)
					{
						answer.AddRange(_agregator(seq.Value));
					}
				}
				_buffer.Clear();
			}
			return answer.ToArray();
		}
	}
}