namespace ServiceBus.TestPublish.NetCore
{
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
    using CommonX.EventBus.Events;
    using CommonX.EventBus.Abstractions;
    using Confluent.Kafka;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using CommonX.Quartz;
    using CommonX.Cache.Redis;
    using Quartz;
    using System.Threading;

    public class RequestIntegrationEvent : IntegrationEvent
    {
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
        private static IKafkaPersisterConnection _consumerClient;
        private static IScheduler _scheduler;
        private static void HandlerMessage(Message<string, string> msg)
        {
            Console.WriteLine($"消息的值为{msg.Value}");
        }
        static void Main(string[] args)
        {
            Setup();




            //Console.WriteLine();
            //List<string> topics = new List<string>();
            //topics.Add("JTmdb_Fd_Good");
            //_consumerClient.Consume(topics, HandlerMessage);


            //_bus.Send("ServiceBus.TestPublish.NetCore", new RequestIntegrationEvent() { Message = "Masstransit test Succees!" });
            //_logger.Info("ServiceBus Logs test!!");
            //Console.WriteLine("Masstransit test Succees！");
            //Console.ReadKey();
        }

        public static void Setup()
        {
            var assambly = Assembly.GetAssembly(typeof(Program));
            var config = Configuration.Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .UseKafka("");
             config.UseQuartz(new Assembly[] { assambly });
            //.UseRabbitMQ("localhost","/","guest","guest")
            //.UseRedisCache()
            //.UseMassTransit(new Assembly[] { assambly })
            //.UseKafka("");

            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
            {
                _logger = scope.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
                //_bus = scope.Resolve<IBus>();
                //_conn = scope.Resolve<IConnectionPool>();
                //factory = scope.Resolve<IConsumerClientFactory>();
                _consumerClient = scope.Resolve<IKafkaPersisterConnection>();
                _scheduler = scope.Resolve<IScheduler>();
            }
        }
    }
}
