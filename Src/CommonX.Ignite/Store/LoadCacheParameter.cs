using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonX.Cache.Ignite.Store
{
    [Serializable]
    public class LoadCacheParameter
    {
        public string[] TypeNames { get; set; }
        public string CacheName { get; set; }
        /// <summary>
        /// 是否在装载的时候以覆盖的形式进行，而不是clear先
        /// </summary>
        public bool IsOverwrite { get; set; }
        public string[] ToStringArrays()
        {
            return (from x in TypeNames select x).Union<string>(new string[] { CacheName }).ToArray();
        }
        public static LoadCacheParameter Parse(string[] parameters)
        {
            var l = new LoadCacheParameter()
            {
                TypeNames = parameters.Take(parameters.Length > 1 ? (parameters.Length - 1) : 1).ToArray(),
                CacheName = parameters[parameters.Length - 1]
            };
            return l;
        }
    }
}