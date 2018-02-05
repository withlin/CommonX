using System;
using System.Threading;

namespace CommonX.ServiceBus
{
    [Serializable]
    public class EmptyBus : IBus
    {
        public void Dispose()
        {
            
        }

        public void Publish<T>(T message) where T : class
        {
            
        }

        /// <summary>
        /// Publish the message to subscribers.
        /// 
        /// </summary>
        public void Publish(object message)
        {
            
        }

        public void OrderPublish<T>(T message) where T : class
        {
        }

        public void Publish<T>(Action<T> messageConstructor)
        {
            
        }

        public void Send(object message)
        {
            
        }

        public void Send<T>(Action<T> messageConstructor)
        {
            
        }

        public void Send(string destination, object message, bool isFullUri = false)
        {
            
        }

        public void OrderSend<T>(string destination, T message, bool isFullUri = false) where T:class 
        {
        }

        public void OrderSend<T>(string serviceEndPoint, string branchId, T message) where T : class
        {
        }

        public void Send<T>(string destination, Action<T> messageConstructor)
        {
            
        }

        //public void Send(string destination, string correlationId, object message)
        //{
            
        //}

        public void Send(string serviceEndPoint, string branchId, object message)
        {

        }

        public void Send<T>(string destination, string correlationId, Action<T> messageConstructor)
        {
            
        }

        public void OrderSend<T>(string destination, T message, CancellationToken cancelSend, bool isFullUri = false) where T : class
        {

        }

        public void OrderSend<T>(string serviceEndPoint, string branchId, T message, CancellationToken cancelSend) where T : class
        {
        }
    }
}
