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

        public void Start()
        {
            NamedPipeServerStream stream = null;
            try
            {
                stream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Message, PipeOptions.Asynchronous, InBufferSize, OutBufferSize, _pipeSecurity);

                stream.WaitForConnection();

                var buffer = new byte[MaxMessageReadSize];
                while (stream.IsConnected && !_cancellation.IsCancellationRequested)
                {
                    var messageSize = new byte[sizeof(int)];
                    var count = stream.Read(messageSize, 0, messageSize.Length);
                    if (count != 0)
                    {
                        var length = stream.Read(buffer, 0, BitConverter.ToInt32(messageSize, 0));
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