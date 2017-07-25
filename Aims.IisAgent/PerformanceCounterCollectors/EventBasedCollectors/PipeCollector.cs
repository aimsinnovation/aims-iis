using System;
using Aims.IISAgent.Module.Pipe;
using Aims.IISAgent.Pipe;
using Aims.Sdk;

namespace Aims.IISAgent.PerformanceCounterCollectors.EventBasedCollectors
{
	public class PipeCollector : IEventBasedCollector
	{
		private readonly PipeManager _manager;

		public event EventHandler<StatPointEventArgs> StatPointRecieved;

		public PipeCollector(PipeManager manager)
		{
			_manager = manager;
			_manager.Start();
			_manager.PipeReaderCreated += (sender, reader) => reader.MessageRead += (pipeReader, message) =>
			{
				if (StatPointRecieved != null)
					StatPointRecieved.Invoke(this, new StatPointEventArgs(new StatPoint
					{
						NodeRef = null,
						StatType = message.StatType,
						Time = message.DateTime,
						Value = message.Code,
					}));
			};
		}
	}
}