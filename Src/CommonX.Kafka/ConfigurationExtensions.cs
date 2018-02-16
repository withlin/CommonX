using Autofac;
using CommonX.Autofac;
using CommonX.Components;
using CommonX.Configurations;
using CommonX.EventBus;
using CommonX.Kafka.Impl;
using CommonX.Logging;
using System.Collections.Generic;

namespace CommonX.Kafka
{
    public static class ConfigurationExtensions
    {
        public static Configuration UseKafka(this Configuration configuration, string gropId, IEnumerable<KeyValuePair<string, object>> kafkaSetting = null)
        {
            configuration = configuration.SetDefault<IConnectionPool, ConnectionPool>(new ConnectionPool(new KafkaSetting()));
            configuration = configuration.SetDefault<IConsumerClientFactory, KafkaConsumerClientFactory>(new KafkaConsumerClientFactory(new KafkaSetting()));
            configuration = configuration.SetDefault<IConsumerClient, KafkaConsumerClient>();
            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
            {
                var loggerFactory = scope.Resolve<ILoggerFactory>();
                configuration = configuration.SetDefault<IKafkaPersisterConnection, KafkaDefaultPersisterConnection>(new KafkaDefaultPersisterConnection(loggerFactory, gropId, kafkaSetting));
            }
                

            return configuration;
        }
    }
}
