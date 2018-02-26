using CommonX.Configurations;
using CommonX.Log4Net;
using System;
using System.Reflection;
using CommonX.Quartz;
using JetBrains.Annotations;
using System.Threading;
using CommonX.Components;
using CommonX.Logging;

namespace Quartz.Test.NetCore

{
    class Program
    {
        [NotNull] static IScheduler _scheduler;
        [NotNull] static ILogger _logger;
        static void Main(string[] args)
        {
            BootStrap();

            var job = JobBuilder.Create<TestJob>().WithIdentity("test", "test-job").Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("test", "test-job")
                .StartNow()
                .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(1)).Build();

            var cts = new CancellationTokenSource();

            _scheduler.ScheduleJob(job, trigger, cts.Token);

            _scheduler.Start().Wait();

            Console.WriteLine("schedulejob is starting");
            Console.ReadKey();
            

        }
        

        static void BootStrap()
        {
            var assambly = Assembly.GetAssembly(typeof(Program));
            var config = Configuration.Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .UseQuartz(new Assembly[] { assambly });


            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
            {
                _logger = scope.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
                _scheduler = scope.Resolve<IScheduler>();
            }
        }

        
    }
}
