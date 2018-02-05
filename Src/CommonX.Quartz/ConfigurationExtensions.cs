using Autofac;
using Common.Logging;
using CommonX.Autofac;
using CommonX.Components;
using CommonX.Quartz.Logging.EntLib;
using Quartz;
using Quartz.Xml.JobSchedulingData20;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace CommonX.Quartz
{
    public static class ConfigurationExtensions
    {
        public static Configuration UseQuartz(this Configuration configuration, Assembly[] assembiles)
        {
            LogManager.Adapter = new EntLibLoggerFactoryAdapter();

            var objectContainer = ObjectContainer.Current as AutofacObjectContainer;
            if (objectContainer != null)
            {
                var container = objectContainer.Container;
                var builder = new ContainerBuilder();

                var schedulerConfig = new NameValueCollection {
                    { "quartz.scheduler.instanceName","QuartzScheduler"},
                    { "quartz.scheduler.instanceId","AUTO"},
                    { "quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz"},
                    { "quartz.threadPool.threadCount", "10"},
                    { "quartz.threadPool.threadPriority", "Normal"},
                    { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz"}
                    //{ "quartz.plugin.jobInitializer.type", "Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin, Quartz"},
                    //{ "quartz.plugin.jobInitializer.fileNames","~/Cfg/quartz_jobs.xml"},
                    //{ "quartz.plugin.jobInitializer.failOnFileNotFound","false"},
                    //{ "quartz.plugin.jobInitializer.scanInterval", "60"}
                };

                //string path = Path.Combine(configuration.Setting.RootPath, configuration.Setting.ConfigBase, "quartz_jobs.xml");
                //string dir = Path.GetDirectoryName(path);
                //if (!Directory.Exists(dir))
                //    Directory.CreateDirectory(dir);
                //if (!File.Exists(path)) {
                //    QuartzXmlConfiguration20 config = new QuartzXmlConfiguration20();
                //    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(QuartzXmlConfiguration20));
                //    using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(path)) {
                //        xs.Serialize(writer, config);
                //    }
                //}

                builder.RegisterModule(new QuartzAutofacFactoryModule() {
                    ConfigurationProvider=c=>schedulerConfig
                });
                builder.RegisterModule(new QuartzAutofacJobsModule(assembiles)
                {
                    AutoWireProperties = true
                });
                builder.Update(container);
                IScheduler scheduler = container.Resolve<IScheduler>();
                scheduler.Start();                
            }
            return configuration;
        }
    }
}
