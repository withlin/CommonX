using Castle.DynamicProxy;
using CommonX.Cache;
using CommonX.Components;
using CommonX.Logging;
using CommonX.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CommonX.Autofac
{
    /// <summary>
    ///     表示用于方法缓存功能的拦截行为。需要放到最后一步
    /// </summary>
    public class CachingInterceptor : IInterceptor
    {
        private readonly ICacheFactory _cacheFactory;
        private readonly ILogger _logger;
        private readonly string carrierCode = "CachingInterceptor";
        private readonly ICacheConfiguration _cacheConfiguration;

        public CachingInterceptor(ILoggerFactory loggerFactory, ICacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
            _logger = loggerFactory.Create(GetType());
            //ICacheConfiguration在这里不能注入，会导致循环注入异常
            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
            {
                _cacheConfiguration = scope.Resolve<ICacheConfiguration>();
            }
        }

        #region IInterceptionBehavior Members

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;
            var key = CacheUtility.GetCacheName(invocation.MethodInvocationTarget.ReflectedType);
            //var config =
            //    CacheConfiguration.Instance.CacheConfigurations.FirstOrDefault(
            //        a => TypeUtils.GetFormattedName(a.Name).Equals(key));

            var config = _cacheConfiguration.Instance.CacheConfigurations.FirstOrDefault(a => TypeUtils.GetFormattedName(a.Name).Equals(key));

            if (method.IsDefined(typeof(CachingAttribute), true) && config != null)
            {
                var cachingAttribute =
                    (CachingAttribute)method.GetCustomAttributes(typeof(CachingAttribute), true)[0];
                var cache = _cacheFactory.GetCache(carrierCode, key);
                switch (cachingAttribute.Method)
                {
                    case CachingMethod.Get:
                        {
                            //全局和实体都为true,才走ignite
                            //if (CacheConfiguration.Instance.UseFullCache && config.AllCache)
                            if (_cacheConfiguration.Instance.UseFullCache && config.AllCache)
                            {
                                cache = _cacheFactory.GetCache(carrierCode, invocation.TargetType.GenericTypeArguments[0]);
                                var arg = invocation.Arguments.Length > 0 ? invocation.Arguments[0] : null;
                                //此处只支持表达式的解析,类似Get(PK)的方法会绕过,从DbContext走EF的内部机制
                                if ((arg != null) && (arg.GetType().BaseType?.Name == "LambdaExpression"))
                                {
                                    var cacheType = cache.GetType();
                                    var getMethod = GetMethod(cacheType, "GetByExpression", true,
                                        invocation.TargetType.GenericTypeArguments[0]);
                                    var result = getMethod.MakeGenericMethod(invocation.TargetType.GenericTypeArguments[0])
                                        .Invoke(
                                            cache, new[]
                                            {
                                            arg
                                            });
                                    //缓存没数据
                                    if (result == null || ((IList)result).Count == 0)
                                    {
                                        invocation.Proceed();
                                    }
                                    else
                                    {
                                        //有可能是first查询
                                        if (invocation.GetConcreteMethod().ReturnType.Name.Contains("IQueryable"))
                                            invocation.ReturnValue = ((IList)result).AsQueryable();
                                        else
                                            invocation.ReturnValue = ((IList)result).Count > 0 ? ((IList)result)[0] : null;
                                    }
                                }
                                else
                                {
                                    invocation.Proceed();
                                }
                            }
                            else
                            {
                                var valKey = GetValueKey(cachingAttribute, invocation);
                                if (cache.Contains(valKey))
                                {
                                    var obj = cache.Get(valKey);
                                    invocation.ReturnValue =
                                        invocation.GetConcreteMethod().ReturnType.Name.Contains("IQueryable")
                                            ? ((IList)obj).AsQueryable()
                                            : obj;
                                }
                                else
                                {
                                    invocation.Proceed();
                                    var val = invocation.ReturnValue;
                                    if (val is IQueryable)
                                    {
                                        //转换为list，queryable无法序列化
                                        var q = val as IQueryable;
                                        var genericType = typeof(List<>).MakeGenericType(q.ElementType);
                                        var constructor =
                                            genericType.GetDeclaredConstructors().Skip(2).Take(1).FirstOrDefault();
                                        val = constructor?.Invoke(new object[] { q }) as IList;
                                    }
                                    if (val != null)
                                    {
                                        if (invocation.ReturnValue is IQueryable && !(val is IQueryable))
                                            invocation.ReturnValue = (val as IList).AsQueryable();
                                        else
                                            invocation.ReturnValue = val;
                                        cache.Add(valKey, val);
                                        if (_logger.IsDebugEnabled)
                                            _logger.Debug(
                                                string.Format("CachingInterceptor Get: Key:{0} valKey:{1} addTime:{2}", key,
                                                    valKey, DateTime.Now));
                                    }
                                }
                            }
                        }
                        break;
                    //TODO 需要全缓存才有用 改ogg方式
                    //case CachingMethod.Put:
                    //{
                    //    invocation.Proceed();
                    //    if (cache.Contains(valKey))
                    //    {
                    //        if (cachingAttribute.Force)
                    //        {
                    //            cache.Remove(valKey);
                    //            cache.Add(valKey, invocation.ReturnValue);
                    //        }
                    //        else
                    //            cache.Put(valKey, invocation.ReturnValue);
                    //    }
                    //    else
                    //        cache.Add(valKey, invocation.ReturnValue);

                    //    if (_logger.IsInfoOn)
                    //        _logger.Info(string.Format("CachingInterceptor Put: Key:{0} valKey:{1} addTime:{2}", key,
                    //            valKey, DateTime.Now));
                    //}
                    //    break;
                    //case CachingMethod.Remove:
                    //{
                    //    var removeKeys = cachingAttribute.CorrespondingMethodNames;
                    //    foreach (var removeKey in removeKeys)
                    //    {
                    //        if (key.Equals(TypeUtils.GetFormattedName(removeKey)))
                    //        {
                    //            cache.Flush();
                    //        }
                    //        // if (cache.Contains(removeKey))
                    //        //     cache.Remove(removeKey);
                    //    }
                    //    invocation.Proceed();

                    //    if (_logger.IsInfoOn)
                    //        _logger.Info(string.Format("CachingInterceptor Remove: Key:{0} valKey:{1} addTime:{2}", key,
                    //            valKey, DateTime.Now));
                    //}
                    //    break;
                    default:
                        invocation.Proceed();
                        break;
                }
            }
            else
            {
                invocation.Proceed();
            }
        }

        #endregion

        #region Private Methods

        private MethodInfo GetMethod(Type type, string name, bool IsGeneric, params Type[] types)
        {
            var lstMethods =
                new List<MethodInfo>(
                    type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            var mi =
                lstMethods.FirstOrDefault(
                    r => (r.Name == name) && ((r.IsGenericMethod && IsGeneric) || (!r.IsGenericMethod && !IsGeneric)) &&
                         (r.GetGenericArguments().Length == types.Length));
            if (mi != null) return mi;
            throw new MissingMethodException();
        }

        /// <summary>
        ///     根据指定的<see cref="CachingAttribute" />以及<see cref="IInvocation" />实例，
        ///     获取与某一特定参数值相关的键名。
        /// </summary>
        /// <param name="cachingAttribute"><see cref="CachingAttribute" />实例。</param>
        /// <param name="invocation"><see cref="IInvocation" />实例。</param>
        /// <returns>与某一特定参数值相关的键名。</returns>
        private string GetValueKey(CachingAttribute cachingAttribute, IInvocation invocation)
        {
            switch (cachingAttribute.Method)
            {
                // 如果是Remove，则不存在特定值键名，所有的以该方法名称相关的缓存都需要清除
                case CachingMethod.Remove:
                    return null;
                // 如果是Get或者Put，则需要产生一个针对特定参数值的键名
                case CachingMethod.Get:
                case CachingMethod.Put:
                    if ((invocation.Arguments != null) &&
                        invocation.Arguments.Any())
                    {
                        var args = new List<string> { invocation.Method.Name };
                        foreach (var arg in invocation.Arguments)
                            if (arg != null)
                            {
                                var ar = arg.ToString();
                                if (arg.GetType().BaseType.Name == "LambdaExpression")
                                    ar = new ExpressionTranslator().Translate(arg as Expression);
                                args.Add(ar);
                            }
                        return string.Join("_", args);
                    }
                    return invocation.Method.Name;
                default:
                    throw new InvalidOperationException("无效的缓存方式。");
            }
        }

        #endregion
    }
}
