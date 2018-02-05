using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Cache.Configuration
{
    public interface ICacheManagersEntry
    {
        String Name
        {
            get;
            set;
        }

        Boolean UseDataCacheClientConfigruation
        {
            get;
            set;
        }

        Boolean LocalCacheEnabled
        {
            get;
            set;
        }

        String DataCacheLocalCacheInvalidationPolicy
        {
            get;
            set;
        }

        Int32 DefaultTimeOut
        {
            get;
            set;
        }

        Int32 ObjectCount
        {
            get;
            set;
        }

    }
}
