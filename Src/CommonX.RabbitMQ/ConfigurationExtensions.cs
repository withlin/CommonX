
namespace Shared.MiddleWare.RabbitMQ
{
    using CommonX.Components;
    using CommonX.Configurations;
    using CommonX.Logging;
    using CommonX.Serializing;
    using global::RabbitMQ.Client;

    public static class ConfigurationExtensions
    {
        public static Configuration UseRabbitMQ(this Configuration configuration,string hostName,string virtualHost,string userName,string passWord, int port =5672)
        {

            using (var scope = ObjectContainer.BeginLifetimeScope())
            {
                var logger = scope.Resolve<ILoggerFactory>();
                var jsonSerializer = scope.Resolve<IJsonSerializer>();

                var factory = new ConnectionFactory()
                {
                    HostName = hostName??"localhost",
                    Port = port,
                    VirtualHost =virtualHost??"/",
                    UserName = userName??"guest",
                    Password = passWord??"guest",
                    AutomaticRecoveryEnabled = true,
                    RequestedConnectionTimeout = 15000
                };
                DefaultRabbitMQPersistentConnection conn = new DefaultRabbitMQPersistentConnection(factory, logger, jsonSerializer);

                configuration = configuration.SetDefault<IRabbitMQPersistentConnection, DefaultRabbitMQPersistentConnection>(conn);

            }
            return configuration;
        }
    }
}
