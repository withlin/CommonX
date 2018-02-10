using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommonX.Components;
using CommonX.Logging;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;

namespace CommonX.Kafka.Impl
{
    [Component]
    public class KafkaConsumerPersistentConnection : IKafkaPersisterConnection
    {

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
