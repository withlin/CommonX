using CommonX.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace CommonX.Quartz
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public abstract class BaseJob : IJob
    {
        protected ILogger _logger;

        public BaseJob(ILoggerFactory loggerFactory) {
            _logger = loggerFactory.Create(GetType());
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.Debug($"任务开始执行：任务名[{context.JobDetail.Key.Name}]-任务组[{context.JobDetail.Key.Group}]-触发器名[{context.Trigger.Key.Name}]-触发器组[{context.Trigger.Key.Group}]！开始时间UTC：{SystemTime.UtcNow().AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}");
            try
            {
               await ExecJob(context);
            }
            catch (Exception ex) {
                _logger.Error($"任务执行过程中发生异常：{context.JobDetail.Key.Name}-{context.JobDetail.Key.Group}！异常信息：{ex.Message} Stack:{ex.StackTrace}");
            }

            if (context.Trigger.GetNextFireTimeUtc() != null)
            {
                _logger.Debug($"任务执行完毕：任务名[{context.JobDetail.Key.Name}]-任务组[{context.JobDetail.Key.Group}]-触发器名[{context.Trigger.Key.Name}]-触发器组[{context.Trigger.Key.Group}]！下次执行时间UTC：{context.Trigger.GetNextFireTimeUtc().Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            else
            {
                _logger.Debug($"任务执行完毕：任务名[{context.JobDetail.Key.Name}]-任务组[{context.JobDetail.Key.Group}]-触发器名[{context.Trigger.Key.Name}]-触发器组[{context.Trigger.Key.Group}]");
            }
        }

        public abstract  Task  ExecJob(IJobExecutionContext context);
    }
}
