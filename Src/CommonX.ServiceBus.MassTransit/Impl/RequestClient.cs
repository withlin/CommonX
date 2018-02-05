using MassTransit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommonX.ServiceBus.MassTransit.Impl
{
    /// <summary>
    ///     the message request client
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class RequestClient<TRequest, TResponse> : IRequestClient<TRequest, TResponse> where TRequest : class
        where TResponse : class
    {
        private readonly global::MassTransit.IBus _bus;

        public RequestClient(global::MassTransit.IBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        ///     request to a service
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<TResponse> Request(TRequest request, CancellationToken cancellationToken)
        {
            return Request(Utils.GetFullUri(typeof(TRequest).Name), request, cancellationToken);
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
            return Request(Utils.GetFullUri(destination), request, cancellationToken);
        }

        public Task<TResponse> Request(string serviceEndPoint, string branchId, TRequest request, CancellationToken cancellationToken)
        {
            string destination = $"{serviceEndPoint}-{branchId}";
            return Request(Utils.GetFullUri(destination, true), request, cancellationToken);
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
            var requestTimeout = TimeSpan.FromSeconds(300);

            var client = new MessageRequestClient<TRequest, TResponse>(_bus, address, requestTimeout);

            return client.Request(request, cancellationToken);
        }
    }
}
