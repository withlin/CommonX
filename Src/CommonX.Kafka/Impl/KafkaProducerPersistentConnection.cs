using CommonX.Components;
using CommonX.Logging;
using Confluent.Kafka;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CommonX.Kafka.Impl
{
    [Component]
    public class KafkaProducerPersistentConnection : IKafkaPersisterConnection
    {
        private readonly ILogger _logger;
        private readonly IConnectionPool _producerConntion;

        public KafkaProducerPersistentConnection(ILoggerFactory loggerFactory,IConnectionPool producerConntion)
        {
            _logger = loggerFactory.Create(GetType());
            _producerConntion = producerConntion;
        }
        public async Task Publish(string topic,byte[] value,byte[] key=null)
        {
            try
            {
                await _producerConntion.Rent()?.ProduceAsync(topic, key, value);
            }
            catch (Exception ex)
            {
                _logger.Error($"Kafka ProduceAsync has Error-{ex.Message}- {ex.StackTrace}");
            }
            
        }

        public void Dispose()
        {
            if (_producerConntion != null)
            {
                _producerConntion.Dispose();
            }
        }
    }
}
