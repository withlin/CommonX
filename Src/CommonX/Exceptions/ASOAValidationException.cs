using System;
using System.Runtime.Serialization;
using CommonX.Messages;

namespace CommonX.Exceptions
{
    [Serializable]
    public class ASOAValidationException : BaseApplicationException
    {

        #region Constructors

        /// <summary>
        /// Protected constructor to de-serialize data
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ASOAValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ASOAValidationException(
             string carrierCode,
             MessageKey messageKey,
             Exception innerException,
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

        public ASOAValidationException(string carrierCode, MessageKey messageKey, string[] messageVariables = null, object request = null, object response = null)
            : this(carrierCode, messageKey, null, messageVariables, request, response)
        {

        }

        #endregion

        protected override MessageKey DefaultMessageKey
        {
            get { return new MessageKey(MessageType.ValidationError, "999"); }
        }
    }
}
