using CommonX.ZKClient.Watcher;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZooKeeperNet;

namespace CommonX.Watcher
{
    /// <summary>
    ///     maintains the connection
    /// </summary>
    public class Executor : IWatcher, DataMonitor.IDataMonitorListener
    {
        private static readonly object _lockObj = new object();
        private readonly DataMonitor _dm;
        private Thread _child;
        private string _znode;


        private readonly ZooKeeper _zk;

        public Executor(ZooKeeper zk, string znode = "/jzterp")
        {
            _zk = zk;
            _dm = new DataMonitor(_zk, znode, null, this);
        }

        public void Closing(int rc)
        {
            lock (_lockObj)
            {
                _zk.Dispose();
            }
        }

        public void Exists(byte[] data)
        {
            if (data == null)
            {
                if (_child != null)
                {
                    _child.Abort();
                    try
                    {
                        //_child.waitFor();
                    }
                    catch (Exception e)
                    {
                    }
                }
                _child = null;
            }
            else
            {
                if (_child != null)
                {
                    //_child.destroy();
                    try
                    {
                        //_child.waitFor();
                    }
                    catch (Exception e)
                    {
                    }
                }
                try
                {
                    //FileOutputStream fos = new FileOutputStream(filename);
                    //fos.write(data);
                    //fos.close();
                }
                catch (IOException e)
                {
                }
                try
                {
                    //child = Runtime.getRuntime().exec(exec);
                    //new StreamWriter(_child.getInputStream(), System.out);
                    //new StreamWriter(_child.getErrorStream(), System.err);
                }
                catch (IOException e)
                {
                }
            }
        }

        public  void Process(WatchedEvent ev)
        {
            _dm.Process(ev);
        }

        public void Run()
        {
            try
            {
                lock (_lockObj)
                {
                    while (!_dm.Dead)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}