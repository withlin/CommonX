using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Cache
{
    public class CacheStatus
    {
        public CacheStatus(string key, DateTime now, DateTimeOffset expiration)
        {
            Key = key;
            this.AddTime = now;
            this.TimeOut = expiration - AddTime;
        }

        public string Key { get; set; }
        public DateTime AddTime { get; set; }
        public int Count { get; set; }
        public TimeSpan TimeOut { get; set; }


    }
}
