using CommonX.Autofac;
using CommonX.Components;
using CommonX.Configurations;
using CommonX.Logging;
using System;
using System.Reflection;
using CommonX.ServiceBus.MassTransit.Configuration;
using CommonX.ServiceBus;
using CommonX.Log4Net;
using CommonX.RabbitMQ;
using CommonX.Kafka;
using CommonX.Kafka.Impl;
using CommonX.EventBus;
using CommonX.EventBus.Events;
using CommonX.EventBus.Abstractions;
using Confluent.Kafka;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using CommonX.Quartz;
using CommonX.Cache.Redis;

namespace ServiceBus.TestPublish.NetCore
{
    public class RequestIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
    }
    public class RequestIntegrationEventHandler : IIntegrationEventHandler<RequestIntegrationEvent>
    {
        public async Task Handle(RequestIntegrationEvent @event)
        {
            Console.WriteLine(@event.Id);
            Console.WriteLine(@event.CreationDate);
            Console.WriteLine(@event.Message);
            Console.WriteLine(@event.Id);
            await Task.FromResult(0);
        }
    }

    class Program
    {
        private static IBus _bus;
        private static ILogger _logger;
        private static IConnectionPool _conn;
        private static IConsumerClientFactory factory;
        static void Main(string[] args)
        {
            //Producer aa = new Producer(new KafkaSetting().AsKafkaSetting());
            Setup();
            List<string> topics = new List<string>();
           var prodocer= _conn.Rent();
            var aa = Encoding.Default.GetBytes("aaaa");
            prodocer.ProduceAsync("test1", null, aa);
            topics.Add("test1");
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken tok = source.Token;
            factory.Create("simple-csharp-consumer").Subscribe(topics);
            factory.Create("simple-csharp-consumer").OnMessageReceived += Program_OnMessageReceived;
            Task t = new Task(() =>
            {
                factory.Create("simple-csharp-consumer").Listening(TimeSpan.FromSeconds(1), tok);
            }, tok);

            t.Start();
           


            _bus.Send("ServiceBus.TestPublish.NetCore", new RequestIntegrationEvent() { Message = "Masstransit test Succees!" });
            _logger.Info("ServiceBus Logs test!!");
            Console.WriteLine("Masstransit test Succees！");
            Console.ReadKey();
        }

        private static void Program_OnMessageReceived(object sender, CommonX.Models.MessageContext e)
        {
            Console.WriteLine(e.Content);
            Console.WriteLine(e.Name);
        }

        public static Func<RequestIntegrationEventHandler> Test()
        {
            return () => new RequestIntegrationEventHandler();
        }
            
        public static void Setup()
        {
            var assambly = Assembly.GetAssembly(typeof(Program));
            var config = Configuration.Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .UseRabbitMQ("localshot", "/", "guest", "guest")
                .UseMassTransit(new[] { assambly })
                .UseKafka()
                .UseRedis();
            //_logger = ObjectContainer.Current.BeginLifetimeScope().Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            using (var log = ObjectContainer.Current.BeginLifetimeScope())
            {
                _logger = log.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            }
            //config.SetDefault<IRequestClient<Request, RequestResult>, RequestClient<Request, RequestResult>>();
            using (var scope = ObjectContainer.BeginLifetimeScope())
            {

                _bus = scope.Resolve<IBus>();
            }
            using (var scope = ObjectContainer.BeginLifetimeScope())
            {

                //_eventBus = scope.Resolve<IEventBus>();
                _conn = scope.Resolve<IConnectionPool>();
                factory= scope.Resolve<IConsumerClientFactory>();
            }

        }
    }
}
