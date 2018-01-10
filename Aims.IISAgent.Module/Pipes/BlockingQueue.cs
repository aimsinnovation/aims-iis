using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Aims.IISAgent.Module.Pipes
{
	internal class BlockingQueue<T> : IEnumerable<T>
	{
		private readonly int _capacity;
		private readonly Queue<T> _queue;

		private readonly Semaphore _semaphore;

		public BlockingQueue(int capacity)
		{
			_capacity = capacity;
			_queue = new Queue<T>(capacity);
			_semaphore = new Semaphore(0, _capacity + 1);
		}

		public void Add(T item)
		{
			lock (_queue)
			{
				if (_queue.Count == _capacity)
				{
					_queue.Dequeue();
				}
				else
				{
					_semaphore.Release();
				}

				_queue.Enqueue(item);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			while (true)
			{
				if (!_semaphore.WaitOne(60000))
					throw new TimeoutException();

				T item;
				lock (_queue)
				{
					item = _queue.Dequeue();
				}

				yield return item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void WaitForItem()
		{
			_semaphore.WaitOne();
			_semaphore.Release();
		}
	}
}