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
using Confluent.Kafka.Serialization;

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
           
        }

        public static void Run_Consume(string topics)
        {
            using (var consumer= factory.Create("simple-csharp-consumer") as Consumer<Ignore, string>)
            {
                // Note: All event handlers are called on the main thread.

                consumer.OnPartitionEOF += (_, end)
                    => Console.WriteLine($"Reached end of topic {end.Topic} partition {end.Partition}, next message will be at offset {end.Offset}");

                consumer.OnError += (_, error)
                    => Console.WriteLine($"Error: {error}");

                consumer.OnConsumeError += (_, error)
                    => Console.WriteLine($"Consume error: {error}");

                consumer.OnPartitionsAssigned += (_, partitions) =>
                {
                    Console.WriteLine($"Assigned partitions: [{string.Join(", ", partitions)}], member id: {consumer.MemberId}");
                    consumer.Assign(partitions);
                };

                consumer.OnPartitionsRevoked += (_, partitions) =>
                {
                    Console.WriteLine($"Revoked partitions: [{string.Join(", ", partitions)}]");
                    consumer.Unassign();
                };

                consumer.OnStatistics += (_, json)
                    => Console.WriteLine($"Statistics: {json}");

                consumer.Subscribe(topics);

                Console.WriteLine($"Started consumer, Ctrl-C to stop consuming");

                var cancelled = false;
                Console.CancelKeyPress += (_, e) => {
                    e.Cancel = true; // prevent the process from terminating.
                    cancelled = true;
                };

                while (!cancelled)
                {
                    Message<Ignore, string> msg;
                    if (!consumer.Consume(out msg, TimeSpan.FromMilliseconds(100)))
                    {
                        continue;
                    }

                    Console.WriteLine($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");

                    if (msg.Offset % 5 == 0)
                    {
                        Console.WriteLine($"Committing offset");
                        var committedOffsets = consumer.CommitAsync(msg).Result;
                        Console.WriteLine($"Committed offset: {committedOffsets}");
                    }
                }
            }
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
                .UseMassTransit(new[] { assambly })
                .UseKafka();

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
                factory = scope.Resolve<IConsumerClientFactory>();
            }

        }
    }
}
