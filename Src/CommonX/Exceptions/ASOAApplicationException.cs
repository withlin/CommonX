using CommonX.Messages;
using System;
using System.Runtime.Serialization;

namespace CommonX.Exceptions
{
    [Serializable]
    public class ASOAApplicationException : BaseApplicationException
    {

        #region Constructors

        /// <summary>
        /// Protected constructor to de-serialize data
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public ASOAApplicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ASOAApplicationException(string message, System.Exception innerException)
            : base(message, (System.Exception) innerException)
        {

        }

        public ASOAApplicationException(
            string carrierCode,
            MessageKey messageKey,
            System.Exception innerException,
            string[] messageVariables = null,
            object request = null,
            object response = null)
            : base(
                carrierCode,
                messageKey,
                innerException,
                true,
                messageVariables,
                request,
                response)
        {
        }

        public ASOAApplicationException(string carrierCode, MessageKey messageKey, string[] messageVariables = null, object request = null, object response = null)
            : this(carrierCode, messageKey, null, messageVariables, request, response)
        {

        }

        #endregion

        protected override MessageKey DefaultMessageKey
        {
            get { return new MessageKey(MessageType.ApplicationError, "999"); }
        }
    }
}
