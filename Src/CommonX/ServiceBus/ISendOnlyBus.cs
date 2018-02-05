using System.Threading;

namespace CommonX.ServiceBus
{
    /// <summary>
    ///     Provides the subset of bus operations that is applicable for a send only bus
    /// </summary>
    public interface ISendOnlyBus
    {
        ///// <summary>
        ///// Gets the list of key/value pairs that will be in the header of
        /////             messages being sent by the same thread.
        ///// 
        /////             This value will be cleared when a thread receives a message.
        ///// 
        ///// </summary>
        //IDictionary<string, string> OutgoingHeaders { get; }

        /// <summary>
        ///     Publish the message to subscribers.
        /// </summary>
        //void Publish<T>(T message) where T : class;

        ///// <summary>
        /////     Publish the message to subscribers.
        ///// </summary>
        //void Publish(object message);

        //void OrderPublish<T>(T message) where T : class;
        ///// <summary>
        ///// Instantiates a message of type T and publishes it.
        ///// 
        ///// </summary>
        ///// <typeparam name="T">The type of message, usually an interface</typeparam><param name="messageConstructor">An action which initializes properties of the message</param>
        //void Publish<T>(Action<T> messageConstructor);

        ///// <summary>
        ///// Sends the provided message.
        ///// </summary>
        ///// <param name="message">The message to send.</param>
        //void Send(object message);

        ///// <summary>
        ///// Instantiates a message of type T and sends it.
        ///// </summary>
        ///// <typeparam name="T">The type of message, usually an interface</typeparam>
        ///// <param name="messageConstructor">An action which initializes properties of the message</param>
        ///// <remarks>
        ///// The message will be sent to the destination configured for T
        ///// </remarks>
        //void Send<T>(Action<T> messageConstructor);

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="destination">
        ///     The address of the destination to which the message will be sent.
        /// </param>
        /// <param name="message">The message to send.</param>
        void Send(string destination, object message, bool isFullUri = false);

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="destination">
        ///     The address of the destination to which the message will be sent.
        /// </param>
        /// <param name="message">The message to send.</param>
        void OrderSend<T>(string destination, T message, CancellationToken cancelSend, bool isFullUri = false) where T : class;

        void OrderSend<T>(string serviceEndPoint, string branchId, T message, CancellationToken cancelSend) where T : class;
        void Send(string serviceEndPoint, string branchId, object message);
        ///// <param name="destination">The destination to which the message will be sent.</param>
        ///// <typeparam name="T">The type of message, usually an interface</typeparam>
        ///// </summary>
        ///// Instantiates a message of type T and sends it to the given destination.

        ///// <summary>
        ///// <param name="messageConstructor">An action which initializes properties of the message</param>
        //void Send<T>(string destination, Action<T> messageConstructor);


        ///// <summary>
        ///// Sends the message to the destination as well as identifying this
        ///// as a response to a message containing the Id found in correlationId.
        ///// </summary>
        //void Send(string destination, string correlationId, object message);

        ///// <summary>
        ///// Instantiates a message of the type T using the given messageConstructor,
        ///// and sends it to the destination identifying it as a response to a message
        ///// containing the Id found in correlationId.
        ///// </summary>
        //void Send<T>(string destination, string correlationId, Action<T> messageConstructor);
    }
}