using System.Configuration;

namespace CommonX.Messages
{
    public class MessageDataCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MessageData();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as MessageData).Name;
        }
    }
}
