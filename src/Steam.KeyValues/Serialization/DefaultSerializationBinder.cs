using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Steam.KeyValues.Serialization
{
    /// <summary>
    /// The default serialization binder used when resolving and loading classes from type names.
    /// </summary>
#pragma warning disable 618
    public class DefaultSerializationBinder : SerializationBinder, ISerializationBinder
    #pragma warning restore 618
    {
        internal static readonly DefaultSerializationBinder Instance = new DefaultSerializationBinder();

        private readonly ThreadSafeStore<TypeNameKey, Type> _typeCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSerializationBinder"/> class.
        /// </summary>
        public DefaultSerializationBinder()
        {
            _typeCache = new ThreadSafeStore<TypeNameKey, Type>(GetTypeFromTypeNameKey);
        }

        private Type GetTypeFromTypeNameKey(TypeNameKey typeNameKey)
        {
            string assemblyName = typeNameKey.AssemblyName;
            string typeName = typeNameKey.TypeName;

            if (assemblyName != null)
            {
                // look, I don't like using obsolete methods as much as you do but this is the only way
                // Assembly.Load won't check the GAC for a partial name
                Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));

                if (assembly == null)
                {
                    throw new KeyValueSerializationException("Could not load assembly '{0}'.".FormatWith(CultureInfo.InvariantCulture, assemblyName));
                }

                Type type = assembly.GetType(typeName);
                if (type == null)
                {
                    // if generic type, try manually parsing the type arguments for the case of dynamically loaded assemblies
                    // example generic typeName format: System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
                    if (typeName.IndexOf('`') >= 0)
                    {
                        try
                        {
                            type = GetGenericTypeFromTypeName(typeName, assembly);
                        }
                        catch (Exception ex)
                        {
                            throw new KeyValueSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(CultureInfo.InvariantCulture, typeName, assembly.FullName), ex);
                        }
                    }

                    if (type == null)
                    {
                        throw new KeyValueSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(CultureInfo.InvariantCulture, typeName, assembly.FullName));
                    }
                }

                return type;
            }
            else
            {
                return Type.GetType(typeName);
            }
        }

        private Type GetGenericTypeFromTypeName(string typeName, Assembly assembly)
        {
            Type type = null;
            int openBracketIndex = typeName.IndexOf('[');
            if (openBracketIndex >= 0)
            {
                string genericTypeDefName = typeName.Substring(0, openBracketIndex);
                Type genericTypeDef = assembly.GetType(genericTypeDefName);
                if (genericTypeDef != null)
                {
                    List<Type> genericTypeArguments = new List<Type>();
                    int scope = 0;
                    int typeArgStartIndex = 0;
                    int endIndex = typeName.Length - 1;
                    for (int i = openBracketIndex + 1; i < endIndex; ++i)
                    {
                        char current = typeName[i];
                        switch (current)
                        {
                            case '[':
                                if (scope == 0)
                                {
                                    typeArgStartIndex = i + 1;
                                }
                                ++scope;
                                break;
                            case ']':
                                --scope;
                                if (scope == 0)
                                {
                                    string typeArgAssemblyQualifiedName = typeName.Substring(typeArgStartIndex, i - typeArgStartIndex);

                                    TypeNameKey typeNameKey = ReflectionUtils.SplitFullyQualifiedTypeName(typeArgAssemblyQualifiedName);
                                    genericTypeArguments.Add(GetTypeByName(typeNameKey));
                                }
                                break;
                        }
                    }

                    type = genericTypeDef.MakeGenericType(genericTypeArguments.ToArray());
                }
            }

            return type;
        }

        private Type GetTypeByName(TypeNameKey typeNameKey)
        {
            return _typeCache.Get(typeNameKey);
        }

        /// <summary>
        /// When overridden in a derived class, controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="assemblyName">Specifies the <see cref="Assembly"/> name of the serialized object.</param>
        /// <param name="typeName">Specifies the <see cref="System.Type"/> name of the serialized object.</param>
        /// <returns>
        /// The type of the object the formatter creates a new instance of.
        /// </returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            return GetTypeByName(new TypeNameKey(assemblyName, typeName));
        }

        /// <summary>
        /// When overridden in a derived class, controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
        /// <param name="assemblyName">Specifies the <see cref="Assembly"/> name of the serialized object.</param>
        /// <param name="typeName">Specifies the <see cref="System.Type"/> name of the serialized object.</param>
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = serializedType.GetTypeInfo().Assembly.FullName;
            typeName = serializedType.FullName;
        }
    }
}
