using System;
using Aims.IISAgent.Module.Pipes;

namespace Aims.IISAgent.Pipes
{
	public class MsgBatchReadyArgs<T> : EventArgs where T : Message
	{
		public T[] Messages { get; set; }
	}
}