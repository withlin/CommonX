using Confluent.Kafka;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace CommonX.Kafka.Impl
{
   public class ConnectionPool:IConnectionPool
    {
        private readonly Func<Producer> _activator;
        private readonly ConcurrentQueue<Producer> _pool = new ConcurrentQueue<Producer>();
        private int _count;

        private int _maxSize;

        public ConnectionPool(KafkaSetting setting)
        {
            _maxSize = setting.ConnectionPoolSize;
            _activator = CreateActivator(setting);
        }

        Producer IConnectionPool.Rent()
        {
            return Rent();
        }

        bool IConnectionPool.Return(Producer connection)
        {
            return Return(connection);
        }

        public void Dispose()
        {
            _maxSize = 0;

            while (_pool.TryDequeue(out var context))
                context.Dispose();
        }

        private static Func<Producer> CreateActivator(KafkaSetting setting)
        {
            return () => new Producer(setting.AsKafkaSetting());
        }

        public virtual Producer Rent()
        {
            if (_pool.TryDequeue(out var connection))
            {
                Interlocked.Decrement(ref _count);

                Debug.Assert(_count >= 0);

                return connection;
            }

            connection = _activator();

            return connection;
        }

        public virtual bool Return(Producer connection)
        {
            if (Interlocked.Increment(ref _count) <= _maxSize)
            {
                _pool.Enqueue(connection);

                return true;
            }

            Interlocked.Decrement(ref _count);

            Debug.Assert(_maxSize == 0 || _pool.Count <= _maxSize);

            return false;
        }
    }
}
