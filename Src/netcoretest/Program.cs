using CommonX.Components;
using CommonX.Configurations;
using CommonX.Log4Net;
using CommonX.Logging;
using System;
using System.Reflection;
using CommonX.ServiceBus.MassTransit.Configuration;
using CommonX.ServiceBus;
using CommonX.ServiceBus.MassTransit.Impl;
using System.Threading.Tasks;
using System.Threading;
using CommonX.Autofac;

namespace netcoretest
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
    [Component]
    class Program
    {
        private static IBus _bus;
        private static ILogger _logger;
        static void Main(string[] args)
        {
            Setup();

            _bus.Send("netcoretest", new Request() { Message = "Masstransit test Succees!" });

            Console.WriteLine("Masstransit test Succees！");
                _logger.Info("Masstransit test Succees...........！");

            _logger.Error("Masstransit test Succees！");
            _logger.Warn("Masstransit test Succees！");
            Console.ReadKey();
        }
        public static void Setup()
        {
            var assambly = Assembly.GetAssembly(typeof(Program));
            var config = Configuration.Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net();
            config.UseMassTransit(new[] { assambly });
            var objectContainer = new AutofacObjectContainer();
            _logger = ObjectContainer.Current.BeginLifetimeScope().Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            var log = ObjectContainer.Current.BeginLifetimeScope();
            _logger=log.Resolve<ILoggerFactory>().Create(typeof(Program).Name);


            //config.SetDefault<IRequestClient<Request, RequestResult>, RequestClient<Request, RequestResult>>();
            using (var scope = ObjectContainer.BeginLifetimeScope())
            {

                _bus = scope.Resolve<IBus>();
            }
        }
    }
}
