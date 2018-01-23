using System;
using System.Threading;

namespace Aims.IISAgent
{
	public class Cache<TData>
	{
		private const int MinWaitTime = 500;//ms
		private readonly TimeSpan _cacheTime;
		private readonly Func<TData> _produceFunc;
		private readonly int _retries;
		private readonly int _retriesInterval;
		private readonly ReaderWriterLock _syncData;
		private readonly ReaderWriterLock _sync;
		private readonly object _syncUpdate;
		private TData _cachedResult;
		private DateTimeOffset _lastUpdateTime;

		/// <summary>
		/// </summary>
		/// <param name="produceFunc">Function that return some result</param>
		/// <param name="cacheTime">how much time you need to store data</param>
		/// <param name="retries">retries count</param>
		/// <param name="retriesInterval">time interval in ms between retries</param>
		public Cache(Func<TData> produceFunc, TimeSpan cacheTime, int retries = 3, int retriesInterval = 10)
		{
			if (retries < 0) throw new ArgumentOutOfRangeException(nameof(retries));
			if (retriesInterval < 0) throw new ArgumentOutOfRangeException(nameof(retriesInterval));
			_produceFunc = produceFunc ?? throw new ArgumentNullException(nameof(produceFunc));
			_lastUpdateTime = DateTimeOffset.MinValue;//I think it will work on any PC with RIGHT TIME
			_retriesInterval = retriesInterval;
			_retries = retries;
			_cacheTime = cacheTime;
			_cachedResult = default(TData);
			_syncData = new ReaderWriterLock();
			_sync = new ReaderWriterLock();
			_syncUpdate = new object();
		}

		public TData Value
		{
			get
			{
				_sync.AcquireReaderLock(_retries * (_retriesInterval + MinWaitTime) + MinWaitTime);
				if (DateTimeOffset.UtcNow > _lastUpdateTime + _cacheTime)
					RefreshValue();
				var ans = _cachedResult;
				_sync.ReleaseReaderLock();
				if (ans.Equals(default(TData)))
					return Value;
				return ans;
			}
		}

		private static T Retry<T>(Func<T> func, int retries, int retriesInterval)
		{
			do
			{
				try
				{
					return func();
				}
				catch
				{
					if (retries == 0) throw new TimeoutException("End retries");
					Thread.Sleep(retriesInterval);
				}
			} while (retries-- > 0);

			return default(T);
		}

		private void RefreshValue()
		{
			if (Monitor.TryEnter(_syncUpdate, 0))
			{
				try
				{
					var cookie = _sync.UpgradeToWriterLock((_retries + 1) * _retriesInterval + MinWaitTime);
					try
					{
						_cachedResult = Retry(_produceFunc, _retries, _retriesInterval);
						_lastUpdateTime = DateTimeOffset.Now;
					}
					finally
					{
						_sync.DowngradeFromWriterLock(ref cookie);
					}
				}
				finally
				{
					Monitor.Exit(_syncUpdate);
				}
			}
		}
	}
}