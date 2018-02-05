using Apache.Ignite.Core;
using Apache.Ignite.Core.Datastream;
using System;

namespace JECommon.Cache.Ignite.Store.Streamers
{

    public class IgniteDataForUpdatingStreamer<TK, TV> : IDisposable
    {
        private IDataStreamer<TK, TV> _streamer;
        private string cacheName;
        private IIgnite ignite;
        public IgniteDataForUpdatingStreamer(IIgnite ignite, bool isOverwrite, string cacheName)
        {
            this.ignite = ignite;
            _streamer = ignite.GetDataStreamer<TK, TV>(cacheName);
            _streamer.AllowOverwrite = isOverwrite;
            this.cacheName = cacheName;
        }
        public void Prepare()
        {
            if (!_streamer.AllowOverwrite)
            {
                //clear all data 
                var cache = ignite.GetCache<TK, TV>(cacheName);
                cache.Clear();
            }
        }

        public void AddData(TK key, TV value)
        {
            _streamer.AddData(key, value);
        }

        public void Flush()
        {
            _streamer.Flush();
        }

        public void Dispose()
        {
            if (_streamer != null)
            {
                _streamer.Close(false);
            }
        }
    }
}
