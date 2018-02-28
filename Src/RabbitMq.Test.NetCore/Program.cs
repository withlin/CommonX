using CommonX.Configurations;
using CommonX.Log4Net;
using System;
using System.Reflection;
using CommonX.RabbitMQ;
using CommonX.Components;
using JetBrains.Annotations;
using CommonX.EventBus.Events;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMq.Test.NetCore
{
    public class RequestIntegrationEvent : IntegrationEvent
    {
        public string Message { get; set; }
    }
    
    class Program
    {
        [NotNull] static IRabbitMQPersistentConnection _rabbitMQPersistentConnection;
        static void Main(string[] args)
        {
            BootStrap();

            //Pubilsh
            _rabbitMQPersistentConnection.Publish(new RequestIntegrationEvent() { Message = "hello" }, "", "hello", "hello");

            
            //Subscribe
            _rabbitMQPersistentConnection.Subscribe("hello", HandleMessages);


        }

        static void HandleMessages(object model, BasicDeliverEventArgs r)
        {
            var msg = Encoding.UTF8.GetString(r.Body);

            //Todo Something
        }

        static void BootStrap()
        {
            //HostName = setting.HostName,
            //        Port = setting.Port,
            //        VirtualHost = setting.VirtualHost,
            //        UserName = setting.UserName,
            //        Password = setting.Password,
            //        AutomaticRecoveryEnabled = setting.AutomaticRecoveryEnabled,
            //        RequestedConnectionTimeout = setting.RequestedConnectionTimeout
            var assambly = Assembly.GetAssembly(typeof(Program));
            var config = Configuration.Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .UseRabbitMQ(x=> 
                {
                    x.HostName = "10.3.80.42";
                    x.Port = 5067;
                    x.VirtualHost = "/hr";
                    x.UserName = "admin";
                    x.Password = "admin";
                    x.AutomaticRecoveryEnabled = true;
                    x.RequestedConnectionTimeout = 20000;
                });


            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
            {
                _rabbitMQPersistentConnection = scope.Resolve<IRabbitMQPersistentConnection>();
            }
        }
    }
}
