using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Models
{
    public enum KafkaLogType
    {
        ConsumeError,
        ServerConnError
    }
    public class LogMessageEventArgs: EventArgs
    {
        public string Reason { get; set; }

        public KafkaLogType LogType { get; set; }
    }
}
