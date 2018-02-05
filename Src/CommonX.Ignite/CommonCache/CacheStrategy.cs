using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CommonX.Cache.Ignite.CommonCache
{
    public interface ICacheStrategy
    {
        string[] GetSelectedFields();

        string[] GetPropertyNames();

        Dictionary<string, string> GetPropertyNames(string[] compositeFields);

        string GetTablePrefixByField(string fieldName);
    }

    public class CacheStrategy<TEntity> : ICacheStrategy
        where TEntity : class
    {
        private Dictionary<string, ICacheElement> _elements;
        private Dictionary<string, CacheField> _entityFields;
        private readonly Func<TEntity> _entityConstructor;

        public CacheStrategy()
        {
            _elements = new Dictionary<string, ICacheElement>();
            _entityFields = new Dictionary<string, CacheField>();

            string[] properites = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToArray();
            foreach (string p in properites)
            {
                _entityFields.Add(p, new CacheField());
            }

            ConstructorInfo constructor = typeof(TEntity).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if(constructor != null)
                _entityConstructor = (Func<TEntity>)Expression.Lambda(Expression.New(constructor)).Compile();
        }

        internal void MapFields(string entityProperty, CacheField field)
        {
            if (_entityFields.ContainsKey(entityProperty))
                _entityFields[entityProperty] = field;
        }

        public CacheStrategy<TEntity> Use<TElement>(string prefix, Action<CacheElement<TEntity, TElement>> elementToAdd)
            where TElement : class
        {
            if (string.IsNullOrEmpty(prefix))
                prefix = typeof(TElement).Name;

            if (_elements.ContainsKey(prefix))
                throw new InvalidOperationException(string.Format("缓存集别名 {0} 已经添加",prefix));

            var cache = new CacheElement<TEntity, TElement>(this, prefix);
            elementToAdd?.Invoke(cache);
            _elements.Add(prefix, cache);

            return this;
        }

        public TEntity GetInstanceForEntity()
        {
            if (_entityConstructor != null)
                return _entityConstructor();
            else
                return null;
        }

        #region ICacheStrategy Members
        public string[] GetSelectedFields()
        {
            return _entityFields.Where(f => f.Value.IsAccept()).Select(f => f.Key).ToArray();
        }

        public string GetTablePrefixByField(string fieldName)
        {
            CacheField field;
            if(!_entityFields.ContainsKey(fieldName))
            {
                KeyValuePair<string, CacheField>[] fields = _entityFields.Where(f => string.Compare(f.Value.PropertyName, fieldName, false) == 0).ToArray();
                if (fields.Count() > 1)
                    throw new InvalidOperationException("无法确定属性来源");
                if (fields.Count() == 0)
                    throw new InvalidOperationException(string.Format("使用了未映射的字段 {0} 作为筛选条件", fieldName));

                field = fields[0].Value;
            }
            else
            {
                field = _entityFields[fieldName];
            }

            return field.ElementPrefix;
        }

        public string[] GetPropertyNames()
        {
            return _entityFields.Keys.ToArray();
        }

        public Dictionary<string, string> GetPropertyNames(string[] compositeFields)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (compositeFields?.Length > 0)
            {
                var properties = _entityFields.Select(e => new { PropertyName = e.Key, FieldName = e.Value.PropertyName });
                for (int i = 0; i < compositeFields.Length; i++)
                {
                    string p_Name = compositeFields[i].ToUpper();
                    if (properties.Any(f => f.FieldName?.ToUpper() == p_Name))
                    {
                        string name = properties.Where(f => f.FieldName?.ToUpper() == p_Name).First().PropertyName;
                        result.Add(name, p_Name);
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
