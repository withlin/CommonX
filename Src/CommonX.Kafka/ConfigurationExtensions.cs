using Autofac;
using CommonX.Autofac;
using CommonX.Components;
using CommonX.Configurations;
using CommonX.EventBus;
using CommonX.Kafka.Impl;
using CommonX.Logging;
namespace CommonX.Kafka
{
    public static class ConfigurationExtensions
    {
        public static Configuration UseKafka(this Configuration configuration)
        {
            configuration = configuration.SetDefault<IConnectionPool, ConnectionPool>(new ConnectionPool(new KafkaSetting()));
            configuration = configuration.SetDefault<IConsumerClientFactory, KafkaConsumerClientFactory>(new KafkaConsumerClientFactory(new KafkaSetting()));
            configuration = configuration.SetDefault<IConsumerClient, KafkaConsumerClient>();

            return configuration;
        }
    }
}
