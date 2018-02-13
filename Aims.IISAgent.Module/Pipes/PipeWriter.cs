using System;
using System.Text;
using System.Threading;
using Aims.IISAgent.Loggers;
using Aims.IISAgent.Pipes;

namespace Aims.IISAgent.Module.Pipes
{
	public class PipeWriter
	{
		private const string ControlPipeName = AgentConstants.Pipes.NameOfMainPipe;
		private const int MaxPipeNameLenght = 64;
		private const int MaxWaitConnectionTime = 60000;
		private const int MessageCacheSize = 2000;
		private static BlockingQueue<Message> _messages;
		private readonly ILogger _logger;

		public PipeWriter(ILogger logger)
		{
			_logger = logger;
			_messages = new BlockingQueue<Message>(MessageCacheSize);
			ThreadPool.QueueUserWorkItem(Run);
		}

		public void AddMessage(Message message)
		{
			_messages.Add(message);
		}

		private NamedPipeClient GetNamedPipeClient()
		{
			using (var pipeStream = new NamedPipeClient(ControlPipeName))
			{
				pipeStream.Connect();

				var buffer = new byte[MaxPipeNameLenght];
				var bufferSize = pipeStream.Read(buffer, 0, buffer.Length);
				var pipeName = Encoding.UTF8.GetString(buffer, 0, bufferSize);

				return new NamedPipeClient(pipeName);
			}
		}

		private void Run(object state)
		{
			string s = null;
			while (true)
			{
				try
				{
					_messages.WaitForItem();

					using (var pipeStream = GetNamedPipeClient())
					{
						pipeStream.Connect(MaxWaitConnectionTime);

						foreach (Message message in _messages)
						{
							try
							{
								if (!pipeStream.IsConnected)
								{
									_messages.Add(message);
									break;
								}

								if (!pipeStream.Transact(message.Serialize()))
								{
									_messages.Add(message);
								}
							}
							catch
							{
								_messages.Add(message);
								throw;
							}
						}
					}
				}
				catch (ThreadAbortException)
				{
					return;
				}
				catch (TimeoutException)
				{
				}
				catch (Exception ex)
				{
					var str = ex.ToString();
					if (str != s)
					{
						s = str;
						_logger.WriteError(s);
					}
					Thread.Sleep(100);
				}
			}
		}
	}
}