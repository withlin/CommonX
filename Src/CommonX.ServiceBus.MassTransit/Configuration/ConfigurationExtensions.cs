using Autofac;
using CommonX.Autofac;
using CommonX.Components;
using CommonX.Logging;
using CommonX.ServiceBus.MassTransit.Observers;
using GreenPipes;
using MassTransit;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommonX.ServiceBus.MassTransit.Configuration
{
    /// <summary>
    ///     configuration class Autofac extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        ///     Use MassTransit for message bus.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="assembles"></param>
        /// <param name="endpointName"></param>
        /// <param name="retry"></param>
        /// <param name="isTransaction">Encapsulate the pipe behavior in a transaction</param>
        /// <param name="prefetchCount">Specify the maximum number of concurrent messages that are consumed</param>
        /// <returns></returns>
        public static Configurations.Configuration UseMassTransit(
            this Configurations.Configuration configuration, Assembly[] assembles = null,
            string endpointName = "", int retry = 0, bool isTransaction = false, ushort prefetchCount = 16)
        {
            var objectContainer = new AutofacObjectContainer();
            string assemblyName = Assembly.GetCallingAssembly().GetName().Name;
            if (string.IsNullOrEmpty(endpointName))
            {
                endpointName = assemblyName + "-" + Environment.MachineName;
            }

            Configurations.Configuration.Instance.Setting.EndPoint = endpointName;
            if (objectContainer != null)
            {
                var container = objectContainer.Container;
                var builder = new ContainerBuilder();

                //consumer 不能加component，不然会注册2次

                if (assembles != null)
                    foreach (var ass in assembles)
                    {
                        try
                        {
                            builder.RegisterConsumers(ass);
                        }
                        catch (ReflectionTypeLoadException loadException)
                        {
                            //目前写死判断，少dll报错，多dll跳过
                            var message = string.Join(",",
                                loadException.LoaderExceptions.Select(c => c.Message).Distinct());
                            if (message.Contains("系统找不到指定的文件"))
                            {
                                throw new ApplicationException(message);
                            }
                        }
                    }

                var clusterHosts = configuration.Setting.TransportIp.Split(',');
                //var clusterHosts = "10.3.80.41";
                builder.Register(context =>
                {
                    var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        configuration.Setting.TransportHost = string.Format("rabbitmq://{0}/{1}",
                            clusterHosts[0],
                            string.IsNullOrEmpty(configuration.Setting.TransportVirtualHost)
                                ? ""
                                : configuration.Setting.TransportVirtualHost + "/");
                        var host = cfg.Host(new Uri(configuration.Setting.TransportHost), h =>
                        {
                            h.Username(configuration.Setting.TransportUserName);
                            h.Password(configuration.Setting.TransportPassword);
                            if (clusterHosts.Length > 0)
                            {
                                h.UseCluster(c =>
                                {
                                    foreach (var clusterHost in clusterHosts)
                                    {
                                        c.Node(clusterHost);
                                    }
                                });
                            }
                        });

                        //以机器名为消息总线队列名的方式暂保留
                        var endpointNames = new ArrayList { endpointName };

                        //分公司标识作为消息总线队列名
                        var branchs = CommonX.Configurations.Configuration.Instance.Setting.Branch?.Split(',');
                        if (branchs != null && branchs.Length > 0)
                        {
                            foreach (var branchId in branchs)
                            {
                                string name = assemblyName + "-" + branchId;
                                endpointNames.Add(name);
                            }
                        }

                        //绑定消息总线
                        foreach (var a in endpointNames)
                        {
                            cfg.ReceiveEndpoint(host, a.ToString(), ec =>
                            {
                                if (retry > 0)
                                {
                                    ec.UseRetry(x=>x.Incremental(retry, new TimeSpan(0, 0, 5), new TimeSpan(0, 0, 5)));
                                }
                                if (isTransaction)
                                {
                                    ec.UseTransaction(x => { x.Timeout = TimeSpan.FromSeconds(300); });
                                }

                                cfg.PrefetchCount = prefetchCount;
                                ec.LoadFrom(context);
                            });
                        }

                        cfg.ConfigureJsonSerializer(c => new JsonSerializerSettings()
                        {
                            Converters = new List<JsonConverter>
                            {
                                new IsoDateTimeConverter(),
                                new StringEnumConverter {CamelCaseText = true}
                            },
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            //NullValueHandling = NullValueHandling.Ignore
                        });
                    });

                    return busControl;
                })
                    .SingleInstance()
                    .As<IBusControl>()
                    .As<global::MassTransit.IBus>();
                builder.Update(container);

                var bc = container.Resolve<IBusControl>();
                configuration.SetDefault<IBus, MassTransitBus>(new MassTransitBus(bc));
               var log=  ObjectContainer.Current.BeginLifetimeScope();
                var receiveObserver = new ReceiveObserver(log.Resolve<ILoggerFactory>().Create(typeof(ReceiveObserver)));
                //var receiveObserver = new ReceiveObserver();
                var receiveHandle = bc.ConnectReceiveObserver(receiveObserver);

                var publishObserver = new PublishObserver();
                var publishHandle = bc.ConnectPublishObserver(publishObserver);

                var sendObserver = new SendObserver();
                var shendHandle = bc.ConnectSendObserver(sendObserver);
                //var result = bc.GetProbeResult();
                //Console.WriteLine(result.ToJsonString());

                ObjectContainer.Current.RegisterGeneric(typeof(ServiceBus.IRequestClient<,>),
                    typeof(CommonX.ServiceBus.MassTransit.Impl.RequestClient<,>),
                    LifeStyle.Transient);

                bc.Start();
            }


            return configuration;
        }
    }
}
