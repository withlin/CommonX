//using Apache.Ignite.Core;
//using Apache.Ignite.Core.Cache.Query;
//using JZTERP.Common.Shared.Entities.Common.Common;
//using JZTERP.Frameworks.Common.Cache.Ignite.CommonCache;
//using JZTERP.Frameworks.Common.FastMember;
//using JZTERP.Frameworks.Common.Logging;
//using JZTERP.Frameworks.DataAccess;
//using JZTERP.Frameworks.DataAccess.EF;
//using RabbitMQ.Client;
//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Diagnostics;
//using System.Linq;
////using Oracle.ManagedDataAccess.Client;

//namespace ECommon.Cache.Ignite.Store.Streamers
//{
//    /// <summary>
//    ///     通用数据ogg更新
//    ///     只支持有pk的
//    /// </summary>
//    /// <typeparam name="TVal"></typeparam>
//    public class DbChangeProcessor<TVal> : IDbChangeProcessor<long, TVal> where TVal : BaseObject, new()
//    {
//        private readonly IIgnite _ignite;
//        private readonly ILogger _logger;
//        private readonly DbContext _dbContext;
//        private readonly IRepository<TVal, long> _repository;
//        private List<string> alreadyNav = new List<string>();
//        private readonly string schema;
//        private IAssignment _assignment;
//        private Dictionary<string, string> mapping;

//        public DbChangeProcessor(IIgnite ignite, ILoggerFactory loggerFactory, DbContext dbContext, IRepository<TVal, long> repository)
//        {
//            _ignite = ignite;
//            _dbContext = dbContext;
//            _repository = repository;
//            _logger = loggerFactory.Create(GetType());
//            _assignment = new Assignment();
//            //有可能有子表，
//            schema = _dbContext.Database.Connection.ConnectionString.Split(';')[0].Split('=')[1].ToUpper();
//            Table = GetCacheTable(typeof(TVal));
//            //加载数据库数据
//            //var nav = _unitOfWork.DbContext.GetNavigationPropertiesForType(typeof(TVal));
//            //foreach (var navigationProperty in nav)
//            //{
//            //    _repository.Include(navigationProperty.Name);
//            //}
//            mapping = _dbContext.GetCommonEntityMapping(typeof(TVal));
//        }

//        public string CacheName => typeof(TVal).Name;
//        public CacheTable Table { get; set; }
//        public string[] ChildTables { get; set; }

//        public void Execute(OggEntry entry)
//        {
//            try
//            {
//                var cdWatch = new Stopwatch();
//                cdWatch.Start();
//                OnExecute(entry);
//                cdWatch.Stop();
//                if (_logger.IsTraceOn)
//                    _logger.Trace(string.Format("DbChangeProcessor Execute of {1} {2} consume elapse {0} ms",
//                        cdWatch.ElapsedMilliseconds,
//                        CacheName, entry.Op));
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex);
//                _logger.Error(ex);
//            }
//        }

//        /// <summary>
//        /// 绑定消息队列
//        /// </summary>
//        public IEnumerable<string> Binding(IModel channel, string queueName, string exchangeName)
//        {
//            return Binding(channel, queueName, exchangeName, Table);
//        }



