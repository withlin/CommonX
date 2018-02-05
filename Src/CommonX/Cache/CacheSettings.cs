using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Cache
{
    public class CacheSettings
    {
        /// <summary>
        /// Cache Key format is {CacheKeyPrefix}_{CarrierCode}.{Key1}.{Key2}...{KeyN}
        /// For instance: BETA_CO.ABC.XXX 
        /// </summary>d
        public const char CacheKeySeparator = '.';
        public const char CachePrefixLinkedChar = '_';
    }
}
