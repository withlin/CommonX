using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using CommonX.DataAccess.InMemory;
using CommonX.Logging;
using ExtendedXmlSerializer.Configuration;
using ExtendedXmlSerializer.ExtensionModel.Xml;

namespace CommonX.DataAccess.Xml
{
    public class XmlRepository<TEntity> : InMemoryRepository<TEntity>, IXmlRepository<TEntity, long>
        where TEntity : class, new()
    {
        private static readonly object SyncRoot = new object();
        /*  private readonly ExtendedXmlSerializer.Core. _xmlSerializer = new ExtendedXmlSerializer();*///可以注入AutoFac
        private readonly IExtendedXmlSerializer _xmlSerializer = new ConfigurationContainer()
                                              // Configure...
                                              .Create();

        private string fileName;
        /// <summary>
        /// 设置是否每次都读文件
        /// </summary>
        protected virtual bool needCache { get; set; } = true;

        public XmlRepository(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            AfterActionHandler += Save;
        }

        public string Path { get; set; }

        public override string KeyField
        {
            set
            {
                var blockStore = GetBlockStore(FileName);
                if(!needCache) blockStore.Clear();
                if (blockStore.Any()) return;
                if (!File.Exists(FileName)) return;

                string text = File.ReadAllText(FileName);

                var data = _xmlSerializer.Deserialize<List<TEntity>>(text);
                foreach (var entity in data)
                {
                    var keyValue = GetKeyValue(value, entity);
                    if (!blockStore.ContainsKey(keyValue))
                    {
                        blockStore.AddOrUpdate(keyValue, entity, (k, v) => entity);
                    }
                }
            }
        }

        public string FileName
        {
            get
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    return fileName;
                }
                if (string.IsNullOrEmpty(Path))
                    return typeof (TEntity).Name;
                return System.IO.Path.Combine(Path, string.Format("{0}.xml", typeof (TEntity).Name));
            }
            set
            {
                fileName = value;
                Path = fileName.Substring(0, fileName.LastIndexOf("/", StringComparison.Ordinal));
            }
        }

        private long GetKeyValue(string keyField, TEntity entity)
        {
            foreach (var p in typeof (TEntity).GetProperties())
            {
                if (p.Name == keyField)
                    return (long) p.GetValue(entity, null);
            }
            return default(long);   
        }

        private void Save()
        {
            lock (SyncRoot)
            {
                var blockStore = GetBlockStore(DefaultBlock);
                try
                {
                    var xml = _xmlSerializer.Serialize(blockStore.Values.ToList());
                    //IFileManager fileManager = new TxFileManager();
                    using (var scope = new TransactionScope())
                    {
                        //fileManager.WriteAllText(FileName, xml);
                        scope.Complete();
                    }
                }
                catch (Exception)
                {
                    //出错后清除内存
                    blockStore.Clear();
                    throw;
                }
                finally
                {

                }
            }
        }
    }
}