//        /// <summary>
//        ///     具体处理，
//        ///     特殊处理，请override
//        /// </summary>
//        /// <param name="entry"></param>
//        protected virtual void OnExecute(OggEntry entry)
//        {
//            var cache = _ignite.GetCache<long, TVal>(CacheName);
//            var pk = long.Parse(entry.Op == DbOperation.DELETE ? (entry.Before["PK"] + "") : (entry.After["PK"] + ""));
//            if (entry.Before.ContainsKey("DeleteFlag")&& entry.After.ContainsKey("DeleteFlag"))
//            {
//                var dbefore = (int)entry.Before["DeleteFlag"];
//                var dafter = (int)entry.After["DeleteFlag"];
//                if(dbefore == 0&&dafter == 1)
//                {
//                    entry.Op = DbOperation.DELETE;
//                }
//            }
//            if (entry.Before.ContainsKey("IsActive")&&entry.After.ContainsKey("IsActive"))
//            {
//                var dbefore = entry.Before["IsActive"].ToString();
//                var dafter = entry.After["IsActive"].ToString();
//                if (dbefore.Equals("01", StringComparison.InvariantCultureIgnoreCase) 
//                    && !dafter.Equals("01", StringComparison.InvariantCultureIgnoreCase))
//                {
//                    entry.Op = DbOperation.DELETE;
//                }
//            }
//            Dictionary<string, object> changedProperties = new Dictionary<string, object>();
//            var access = TypeAccessor.Create(typeof(TVal));
//            if (mapping.Count == 0)
//            {
//                //如果从ef中没有读出实体和表的映射关系，则使用实体的属性名替代
//                MemberSet members = access.GetMembers();
//                foreach (var m in members)
//                {
//                    mapping.Add(m.Name, m.Name);
//                }
//            }
//            TVal cacheEntity = null ;
//            if (cache.ContainsKey(pk))
//                cacheEntity = cache.Get(pk);
//            switch (entry.Op)
//            {
//                case DbOperation.INSERT:
//                    if (cacheEntity != null)
//                        throw new ArgumentNullException("DbChangeProcessor OnExecute " + typeof(TVal).FullName + " exsit");
//                    changedProperties = entry.After;
//                    MemberSet insert_entityMembers = access.GetMembers();
//                    var newEntity = access.CreateNew() as TVal;

//                    foreach (var item in changedProperties)
//                    {
//                        if (mapping.ContainsKey(item.Key.ToUpper()))
//                        {
//                            string oggPropertyName = mapping[item.Key.ToUpper()];
//                            Member member = insert_entityMembers.FirstOrDefault(m => m.Name.ToUpper() == oggPropertyName.ToUpper());
//                            if (member != null)
//                                this.AssignValue(access, newEntity, member, item.Value);
//                        }
//                    }

//                    cache.Put(pk, newEntity);
//                    break;
//                case DbOperation.UPDATE:
//                    if (cacheEntity == null)
//                    {
//                        cacheEntity = _dbContext.Set<TVal>().Find(pk);
//                        if(cacheEntity != null)
//                        {
//                            cache.Put(pk, cacheEntity);
//                        }
//                    }
//                    else
//                    {
//                        changedProperties = GetChangedProperties(entry);
//                        MemberSet update_entityMembers = access.GetMembers();

//                        foreach (var item in changedProperties)
//                        {
//                            if (mapping.ContainsKey(item.Key.ToUpper()))
//                            {
//                                string oggPropertyName = mapping[item.Key.ToUpper()];
//                                Member member = update_entityMembers.FirstOrDefault(m => m.Name.ToUpper() == oggPropertyName.ToUpper());
//                                if (member != null)
//                                    this.AssignValue(access, cacheEntity, member, item.Value);
//                            }
//                        }

//                        cache.Put(pk, cacheEntity);
//                    }
//                    break;
//                case DbOperation.DELETE:
//                    cache.Remove(pk);
//                    break;
//            }
//        }

//        void AssignValue(TypeAccessor access, TVal entity, Member member, object value)
//        {
//            if (member.Type == typeof(long) || member.Type == typeof(long?))
//                access[entity, member.Name] = Convert.ToInt64(value);
//            if (member.Type == typeof(decimal) || member.Type == typeof(decimal?))
//                access[entity, member.Name] = Convert.ToDecimal(value);
//            if (member.Type == typeof(double) || member.Type == typeof(double?))
//                access[entity, member.Name] = Convert.ToDouble(value);
//            if (member.Type == typeof(Boolean) || member.Type == typeof(Boolean?))
//                access[entity, member.Name] = Convert.ToBoolean(value);
//            if (member.Type == typeof(Int32) || member.Type == typeof(Int32?))
//                access[entity, member.Name] = Convert.ToInt32(value);
//            if (member.Type == typeof(DateTime) || member.Type == typeof(DateTime?))
//            {
//                int separate_index = value.ToString().IndexOf(':');
//                int length = value.ToString().Length;
//                string date_part = value.ToString().Substring(0, separate_index);
//                string time_part = value.ToString().Substring(separate_index + 1, length - separate_index - 1);
//                access[entity, member.Name] = Convert.ToDateTime(string.Format("{0} {1}", date_part, time_part));
//            }
//            if (member.Type == typeof(string))
//                access[entity, member.Name] = value.ToString();
//        }

