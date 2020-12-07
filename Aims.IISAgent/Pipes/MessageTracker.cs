using System;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Aims.IISAgent.Collectors.BufferedCollector.EventBasedCollectors;
using Aims.IISAgent.Loggers;

namespace Aims.IISAgent.Pipes
{
	public class MessageTracker : IRunnable, IEventSource<Message>, IDisposable
	{
        private readonly ILogger _logger;
        private const int IterationPeriod = 2000;

		private readonly PipeManager _pipeManager;

		private readonly ConcurrentDictionary<string, MessagePipeReader> _pipeReaders =
			new ConcurrentDictionary<string, MessagePipeReader>();

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public MessageTracker(int batchSize, ILogger logger)
		{
            _logger = logger;
            _pipeManager = new PipeManager(logger);
			_pipeManager.PipeReaderCreated += OnPipeReaderCreated;
		}

		public event EventHandler<GenericEventArgs<Message>> EventOccured;

		public bool IsRunning { get; private set; }

		public void Dispose()
		{
			Stop();
		}

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
                cancellationTokenSource.Cancel();
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
            EventOccured?.Invoke(this, new GenericEventArgs<Message>(message));
        }

		private void OnPipeReaderCreated(PipeManager sender, string pipeName)
		{
            var pipeReader = new MessagePipeReader(pipeName, sender.PipeSecurity, _logger, cancellationTokenSource.Token);
			pipeReader.MessageRead += OnMessageRead;
			pipeReader.ConnectionClosed += OnConnectionClosed;
            Task.Factory.StartNew(() => pipeReader.Start(), TaskCreationOptions.LongRunning);
			_pipeReaders[pipeReader.PipeName] = pipeReader;
		}
	}
}