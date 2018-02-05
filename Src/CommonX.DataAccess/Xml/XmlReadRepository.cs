using CommonX.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace CommonX.DataAccess.Xml
{
    public class XmlReadRepository<TEntity> : XmlRepository<TEntity>, IXmlRepository<TEntity, long>
        where TEntity : class, new()
    {
        /// <summary>
        /// 设置是否每次都读文件
        /// </summary>
        protected override bool needCache { get; set; } = false;
        public XmlReadRepository(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }
        
        
    }
}