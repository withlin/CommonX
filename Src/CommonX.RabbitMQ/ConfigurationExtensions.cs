
namespace CommonX.RabbitMQ
{
    using CommonX.Components;
    using CommonX.Configurations;
    using CommonX.Logging;
    using CommonX.Serializing;
    using global::RabbitMQ.Client;
    using System;
    using System.Linq.Expressions;

    public static class ConfigurationExtensions
    {
        public static Configuration UseRabbitMQ(this Configuration configuration, Action<Setting> rabbitmqSetting=null)
        {

            using (var scope = ObjectContainer.BeginLifetimeScope())
            {
                var logger = scope.Resolve<ILoggerFactory>();
                var jsonSerializer = scope.Resolve<IJsonSerializer>();
                var setting = new Setting();
                rabbitmqSetting(setting);

                var factory = new ConnectionFactory()
                {

                    HostName = setting.HostName,
                    Port = setting.Port,
                    VirtualHost = setting.VirtualHost,
                    UserName = setting.UserName,
                    Password = setting.Password,
                    AutomaticRecoveryEnabled = setting.AutomaticRecoveryEnabled,
                    RequestedConnectionTimeout = setting.RequestedConnectionTimeout
                };
                DefaultRabbitMQPersistentConnection conn = new DefaultRabbitMQPersistentConnection(factory, logger, jsonSerializer);

                configuration = configuration.SetDefault<IRabbitMQPersistentConnection, DefaultRabbitMQPersistentConnection>(conn);

            }
            return configuration;
        }
    }
}
