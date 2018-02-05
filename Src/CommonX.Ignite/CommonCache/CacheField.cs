namespace CommonX.Cache.Ignite.CommonCache
{
    public class CacheField
    {
        readonly string _elementPrefix;
        readonly string _propertyName;

        public string ElementPrefix => _elementPrefix;
        public string PropertyName => _propertyName;

        public CacheField() { }

        public CacheField(string elementPropertyName)
        {
            _propertyName = elementPropertyName;
        }

        public CacheField(string elementName, string propertyName)
        {
            _elementPrefix = elementName;
            _propertyName = propertyName;
        }

        public bool IsAccept()
        {
            return (!string.IsNullOrEmpty(_elementPrefix)) && (!string.IsNullOrEmpty(_propertyName));
        }
    }
}
