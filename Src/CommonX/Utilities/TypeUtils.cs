using CommonX.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace CommonX.Utilities
{
    /// <summary>A class provides utility methods.
    /// </summary>
    public static class TypeUtils
    {
        /// <summary>
        ///     Check whether a type is a component type.
        /// </summary>
        public static bool IsComponent(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.GetCustomAttributes(typeof(ComponentAttribute), true).Any();
        }
        /// <summary>Convert the given object to a given strong type.
        /// </summary>
        public static T ConvertType<T>(object value)
        {
            if (value == null)
            {
                return default(T);
            }

            var typeConverter1 = TypeDescriptor.GetConverter(typeof(T));
            if (typeConverter1.CanConvertFrom(value.GetType()))
            {
                return (T)typeConverter1.ConvertFrom(value);
            }

            var typeConverter2 = TypeDescriptor.GetConverter(value.GetType());
            if (typeConverter2.CanConvertTo(typeof(T)))
            {
                return (T)typeConverter2.ConvertTo(value, typeof(T));
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
        public static Type GetPureType(Type type)
        {
            if (type.HasElementType || type.IsArray)
            {
                return type.GetElementType();
            }
            if (type.IsGenericType)
            {
                var itemType = type.GetGenericArguments().FirstOrDefault();

                return itemType;
            }

            return type;
        }

        public static Type GetPureType<T>()
        {
            var type = typeof(T);

            if (type.HasElementType || type.IsArray)
            {
                return type.GetElementType();
            }
            if (type.IsGenericType)
            {
                var itemType = type.GetGenericArguments().FirstOrDefault();

                return itemType;
            }

            return type;
        }

        public static string GetTypeFullName(Type type)
        {
            if (type.HasElementType || type.IsArray)
            {
                return GetFormattedName(type.GetElementType().FullName);
            }
            if (type.IsGenericType)
            {
                var itemType = type.GetGenericArguments().FirstOrDefault();

                return GetFormattedName(itemType.FullName);
            }

            return GetFormattedName(type.FullName);
        }


        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetFormattedName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Unformatted  Name");
            }

            return name.Split(',')[0].Trim().Replace('.', '_').Replace('+', '_');
        }


        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type)
        {
#if NET40
            const BindingFlags bindingFlags
                = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            return type.GetConstructors(bindingFlags);
#else
            return type.GetTypeInfo().DeclaredConstructors;
#endif
        }

        public static ConstructorInfo GetDeclaredConstructor(this Type type, params Type[] parameterTypes)
        {
            return type.GetDeclaredConstructors().SingleOrDefault(
                c => !c.IsStatic && c.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        }
    }
}