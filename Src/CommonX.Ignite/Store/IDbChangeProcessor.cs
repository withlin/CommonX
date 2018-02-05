using System.Collections.Generic;
using CommonX.Components;
using CommonX.Logging;
using RabbitMQ.Client;

namespace CommonX.Cache.Ignite.Store
{
    public interface IDbChangeProcessor
    {
        ILoggerFactory LoggerFactory { get; set; }
        IScope Scope { get; set; }
        string CacheName { get; set; }
        bool Delete { get; set; }
        string EntityType { get; set; }
        bool Insert { get; set; }
        string Table { get; set; }
        bool Update { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry">OggEntry</param>
        void Execute(object entry);
    }
    public interface ICommonDbChangeProcessor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry">OggEntry</param>
        void Execute(OggEntry entry);

        /// <summary>
        /// 绑定消息队列
        /// </summary>
        IEnumerable<string> Binding(IModel channel, string queueName, string exchangeName);
    }
    public interface IDbChangeProcessor<TKey,TVal>: ICommonDbChangeProcessor
    {
    }

    /// <summary>
    ///     表层级关系
    /// </summary>
    public class CacheTable
    {
        public string MainTable { get; set; }
        public string PkField { get; set; } = "PK";
        public string FkField { get; set; } = "FK";
        public List<CacheTable> ChildTables { get; set; } = new List<CacheTable>();
    }
}