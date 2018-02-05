using Aims.Sdk;

namespace Aims.IISAgent.Collectors.BufferedCollector
{
	public class MemorylessSummator : IAgregator
	{
		private StatPoint _sum;

		public MemorylessSummator()
		{
			_sum = null;
		}

		public void Add(StatPoint point)
		{
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
			_sum = null;
			return ans;
		}
	}
}