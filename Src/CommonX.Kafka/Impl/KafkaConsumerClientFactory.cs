using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Kafka.Impl
{
    public class KafkaConsumerClientFactory: IConsumerClientFactory
    {
        private readonly KafkaSetting _kafkaSetting;

        public KafkaConsumerClientFactory(KafkaSetting kafkaSetting)
        {
            _kafkaSetting = kafkaSetting;
        }

        public IConsumerClient Create(string groupId)
        {
            return new KafkaConsumerClient(groupId, _kafkaSetting);
        }
    }
}
