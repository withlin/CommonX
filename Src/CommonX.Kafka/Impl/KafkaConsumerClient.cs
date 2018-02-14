using CommonX.Logging;
using CommonX.Models;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CommonX.Kafka.Impl
{
    internal sealed class KafkaConsumerClient : IConsumerClient
    {
        private readonly string _groupId;
        private readonly KafkaSetting _kafkaSetting;
        private Consumer<Ignore, string> _consumerClient;

        public KafkaConsumerClient(string groupId, KafkaSetting kafkaSetting)
        {
            _groupId = groupId;
            _kafkaSetting = kafkaSetting ?? throw new ArgumentNullException(nameof(KafkaSetting));
            StringDeserializer = new StringDeserializer(Encoding.UTF8);
        }

        public IDeserializer<string> StringDeserializer { get; set; }

        public event EventHandler<MessageContext> OnMessageReceived;

        public event EventHandler<LogMessageEventArgs> OnLog;

        public void Subscribe(IEnumerable<string> topics)
        {
            if (topics == null)
                throw new ArgumentNullException(nameof(topics));

            if (_consumerClient == null)
                

            using (_consumerClient)
            {
                    InitKafkaClient();
                    _consumerClient.Subscribe(topics);
            }
        }

        public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (_consumerClient == null)
                    InitKafkaClient();
                cancellationToken.ThrowIfCancellationRequested();
                _consumerClient.Poll(timeout);
            }

        }

        public void Commit()
        {
            _consumerClient.CommitAsync();
        }

        public void Reject()
        {
            _consumerClient.Assign(_consumerClient.Assignment);
        }

        public void Dispose()
        {
            _consumerClient.Dispose();
        }

        #region private methods

        private void InitKafkaClient()
        {
            _kafkaSetting._setting["group.id"] = _groupId;

            var config = _kafkaSetting.AsKafkaSetting();
            _consumerClient = new Consumer<Ignore, string>(config, null, StringDeserializer);
            _consumerClient.OnConsumeError += ConsumerClient_OnConsumeError;
            _consumerClient.OnError += ConsumerClient_OnError;
            _consumerClient.OnMessage += _consumerClient_OnMessage;
        }

        private void _consumerClient_OnMessage(object sender, Message<Ignore, string> e)
        {
            Console.WriteLine(e.Value);
        }

        private void ConsumerClient_OnConsumeError(object sender, Message e)
        {
            var message = e.Deserialize<Null, string>(null, StringDeserializer);
            var logArgs = new LogMessageEventArgs
            {
                LogType = KafkaLogType.ConsumeError,
                Reason = $@"An error occurred during consume the message; Topic:'{e.Topic}',
                            Message:'{message.Value}', Reason:'{e.Error}'."
            };
            OnLog?.Invoke(sender, logArgs);
        }

        private void ConsumerClient_OnMessage(object sender, Message<Null, string> e)
        {
            var message = new MessageContext
            {
                Group = _groupId,
                Name = e.Topic,
                Content = e.Value
            };

            OnMessageReceived?.Invoke(sender, message);
        }

        private void ConsumerClient_OnError(object sender, Error e)
        {
            var logArgs = new LogMessageEventArgs
            {
                LogType = KafkaLogType.ServerConnError,
                Reason = e.ToString()
            };
            OnLog?.Invoke(sender, logArgs);
        }

        #endregion private methods
    }
}
