using System;
using Aims.IISAgent.Module.Pipes;

namespace Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector
{
	public partial class MessageConverterToStatPoint : IConverterToStatPoint<Message>
	{
		private class SiteBindings// : IComparable<SiteBindings>
		{
			public string Protocol { get; set; }
			public string Domain { get; set; }
			public int Port { get; set; } //it's int, but in real it's ushort
			public string Application { get; set; }

			private bool Equals(SiteBindings other)
			{
				return string.Equals(Protocol, other.Protocol)
					&& string.Equals(Domain, other.Domain)
					&& Port == other.Port
					&& string.Equals(Application, other.Application, StringComparison.InvariantCultureIgnoreCase);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((SiteBindings)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = (Protocol != null ? Protocol.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (Domain != null ? Domain.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ Port;
					hashCode = (hashCode * 397) ^ (Application != null ? Application.GetHashCode() : 0);
					return hashCode;
				}
			}
		}
	}
}