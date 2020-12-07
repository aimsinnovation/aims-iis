using System;
using System.IO.Pipes;
using System.Threading;
using Aims.IISAgent.Loggers;
using Aims.IISAgent.Pipes.Tools;

namespace Aims.IISAgent.Pipes
{
	public class MessagePipeReader
	{
		private const int InBufferSize = 1000000;
		private const int MaxMessageReadSize = 200;
		private const int OutBufferSize = 10000;

		private readonly ILogger _logger;
        private readonly CancellationToken _cancellation;
        private readonly PipeSecurity _pipeSecurity;

		public MessagePipeReader(string pipeName, PipeSecurity pipeSecurity, ILogger logger, CancellationToken cancellation)
		{
			_pipeSecurity = pipeSecurity;
			PipeName = pipeName;
			_logger = logger;
            _cancellation = cancellation;
        }

		public event CustomEventHandler<MessagePipeReader, Message> MessageRead;

		public event EventHandler<EventArgs> ConnectionClosed;

		public string PipeName { get; }

        public async void Start()
        {
            NamedPipeServerStream stream = null;
            try
            {
                stream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Message, PipeOptions.Asynchronous, InBufferSize, OutBufferSize, _pipeSecurity);

                await stream.WaitForConnectionAsync(_cancellation)
                    .ConfigureAwait(false);

                var buffer = new byte[MaxMessageReadSize];
                while (stream.IsConnected && !_cancellation.IsCancellationRequested)
                {
                    var messageSize = new byte[sizeof(int)];
                    var count = await stream.ReadAsync(messageSize, 0, messageSize.Length, _cancellation)
                        .ConfigureAwait(false);
                    if (count != 0)
                    {
                        var length = await stream.ReadAsync(buffer, 0, BitConverter.ToInt32(messageSize, 0),
                                _cancellation)
                            .ConfigureAwait(false);
                        var message = Message.Deserialize(buffer, 0, length);
                        MessageRead.Raise(this, message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.WriteError(string.Join(Environment.NewLine,
                    "Failed to read a serializable from pipe '{0}'.", ex.ToString()));
            }
            finally
            {
                stream?.Dispose();
            }

            ConnectionClosed?.Invoke(this, EventArgs.Empty);
        }
	}
}