using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Kafka
{
    public interface IKafkaPersisterConnection:IDisposable
    {
        void Consume(List<string> topics, Action<Message<string, string>> handleMessages);
        void CommitAsync();
    }
}
