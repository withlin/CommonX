using Castle.DynamicProxy;
using CommonX.Attribute;
using CommonX.Components;
using CommonX.Logging;
using CommonX.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CommonX.Autofac
{
    /// <summary>
    ///     Interceptor for query lookup,put after cache interceptor
    /// </summary>
    public class LookupQueryInterceptor : IInterceptor
    {
        private static readonly object LockObj = new object();
        private static readonly object LockType = new object();
        private static readonly object LockMethod = new object();
        private static readonly object LockService = new object();

        private static readonly ConcurrentDictionary<Type, Dictionary<PropertyInfo, LookupQueryAttribute>> Lookups =
            new ConcurrentDictionary<Type, Dictionary<PropertyInfo, LookupQueryAttribute>>();

        private static readonly ConcurrentDictionary<string, MethodInfo> MethodInfos =
            new ConcurrentDictionary<string, MethodInfo>();

        private static readonly ConcurrentDictionary<string, Type> ServiceInfos =
            new ConcurrentDictionary<string, Type>();

        private static readonly ConcurrentDictionary<string, PropertyInfo[]> PropertyInfos =
            new ConcurrentDictionary<string, PropertyInfo[]>();

        private readonly ILogger _logger;

        public LookupQueryInterceptor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType());
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
            return;
            var result = invocation.ReturnValue;
            if (result == null || result.GetType().Namespace == "JZTERP.Frameworks.DataAccess.EF") return;

            if (IsSkip(invocation)) return;
            var lookups = GetLookupName(TypeUtils.GetPureType(invocation.MethodInvocationTarget.ReflectedType));

            if (lookups.Any())
            {
                if (result is IQueryable)
                {
                    if ((result as IQueryable).Expression.NodeType == ExpressionType.Call)
                    {
                        var q = (result as IQueryable);
                        var genericType = typeof(List<>).MakeGenericType(q.ElementType);
                        var constructor = genericType.GetDeclaredConstructors().Skip(2).Take(1).FirstOrDefault();
                        result = (constructor?.Invoke(new object[] { q }) as IList)?.AsQueryable();
                    }
                    var r = result as IEnumerable;
                    foreach (var m in r)
                    {
                        SetLookupValue(m, lookups);
                    }
                    invocation.ReturnValue = r;
                }
                else
                {
                    SetLookupValue(result, lookups);
                    invocation.ReturnValue = result;
                }
            }
        }

        #region private method

        /// <summary>
        ///     获取lookup配置
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Dictionary<PropertyInfo, LookupQueryAttribute> GetLookupName(Type type)
        {
            if (Lookups.ContainsKey(type))
            {
                return Lookups[type];
            }
            lock (LockObj)
            {
                var lookup = new Dictionary<PropertyInfo, LookupQueryAttribute>();
                var properties = GetProperties(type);

                foreach (var propertyInfo in properties)
                {
                    var attr = propertyInfo.GetCustomAttribute(typeof(LookupQueryAttribute), true);
                    if (attr != null)
                    {
                        lookup.Add(propertyInfo, attr as LookupQueryAttribute);
                    }
                }
                Lookups.GetOrAdd(type, lookup);
                return Lookups[type];
            }
        }

        private void SetLookupValue(object m, Dictionary<PropertyInfo, LookupQueryAttribute> lookups)
        {
            if (m == null) return;
            foreach (var lookup in lookups)
            {
                var attr = lookup.Value;
                var serviceType = GetServiceType(attr.ServiceType);
                var method = GetMethod(attr);

                var parameter = new ArrayList();

                if (!string.IsNullOrEmpty(attr.PropertyName))
                {
                    var propertyNames = attr.PropertyName.Split(',');

                    foreach (var name in propertyNames)
                    {
                        var property =
                            GetProperties(TypeUtils.GetPureType(m.GetType())).FirstOrDefault(c => c.Name == name);

                        if (property == null)
                            continue;

                        var value = property.GetValue(m);
                        parameter.Add(value?.ToString() ?? "");
                    }

                    //var properties =
                    //    GetProperties(TypeUtils.GetPureType(m.GetType()))
                    //        .Where(c => propertyNames.Any(t => t == c.Name));

                    //foreach (var p in properties)
                    //{
                    //    parameter.Add(p.GetValue(m));
                    //}
                }

                if (!string.IsNullOrEmpty(attr.ConstValue))
                {
                    parameter.AddRange(attr.ConstValue.Split(','));
                }
                //TODO 可能有性能问题
                using (var scope = ObjectContainer.BeginLifetimeScope())
                {
                    var service = scope.Resolve(serviceType);
                    Ensure.NotNull(service, "LookupQueryInterceptor - service:" + service);
                    var val = method.Invoke(service, parameter.ToArray());
                    if (lookup.Key.PropertyType.IsInstanceOfType(val))
                    {
                        lookup.Key.SetValue(m, val);
                    }
                }
            }
        }

        /// <summary>
        ///     获取对象Type
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        private PropertyInfo[] GetProperties(Type classType)
        {
            var key = classType.ToString();
            if (PropertyInfos.ContainsKey(key))
                return PropertyInfos[key];

            lock (LockType)
            {
                var properties = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                Ensure.NotNull(properties,
                    "LookupQueryInterceptor - ClassType:" + properties);

                PropertyInfos.GetOrAdd(key, properties);

                return PropertyInfos[key];
            }
        }

        /// <summary>
        ///     获取method
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private MethodInfo GetMethod(LookupQueryAttribute attr)
        {
            var key = string.Format("{0}-{1}", attr.ServiceType, attr.Method);

            if (MethodInfos.ContainsKey(key))
                return MethodInfos[key];

            lock (LockMethod)
            {
                var serviceType = GetServiceType(attr.ServiceType);

                var method = serviceType.GetMethod(attr.Method);
                Ensure.NotNull(method,
                    string.Format("LookupQueryInterceptor - serviceType:{0} method:{1}", serviceType, method));

                MethodInfos.GetOrAdd(key, method);

                return MethodInfos[key];
            }
        }

        /// <summary>
        ///     获取服务Type
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private Type GetServiceType(string service)
        {
            if (ServiceInfos.ContainsKey(service))
                return ServiceInfos[service];

            lock (LockService)
            {
                var serviceType = Type.GetType(service);
                Ensure.NotNull(serviceType, "LookupQueryInterceptor - serviceType:" + serviceType);

                ServiceInfos.GetOrAdd(service, serviceType);

                return ServiceInfos[service];
            }
        }

        private bool IsSkip(IInvocation invocation)
        {
            if (invocation.Arguments != null &&
                invocation.Arguments.Any())
            {
                var arg = invocation.Arguments.Last();
                if (arg != null && arg.ToString() == "False")
                    return true;
            }
            return false;
        }

        #endregion
    }
}
