//using Apache.Ignite.Core;
//using JECommon.Cache.Ignite.Store.Streamers;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;

//namespace ECommon.Cache.Ignite.Store.CacheLoaders
//{
//    /// <summary>
//    /// 通用ignite缓存加载
//    /// </summary>
//    /// <typeparam name="TVal">需要branch，所以tval限定为BaseEntityObject</typeparam>
//    [Serializable]
//    public class CacheLoader<TVal> : CacheLoaderBase<TVal>  where TVal : BaseEntityObject, new()
//    {
//        private static string[] IgnoreBranch = new string[] { "DictItem_Info","StaffInfo"};
//        public CacheLoader(IIgnite ignite, IRepository<TVal, long> repository, DbContext dbContext, ILoggerFactory loggerFactory):base(ignite,repository,dbContext,loggerFactory)
//        {
            
//        }
        

//        public override int Load(bool isOverwrite = false)
//        {
//            try
//            {

//                int count = 0;
//                using (var ds = new IgniteDataForUpdatingStreamer<long, TVal>(_ignite, isOverwrite, CacheName))
//                {
//                    IEnumerable<TVal> list;
//                    ds.Prepare();
//                    if (IgnoreBranch.Contains(typeof(TVal).Name))
//                    {
//                        list = _repository.Find(c => c.DeleteFlag == 0, false);
//                    }
//                    else
//                    {
//                        list = _repository.Find(c => Branches.Contains(c.BranchID) && c.DeleteFlag == 0, false);
//                    }

//                    //放入缓存
//                    foreach (var val in list)
//                    {
//                        if (val == null) continue;
//                        ds.AddData(val.PK, val);
//                        count++;
//                        if (count % 1000 == 0)
//                            Console.WriteLine(CacheName + @"：" + count);
//                    }
//                    Console.WriteLine($@"{TargetEntityTypeName} load completed for {count} records.");
//                    ds.Flush();
//                }
//                return count;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex);
//                _logger.Error(ex);
//            }

//            return 0;
//        }
//    }

//    /// <summary>
//    /// 通用ignite缓存加载
//    /// </summary>
//    /// <typeparam name="TVal"></typeparam>
//    [Serializable]
//    public class CacheLoaderBase<TVal> : ICacheLoader<long, TVal> where TVal : BaseObject, new()
//    {
//        protected readonly IIgnite _ignite;
//        protected readonly IRepository<TVal, long> _repository;
//        protected readonly DbContext _dbContext;
//        protected readonly ILogger _logger;

//        public CacheLoaderBase(IIgnite ignite, IRepository<TVal, long> repository, DbContext dbContext, ILoggerFactory loggerFactory)
//        {
//            _ignite = ignite;
//            _repository = repository;
//            _dbContext = dbContext;
//            _logger = loggerFactory.Create(GetType());
//            Branches = ConfigurationManager.AppSettings["Branch"].Split(',').ToList();
//            Branches.Add("ZDA");
//        }

//        public string TargetEntityTypeName => typeof(TVal).FullName;
//        public string CacheName => typeof(TVal).Name;
//        public string TargetKeyTypeName => typeof(long).FullName;
//        public List<string> Branches { get; set; }

//        public bool IsAsync { get; set; }

//        public virtual int Load(bool isOverwrite = false)
//        {
//            try
//            {

//                int count = 0;
//                using (var ds = new IgniteDataForUpdatingStreamer<long, TVal>(_ignite, isOverwrite, CacheName))
//                {
//                    ds.Prepare();

//                    var list = _repository.GetAll();

//                    //放入缓存
//                    foreach (var val in list)
//                    {
//                        if (val == null) continue;
//                        ds.AddData(val.PK, val);
//                        count++;
//                        if (count % 1000 == 0)
//                            Console.WriteLine(CacheName + @"：" + count);
//                    }
//                    Console.WriteLine($@"{TargetEntityTypeName} load completed for {count} records.");
//                    ds.Flush();
//                }
//                return count;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex);
//                _logger.Error(ex);
//            }

//            return 0;
//        }
//    }

//}
