using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CommonX.Cache.Configuration
{
    [Serializable]
    [ConfigurationCollection(typeof(SettingsEntry))]
    public class SettingsCollection : ConfigurationElementCollection
    {
        public SettingsEntry Get(string name)
        {
            return (SettingsEntry)this.BaseGet((object)name);
        }

        public bool Contains(string name)
        {
            return this.BaseGet((object)name) != null;
        }
        protected override ConfigurationElement CreateNewElement()
        {
            return new SettingsEntry();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as SettingsEntry).Key;
        }
    }
}
