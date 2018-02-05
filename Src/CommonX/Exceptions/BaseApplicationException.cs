using CommonX.Components;
using CommonX.Messages;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
namespace CommonX.Exceptions
{
    /// <summary>
    ///     Base Application Exception Class.
    ///     You can use this as the base exception object from which to derive your applications exception hierarchy.
    /// </summary>
    [Serializable]
    public abstract class BaseApplicationException : ApplicationException, ISerializable
    {
        protected abstract MessageKey DefaultMessageKey { get; }

        /// <summary>
        ///     Override the GetObjectData method to serialize custom values.
        /// </summary>
        /// <param name="info">Represents the SerializationInfo of the exception.</param>
        /// <param name="context">Represents the context information of the exception.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("machineName", MachineName, typeof(string));
            info.AddValue("createdDateTime", CreatedDateTime);
            info.AddValue("appDomainName", AppDomainName, typeof(string));
            info.AddValue("threadIdentity", ThreadIdentityName, typeof(string));
            info.AddValue("windowsIdentity", WindowsIdentityName, typeof(string));
            info.AddValue("additionalInformation", AdditionalInformation, typeof(NameValueCollection));
            info.AddValue("richMessage", RichMessage, typeof(Message));
            base.GetObjectData(info, context);
        }

        /// <summary>
        ///     Initialization function that gathers environment information safely.
        /// </summary>
        private void InitializeEnvironmentInformation()
        {
            try
            {
                MachineName = Environment.MachineName;
            }
            catch (SecurityException)
            {
                MachineName = string.Empty; // resourceManager.GetString("RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED");
            }
            catch
            {
                MachineName = string.Empty;
                // resourceManager.GetString("RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION");
            }

            try
            {
                ThreadIdentityName = Thread.CurrentPrincipal.Identity.Name;
            }
            catch (SecurityException)
            {
                ThreadIdentityName = string.Empty;
                // resourceManager.GetString("RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED");
            }
            catch
            {
                ThreadIdentityName = string.Empty;
                // resourceManager.GetString("RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION");
            }

            try
            {
                WindowsIdentityName = WindowsIdentity.GetCurrent().Name;
            }
            catch (SecurityException)
            {
                WindowsIdentityName = string.Empty;
                // resourceManager.GetString("RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED");
            }
            catch
            {
                WindowsIdentityName = string.Empty;
                // resourceManager.GetString("RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION");
            }

            try
            {
                AppDomainName = AppDomain.CurrentDomain.FriendlyName;
            }
            catch (SecurityException)
            {
                AppDomainName = string.Empty; // resourceManager.GetString("RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED");
            }
            catch
            {
                AppDomainName = string.Empty;
                // resourceManager.GetString("RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION");
            }

            if (Data.Contains(MessageConstants.LoggedIndicator))
            {
                Data[MessageConstants.LoggedIndicator] = false;
            }
            else
            {
                Data.Add(MessageConstants.LoggedIndicator, false);
            }

            RichMessage = new Message(DefaultMessageKey);
        }

        private void BuildMessageFromInnerException()
        {
            var key = DefaultMessageKey;
            string[] variables = null;

            if (InnerException != null)
            {
                variables = new[] { InnerException.Message };

                var errorCode = MessageManager.GetErrorCode(InnerException.GetType().Name);
                if (!string.IsNullOrEmpty(errorCode))
                    key = new MessageKey(DefaultMessageKey.Type, errorCode);
            }

            RichMessage = MessageManager.GetMessage(key, variables);
        }

        private void SetDefaultErrorCode()
        {
            if (RichMessage != null &&
                RichMessage.Key != null &&
                RichMessage.Key == MessageKeys.Unassigned)
            {
                RichMessage.Key = DefaultMessageKey;
            }
        }

        #region Declare Member Variables

        // Member variable declarations

        private static IMessageManager MessageManager
        {
            get
            {
                using (var scope = ObjectContainer.BeginLifetimeScope())
                {
                    return scope.Resolve<IMessageManager>();
                }
            }
        }


