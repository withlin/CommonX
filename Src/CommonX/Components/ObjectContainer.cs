using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonX.Components
{
    /// <summary>
    ///     Represents an object container.
    /// </summary>
    public class ObjectContainer
    {
        /// <summary>
        ///     Represents the current object container.
        /// </summary>
        public static IObjectContainer Current { get; private set; }

        /// <summary>
        ///     Set the object container.
        /// </summary>
        /// <param name="container"></param>
        public static void SetContainer(IObjectContainer container)
        {
            Current = container;
        }

        /// <summary>
        ///     Register a implementation type.
        /// </summary>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="life">The life cycle of the implementer type.</param>
        public static void RegisterType(Type implementationType, LifeStyle life = LifeStyle.Singleton)
        {
            Current.RegisterType(implementationType, life);
        }

        /// <summary>
        ///     Register a implementer type as a service implementation.
        /// </summary>
        /// <param name="serviceType">The implementation type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="life">The life cycle of the implementer type.</param>
        public static void RegisterType(Type serviceType, Type implementationType, LifeStyle life = LifeStyle.Singleton)
        {
            Current.RegisterType(serviceType, implementationType, life);
        }

        /// <summary>
        ///     Register a implementer type as a service implementation.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="key"></param>
        /// <param name="life">The life cycle of the implementer type.</param>
        public static void RegisterTypeKeyed(Type serviceType, Type implementationType, object key,
            LifeStyle life = LifeStyle.Singleton)
        {
            Current.RegisterTypeKeyed(serviceType, implementationType, key, life);
        }
        /// <summary>
        ///     Register a implementer type as a service implementation.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="life">The life cycle of the implementer type.</param>
        public static void Register<TService, TImplementer>(LifeStyle life = LifeStyle.Singleton) where TService : class
            where TImplementer : class, TService
        {
            Current.Register<TService, TImplementer>(life);
        }

        /// <summary>
        ///     Register a implementer type instance as a service implementation.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="instance">The implementer type instance.</param>
        public static void RegisterInstance<TService, TImplementer>(TImplementer instance, LifeStyle life = LifeStyle.Singleton, IEnumerable<KeyValuePair<string, object>> meta = null) where TService : class where TImplementer : class, TService
        {
            Current.RegisterInstance<TService, TImplementer>(instance, life, meta);
        }

        /// <summary>
        /// Register a component
        /// </summary>
        /// <param name="type"></param>
        /// <param name="moduleLevel">模块加载级别</param>
        public static void RegisterComponent(Type type, ModuleLevel moduleLevel = ModuleLevel.All)
        {
            var component = ParseComponent(type);

            if (component.ModuleLevel <= moduleLevel)
            {
                //Current.RegisterComponent(type, component.LifeStyle, component.Interceptor,component.KeyedInterfaceType);
                Current.RegisterComponent(type, component.LifeStyle, component.Interceptor);
            }

        }

        /// <summary>
        /// Register a component
        /// </summary>
        /// <param name="moduleLevel">模块加载级别</param>
        public static void RegisterComponent<TImplementer>(ModuleLevel moduleLevel = ModuleLevel.All)
        {
            var component = ParseComponent(typeof(TImplementer));
            if (component.ModuleLevel <= moduleLevel)
            {
                Current.RegisterComponent<TImplementer>(component.LifeStyle, component.Interceptor);
            }
        }

        /// <summary>
        ///     获取一个请求scope
        ///     必须使用using，要不然无法释放
        /// </summary>
        /// <returns></returns>
        public static IScope BeginLifetimeScope()
        {
            return Current.BeginLifetimeScope();
        }

        ///// <summary>
        /////     Resolve a service.
        ///// </summary>
        ///// <typeparam name="TService">The service type.</typeparam>
        ///// <returns>The component instance that provides the service.</returns>
        //[Obsolete("will delete,use constructor instead")]
        //public static TService Resolve<TService>() where TService : class
        //{
        //    return Current.Resolve<TService>();
        //}

        ///// <summary>
        /////     Resolve a service.
        ///// </summary>
        ///// <param name="serviceType">The service type.</param>
        ///// <returns>The component instance that provides the service.</returns>
        //[Obsolete("will delete,use constructor instead")]
        //public static object Resolve(Type serviceType)
        //{
        //    return Current.Resolve(serviceType);
        //}

        /// <summary>
        /// Indicates if a component of the given type has been configured.
        /// </summary>
        /// <param name="componentType">Component type to check.</param>
        /// <returns><c>true</c> if the <paramref name="componentType"/> is registered in the container or <c>false</c> otherwise.</returns>
        public static bool IsRegistered(Type componentType)
        {
            return Current.IsRegistered(componentType);
        }
        /// <summary>
        /// Indicates if a component of the given type has been configured.
        /// </summary>
        public static bool IsRegistered<TComponentType>()
        {
            return Current.IsRegistered<TComponentType>();
        }

        public static ComponentAttribute ParseComponent(Type type)
        {
            var component = (ComponentAttribute)type.GetCustomAttributes(typeof(ComponentAttribute), true).FirstOrDefault();
            if (component == null) component = new ComponentAttribute();
            return component;
        }

        #region Private Methods



        #endregion
    }
}
