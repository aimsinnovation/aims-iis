using System;
using System.Collections.Concurrent;
using Aims.IISAgent.Loggers;
using Aims.IISAgent.Module.Pipes;
using Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector.EventBasedCollectors;
using Aims.IISAgent.Pipes.Tools;

namespace Aims.IISAgent.Pipes
{
	public class MessageTracker : IRunnable, IEventSource<Message>
	{
		private const int IterationPeriod = 2000;

		private readonly PipeManager _pipeManager;

		private readonly ConcurrentDictionary<string, MessagePipeReader> _pipeReaders =
			new ConcurrentDictionary<string, MessagePipeReader>();

		public MessageTracker(int batchSize, ILogger logger)
		{
			_pipeManager = new PipeManager(logger);
			_pipeManager.PipeReaderCreated += OnPipeReaderCreated;
		}

		public bool IsRunning { get; private set; }

		public void Start()
		{
			if (!IsRunning)
			{
				IsRunning = true;
				_pipeManager.Start();
			}
		}

		public void Stop()
		{
			if (IsRunning)
			{
				_pipeManager.Stop();
				foreach (MessagePipeReader pipeReader in _pipeReaders.Values)
				{
					pipeReader.Stop();
					pipeReader.ConnectionClosed -= OnConnectionClosed;
					pipeReader.MessageRead -= OnMessageRead;
				}
				IsRunning = false;
			}
		}

		private void OnConnectionClosed(object sender, EventArgs e)
		{
			var pipeReader = (MessagePipeReader)sender;
			pipeReader.ConnectionClosed -= OnConnectionClosed;
			pipeReader.MessageRead -= OnMessageRead;
			_pipeReaders.TryRemove(pipeReader.PipeName, out pipeReader);
		}

		private void OnMessageRead(MessagePipeReader sender, Message message)
		{
			if (EventOccured != null)
				EventOccured.Invoke(this, new GenericEventArgs<Message>(message));
		}

		private void OnPipeReaderCreated(PipeManager sender, MessagePipeReader pipeReader)
		{
			pipeReader.MessageRead += OnMessageRead;
			pipeReader.ConnectionClosed += OnConnectionClosed;
			pipeReader.Start();
			_pipeReaders[pipeReader.PipeName] = pipeReader;
		}

		public event EventHandler<GenericEventArgs<Message>> EventOccured;
	}
}