        // Collection provided to store any extra information associated with the exception.

        #endregion

        #region Constructors

        protected BaseApplicationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BaseApplicationException(
            string carrierCode,
            MessageKey messageKey,
            Exception innerException,
            bool knownMessage = true,
            string[] messageVariables = null,
            object request = null,
            object response = null)
            : this(
                carrierCode, MessageManager.GetMessage(messageKey, messageVariables), innerException, knownMessage,
                messageVariables, request, response)
        {
        }

        protected BaseApplicationException(
            string carrierCode,
            Message message,
            Exception innerException,
            bool knownMessage = true,
            string[] messageVariables = null,
            object request = null,
            object response = null)
            : base(message.Format(messageVariables).ToString(), innerException)
        {
            InitializeEnvironmentInformation();
            if (!knownMessage)
                BuildMessageFromInnerException();
            CarrierCode = carrierCode;
            RichMessage = message;
            Request = request;
            Response = response;
            Params = messageVariables;
        }

        /// <summary>
        ///     Constructor used for deserialization of the exception class.
        /// </summary>
        /// <param name="info">Represents the SerializationInfo of the exception.</param>
        /// <param name="context">Represents the context information of the exception.</param>
        protected BaseApplicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MachineName = info.GetString("machineName");
            CreatedDateTime = info.GetDateTime("createdDateTime");
            AppDomainName = info.GetString("appDomainName");
            ThreadIdentityName = info.GetString("threadIdentity");
            WindowsIdentityName = info.GetString("windowsIdentity");
            AdditionalInformation =
                (NameValueCollection)info.GetValue("additionalInformation", typeof(NameValueCollection));
            RichMessage = (Message)info.GetValue("richMessage", typeof(Message));
        }

        #endregion

        //    if (this.RichMessage != null)

        #region Public Properties

        public string[] Params { get; set; }
        public string CarrierCode { get; set; }
        public object Request { get; set; }
        public object Response { get; set; }

        public CultureInfo LocalCulture { get; set; }

        /// <summary>
        ///     Machine name where the exception occurred.
        /// </summary>
        public string MachineName { get; private set; }

        /// <summary>
        ///     Date and Time the exception was created.
        /// </summary>
        /// 

        private DateTime _createDateTime = DateTime.Now;
        public DateTime CreatedDateTime
        {
            get { return _createDateTime; }
            set { _createDateTime = value; }
        }

        /// <summary>
        ///     AppDomain name where the exception occurred.
        /// </summary>
        public string AppDomainName { get; private set; }

        /// <summary>
        ///     Identity of the executing thread on which the exception was created.
        /// </summary>
        public string ThreadIdentityName { get; private set; }

        /// <summary>
        ///     Windows identity under which the code was running.
        /// </summary>
        public string WindowsIdentityName { get; private set; }

        /// <summary>
        ///     Collection allowing additional information to be added to the exception.
        /// </summary>
        private NameValueCollection _additionalInformation = new NameValueCollection();

        public NameValueCollection AdditionalInformation
        {
            get { return _additionalInformation; }
            set { _additionalInformation = value; }
        }

        /// <summary>
        ///     Rich message
        /// </summary>
        public Message RichMessage { get; private set; }

        public override string ToString()
        {
            return RichMessage.Description;
        }

        /// <summary>
        ///     attached innerException stacktrace information
        /// </summary>
        public override string StackTrace
        {
            get
            {
                if (InnerException == null)
                {
                    return base.StackTrace;
                }
                var messageContent = InnerException is BaseApplicationException
                    ? (InnerException as BaseApplicationException).RichMessage.Description
                    : InnerException.Message;
                return
                    string.Format("{0}" + Environment.NewLine + "InnerException({1}):{2}" + Environment.NewLine + "{3}",
                        base.StackTrace, InnerException.GetType().Name, messageContent, InnerException.StackTrace);
            }
        }

        #endregion

    }
}