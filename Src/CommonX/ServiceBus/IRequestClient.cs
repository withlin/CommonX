using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommonX.ServiceBus
{
    /// <summary>
    /// the message request client
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IRequestClient<in TRequest, TResponse>
    {
        /// <summary>
        /// request to a service
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResponse> Request(TRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// request to a service
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResponse> Request(string destination, TRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceEndPoint"></param>
        /// <param name="branchId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResponse> Request(string serviceEndPoint, string branchId, TRequest request, CancellationToken cancellationToken);


        /// <summary>
        ///     request to a service
        /// </summary>
        /// <param name="address"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResponse> Request(Uri address, TRequest request, CancellationToken cancellationToken);
    }
}
