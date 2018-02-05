using System.Configuration;

namespace CommonX.Messages
{
    public class MessageSettings : ConfigurationSection 
    {
        public static string SectionName = "messagingConfiguration";

        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("defaultCulture", DefaultValue = "zh-CN", IsRequired = false, IsKey = false)]
        public string DefaultCulture
        {
            get { return (string)base["defaultCulture"]; }
            set { base["defaultCulture"] = value; }
        }

        [ConfigurationProperty("messages")]
        public MessageDataCollection Messages
        {
            get { return (MessageDataCollection)base["messages"]; }
        }

        [ConfigurationProperty("exceptionMappings")]
        public ExceptionMappingDataCollection ExceptionMappings
        {
            get { return (ExceptionMappingDataCollection)base["exceptionMappings"]; }
        }

        [ConfigurationProperty("projectSpecificXmlMessageFile")]
        public string ProjectSpecificXmlMessageFile
        {
            get { return (string)base["projectSpecificXmlMessageFile"]; }
        }

        public string MessageLoaderType { get; private set; }

        public string DefaultMessageLanguage
        {
            get { return "zh-CN"; } 
        }
    }
}
