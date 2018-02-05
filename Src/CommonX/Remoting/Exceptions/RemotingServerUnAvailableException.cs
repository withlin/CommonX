using System;
using System.Net;

namespace CommonX.Remoting.Exceptions
{
    public class RemotingServerUnAvailableException : Exception
    {
        public RemotingServerUnAvailableException(EndPoint serverEndPoint)
            : base(string.Format("Remoting server is unavailable, server address:{0}", serverEndPoint))
        {
        }
    }
}
