using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CommonX.Cache.Ignite.CommonCache
{
    public interface ICacheElement
    {
        string Prefix { get; }

        Type ElementType { get; }

        string GetTableName();
    }

    public class CacheElement<TEntity, TElement> : ICacheElement
        where TEntity : class where TElement : class
    {
        private readonly CacheStrategy<TEntity> _strategy;
        private readonly string _prefix;

        public CacheElement(CacheStrategy<TEntity> strategy, string prefiex)
        {
            _strategy = strategy;
            _prefix = prefiex;
        }

        public string Prefix => _prefix;

        public Type ElementType => typeof(TElement);

        public string GetTableName()
        {
            string name = typeof(TElement).Name;

            return string.Format("\"{0}\".{1} as {2}", name, name.ToUpper(), _prefix);
        }

        /// <summary>
        /// 添加所有源类型和目标类型可匹配的属性映射
        /// </summary>
        /// <returns></returns>
        public CacheElement<TEntity, TElement> IncludeAllFields()
        {
            string elementName = typeof(TElement).Name;
            Type elementType = typeof(TElement);
            string[] properties = elementType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Select(p => p.Name).ToArray();
            string[] dest_properties = _strategy.GetPropertyNames();

            //源字段和目标字段一致，可以自动匹配
            string[] matched_properties = dest_properties.Intersect(properties).ToArray();
            foreach (string p in matched_properties)
            {
                _strategy.MapFields(p, new CacheField(_prefix, p));
            }

            return this;
        }

        /// <summary>
        /// 匹配源对象和目标对象不一致的字段
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="srcProerpty"></param>
        /// <param name="destProperty"></param>
        /// <returns></returns>
        public CacheElement<TEntity, TElement> Map<TProperty>(Expression<Func<TElement, TProperty>> srcProerpty, Expression<Func<TEntity, TProperty>> destProperty)
        {
            MemberExpression srcExp = GetMember(srcProerpty);
            MemberExpression destExp = GetMember(destProperty);
            string src = srcExp.Member.Name;
            string dest = destExp.Member.Name;
            string elementName = typeof(TElement).Name;
            CacheField field = new CacheField(_prefix, src);

            _strategy.MapFields(dest, field);

            return this;
        }

        public CacheElement<TEntity, TElement> Include<TProperty>(Expression<Func<TElement, TProperty>> srcProerpty, Expression<Func<TEntity, TProperty>> destProperty)
        {
            MemberExpression srcExp = GetMember(srcProerpty.Body);
            MemberExpression destExp = GetMember(destProperty.Body);
            string src = srcExp.Member.Name;
            string dest = destExp.Member.Name;
            string elementName = typeof(TElement).Name;
            CacheField field = new CacheField(_prefix, src);
            _strategy.MapFields(dest, field);

            return this;
        }

        MemberExpression GetMember(Expression exp)
        {
            if (exp.NodeType == ExpressionType.MemberAccess)
                return exp as MemberExpression;
            else 
            {
                Expression t = (exp as UnaryExpression)?.Operand;
                return GetMember(t);
            }            
        }
    }
}
