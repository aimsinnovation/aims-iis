using System;

namespace Aims.IISAgent.Collectors.BufferedCollector.EventBasedCollectors
{
	public class GenericEventArgs<T> : EventArgs
	{
		public GenericEventArgs(T item)
		{
			Item = item;
		}

		public T Item { get; private set; }
	}
}