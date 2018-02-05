using System.ComponentModel;

namespace CommonX.Exceptions
{
    public enum ExceptionType
    {
        [Description("Validation Exception")]
        ValidationException = 0,

        [Description("Application Exception")]
        ApplicationException = 1,

        [Description("System Exception")]
        SystemException = 2

    }
}
