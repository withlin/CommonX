using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Components
{
    /// <summary>
    ///     ioc 生命周期容器
    /// </summary>
    public interface IScope : IDisposable
    {
        
        /// <summary>
        ///     Resolve a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The component instance that provides the service.</returns>
        TService Resolve<TService>() where TService : class;

        /// <summary>
        ///     Resolve a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The component instance that provides the service.</returns>
        TService ResolveKeyed<TService>(object key) where TService : class;
        /// <summary>
        ///     Resolve a service.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The component instance that provides the service.</returns>
        object Resolve(Type serviceType);

        /// <summary>
        /// Indicates if a component of the given type has been configured.
        /// </summary>
        /// <param name="componentType">Component type to check.</param>
        /// <returns><c>true</c> if the <paramref name="componentType"/> is registered in the container or <c>false</c> otherwise.</returns>
        bool IsRegistered(Type componentType);
        bool IsRegistered<TComponentType>();
    }
}
