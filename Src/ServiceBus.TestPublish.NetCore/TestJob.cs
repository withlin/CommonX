using CommonX.Logging;
using CommonX.Quartz;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBus.TestPublish.NetCore
{
    public class TestJob : BaseJob
    {
        public TestJob(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _logger = loggerFactory.Create(GetType());
        }
        public override Task ExecJob(IJobExecutionContext context)
        {
            Console.WriteLine("test-job");
            _logger.Info("test is Success!!");
            return Task.CompletedTask;
        }
    }
}
