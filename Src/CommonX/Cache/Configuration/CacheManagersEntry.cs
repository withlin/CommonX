using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Text;

namespace CommonX.Cache.Configuration
{
    /// <summary>
    ///     <para>
    ///         NOTE: This class is intended to be used by the cache framework
    ///         and not directly from application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///       
    ///     </para>
    ///     <para>
    ///         Instances of this class are created by the CacheConfig class, and
    ///         the values of the properties are obtained from a configuration
    ///         file.
    ///     </para>
    /// </remarks>
    public class CacheManagersEntry : ConfigurationElement, ICacheManagersEntry
    {
        /// <summary>
        ///     Propery/attribute names.
        /// </summary>
        private const string useDataCacheClientConfigruationProperty = "useDataCacheClientConfigruation";
        private const string localCacheEnabledProperty = "localCacheEnabled";
        private const string dataCacheLocalCacheInvalidationPolicyProperty = "dataCacheLocalCacheInvalidationPolicy";
        private const string defaultTimeOutProperty = "defaultTimeOut";
        private const string objectCountProperty = "objectCount";
        private const string allCacheProperty = "allCache";

        private static volatile CacheManagersEntry defaultEntryValue;
        private static readonly object syncRoot = new object();

        internal static CacheManagersEntry DefaultEntry
        {
            get
            {
                if (defaultEntryValue == null)
                {
                    lock (syncRoot)
                    {
                        if (defaultEntryValue == null)
                        {
                            defaultEntryValue = new CacheManagersEntry();
                        }
                    }
                }

                return defaultEntryValue;
            }
        }



        #region  Public Constructors 

        //%***************************************************************************
        //%*
        //%*                   Public Constructors
        //%*
        //%***************************************************************************

        #endregion

        #region  Public Properties 

        //%***************************************************************************
        //%*
        //%*                   Public Properties
        //%*
        //%***************************************************************************


        [ConfigurationProperty(useDataCacheClientConfigruationProperty, DefaultValue = false)]
        public bool UseDataCacheClientConfigruation
        {
            get { return Convert.ToBoolean(this[useDataCacheClientConfigruationProperty], CultureInfo.InvariantCulture); }
            set { this[useDataCacheClientConfigruationProperty] = value; }
        }

        [ConfigurationProperty(localCacheEnabledProperty, DefaultValue = false)]
        public bool LocalCacheEnabled
        {
            get { return Convert.ToBoolean(this[localCacheEnabledProperty], CultureInfo.InvariantCulture); }
            set { this[localCacheEnabledProperty] = value; }
        }

        [ConfigurationProperty(dataCacheLocalCacheInvalidationPolicyProperty, DefaultValue = "")]
        public string DataCacheLocalCacheInvalidationPolicy
        {
            get { return Convert.ToString(this[dataCacheLocalCacheInvalidationPolicyProperty], CultureInfo.InvariantCulture); }
            set { this[dataCacheLocalCacheInvalidationPolicyProperty] = value; }
        }

        [ConfigurationProperty(defaultTimeOutProperty, DefaultValue = 30000)]
        public Int32 DefaultTimeOut
        {
            get { return Convert.ToInt32(this[defaultTimeOutProperty], CultureInfo.InvariantCulture); }
            set { this[defaultTimeOutProperty] = value; }
        }

        [ConfigurationProperty(objectCountProperty, DefaultValue = 10000)]
        public Int32 ObjectCount
        {
            get { return Convert.ToInt32(this[objectCountProperty], CultureInfo.InvariantCulture); }
            set { this[objectCountProperty] = value; }
        }
        [ConfigurationProperty(allCacheProperty, DefaultValue = true)]
        public bool AllCache
        {
            get { return Convert.ToBoolean(this[allCacheProperty]); }
            set { this[allCacheProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the element.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// The name of the element.
        /// 
        /// </value>
        [StringValidator(MinLength = 1)]
        [ConfigurationProperty("name", DefaultValue = "Name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = (object)value;
            }
        }
        [ConfigurationProperty("key")]
        public string Key
        {
            get
            {
                return (string)this["key"];
            }
            set
            {
                this["key"] = value;
            }
        }
        [ConfigurationProperty("indexs")]
        public string Indexs
        {
            get
            {
                return this["indexs"]?.ToString();
            }
            set
            {
                this["indexs"] = value;
            }
        }
        [ConfigurationProperty("loader")]
        public string Loader
        {
            get { return this["loader"].ToString(); }
            set { this["loader"] = value; }
        }

        [ConfigurationProperty("cacheMode")]
        public string CacheMode
        {
            get
            {
                return this["cacheMode"].ToString();
            }
            set
            {
                this["cacheMode"] = value;
            }
        }
        [ConfigurationProperty("nodeCount")]
        public string NodeCount
        {
            get { return this["nodeCount"].ToString(); }
            set { this["nodeCount"] = value; }
        }
        #endregion

    }
}
