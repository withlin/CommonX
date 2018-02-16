using System;
using System.Collections.Generic;
using System.Text;
using CommonX.Logging;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;

namespace CommonX.Kafka.Impl
{
    public class KafkaDefaultPersisterConnection : IKafkaPersisterConnection
    {
        private readonly ILogger _logger;
        private readonly string _groupId;
        private Consumer<string, string> _consumerClient;
        public KafkaDefaultPersisterConnection(ILoggerFactory loggerFactory,string gropId, IEnumerable<KeyValuePair<string, object>> kafkaSetting=null)
        {
            _logger = loggerFactory.Create(GetType());
            _groupId = gropId;
            _consumerClient = new Consumer<string, string>(kafkaSetting??new KafkaSetting().AsKafkaSetting(), new StringDeserializer(Encoding.UTF8), new StringDeserializer(Encoding.UTF8));
        }
        public void CommitAsync()
        {
            _consumerClient.CommitAsync();
        }

        public void Consume(List<string> topics, Action<Message<string, string>> handleMessages)
        {
            _consumerClient.OnPartitionEOF += (_, end)
                    => _logger.Info($"Reached end of topic {end.Topic} partition {end.Partition}, next message will be at offset {end.Offset}");

            _consumerClient.OnError += (_, error)
                => _logger.Info($"Error: {error}");

            _consumerClient.OnConsumeError += (_, error)
                => _logger.Info($"Consume error: {error}");

            _consumerClient.OnPartitionsAssigned += (_, partitions) =>
            {
                _logger.Info($"Assigned partitions: [{string.Join(", ", partitions)}], member id: {_consumerClient.MemberId}");
                _consumerClient.Assign(partitions);
            };

            _consumerClient.OnPartitionsRevoked += (_, partitions) =>
            {
                _logger.Info($"Revoked partitions: [{string.Join(", ", partitions)}]");
                _consumerClient.Unassign();
            };

            _consumerClient.OnStatistics += (_, json)
                => _logger.Info($"Statistics: {json}");

            _consumerClient.Subscribe(topics);

            _logger.Info($"Started consumer, Ctrl-C to stop consuming");

            var cancelled = false;
            Console.CancelKeyPress += (_, e) => {

                e.Cancel = true; // prevent the process from terminating.
                cancelled = true;
            };
            string value = string.Empty;
            while (!cancelled)
            {
                Message<string, string> msg;
                if (!_consumerClient.Consume(out msg, TimeSpan.FromMilliseconds(100)))
                {
                    continue;
                }

                handleMessages?.Invoke(msg);
                if (msg.Offset % 5 == 0)
                {
                    _logger.Info($"Committing offset");
                    var committedOffsets = _consumerClient.CommitAsync(msg).Result;
                    _logger.Info($"Committed offset: {committedOffsets}");
                }
            }
        }

        public void Dispose()
        {
            if (_consumerClient != null)
            {
                _consumerClient.Dispose();
            }
        }
    }
}
