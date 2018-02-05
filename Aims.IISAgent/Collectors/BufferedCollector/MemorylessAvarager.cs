using Aims.Sdk;

namespace Aims.IISAgent.Collectors.BufferedCollector
{
	public class MemorylessAvarager : IAgregator
	{
		private int _count;

		private StatPoint _sum;

		public MemorylessAvarager()
		{
			_sum = null;
			_count = 0;
		}

		public void Add(StatPoint point)
		{
			_count++;
			if (_sum == null)
				_sum = point;
			else if (_sum.NodeRef.Equals(point.NodeRef) && _sum.StatType.Equals(point.StatType))
			{
				_sum.Value += point.Value;
				_sum.Time = _sum.Time > point.Time ? _sum.Time : point.Time;
			}
		}

		public StatPoint Get()
		{
			var ans = _sum;
			if (_count != 0)
				ans.Value /= _count;
			_sum = null;
			_count = 0;
			return ans;
		}
	}
}