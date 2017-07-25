using System;
using Aims.IISAgent.Module.Pipe;
using Aims.IISAgent.NodeRefCreators;
using Aims.IISAgent.Pipe;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors.EventBasedCollectors
{
	public class ModuleEventCollector : IEventBasedCollector
	{
		private readonly INodeRefCreator _nodeRefCreator;

		public ModuleEventCollector(INodeRefCreator nodeRefCreator)
		{
			_nodeRefCreator = nodeRefCreator;
		}

		public event EventHandler<StatPointEventArgs> StatPointRecieved;

		public void ReadPipe(object state, PipeReaderEventArgs reader)
		{
			reader.PipeReader.MessageRead += OnPipeReaderMessageRead;
			reader.PipeReader.Start();
		}

		private void OnPipeReaderMessageRead(MessagePipeReader messagePipeReader, Message message)
		{
			if (StatPointRecieved == null) return;

			StatPointRecieved(this, new StatPointEventArgs(
				new StatPoint
				{
					NodeRef = _nodeRefCreator.CreateFromInstanceName(message.Site),
					StatType = message.StatType,
					Time = message.DateTime,
					Value = message.Code,
				}
			));
		}
	}
}