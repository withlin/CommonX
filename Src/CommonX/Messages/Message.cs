using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CommonX.Messages
{
    [Serializable]
    public class Message
    {
        private const string FormatVariablePattern = "{\\d+}";
        private const string NullMessageVariable = "null";
        private const string BlankMessageVariable = "blank";
        private const string DummyMessageVariable = "???";
        private string _description = string.Empty;
        private int _eventId = MessageConstants.DefaultEventId;
        private string _iataCode = string.Empty;

        private MessageKey _key;
        private string _shortText = string.Empty;
        private string _status = string.Empty;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public Message(MessageKey key)
        {
            Key = key;
        }

        public Message(Message msg) : this(msg._key)
        {
            _shortText = msg._shortText;
            _status = msg._status;
            _description = msg._description;
            _iataCode = msg._iataCode;
            _eventId = msg._eventId;
        }

        public MessageKey Key
        {
            get { return _key; }
            set { _key = (value ?? new MessageKey()); }
        }

        public int EventId
        {
            get { return _eventId; }
            set { _eventId = value; }
        }

        public string ShortText
        {
            get { return _shortText; }
            set { _shortText = (value ?? string.Empty); }
        }

        public string Description
        {
            get { return _description; }
            set { _description = (value ?? string.Empty); }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public string IATACode
        {
            get { return _iataCode; }
            set { _iataCode = (value ?? string.Empty); }
        }

        public override string ToString()
        {
            return (_description + " [" + _key + "]").Trim();
        }

        public Message Format(params string[] variables)
        {
            var copy = new Message(this);

            if (variables != null && variables.Length > 0)
            {
                var matchCollection = new Regex(FormatVariablePattern).Matches(copy._shortText + copy._description);

                long varCount = 0;
                var tempMatchCollection = new List<string>();
                foreach (Match match in matchCollection)
                {
                    if (tempMatchCollection.Contains(match.Value) == false)
                    {
                        tempMatchCollection.Add(match.Value);
                    }
                }
                varCount = tempMatchCollection.Count;

                var msgVars = variables;
                var editedMsgVars = new string[varCount];
                for (var i = 0; i < editedMsgVars.Length; i++)
                {
                    editedMsgVars[i] = (msgVars != null && i < msgVars.Length
                        ? EditMessageVariable(msgVars[i])
                        : DummyMessageVariable);
                }
                copy._shortText = string.Format(copy._shortText, editedMsgVars);
                copy._description = string.Format(copy._description, editedMsgVars);
            }

            return copy;
        }

        /// <summary>
        ///     Converts message variable to 'null' or 'blank' when appropriate; otherwise returns
        ///     value untouched.
        /// </summary>
        /// <param name="msgVar">Message variable</param>
        /// <returns>Edited message variable</returns>
        private string EditMessageVariable(string msgVar)
        {
            return (msgVar == null ? NullMessageVariable : (msgVar.Length == 0 ? BlankMessageVariable : msgVar));
        }

        /// <summary>
        ///     Get Message Code
        /// </summary>
        /// <returns></returns>
        public string GetMessageCode()
        {
            if (string.IsNullOrEmpty(IATACode))
                return _key.Code;
            return IATACode;
        }

        /// <summary>
        ///     Get abbreviate of Message Type
        /// </summary>
        /// <returns></returns>
        public string GetMessageType()
        {
            var type = string.Empty;
            switch (Key.Type)
            {
                case MessageType.ValidationError:
                    type = "VAL";
                    break;
                case MessageType.ApplicationError:
                    type = "APP";
                    break;
                case MessageType.SystemError:
                    type = "SYS";
                    break;
                case MessageType.Info:
                    type = "INF";
                    break;
                case MessageType.Warn:
                    type = "WRN";
                    break;
                case MessageType.Debug:
                    type = "DEB";
                    break;
            }
            return type;
        }
    }
}