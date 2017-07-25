using System;
using System.Collections.Concurrent;
using Aims.IISAgent.Module.Loggers;
using Aims.IISAgent.Pipe;
using InoSoft.Tools;

namespace Aims.IISAgent.Module.Pipe
{
	public class MessageTracker : IRunnable
	{
		private const int IterationPeriod = 2000;

		private readonly MessageBatcher _messageBatcher;

		private readonly PipeManager _pipeManager;

		private readonly ConcurrentDictionary<string, MessagePipeReader> _pipeReaders =
			new ConcurrentDictionary<string, MessagePipeReader>();

		public MessageTracker(int batchSize, ILogger logger)
		{
			_messageBatcher = new MessageBatcher(IterationPeriod, batchSize);
			_pipeManager = new PipeManager(logger);
			_pipeManager.PipeReaderCreated += OnPipeReaderCreated;
			_messageBatcher.MessageBatchReady += (o, messages) => MessageBatchReady.Raise(this, messages);
		}

		public event CustomEventHandler<MessageTracker, Message[]> MessageBatchReady;

		public bool IsRunning { get; private set; }

		public void Start()
		{
			if (!IsRunning)
			{
				IsRunning = true;
				_messageBatcher.Start();
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
				_messageBatcher.Stop();
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
			_messageBatcher.Add(message);
		}

		private void OnPipeReaderCreated(PipeManager sender, MessagePipeReader pipeReader)
		{
			pipeReader.MessageRead += OnMessageRead;
			pipeReader.ConnectionClosed += OnConnectionClosed;
			pipeReader.Start();
			_pipeReaders[pipeReader.PipeName] = pipeReader;
		}
	}
}