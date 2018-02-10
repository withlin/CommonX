using CommonX.Components;
using CommonX.Logging;
using Confluent.Kafka;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Kafka.Impl
{
    [Component]
    public abstract class KafkaPersistentConnectionBase : IKafkaPersisterConnection
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
