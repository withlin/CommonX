using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Extras.DynamicProxy;
using Autofac.Features.AttributeFilters;
using CommonX.Autofac.Impl;
using CommonX.Components;
using CommonX.Configurations;

namespace CommonX.Autofac
{
    /// <summary>
    ///     Autofac implementation of IObjectContainer.
    /// </summary>
    public class AutofacObjectContainer : IObjectContainer
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public AutofacObjectContainer() : this(new ContainerBuilder())
        {
        }

        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        /// <param name="containerBuilder"></param>
        public AutofacObjectContainer(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<LogInterceptor>();
            containerBuilder.RegisterType<ExceptionHandlingInterceptor>();
            containerBuilder.RegisterType<CachingInterceptor>();
            containerBuilder.RegisterType<LookupQueryInterceptor>();

            Container = containerBuilder.Build();
        }

        /////<summary>
        ///// Instantiates the class utilizing the given container.
        /////</summary>
        //private AutofacObjectContainer(ILifetimeScope container)
        //{
        //    this.Container = container ?? new ContainerBuilder().Build();
        //}

        /// <summary>
        ///     Represents the inner autofac container.
        /// </summary>
        public IContainer Container { get; private set; }

        /// <summary>
        ///     Register a implementation type.
        /// </summary>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="life">The life cycle of the implementer type.</param>
        public void RegisterType(Type implementationType, LifeStyle life = LifeStyle.Singleton)
        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterType(implementationType).PropertiesAutowired().WithAttributeFiltering();

            foreach (var interfaceType in implementationType.GetInterfaces())
            {
                registrationBuilder = builder.RegisterType(implementationType).As(interfaceType).WithAttributeFiltering();
            }


