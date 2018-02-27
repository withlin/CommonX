using CommonX.Components;
using System.Reflection;
using CommonX.Configurations;
using CommonX.Log4Net;
using CommonX.ServiceBus.MassTransit.Configuration;
using JetBrains.Annotations;
using CommonX.ServiceBus;
using CommonX.EventBus.Events;
using MassTransit;
using System.Threading.Tasks;

namespace ESB.Test.NetCore
{
    public class RequestIntegrationEvent : IntegrationEvent
    {
        public string Message { get; set; }
    }
    public class Handle : IConsumer<RequestIntegrationEvent>
    {
        public Task Consume(ConsumeContext<RequestIntegrationEvent> context)
        {
            System.Console.WriteLine(context.Message);
            return Task.FromResult(context.Message);
        }
    }
    class Program
    {
        [NotNull] static CommonX.ServiceBus.IBus _bus;
        static void Main(string[] args)
        {
            //Send
            _bus.Send("ESB.Test.NetCore", new RequestIntegrationEvent() { Message = "Test" });

            //Consume
            
        }


        static void BootStrap()
        {
            var assambly = Assembly.GetAssembly(typeof(Program));

            var config = Configuration.Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .UseMassTransit(new Assembly[] { assambly });



            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
            {
                _bus = scope.Resolve<CommonX.ServiceBus.IBus>();
            }
        }
    }
}
