using Apache.Ignite.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CommonX.Cache.Ignite
{
    public class ExpressionConverter<TKey, TEntity> : ExpressionVisitor
    {
        private string theValuePropertyName { get; }
        public ExpressionConverter():this("Value")
        {
            
        }
        public ExpressionConverter(string theValuePropertyName)
        {
            this.theValuePropertyName = theValuePropertyName;
        }
        private ParameterExpression parameter;

        public Expression<Func<ICacheEntry<TKey, TEntity>, bool>> Modify(
            Expression<Func<TEntity, bool>> expression, 
            ParameterExpression parameter)
        {
            //这个是必须的，因为后面的visit遍历需要时刻用到这个parameter（在visitparameter方法中）
            this.parameter = parameter;
            return Expression.Lambda<Func<ICacheEntry<TKey, TEntity>, bool>>(
               Visit(expression.Body),
               this.parameter);
        }
        /// <summary>
        /// 这个目前还没有实现。参考VisitParameter。主要是不太会操作泛型
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Expression<Func<ICacheEntry<TKey, TEntity>, bool>> ModifyWithJoin(
            Expression<Func<TEntity, bool>> expression,
            params ParameterExpression[] parameters)
        {
            Expression<Func<ICacheEntry<TKey, TEntity>, bool>> exp = null;
            foreach(var p in parameters)
            {
                this.parameter = p;
                exp = Expression.Lambda<Func<ICacheEntry<TKey, TEntity>, bool>>(
                    Visit(expression.Body),
                    p);
            }
            return exp;
        }
        

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return Expression.Lambda<Func<ICacheEntry<TKey, TEntity>, bool>>(Visit(node.Body), Expression.Parameter(typeof(ICacheEntry<TKey, TEntity>)));
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // 这里通过识别 this.parameter 中包含的ICacheEntry.TEntity类型，替代typeof(TEntity), 就可以实现join
            if (node.Type == typeof(TEntity))
            {
                return Expression.Property(this.parameter, this.theValuePropertyName);
            }
            throw new InvalidOperationException();
        }
    }
}
