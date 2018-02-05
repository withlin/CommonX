using System.ComponentModel;

namespace CommonX.Messages
{
    public enum MessageType
    {
        [Description("Type Code: 0")] Unassigned = 0,
        [Description("Type Code: 1")] ValidationError = 1,
        [Description("Type Code: 2")] ApplicationError = 2,
        [Description("Type Code: 3")] SystemError = 3,
        [Description("Type Code: 4")] Info = 4,
        [Description("Type Code: 5")] Warn = 5,
        [Description("Type Code: 6")] Debug = 6,
        [Description("Type Code: 7")] DatabaseError = 7
    }
}