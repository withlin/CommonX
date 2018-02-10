using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Kafka
{
    public interface IConnectionPool:IDisposable
    {
        Producer Rent();

        bool Return(Producer context);
    }
}
