using System.Threading.Tasks;
using Org.Apache.Zookeeper.Data;
using ZooKeeperNet;

namespace CommonX.ZKClient.Watcher
{
    /// <summary>
    ///     monitor data
    /// </summary>
    public class DataMonitor : IWatcher//, StatCallback
    {
        private readonly IWatcher _chainedWatcher;

        private readonly IDataMonitorListener _listener;
        private readonly ZooKeeper _zk;

        private readonly string _znode;

        private byte[] _prevData;

        public bool Dead;

        public DataMonitor(ZooKeeper zk, string znode, IWatcher chainedWatcher,
            IDataMonitorListener listener)
        {
            _zk = zk;
            _znode = znode;
            _chainedWatcher = chainedWatcher;
            _listener = listener;
            // Get things started by checking if the node exists. We are going
            // to be completely event driven
            zk.Exists(znode, true);
        }

        public  void Process(WatchedEvent ev)
        {
            string path = ev.Path;
            if (ev.Type == EventType.None)
            {
                // We are are being told that the state of the
                // connection has changed
                switch (ev.State)
                {
                    case KeeperState.SyncConnected:
                        // In this particular example we don't need to do anything
                        // here - watches are automatically re-registered with 
                        // server and any watches triggered while the client was 
                        // disconnected will be delivered (in order of course)
                        break;
                    case KeeperState.Expired:
                        // It's all over
                        Dead = true;
                        _listener.Closing((int) KeeperException.Code.SESSIONEXPIRED);
                        break;
                }
            }
            else
            {
                if (path != null && path == _znode)
                {
                    // Something has changed on the node, let's find out
                    _zk.Exists(_znode, true);
                }
            }
            _chainedWatcher?.Process(ev);
        }

        public void processResult(KeeperException.Code rc, string path, object ctx, Stat stat)
        {
            bool exists;
            switch (rc)
            {
                case KeeperException.Code.OK:
                    exists = true;
                    break;
                case KeeperException.Code.NONODE:
                    exists = false;
                    break;
                case KeeperException.Code.SESSIONEXPIRED:
                case KeeperException.Code.NOAUTH:
                    Dead = true;
                    _listener.Closing((int) rc);
                    return;
                default:
                    // Retry errors
                    _zk.Exists(_znode, true);
                    return;
            }

            byte[] b = null;
            if (exists)
            {
                try
                {
                    b = _zk.GetData(_znode, false, stat);
                }
                catch (KeeperException e)
                {
                    // We don't need to worry about recovering now. The watch
                    // callbacks will kick off any exception handling
                }
            }
            if ((b == null && b != _prevData)
                || (b != null && b != _prevData))
            {
                _listener.Exists(b);
                _prevData = b;
            }
        }

        /// <summary>
        ///     Other classes use the DataMonitor by implementing this method
        /// </summary>
        public interface IDataMonitorListener
        {
            /**
             * The existence status of the node has changed.
             */
            void Exists(byte[] data);

            /**
             * The ZooKeeper session is no longer valid.
             *
             * @param rc
             *                the ZooKeeper reason code
             */
            void Closing(int rc);
        }
    }
}