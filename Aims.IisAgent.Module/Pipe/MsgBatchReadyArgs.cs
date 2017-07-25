using System;

namespace Aims.IISAgent.Pipe
{
	public class MsgBatchReadyArgs<T> : EventArgs where T : Message
	{
		public T[] Messages { get; set; }
	}
}