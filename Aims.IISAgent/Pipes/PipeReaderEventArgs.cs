using System;

namespace Aims.IISAgent.Pipes
{
	public class PipeReaderEventArgs : EventArgs
	{
		public PipeReaderEventArgs(MessagePipeReader pipeReader)
		{
			PipeReader = pipeReader;
		}

		public MessagePipeReader PipeReader { get; }
	}
}