//        protected virtual Dictionary<string, object> GetChangedProperties(OggEntry entry)
//        {
//            Dictionary<string, object> changedProperties = new Dictionary<string, object>();

//            if (entry.Op == DbOperation.INSERT)
//                changedProperties = entry.After;
//            else if (entry.Op == DbOperation.DELETE)
//                changedProperties = entry.Before;
//            else if (entry.Op == DbOperation.UPDATE)
//            {
//                try
//                {
//                    if (entry.After?.Count > 0)
//                    {
//                        foreach (var item in entry.After)
//                        {
//                            if (entry.Before.ContainsKey(item.Key))
//                            {
//                                if (item.Value.ToString().Trim() == entry.Before[item.Key].ToString().Trim())
//                                    continue;
//                            }

//                            changedProperties.Add(item.Key, item.Value);
//                        }
//                    }
//                }
//                catch (KeyNotFoundException ex)
//                {
//                    ex.Data["BeforeKeys"] = string.Join(", ", entry.Before.Keys.ToArray());
//                    ex.Data["AfterKeys"] = string.Join(", ", entry.After.Keys.ToArray());
//                    throw ex;
//                }
//            }


//            return changedProperties;
//        }

//        #region protect method

//        /// <summary>
//        ///     获取实体表层级关系
//        /// </summary>
//        protected virtual CacheTable GetCacheTable(Type type)
//        {
//            var table = new CacheTable { MainTable = schema + "." + _dbContext.GetTableName(type) };

//            var pkField = _dbContext.GetPrimaryKeyFieldsFor(type).FirstOrDefault();
//            if (pkField != null)
//            {
//                table.PkField = pkField.Name;
//            }
//            //var nav = _unitOfWork.DbContext.GetNavigationPropertiesForType(type);
//            //foreach (var navigationProperty in nav)
//            //{
//            //    if(alreadyNav.Contains(navigationProperty.Name)) continue;
//            //    alreadyNav.Add(navigationProperty.Name);
//            //    var childType = TypeUtils.GetPureType(type.GetProperty(navigationProperty.Name).PropertyType);
//            //    //暂时只支持一级 
//            //    //var c = GetCacheTable(childType);
//            //    var c =  new CacheTable { MainTable = schema + "." + _unitOfWork.DbContext.GetTableName(childType) };
//            //    table.ChildTables.Add(c);
//            //}
//            return table;
//        }

//        protected virtual long GetPK(CacheTable table, OggEntry entry)
//        {
//            var pkField = table.PkField;
//            //if (entry.Table != table.MainTable && table.ChildTables.Any(c=>c.MainTable == entry.Table))
//            //{
//            //    //子表取FK
//            //    pkField = table.ChildTables.FirstOrDefault(c=>c.MainTable == entry.Table)?.FkField;
//            //}
//            if (entry.Op == DbOperation.DELETE)
//            {
//                if (entry.Before.ContainsKey(pkField))
//                    return long.Parse(entry.Before[pkField].ToString());

//            }
//            else
//            {
//                if (entry.After.ContainsKey(pkField))
//                    return long.Parse(entry.After[pkField].ToString());
//            }
//            string error = $"没有找到{table.MainTable}的主键字段{pkField}";
//            _logger.Error(error);
//            Debug.WriteLine(error);
//            return 0;
//        }

//        protected IEnumerable<string> Binding(IModel channel, string queueName, string exchangeName, CacheTable table)
//        {
//            //订阅消息类型
//            channel.QueueDeclare(queueName + "-" + table.MainTable, true, false, false, null);
//            channel.QueueBind(queue: queueName + "-" + table.MainTable, exchange: exchangeName, routingKey: table.MainTable);

//            return new string[] { table.MainTable };
//            //yield return table.MainTable;
//            //foreach (var child in table.ChildTables)
//            //{
//            //    var result = Binding(channel, queueName, exchangeName, child);
//            //    foreach (var s in result)
//            //    {
//            //        yield return s;
//            //    }
//            //}
//        }
//        #endregion
//    }


//}