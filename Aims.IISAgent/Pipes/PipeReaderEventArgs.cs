using System;

namespace Aims.IISAgent.Module.Pipes
{
	public class PipeReaderEventArgs : EventArgs
	{
		public PipeReaderEventArgs(MessagePipeReader pipeReader)
		{
			PipeReader = pipeReader;
		}

		public MessagePipeReader PipeReader { get; private set; }
	}
}