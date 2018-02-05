using System;
using System.Collections.Generic;

namespace CommonX.Components
{
    /// <summary>
    ///     Represents an object container interface.
    /// </summary>
    public interface IObjectContainer : IDisposable
    {
        /// <summary>
        ///     Register a implementation type.
        /// </summary>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="life">The life cycle of the implementer type.</param>
        void RegisterType(Type implementationType, LifeStyle life = LifeStyle.Singleton);

        /// <summary>
        ///     Register a implementation type.
        /// </summary>
        /// <param name="life">The life cycle of the implementer type.</param>
        void RegisterType<TImplementationType>(LifeStyle life = LifeStyle.Singleton);

        /// <summary>
        ///     Register a implementer type as a service implementation.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="life">The life cycle of the implementer type.</param>
        void RegisterType(Type serviceType, Type implementationType, LifeStyle life = LifeStyle.Singleton);

        /// <summary>
        ///     Register a implementer type as a service implementation.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="key"></param>
        /// <param name="life">The life cycle of the implementer type.</param>
        void RegisterTypeKeyed(Type serviceType, Type implementationType, object key,
            LifeStyle life = LifeStyle.Singleton);
        /// <summary>
        ///     Register a implementer type as a service implementation.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="life">The life cycle of the implementer type.</param>
        void Register<TService, TImplementer>(LifeStyle life = LifeStyle.Singleton) where TService : class where TImplementer : class, TService;

        void RegisterGeneric(Type service, Type implementer, LifeStyle life = LifeStyle.Singleton, params string[] parameter);
        void RegisterGeneric(Type service, Type implementer, LifeStyle life = LifeStyle.Singleton, IEnumerable<KeyValuePair<string, object>> meta = null);

        /// <summary>
        ///     Register a implementer type instance as a service implementation.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementer">The implementer type.</typeparam>
        /// <param name="instance">The implementer type instance.</param>
        /// <param name="life">The life cycle of the implementer type.</param>
        /// <param name="meta"></param>
        void RegisterInstance<TService, TImplementer>(TImplementer instance, LifeStyle life = LifeStyle.Singleton, IEnumerable<KeyValuePair<string, object>> meta = null) where TService : class where TImplementer : class, TService;

        /// <summary>
        /// Register a component
        /// </summary>
        /// <param name="type"></param>
        /// <param name="life"></param>
        /// <param name="interceptor"></param>
        /// <param name="keyed"></param>
        void RegisterComponent(Type type, LifeStyle life = LifeStyle.Singleton,
            Interceptor interceptor = Interceptor.None);

        /// <summary>
        /// Register a component
        /// </summary>
        /// <param name="life"></param>
        /// <param name="interceptor"></param>
        /// <param name="keyed"></param>
        void RegisterComponent<TImplementer>(LifeStyle life = LifeStyle.Singleton,
            Interceptor interceptor = Interceptor.None);

        /// <summary>
        ///     获取一个请求scope
        /// </summary>
        /// <returns></returns>
        IScope BeginLifetimeScope();


        ///// <summary>
        /////     Resolve a service.
        ///// </summary>
        ///// <typeparam name="TService">The service type.</typeparam>
        ///// <returns>The component instance that provides the service.</returns>
        //[Obsolete("will delete,use constructor instead")]
        //TService Resolve<TService>() where TService : class;

        ///// <summary>
        /////     Resolve a service.
        ///// </summary>
        ///// <param name="serviceType">The service type.</param>
        ///// <returns>The component instance that provides the service.</returns>
        //[Obsolete("will delete,use constructor instead")]
        //object Resolve(Type serviceType);

        /// <summary>
        /// Indicates if a component of the given type has been configured.
        /// </summary>
        /// <param name="componentType">Component type to check.</param>
        /// <returns><c>true</c> if the <paramref name="componentType"/> is registered in the container or <c>false</c> otherwise.</returns>
        bool IsRegistered(Type componentType);
        bool IsRegistered<TComponentType>();

        /// <summary>
        /// get all registrations
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> AllRegistrations();
    }
}
