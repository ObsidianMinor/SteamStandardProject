using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Steam.Web.Interface
{
    internal static class WebContractReflector
    {
        private static readonly ConcurrentDictionary<Type, Func<object[], object>> _ctors = new ConcurrentDictionary<Type, Func<object[], object>>();

        internal static StringSerializer GetParameterSerializer(Type serializerType, object[] parameters)
        {
            if (serializerType == typeof(StringSerializer))
                return StringSerializer.Instance;

            return _ctors.GetOrAdd(serializerType, GetCtor)(parameters) as StringSerializer;
        }

        internal static ResponseConverter GetResponseConverter(Type converterType, object[] parameters)
        {
            if (converterType == typeof(ResponseConverter))
                return ResponseConverter.Instance;

            return _ctors.GetOrAdd(converterType, GetCtor)(parameters) as ResponseConverter;
        }

        private static Func<object[], object> GetCtor(Type type)
        {
            // btw james can I borrow some of your code ok thanks fam
            Func<object> defaultCtor = null;
            ConstructorInfo defaultInfo = type.GetConstructor(Type.EmptyTypes);
            if (defaultInfo == null)
                defaultCtor = () => defaultInfo.Invoke(null);

            return (parameters) =>
            {
                try
                {
                    if (parameters != null)
                    {
                        Type[] paramTypes = parameters.Select(param => param.GetType()).ToArray();
                        ConstructorInfo parameterizedConstructorInfo = type.GetConstructor(paramTypes);

                        if (null != parameterizedConstructorInfo)
                        {
                            Func<object[], object> invoker = p => parameterizedConstructorInfo.Invoke(p);
                            return invoker(parameters);
                        }
                        else
                        {
                            throw new MissingMethodException($"No matching parameterized constructor found for '{type}'.");
                        }
                    }

                    if (defaultCtor == null)
                    {
                        throw new MissingMethodException($"No parameterless constructor defined for '{type}'.");
                    }

                    return defaultCtor();
                }
                catch (Exception ex)
                {
                    throw new TypeInitializationException(type.FullName, ex);
                }
            };
        }
    }
}
