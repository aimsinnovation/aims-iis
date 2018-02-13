using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Aims.IISAgent.Loggers;
using Aims.IISAgent.Pipes.Tools;

namespace Aims.IISAgent.Pipes
{
	public class PipeManager
	{
		private const string EveryoneSecId = "S-1-1-0";//"S-1-5-21";
		private const PipeDirection Direction = PipeDirection.InOut;
		private const int InBufferSize = 1000;
		private const int MaxMessageReadSize = 100 * 1024;
		private const int MaxNumberOfServerInstances = -1;
		private const PipeTransmissionMode Mode = PipeTransmissionMode.Message;
		private const PipeOptions Options = PipeOptions.Asynchronous;
		private const int OutBufferSize = 10000;
		private readonly ILogger _logger;
		private readonly string _nameOfMainPipe;
		private readonly ManualResetEvent _terminationEvent = new ManualResetEvent(true);
		private readonly object _threadLock = new object();

		private PipeSecurity _pipeSecurity;

		private Thread _thread;

		public PipeManager(ILogger logger, string nameOfMainPipe = AgentConstants.Pipes.NameOfMainPipe)
		{
			_logger = logger;
			_nameOfMainPipe = nameOfMainPipe;
		}

		public event CustomEventHandler<PipeManager, MessagePipeReader> PipeReaderCreated;

		public bool IsRunning { get; private set; }

		protected PipeSecurity PipeSecurity
		{
			get
			{
				if (_pipeSecurity == null)
				{
					var everyone = new SecurityIdentifier(EveryoneSecId);
					var pipeSecurity = new PipeSecurity();
					pipeSecurity.AddAccessRule(new PipeAccessRule(everyone,
						PipeAccessRights.ReadWrite, AccessControlType.Allow));
					// ReSharper disable once AssignNullToNotNullAttribute
					pipeSecurity.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().Owner,
						PipeAccessRights.FullControl, AccessControlType.Allow));
					_pipeSecurity = pipeSecurity;
				}
				return _pipeSecurity;
			}
		}

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

		private static string CreateMessagePipeName()
		{
			return AgentConstants.Pipes.MessagePipePrefix + Guid.NewGuid();
		}

		private void Run()
		{
			string s = null;
			while (IsRunning)
			{
				try
				{
					using (var pipeStream = new NamedPipeServerStream(_nameOfMainPipe,
						Direction, MaxNumberOfServerInstances, Mode, Options,
						InBufferSize, OutBufferSize, PipeSecurity))
					{
						IAsyncResult result = pipeStream
							.BeginWaitForConnection(ar =>
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
									_logger.WriteError(String.Format("Failed to connect to pipe.\n pipe name:{1}.\n'{0}'.",
										ex.ToString(), _nameOfMainPipe));
								}
							}, pipeStream);

						WaitHandle.WaitAny(new[] { _terminationEvent, result.AsyncWaitHandle });

						if (!IsRunning) break;

						var pipeName = CreateMessagePipeName();
						if (PipeReaderCreated != null)
							PipeReaderCreated.Raise(this, new MessagePipeReader(pipeName, PipeSecurity, _logger));

						var buffer = Encoding.UTF8.GetBytes(pipeName);
						pipeStream.Write(buffer, 0, buffer.Length);
						pipeStream.WaitForPipeDrain();
					}
				}
				catch (Exception ex)
				{
					var str = ex.ToString();
					if (str != s)
					{
						s = str;
						_logger.WriteError(String.Format("Failed to send pipe name. \n {0}", s));
					}
					Thread.Sleep(10);
				}
			}
		}
	}
}