using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CommonX.Cache.Configuration
{
    public class SettingsEntry : ConfigurationSection
    {
        /// <summary>
        ///     Propery/attribute names.
        /// </summary>
        private const string CacheKeyPrefixProperty = "CacheKeyPrefix";
        private const string UseDataCacheClientProperty = "UseDataCacheClient";
        private const string RequestTimeoutProperty = "RequestTimeout";
        private const string transportPropertiesProperty = "transportProperties";
        private const string SecurityPropertiesProperty = "securityProperties";

        private static volatile SettingsEntry defaultEntryValue;

        private static readonly object syncRoot = new object();

        internal static SettingsEntry DefaultEntry
        {
            get
            {
                if (defaultEntryValue == null)
                {
                    lock (syncRoot)
                    {
                        if (defaultEntryValue == null)
                        {
                            defaultEntryValue = new SettingsEntry();
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
        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get { return this["key"].ToString(); }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get { return this["value"].ToString(); }
            set { this["value"] = value; }
        }

        #endregion
    }
}
