using CommonX.Components;
using System.Configuration;

namespace CommonX.DataAccess.Xml
{
    /// <summary>configuration class Xml extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>Use Xml to implement the Repository.
        /// </summary>
        /// <returns></returns>
        //public static Configuration UseXmlRepository(this Configuration configuration)
        //{
        //    ObjectContainer.Current.RegisterGeneric(typeof(IRepository<,>), typeof(XmlRepository<>));
        //    return configuration;
        //}

        /// <summary>Use Xml to implement the Repository.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseXmlOnlyRepository(this Configuration configuration)
        {
            ObjectContainer.Current.RegisterGeneric(typeof(IXmlRepository<,>), typeof(XmlRepository<>));
            return configuration;
        }

        /// <summary>Use Xml to implement the Repository.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseXmlReadOnlyRepository(this Configuration configuration)
        {
            ObjectContainer.Current.RegisterGeneric(typeof(IXmlRepository<,>), typeof(XmlReadRepository<>));
            return configuration;
        }
    }
}