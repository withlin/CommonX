using Autofac;
using CommonX.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Autofac.Impl
{
    /// <summary>
    ///     ioc 生命周期容器
    /// </summary>
    public class Scope : IScope
    {
        private readonly ILifetimeScope _lifetimeScope;

        public Scope(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        /// <summary>
        ///     Resolve a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The component instance that provides the service.</returns>
        public TService Resolve<TService>() where TService : class
        {
            return _lifetimeScope.Resolve<TService>();
        }
        /// <summary>
        ///     Resolve a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The component instance that provides the service.</returns>
        public TService ResolveKeyed<TService>(object key) where TService : class
        {
            return _lifetimeScope.ResolveKeyed<TService>(key);
        }

        /// <summary>
        ///     Resolve a service.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The component instance that provides the service.</returns>
        public object Resolve(Type serviceType)
        {
            return _lifetimeScope.Resolve(serviceType);
        }

        /// <summary>
        ///     Indicates if a component of the given type has been configured.
        /// </summary>
        /// <param name="componentType">Component type to check.</param>
        /// <returns><c>true</c> if the <paramref name="componentType" /> is registered in the container or <c>false</c> otherwise.</returns>
        public bool IsRegistered(Type componentType)
        {
            return _lifetimeScope.IsRegistered(componentType);
        }

        public bool IsRegistered<TComponentType>()
        {
            return _lifetimeScope.IsRegistered<TComponentType>();
        }
        public void Dispose()
        {
            _lifetimeScope.Dispose();
        }
    }
}
