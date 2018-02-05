using System;
using System.Collections.Generic;
using System.Linq;
using Aims.IISAgent.Collectors.BufferedCollector.EventBasedCollectors;
using Aims.Sdk;

namespace Aims.IISAgent.Collectors.BufferedCollector
{
	public class BufferedCollector<T> : ICollector
	{
		private readonly Dictionary<Tuple<NodeRef, string>, IAgregator> _buffer;
		private readonly Func<IAgregator> _agregator;
		private readonly IConverterToStatPoint<T> _converter;

		public BufferedCollector(Func<IAgregator> agregatorFabric, IEventSource<T> source, IConverterToStatPoint<T> converter)
		{
			if (agregatorFabric == null)
				throw new NullReferenceException(nameof(agregatorFabric));
			if (source == null)
				throw new NullReferenceException(nameof(source));
			if (converter == null)
				throw new NullReferenceException(nameof(converter));
			_agregator = agregatorFabric;
			_converter = converter;
			source.EventOccured += AddStatPoint;
			_buffer = new Dictionary<Tuple<NodeRef, string>, IAgregator>();
		}

		private void AddStatPoint(object sender, GenericEventArgs<T> e)
		{
			var statPoint = _converter.ConvertPoint(e.Item);
			if (statPoint == null) return;
			lock (_buffer)
			{
				var key = new Tuple<NodeRef, string>(statPoint.NodeRef, statPoint.StatType);
				if (_buffer.TryGetValue(key, out IAgregator agregator))
					agregator.Add(statPoint);
				else
				{
					agregator = _agregator();
					_buffer.Add(key, agregator);
					agregator.Add(statPoint);
				}
			}
		}

		public StatPoint[] Collect()
		{
			StatPoint[] answer;
			lock (_buffer)
			{
				answer = _buffer.Select(p => p.Value.Get()).Where(p => p != null).ToArray();
				_buffer.Clear();
			}
			return answer.ToArray();
		}
	}
}