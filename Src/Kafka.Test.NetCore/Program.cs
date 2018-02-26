using CommonX.Configurations;
using CommonX.Log4Net;
using System;
using CommonX.Kafka;
using CommonX.Components;
using JetBrains.Annotations;
using System.Collections.Generic;
using Confluent.Kafka;
using System.Text;

namespace Kafka.Test.NetCore
{
    class Program
    {
         [NotNull]static IKafkaPersisterConnection _consumerClient;

         [NotNull] static IConnectionPool _connectionPool;
        static void Main(string[] args)
        {

            //Kafka Consume Messages
            List<string> topics = new List<string>();
            topics.Add("Test Topics");

            _consumerClient.Consume(topics, HandlerMessage);

            var msg = Encoding.UTF8.GetBytes("testMsg");
            //Kafka Producer
            _connectionPool.Rent().ProduceAsync("Test-Topic", null, msg);

        }


        static void HandlerMessage(Message<string, string> msg)
        {
            Console.WriteLine($"消息的值为{msg.Value}");

            //Todo Something
        }

        static void BootStrap()
        {

            var config = Configuration.Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .UseKafka("Test GroupId");


            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
            {
                _consumerClient = scope.Resolve<IKafkaPersisterConnection>();
                _connectionPool = scope.Resolve<IConnectionPool>();
            }
        }
    }
}
