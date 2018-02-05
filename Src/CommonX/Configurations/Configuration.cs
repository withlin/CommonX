using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonX.Cache;
using CommonX.Cache.Configuration;
using CommonX.Cache.Impl;
using CommonX.Components;
using CommonX.IO;
using CommonX.Logging;
using CommonX.Scheduling;
using CommonX.Serializing;
using CommonX.ServiceBus;
using CommonX.Socketing.Framing;
using CommonX.Utilities;
using CommonX.Utilities.AssenblyScanner;

namespace CommonX.Configurations
{
    public class Configuration
    {
        private Configuration(Setting setting)
        {
            Setting = setting ?? new Setting();
        }

        /// <summary>
        ///     Get the configuration setting information.
        /// </summary>
        public Setting Setting { get; }

        /// <summary>
        ///     Provides the singleton access instance.
        /// </summary>
        public static Configuration Instance { get; private set; }

        public static Configuration Create(Setting setting = null)
        {
            if (Instance != null)
            {
                throw new Exception("Could not create configuration instance twice.");
            }
            Instance = new Configuration(setting);
            return Instance;
        }

        public Configuration SetDefault<TService, TImplementer>(LifeStyle life = LifeStyle.Singleton)
            where TService : class
            where TImplementer : class, TService
        {
            ObjectContainer.Register<TService, TImplementer>(life);
            return this;
        }

        public Configuration SetDefault<TService, TImplementer>(TImplementer instance)
            where TService : class
            where TImplementer : class, TService
        {
            ObjectContainer.RegisterInstance<TService, TImplementer>(instance);
            return this;
        }

        public Configuration RegisterCommonComponents()
        {
            SetDefault<IBinarySerializer, DefaultBinarySerializer>();
            SetDefault<ILoggerFactory, EmptyLoggerFactory>();
            SetDefault<ICacheFactory, MemoryCacheFactory>();
            SetDefault<IBus, EmptyBus>();
            //SetDefault<IIgnite, DefaultIgnite>();
            ObjectContainer.Current.RegisterGeneric(typeof(IRequestClient<,>), typeof(EmptyRequestClient<,>));
            SetDefault<IOHelper, IOHelper>();

            //缓存配置默认为JZTERP.Frameworks.Common.Cache.Configuration.CacheConfiguration,JZTERP.Frameworks.Common
            SetDefault<ICacheConfiguration, Cache.Configuration.CacheConfiguration>();
            return this;
        }

        /// <summary>
        ///     Register all the components from the given assemblies.
        /// </summary>
        public Configuration RegisterComponents(Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes().Where(TypeUtils.IsComponent))
                    {
                        ObjectContainer.RegisterComponent(type, Setting.ModuleLevel);
                    }
                }
                catch (ReflectionTypeLoadException loadException)
                {
                    //目前写死判断，少dll报错，多dll跳过
                    var message = string.Join(",", loadException.LoaderExceptions.Select(c => c.Message).Distinct());
                    if (message.Contains("系统找不到指定的文件"))
                    {
                        throw new ApplicationException(message);
                    }
                }
            }
            return this;
        }

        /// <summary>
        ///     Register all the components.
        /// </summary>
        public Configuration RegisterComponents()
        {
            var assemblies = GetAssembliesInDirectory(Setting.RootPath);
            RegisterComponents(assemblies.ToArray());
            return this;
        }
        public Configuration RegisterComponents(Func<string, bool> filter)
        {
            var assemblies = GetAssembliesInDirectory(Setting.RootPath, filter);
            RegisterComponents(assemblies.ToArray());
            return this;
        }
        public Configuration RegisterComponents(Func<string, bool> filter, params string[] assembliesToSkip)
        {
            var assemblies = GetAssembliesInDirectory(Setting.RootPath, filter, assembliesToSkip);
            RegisterComponents(assemblies.ToArray());
            return this;
        }

        private IEnumerable<Assembly> GetAssembliesInDirectory(string path, Func<string, bool> filter = null, params string[] assembliesToSkip)
        {
            var assemblyScanner = new AssemblyScanner(path, filter);
            //assemblyScanner.MustReferenceAtLeastOneAssembly.Add(typeof(IHandleMessages<>).Assembly);

            if (assembliesToSkip != null)
            {
                assemblyScanner.AssembliesToSkip = assembliesToSkip.ToList();
            }
            return assemblyScanner
                .GetScannableAssemblies()
                .Assemblies;
        }
    }
}
