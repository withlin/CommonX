using CommonX.Components;
using CommonX.Configurations;
using CommonX.ServiceBus;
using CommonX.ServiceBus.MassTransit.Impl;
using System;
using System.Reflection;
using System.Threading.Tasks;
using CommonX.ServiceBus.MassTransit.Configuration;
using System.Threading;
using CommonX.Log4Net;
using CommonX.Logging;

namespace ServiceBus.Test.Publish
{
    public class Request
    {
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
    }
    public class RequestResult
    {
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
    }
    public class HpImplementation
    {
        /// <summary>
        /// </summary>
        //[LookupQuery("JZTERP.Service.BasisService.IDictService, JZTERP.Service.BasisService", "GetDictName", "SPDBusiType")]
        public string SystemVersion { get; set; }
    }
    class Program
    {
        private static IBus _bus;
        static void Main(string[] args)
        {
            Setup();
            _bus.Send("netcoretest", new Request() { Message = "test" });
            Console.ReadKey();
        }
        public static void Setup()
        {
            var assambly = Assembly.GetAssembly(typeof(Program));
            var config = Configuration.Create().UseAutofac();
            config.RegisterCommonComponents();
            config.UseLog4Net();
            
            config.SetDefault<ILoggerFactory, Log4NetLoggerFactory>();
            using (var scope = ObjectContainer.BeginLifetimeScope())
            {
               var a = scope.Resolve<ILoggerFactory>();
            }
            config.UseMassTransit(new[] { assambly });

            config.SetDefault<IRequestClient<Request, RequestResult>, RequestClient<Request, RequestResult>>();
            using (var scope = ObjectContainer.BeginLifetimeScope())
            {
                _bus = scope.Resolve<IBus>();
            }
        }
    }
}
