using System.Collections.Generic;
using System.Configuration;

namespace CommonX.Messages
{
    public class MessageData : ConfigurationElement
    {
        private readonly Dictionary<string, string> values = new Dictionary<string, string>();

        public MessageData()
        {
        }

        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("loaderType", DefaultValue = "Util.Messaging.DefaultMessageLoader, Util.Messaging", IsKey = false, IsRequired = true)]
        public string LoaderType
        {
            get { return (string)base["loaderType"]; }
            set { base["loaderType"] = value; }
        }

        [ConfigurationProperty("resourceType")]
        public string ResourceType
        {
            get { return (string)base["resourceType"]; }
            set { base["resourceType"] = value; }
        }

        [ConfigurationProperty("resourceName")]
        public string ResourceName
        {
            get { return (string)base["resourceName"]; }
            set { base["resourceName"] = value; }
        }

        public bool Containes(string name)
        {
            return values.ContainsKey(name);
        }

        public string GetValue(string name)
        {
            if (values.ContainsKey(name))
                return values[name];

            return null;
        }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            values[name] = value;
            return true;
        }
    }
}
