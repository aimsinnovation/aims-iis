using System;

namespace Aims.IISAgent.Pipes
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