using System.Collections.Generic;
using System.Net;
using CommonX.Remoting;
using CommonX.Socketing;

namespace CommonX.Extensions
{
    public static class RemotingClientUtils
    {
        public static IEnumerable<SocketRemotingClient> ToRemotingClientList(this IEnumerable<IPEndPoint> endpointList, SocketSetting socketSetting)
        {
            var remotingClientList = new List<SocketRemotingClient>();
            foreach (var endpoint in endpointList)
            {
                var remotingClient = new SocketRemotingClient(endpoint, socketSetting);
                remotingClientList.Add(remotingClient);
            }
            return remotingClientList;
        }
    }
}
