using CommonX.Logging;
using CommonX.Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Test.NetCore
{
    public class TestJob : BaseJob
    {
        public TestJob(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _logger = loggerFactory.Create(GetType());
        }

        public override Task ExecJob(IJobExecutionContext context)
        {
            _logger.Info("job is start...");

            //ToDo Something

            return Task.FromResult(0);
        }
    }
}
