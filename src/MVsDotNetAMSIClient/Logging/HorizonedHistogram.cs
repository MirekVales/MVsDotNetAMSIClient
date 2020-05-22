using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace MVsDotNetAMSIClient.Logging
{
    public class HorizonedHistogram<T> : IDisposable
    {
        public int Capacity { get; }
        public TimeSpan Length { get; }
        public TimeSpan Granularity { get; }

        readonly Dictionary<T, HorizonedBuffer> store;
        readonly ReaderWriterLockSlim readerWriterLockSlim;

        DateTime lastUpdate;

        public HorizonedHistogram(TimeSpan length, TimeSpan granularity)
        {
            store = new Dictionary<T, HorizonedBuffer>();
            readerWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            Length = length;
            Granularity = granularity;

            Capacity = (int)Math.Ceiling(length.Ticks / (double)granularity.Ticks);

            lastUpdate = DateTime.Now;
        }

        public void Add(T instance)
        {
            try
            {
                readerWriterLockSlim.EnterUpgradeableReadLock();

                if (IsMoveNecessary())
                    Move();

                if (store.TryGetValue(instance, out var value))
                    value.Increase();
                else
                    InitializeNew(instance);
            }
            finally
            {
                readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        void InitializeNew(T instance)
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock();

                store[instance] = new HorizonedBuffer(Capacity, 1);
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        bool IsMoveNecessary()
            => (DateTime.Now - lastUpdate).Ticks > Granularity.Ticks;

        void Move()
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock();

                var steps = (int)Math.Floor((DateTime.Now - lastUpdate).Ticks / (double)Granularity.Ticks);
                foreach (var value in store.Values)
                    value.Move(steps);

                lastUpdate = DateTime.Now;
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public IEnumerable<int> Get(T instance, TimeSpan offset, TimeSpan length)
        {
            try
            {
                readerWriterLockSlim.EnterReadLock();

                if (store.TryGetValue(instance, out var buffer))
                    return buffer
                        .Skip((int)Math.Ceiling(offset.Ticks / (double)Granularity.Ticks))
                        .Take((int)Math.Ceiling(length.Ticks / (double)Granularity.Ticks))
                        .ToArray();

                return Enumerable.Empty<int>();
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void Purge()
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock();

                store
                    .Values
                    .ToList()
                    .ForEach(buffer => buffer.Purge());
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Dispose()
            => readerWriterLockSlim?.Dispose();
    }
}
