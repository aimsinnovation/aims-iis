using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Aims.IISAgent.Pipes.Tools;

namespace Aims.IISAgent.Pipes
{
	internal class MessageBatcher : AsyncIterationRunner
	{
		private readonly int _batchSize;
		private readonly ConcurrentQueue<Message> _messages = new ConcurrentQueue<Message>();

		public MessageBatcher(int iterationTime, int batchSize)
			: base(iterationTime)
		{
			if (batchSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, "Batch size must be a positive value.");
			_batchSize = batchSize;
		}

		public event CustomEventHandler<MessageBatcher, Message[]> MessageBatchReady;

		public void Add(Message message)
		{
			_messages.Enqueue(message);
		}

		protected override void RunIteration()
		{
			var messages = new List<Message>();
			while (_messages.TryDequeue(out Message message))
			{
				messages.Add(message);

				if (messages.Count == _batchSize)
				{
					MessageBatchReady.Raise(this, messages.ToArray());
					messages.Clear();
				}
			}

			if (messages.Count > 0)
			{
				MessageBatchReady.Raise(this, messages.ToArray());
			}
		}
	}
}