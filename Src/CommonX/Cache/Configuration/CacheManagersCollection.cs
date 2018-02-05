using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CommonX.Cache.Configuration
{
    /// <summary>
    ///     <para>
    ///         NOTE: This class is intended to be used by the logging framework
    ///         and not directly from application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class is used to maintain a collection of <see cref="CacheManagersEntry" /> settings.
    ///     </para>
    /// </remarks>
    [Serializable]
    public class CacheManagersCollection : ConfigurationElementCollection
    {
        public object Get(string name)
        {
            return this.BaseGet((object)name);
        }

        public bool Contains(string name)
        {
            return this.BaseGet((object)name) != null;
        }
        protected override ConfigurationElement CreateNewElement()
        {
            return new CacheManagersEntry();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as CacheManagersEntry).Name;
        }
    }
}