            SetLifetimeScope(life, registrationBuilder);
            builder.Update(Container);
        }

        /// <summary>
        ///     Register a implementation type.
        /// </summary>
        /// <param name="life">The life cycle of the implementer type.</param>
        public void RegisterType<TImplementationType>(LifeStyle life = LifeStyle.Singleton)
        {
            RegisterType(typeof(TImplementationType));
        }

        /// <summary>
        ///     Register a implementer type as a service implementation.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="life">The life cycle of the implementer type.</param>
        public void RegisterType(Type serviceType, Type implementationType, LifeStyle life = LifeStyle.Singleton)
        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterType(implementationType).PropertiesAutowired()
                .As(serviceType).WithAttributeFiltering();

            SetLifetimeScope(life, registrationBuilder);
            builder.Update(Container);
        }

        /// <summary>
        ///     Register a implementer type as a service implementation.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="key"></param>
        /// <param name="life">The life cycle of the implementer type.</param>
        public void RegisterTypeKeyed(Type serviceType, Type implementationType, object key, LifeStyle life = LifeStyle.Singleton)
        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterType(implementationType).PropertiesAutowired()
                .Keyed(key, serviceType);

            SetLifetimeScope(life, registrationBuilder);
            builder.Update(Container);
        }

        /// <summary>
        ///     Register a implementer type as a service implementation.
        /// </summary> 
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="life">The life cycle of the implementer type.</param>
        public void Register<TService, TImplementer>(LifeStyle life = LifeStyle.Singleton)
            where TService : class
            where TImplementer : class, TService
        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterType<TImplementer>()
                .As<TService>().PropertiesAutowired().WithAttributeFiltering();

            SetLifetimeScope(life, registrationBuilder);
            builder.Update(Container);
        }

        public void RegisterGeneric(Type service, Type implementer, LifeStyle life = LifeStyle.Singleton,
            params string[] parameter)
        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterGeneric(implementer).As(service).PropertiesAutowired().WithAttributeFiltering();

            SetLifetimeScope(life, registrationBuilder);

            var component = ObjectContainer.ParseComponent(implementer);
            SetIntercepter(component.Interceptor, registrationBuilder);

            if (parameter.Length > 0)
            {
                for (var i = 0; i < parameter.Length; i++)
                {
                    var param = new PositionalParameter(i, parameter[i]);
                    registrationBuilder.WithParameter(param);
                }
            }
            builder.Update(Container);
        }

        public void RegisterGeneric(Type service, Type implementer, LifeStyle life = LifeStyle.Singleton,
            IEnumerable<KeyValuePair<string, object>> meta = null)
        {
            var builder = new ContainerBuilder();
            var registrationBuilder = builder.RegisterGeneric(implementer).As(service).PropertiesAutowired().WithAttributeFiltering();

            SetLifetimeScope(life, registrationBuilder);

            var component = ObjectContainer.ParseComponent(implementer);
            SetIntercepter(component.Interceptor, registrationBuilder);

            if (meta != null)
            {
                registrationBuilder.WithMetadata(meta);
            }
            builder.Update(Container);
        }
        /// <summary>
        ///     Register a implementer type instance as a service implementation.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="instance">The implementer type instance.</param>
        /// <param name="meta"></param>
        public void RegisterInstance<TService, TImplementer>(TImplementer instance, LifeStyle life = LifeStyle.Singleton, IEnumerable<KeyValuePair<string, object>> meta = null)
            where TService : class
            where TImplementer : class, TService
        {
            var builder = new ContainerBuilder();
            var registration = builder.RegisterInstance(instance)
                .As<TService>();
            if (meta != null)
            {
                registration.WithMetadata(meta);
            }
            SetLifetimeScope(life, registration);
            builder.Update(Container);
        }

        /// <summary>
        ///     Register a component
        /// </summary>
        /// <param name="type"></param>
        /// <param name="life"></param>
        /// <param name="interceptor"></param>
        /// <param name="keyedInterfaceType"></param>
        public void RegisterComponent(Type type, LifeStyle life = LifeStyle.Singleton,
            Interceptor interceptor = Interceptor.None)
        {
            var builder = new ContainerBuilder();

            if (type.IsGenericType)
            {
                var registration = builder.RegisterGeneric(type).PropertiesAutowired().WithAttributeFiltering();

                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsGenericType)
                    {
                        //registration = keyedInterfaceType != null && keyedInterfaceType==interfaceType.GetGenericTypeDefinition()
                        //    ? registration.Keyed(keyedInterfaceType, interfaceType.GetGenericTypeDefinition()).PropertiesAutowired()
                        //    : registration.As(interfaceType.GetGenericTypeDefinition()).PropertiesAutowired();
                        registration = registration.As(interfaceType.GetGenericTypeDefinition()).PropertiesAutowired().WithAttributeFiltering();
                    }
                }

                SetLifetimeScope(life, registration);
                SetIntercepter(interceptor, registration);
            }
            else
            {
                var registration = builder.RegisterType(type).PropertiesAutowired().WithAttributeFiltering();

                var interfaceTypes = type.GetInterfaces();
                if (interfaceTypes.Any())
                {
                    foreach (var interfaceType in type.GetInterfaces())
                    {
                        //registration = keyedInterfaceType != null&& keyedInterfaceType == interfaceType
                        //    ? registration.Keyed(keyedInterfaceType, interfaceType).PropertiesAutowired()
                        //    : registration.As(interfaceType).PropertiesAutowired();
                        registration = registration.As(interfaceType).PropertiesAutowired().WithAttributeFiltering();
                    }
                }
                else
                {
                    registration = builder.RegisterType(type).PropertiesAutowired().AsSelf();
                }


                SetLifetimeScope(life, registration);
                SetIntercepter(interceptor, registration);
            }

            builder.Update(Container);
        }

        /// <summary>
        ///     Register a component
        /// </summary>
        /// <param name="life"></param>
        /// <param name="interceptor"></param>
        public void RegisterComponent<TImplementer>(LifeStyle life = LifeStyle.Singleton,
            Interceptor interceptor = Interceptor.None)
        {
            RegisterComponent(typeof(TImplementer), life, interceptor);
        }

        /// <summary>
        ///     获取一个请求scope
        /// </summary>
        /// <returns></returns>
        public IScope BeginLifetimeScope()
        {
            return new Scope(Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag));
        }

        /// <summary>
        ///     Resolve a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The component instance that provides the service.</returns>
        [Obsolete("will delete,use constructor instead,only for test use")]
        public TService Resolve<TService>() where TService : class
        {
            using (var lifetimeScope = Container.BeginLifetimeScope())
            {
                return lifetimeScope.Resolve<TService>();
            }
            //return Container.Resolve<TService>();
        }

        /// <summary>
        ///     Resolve a service.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The component instance that provides the service.</returns>
        [Obsolete("will delete,use constructor instead ,only for test use")]
        public object Resolve(Type serviceType)
        {
            using (var lifetimeScope = Container.BeginLifetimeScope())
            {
                return lifetimeScope.Resolve(serviceType);
            }
            //return Container.Resolve(serviceType);
        }

        /// <summary>
        ///     Indicates if a component of the given type has been configured.
        /// </summary>
        /// <param name="componentType">Component type to check.</param>
        /// <returns><c>true</c> if the <paramref name="componentType" /> is registered in the container or <c>false</c> otherwise.</returns>
        public bool IsRegistered(Type componentType)
        {
            return Container.IsRegistered(componentType);
        }

        public bool IsRegistered<TComponentType>()
        {
            return Container.IsRegistered<TComponentType>();
        }
        /// <summary>
        /// get all registrations
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Type> AllRegistrations()
        {
            return Container.ComponentRegistry.Registrations.SelectMany(x => x.Services)
                .OfType<IServiceWithType>()
                .Select(x => x.ServiceType);

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region protect method

        private void SetLifetimeScope(LifeStyle dependencyLifecycle,
            IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registrationBuilder)
        {
            switch (dependencyLifecycle)
            {
                case LifeStyle.Singleton:
                    registrationBuilder.SingleInstance();
                    break;
                case LifeStyle.InstancePerRequest:
                    registrationBuilder.InstancePerRequest();
                    break;
            }
        }

        private void SetLifetimeScope(LifeStyle life,
            IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> registrationBuilder)
        {
            switch (life)
            {
                case LifeStyle.Singleton:
                    registrationBuilder.SingleInstance();
                    break;
                case LifeStyle.InstancePerRequest:
                    registrationBuilder.InstancePerRequest();
                    break;
            }
        }

        private void SetIntercepter(Interceptor interceptor,
            IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registrationBuilder)
        {
            registrationBuilder.EnableInterfaceInterceptors();
            //if (interceptor.HasFlag(Interceptor.Log) && Configuration.Instance.Setting.UseLogInterceptor)
            //{
            //    registrationBuilder.InterceptedBy(typeof(LogInterceptor));
            //}
            if (interceptor.HasFlag(Interceptor.Measure) && Configuration.Instance.Setting.UseMeasureInterceptor)
            {
                //registrationBuilder.InterceptedBy(typeof(MeasureInterceptor));
            }
            if (interceptor.HasFlag(Interceptor.ExceptionHandle) && Configuration.Instance.Setting.UseExceptionHandlingInterceptor)
            {
                registrationBuilder.InterceptedBy(typeof(ExceptionHandlingInterceptor));
            }
            if (interceptor.HasFlag(Interceptor.Caching) && Configuration.Instance.Setting.UseCachingInterceptor)
            {
                registrationBuilder.InterceptedBy(typeof(CachingInterceptor));
            }
            //if (interceptor.HasFlag(Interceptor.LookupQueryHandle) && Configuration.Instance.Setting.UseLookupQueryInterceptor)
            //{
            //    registrationBuilder.InterceptedBy(typeof(LookupQueryInterceptor));
            //}
        }

        private void SetIntercepter(Interceptor interceptor,
            IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> registrationBuilder)
        {
            registrationBuilder.EnableInterfaceInterceptors();

            //if (interceptor.HasFlag(Interceptor.Log) && Configuration.Instance.Setting.UseLogInterceptor)
            //{
            //    registrationBuilder.InterceptedBy(typeof(LogInterceptor));
            //}
            if (interceptor.HasFlag(Interceptor.Measure) && Configuration.Instance.Setting.UseMeasureInterceptor)
            {
                //registrationBuilder.InterceptedBy(typeof(MeasureInterceptor));
            }
            if (interceptor.HasFlag(Interceptor.ExceptionHandle) && Configuration.Instance.Setting.UseExceptionHandlingInterceptor)
            {
                registrationBuilder.InterceptedBy(typeof(ExceptionHandlingInterceptor));
            }
            if (interceptor.HasFlag(Interceptor.Caching) && Configuration.Instance.Setting.UseCachingInterceptor)
            {
                registrationBuilder.InterceptedBy(typeof(CachingInterceptor));
            }
            //if (interceptor.HasFlag(Interceptor.LookupQueryHandle) && Configuration.Instance.Setting.UseLookupQueryInterceptor)
            //{
            //    registrationBuilder.InterceptedBy(typeof(LookupQueryInterceptor));
            //}
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects). 
            }
            // Free your own state (unmanaged objects).
            Container.Dispose();
            // Set large fields to null. 
        }

        public void Build()
        {
            throw new NotImplementedException();
        }

        public void RegisterType(Type serviceType, Type implementationType, string serviceName = null, LifeStyle life = LifeStyle.Singleton)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

