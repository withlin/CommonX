using System;

namespace CommonX.Messages
{
    [Serializable]
    public class MessageKey
    {
        private MessageType type = MessageType.Unassigned;
        private string code = string.Empty;
        

        /// <summary>
        /// Default constructor
        /// </summary>
        public MessageKey()
        {
        }

        /// <summary>
        /// Two-parm constructor
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="code">Message code</param>
        public MessageKey(MessageType type, string code)
        {
            this.type = type;
            this.code = code;
        }

        public MessageType Type
        {
            get { return type; }
            set { type = value; }
        }

        public string Code
        {
            get { return code; }
            set { code = (value == null ? string.Empty : value); }
        }

        #region Overrides

        public override string ToString()
        {
            string typeAsString = string.Empty;
            string[] typeWords = type.ToString().Split(new char[] { '_' });
            foreach (string word in typeWords)
            {
                if (typeAsString.Length > 0)
                {
                    typeAsString += " ";
                }
                typeAsString += (word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower());
            }
            return (typeAsString + " " + code);
        }

        public override bool Equals(object obj)
        {
            bool equals = false;
            MessageKey compare = obj as MessageKey;
            if (compare != null)
            {
                equals = (type.Equals(compare.type) && code.Equals(compare.code));
            }
            return equals;
        }

        /// <summary>
        /// GetHashCode() override.
        /// </summary>
        /// <remarks>
        /// GetHashCode() override is required to enable using FlightKey in Dictionary<> and
        /// Hashtable<> collections, where we also want to use object attributes for testing object 
        /// equality.  (See Equals() override.)
        /// Refer to http://msdn2.microsoft.com/en-us/library/ms182358(VS.80).aspx.
        /// </remarks>
        /// <returns>Hash code based on object attributes</returns>
        public override int GetHashCode()
        {
            return (type.GetHashCode() ^ code.GetHashCode());
        }

        #endregion Overrides
    }
}
