using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.ServiceBus.MassTransit
{
    public static class Utils
    {
        public static Uri GetFullUri(string destination, bool isFullUri = false)
        {
            return
                new Uri(Configurations.Configuration.Instance.Setting.TransportHost + destination + (isFullUri ? "" : ("-" +
                        Environment.MachineName)));
        }
    }
}
