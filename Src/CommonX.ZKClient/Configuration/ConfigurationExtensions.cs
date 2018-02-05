using CommonX.ZKClient;
using System;
using ZooKeeperNet;

namespace CommonX.Configuration
{
    /// <summary>
    ///     configuration class Autofac extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        ///     Use ZK Client  .
        /// </summary>
        /// <returns></returns>
        public static Configurations.Configuration UseZkClient(
            this Configurations.Configuration configuration, string host,int timeOut = 3, string root = "/jzterp")
        {
            var client = new ZooKeeper(host + root, TimeSpan.FromSeconds(timeOut), null);
            //client.Register(new Executor(client));
            configuration.SetDefault<IZooKeeper, ZooKeeper>(client); //SetDefault<IZooKeeper, ZooKeeper>(client);

            return configuration;
        }
    }
}