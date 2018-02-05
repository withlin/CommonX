using CommonX.Logging;
using System.Configuration;

namespace CommonX.Messages
{
    public class ExceptionMappingDataCollection : ConfigurationElementCollection
    {
        //protected override ConfigurationElement CreateNewElement()
        //{
        //    //return new ExceptionMappingData();
        //}

        //protected override object GetElementKey(ConfigurationElement element)
        //{
        //    //return (element as ExceptionMappingData).Name;
        //    return "";
        //}
        protected override ConfigurationElement CreateNewElement()
        {
            throw new System.NotImplementedException();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            throw new System.NotImplementedException();
        }
    }
}
