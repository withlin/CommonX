using Apache.Ignite.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonX.Cache.Ignite
{
    [Serializable]
    public class StorePredicate<TKey, TEntity> : ICacheEntryFilter<TKey, TEntity>
    {
        public virtual bool Invoke(ICacheEntry<TKey, TEntity> entry)
        {
            return true;
        }
    }
}
