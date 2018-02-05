using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CommonX.Cache
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheObjectAttribute : System.Attribute
    {
        public CacheObjectAttribute() { }

        public CacheObjectAttribute(bool isSpecifyCacheName, string cacheName)
        {
            this.IsSpecifyCacheName = isSpecifyCacheName;
            this.CacheName = cacheName;
        }

        [DefaultValue(false)]
        public bool IsSpecifyCacheName { get; set; }
        [DefaultValue(null)]
        public string CacheName { get; set; }
    }
}
