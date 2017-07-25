using System;
using Aims.IISAgent.Module;

namespace Aims.IISAgent.Pipe
{
	public class MessageEventArgs : EventArgs
	{
		public MessageEventArgs(Message message)
		{
			Message = message;
		}

		public Message Message { get; }
	}
}