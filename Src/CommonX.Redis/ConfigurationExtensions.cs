using CommonX.Cache;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Cache.Redis
{
    /// <summary>configuration class Redis extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>Use Redis to implement the memory cache.
        /// </summary>
        /// <returns></returns>
        public static Configurations.Configuration UseRedisCache(this Configurations.Configuration configuration)
        {
            return UseRedisCache(configuration, "127.0.0.1", 6379);
        }
        /// <summary>Use Redis to implement the memory cache.
        /// </summary>
        /// <returns></returns>
        public static Configurations.Configuration UseRedisCache(this Configurations.Configuration configuration, string host, int port)
        {
            var redis = ConnectionMultiplexer.Connect(host + ":" + port).GetDatabase(1);
            configuration.SetDefault<IDatabase, IDatabase>(redis);
            configuration.SetDefault<ICacheFactory, RedisCacheFactory>();
            return configuration;
        }
    }
}
