
namespace CommonX.Messages
{
    public static class MessageKeys
    {
        public static readonly MessageKey Unassigned = new MessageKey(MessageType.Unassigned, "000");

        public static class SystemErrors
        {
            public static readonly string ErrorType = "SystemErrors";


            public static readonly MessageKey UndefinedDataException = new MessageKey(MessageType.SystemError, "997");
            public static readonly MessageKey UndefinedSystemException = new MessageKey(MessageType.SystemError, "998");
            public static readonly MessageKey UndefinedException = new MessageKey(MessageType.SystemError, "999");

            public static readonly MessageKey SystemError905 = new MessageKey(MessageType.SystemError, "905");
            public static readonly MessageKey SystemError903 = new MessageKey(MessageType.SystemError, "903");
        }

         public static class ApplicationErrors
         {
             public static readonly string ErrorType = "ApplicationErrors";

             public static readonly MessageKey UniqueKeyConstraint9024 = new MessageKey(MessageType.ApplicationError, "9024");
             public static readonly MessageKey UpdateConflict9025 = new MessageKey(MessageType.ApplicationError, "9025");
             public static readonly MessageKey DeadLock9026 = new MessageKey(MessageType.ApplicationError, "9026");
        
         }
        
    }
}
