using System;
using System.Threading;
using System.Threading.Tasks;
namespace CommonX.ServiceBus
{
    /// <summary>
    ///     the message request client
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class EmptyRequestClient<TRequest, TResponse> : IRequestClient<TRequest, TResponse> where TRequest : class
        where TResponse : class
    {
        /// <summary>
        ///     request to a service
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<TResponse> Request(TRequest request, CancellationToken cancellationToken)
        {
            return null;
        }

        /// <summary>
        ///     request to a service
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<TResponse> Request(string destination, TRequest request, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<TResponse> Request(string serviceEndPoint, string branchId, TRequest request, CancellationToken cancellationToken)
        {
            return null;
        }

        /// <summary>
        ///     request to a service
        /// </summary>
        /// <param name="address"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<TResponse> Request(Uri address, TRequest request, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}