using System;
using System.IO.Pipes;
using System.Threading;
using Aims.IISAgent.Loggers;
using Aims.IISAgent.Pipes;
using Aims.IISAgent.Pipes.Tools;

namespace Aims.IISAgent.Module.Pipes
{
	public class MessagePipeReader : IRunnable, IDisposable
	{
		private const PipeDirection Direction = PipeDirection.InOut;
		private const int InBufferSize = 1000000;
		private const int MaxMessageReadSize = 100 * 1024;
		private const int MaxNumberOfServerInstances = -1;//magic, don't change
		private const int MaxWaitTime = 60 * 1000;// ms
		private const PipeOptions Options = PipeOptions.Asynchronous;
		private const int OutBufferSize = 10000;
		private const PipeTransmissionMode TransmissionMode = PipeTransmissionMode.Message;

		private readonly ILogger _logger;
		private readonly PipeSecurity _pipeSecurity;
		private readonly ManualResetEvent _terminationEvent = new ManualResetEvent(true);
		private readonly object _threadLock = new object();

		private Thread _thread;

		public MessagePipeReader(string pipeName, PipeSecurity pipeSecurity, ILogger logger)
		{
			_pipeSecurity = pipeSecurity;
			PipeName = pipeName;
			_logger = logger;
		}

		public event CustomEventHandler<MessagePipeReader, Message> MessageRead;

		public event EventHandler<EventArgs> ConnectionClosed;

		public bool IsRunning { get; private set; }

		public string PipeName { get; private set; }

		public void Start()
		{
			lock (_threadLock)
			{
				if (!IsRunning)
				{
					IsRunning = true;
					_terminationEvent.Reset();
					_thread = new Thread(Run);
					_thread.Start();
				}
			}
		}

		public void Stop()
		{
			lock (_threadLock)
			{
				IsRunning = false;

				if (_thread != null)
				{
					_terminationEvent.Set();
					_thread.Join();
					_thread = null;
				}
			}
		}

		private void ConnectPipe(IAsyncResult ar)
		{
			try
			{
				((NamedPipeServerStream)ar.AsyncState).EndWaitForConnection(ar);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				_logger.WriteError(String.Format("Failed to connect to pipe '{0}'.", ex));
			}
		}

		private bool ReadMessages(NamedPipeServerStream stream)
		{
			byte[] buffer = new byte[MaxMessageReadSize];
			int length;
			bool result = false;

			while ((length = stream.Read(buffer, 0, buffer.Length)) != 0)
			{
				stream.WriteByte(1);
				Message m = Message.Deserialize(buffer, 0, length);
				SendMessage(m);
				result = true;
			}

			return result;
		}

		private void SendMessage(Message m)
		{
			MessageRead.Raise(this, m);
		}

		private void Run()
		{
			string s = null;
			while (IsRunning)
			{
				NamedPipeServerStream pipeStream = null;

				try
				{
					pipeStream = new NamedPipeServerStream(PipeName,
						Direction, MaxNumberOfServerInstances, TransmissionMode,
						Options, InBufferSize, OutBufferSize, _pipeSecurity);

					IAsyncResult result = pipeStream.BeginWaitForConnection(ConnectPipe, pipeStream);
					try
					{
						if (WaitHandle.WaitAny(new[] { _terminationEvent, result.AsyncWaitHandle }, MaxWaitTime) ==
							WaitHandle.WaitTimeout)
							break;
					}
					catch (Exception ex)
					{
						if (ex is TimeoutException || ex is ObjectDisposedException)
						{
							IsRunning = false;
							break;
						}
						else throw;
					}

					SpinLockConnection(pipeStream);

					int time = 0;

					while (IsRunning && time < MaxWaitTime)
					{
						if (!ReadMessages(pipeStream))
						{
							time += 2;
							Thread.Sleep(2);
						}
						else
						{
							time = 0;
						}
					}
				}
				catch (Exception ex)
				{
					var str = ex.ToString();
					if (str != s)
					{
						s = str;
						string message = String.Join(Environment.NewLine,
							"Failed to read a serializible from pipe '{0}'.", s);
						_logger.WriteError(message);
					}
					Thread.Sleep(100);
				}
				finally
				{
					if (pipeStream != null)
					{
						if (pipeStream.IsConnected)
							pipeStream.Disconnect();
						pipeStream.Dispose();
					}
					IsRunning = false;
				}
			}

			if (ConnectionClosed != null)
				ConnectionClosed.Invoke(this, EventArgs.Empty);
		}

		private void SpinLockConnection(NamedPipeServerStream pipeStream)
		{
			int fHigh = 1, fLow = 1;
			while (!pipeStream.IsConnected && IsRunning)
			{
				Thread.Sleep(fHigh / 100);
				if (fHigh < 500)
				{
					fHigh += fLow;
					fLow = fHigh - fLow;
				}
			}
		}

		public void Dispose()
		{
			Stop();
			_terminationEvent?.Dispose();
		}
	}
}