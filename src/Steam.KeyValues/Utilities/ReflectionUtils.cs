﻿using Steam.KeyValues.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Steam.KeyValues.Utilities
{
    internal enum MemberTypes
    {
        Property = 0,
        Field = 1,
        Event = 2,
        Method = 3,
        Other = 4
    }

    [Flags]
    internal enum BindingFlags
    {
        Default = 0,
        IgnoreCase = 1,
        DeclaredOnly = 2,
        Instance = 4,
        Static = 8,
        Public = 16,
        NonPublic = 32,
        FlattenHierarchy = 64,
        InvokeMethod = 256,
        CreateInstance = 512,
        GetField = 1024,
        SetField = 2048,
        GetProperty = 4096,
        SetProperty = 8192,
        PutDispProperty = 16384,
        ExactBinding = 65536,
        PutRefDispProperty = 32768,
        SuppressChangeType = 131072,
        OptionalParamBinding = 262144,
        IgnoreReturn = 16777216
    }

    internal static class ReflectionUtils
    {
        public static readonly Type[] EmptyTypes;

        static ReflectionUtils()
        {
            EmptyTypes = CollectionUtils.ArrayEmpty<Type>();
        }

        public static bool IsVirtual(this PropertyInfo propertyInfo)
        {
            ValidationUtils.ArgumentNotNull(propertyInfo, nameof(propertyInfo));

            MethodInfo m = propertyInfo.GetGetMethod(true);
            if (m != null && m.IsVirtual)
            {
                return true;
            }

            m = propertyInfo.GetSetMethod(true);
            if (m != null && m.IsVirtual)
            {
                return true;
            }

            return false;
        }

        public static MethodInfo GetBaseDefinition(this PropertyInfo propertyInfo)
        {
            ValidationUtils.ArgumentNotNull(propertyInfo, nameof(propertyInfo));

            MethodInfo m = propertyInfo.GetGetMethod(true);
            if (m != null)
            {
                return m.GetBaseDefinition();
            }

            return propertyInfo.GetSetMethod(true)?.GetBaseDefinition();
        }

        public static bool IsPublic(PropertyInfo property)
        {
            if (property.GetGetMethod() != null && property.GetGetMethod().IsPublic)
            {
                return true;
            }
            if (property.GetSetMethod() != null && property.GetSetMethod().IsPublic)
            {
                return true;
            }

            return false;
        }

        public static Type GetObjectType(object v)
        {
            return v?.GetType();
        }

        public static string GetTypeName(Type t, TypeNameAssemblyFormatHandling assemblyFormat, ISerializationBinder binder)
        {
            string fullyQualifiedTypeName = GetFullyQualifiedTypeName(t, binder);

            switch (assemblyFormat)
            {
                case TypeNameAssemblyFormatHandling.Simple:
                    return RemoveAssemblyDetails(fullyQualifiedTypeName);
                case TypeNameAssemblyFormatHandling.Full:
                    return fullyQualifiedTypeName;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetFullyQualifiedTypeName(Type t, ISerializationBinder binder)
        {
            if (binder != null)
            {
                binder.BindToName(t, out string assemblyName, out string typeName);
                return typeName + (assemblyName == null ? "" : ", " + assemblyName);
            }

            return t.AssemblyQualifiedName;
        }

        private static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
        {
            StringBuilder builder = new StringBuilder();

            // loop through the type name and filter out qualified assembly details from nested type names
            bool writingAssemblyName = false;
            bool skippingAssemblyDetails = false;
            for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
            {
                char current = fullyQualifiedTypeName[i];
                switch (current)
                {
                    case '[':
                        writingAssemblyName = false;
                        skippingAssemblyDetails = false;
                        builder.Append(current);
                        break;
                    case ']':
                        writingAssemblyName = false;
                        skippingAssemblyDetails = false;
                        builder.Append(current);
                        break;
                    case ',':
                        if (!writingAssemblyName)
                        {
                            writingAssemblyName = true;
                            builder.Append(current);
                        }
                        else
                        {
                            skippingAssemblyDetails = true;
                        }
                        break;
                    default:
                        if (!skippingAssemblyDetails)
                        {
                            builder.Append(current);
                        }
                        break;
                }
            }

            return builder.ToString();
        }

        public static bool HasDefaultConstructor(Type t, bool nonPublic)
        {
            ValidationUtils.ArgumentNotNull(t, nameof(t));

            if (t.IsValueType())
            {
                return true;
            }

            return (GetDefaultConstructor(t, nonPublic) != null);
        }

        public static ConstructorInfo GetDefaultConstructor(Type t)
        {
            return GetDefaultConstructor(t, false);
        }

        public static ConstructorInfo GetDefaultConstructor(Type t, bool nonPublic)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            if (nonPublic)
            {
                bindingFlags = bindingFlags | BindingFlags.NonPublic;
            }

            return t.GetConstructors(bindingFlags).SingleOrDefault(c => !c.GetParameters().Any());
        }

        public static bool IsNullable(Type t)
        {
            ValidationUtils.ArgumentNotNull(t, nameof(t));

            if (t.IsValueType())
            {
                return IsNullableType(t);
            }

            return true;
        }

        public static bool IsNullableType(Type t)
        {
            ValidationUtils.ArgumentNotNull(t, nameof(t));

            return (t.IsGenericType() && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static Type EnsureNotNullableType(Type t)
        {
            return (IsNullableType(t))
                ? Nullable.GetUnderlyingType(t)
                : t;
        }

        public static bool IsGenericDefinition(Type type, Type genericInterfaceDefinition)
        {
            if (!type.IsGenericType())
            {
                return false;
            }

            Type t = type.GetGenericTypeDefinition();
            return (t == genericInterfaceDefinition);
        }

        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition)
        {
            return ImplementsGenericDefinition(type, genericInterfaceDefinition, out Type implementingType);
        }

        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, out Type implementingType)
        {
            ValidationUtils.ArgumentNotNull(type, nameof(type));
            ValidationUtils.ArgumentNotNull(genericInterfaceDefinition, nameof(genericInterfaceDefinition));

            if (!genericInterfaceDefinition.IsInterface() || !genericInterfaceDefinition.IsGenericTypeDefinition())
            {
                throw new ArgumentNullException("'{0}' is not a generic interface definition.".FormatWith(CultureInfo.InvariantCulture, genericInterfaceDefinition));
            }

            if (type.IsInterface())
            {
                if (type.IsGenericType())
                {
                    Type interfaceDefinition = type.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = type;
                        return true;
                    }
                }
            }

            foreach (Type i in type.GetInterfaces())
            {
                if (i.IsGenericType())
                {
                    Type interfaceDefinition = i.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = i;
                        return true;
                    }
                }
            }

            implementingType = null;
            return false;
        }

        public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition)
        {
            return InheritsGenericDefinition(type, genericClassDefinition, out Type implementingType);
        }

        public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition, out Type implementingType)
        {
            ValidationUtils.ArgumentNotNull(type, nameof(type));
            ValidationUtils.ArgumentNotNull(genericClassDefinition, nameof(genericClassDefinition));

            if (!genericClassDefinition.IsClass() || !genericClassDefinition.IsGenericTypeDefinition())
            {
                throw new ArgumentNullException("'{0}' is not a generic class definition.".FormatWith(CultureInfo.InvariantCulture, genericClassDefinition));
            }

            return InheritsGenericDefinitionInternal(type, genericClassDefinition, out implementingType);
        }

        private static bool InheritsGenericDefinitionInternal(Type currentType, Type genericClassDefinition, out Type implementingType)
        {
            if (currentType.IsGenericType())
            {
                Type currentGenericClassDefinition = currentType.GetGenericTypeDefinition();

                if (genericClassDefinition == currentGenericClassDefinition)
                {
                    implementingType = currentType;
                    return true;
                }
            }

            if (currentType.BaseType() == null)
            {
                implementingType = null;
                return false;
            }

            return InheritsGenericDefinitionInternal(currentType.BaseType(), genericClassDefinition, out implementingType);
        }

        /// <summary>
        /// Gets the type of the typed collection's items.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The type of the typed collection's items.</returns>
        public static Type GetCollectionItemType(Type type)
        {
            ValidationUtils.ArgumentNotNull(type, nameof(type));

            if (type.IsArray)
            {
                return type.GetElementType();
            }
            if (ImplementsGenericDefinition(type, typeof(IEnumerable<>), out Type genericListType))
            {
                if (genericListType.IsGenericTypeDefinition())
                {
                    throw new Exception("Type {0} is not a collection.".FormatWith(CultureInfo.InvariantCulture, type));
                }

                return genericListType.GetGenericArguments()[0];
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return null;
            }

            throw new Exception("Type {0} is not a collection.".FormatWith(CultureInfo.InvariantCulture, type));
        }

        public static void GetDictionaryKeyValueTypes(Type dictionaryType, out Type keyType, out Type valueType)
        {
            ValidationUtils.ArgumentNotNull(dictionaryType, nameof(dictionaryType));

            if (ImplementsGenericDefinition(dictionaryType, typeof(IDictionary<,>), out Type genericDictionaryType))
            {
                if (genericDictionaryType.IsGenericTypeDefinition())
                {
                    throw new Exception("Type {0} is not a dictionary.".FormatWith(CultureInfo.InvariantCulture, dictionaryType));
                }

                Type[] dictionaryGenericArguments = genericDictionaryType.GetGenericArguments();

                keyType = dictionaryGenericArguments[0];
                valueType = dictionaryGenericArguments[1];
                return;
            }
            if (typeof(IDictionary).IsAssignableFrom(dictionaryType))
            {
                keyType = null;
                valueType = null;
                return;
            }

            throw new Exception("Type {0} is not a dictionary.".FormatWith(CultureInfo.InvariantCulture, dictionaryType));
        }

        /// <summary>
        /// Gets the member's underlying type.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The underlying type of the member.</returns>
        public static Type GetMemberUnderlyingType(MemberInfo member)
        {
            ValidationUtils.ArgumentNotNull(member, nameof(member));

            switch (member.MemberType())
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                default:
                    throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo, EventInfo or MethodInfo", nameof(member));
            }
        }

        /// <summary>
        /// Determines whether the member is an indexed property.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>
        /// 	<c>true</c> if the member is an indexed property; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIndexedProperty(MemberInfo member)
        {
            ValidationUtils.ArgumentNotNull(member, nameof(member));
            
            if (member is PropertyInfo propertyInfo)
            {
                return IsIndexedProperty(propertyInfo);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the property is an indexed property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>
        /// 	<c>true</c> if the property is an indexed property; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIndexedProperty(PropertyInfo property)
        {
            ValidationUtils.ArgumentNotNull(property, nameof(property));

            return (property.GetIndexParameters().Length > 0);
        }

        /// <summary>
        /// Gets the member's value on the object.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="target">The target object.</param>
        /// <returns>The member's value on the object.</returns>
        public static object GetMemberValue(MemberInfo member, object target)
        {
            ValidationUtils.ArgumentNotNull(member, nameof(member));
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            switch (member.MemberType())
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).GetValue(target);
                case MemberTypes.Property:
                    try
                    {
                        return ((PropertyInfo)member).GetValue(target, null);
                    }
                    catch (TargetParameterCountException e)
                    {
                        throw new ArgumentException("MemberInfo '{0}' has index parameters".FormatWith(CultureInfo.InvariantCulture, member.Name), e);
                    }
                default:
                    throw new ArgumentException("MemberInfo '{0}' is not of type FieldInfo or PropertyInfo".FormatWith(CultureInfo.InvariantCulture, member.Name), nameof(member));
            }
        }

        /// <summary>
        /// Sets the member's value on the target object.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        public static void SetMemberValue(MemberInfo member, object target, object value)
        {
            ValidationUtils.ArgumentNotNull(member, nameof(member));
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            switch (member.MemberType())
            {
                case MemberTypes.Field:
                    ((FieldInfo)member).SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)member).SetValue(target, value, null);
                    break;
                default:
                    throw new ArgumentException("MemberInfo '{0}' must be of type FieldInfo or PropertyInfo".FormatWith(CultureInfo.InvariantCulture, member.Name), nameof(member));
            }
        }

        /// <summary>
        /// Determines whether the specified MemberInfo can be read.
        /// </summary>
        /// <param name="member">The MemberInfo to determine whether can be read.</param>
        /// /// <param name="nonPublic">if set to <c>true</c> then allow the member to be gotten non-publicly.</param>
        /// <returns>
        /// 	<c>true</c> if the specified MemberInfo can be read; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanReadMemberValue(MemberInfo member, bool nonPublic)
        {
            switch (member.MemberType())
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = (FieldInfo)member;

                    if (nonPublic)
                    {
                        return true;
                    }
                    else if (fieldInfo.IsPublic)
                    {
                        return true;
                    }
                    return false;
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)member;

                    if (!propertyInfo.CanRead)
                    {
                        return false;
                    }
                    if (nonPublic)
                    {
                        return true;
                    }
                    return (propertyInfo.GetGetMethod(nonPublic) != null);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified MemberInfo can be set.
        /// </summary>
        /// <param name="member">The MemberInfo to determine whether can be set.</param>
        /// <param name="nonPublic">if set to <c>true</c> then allow the member to be set non-publicly.</param>
        /// <param name="canSetReadOnly">if set to <c>true</c> then allow the member to be set if read-only.</param>
        /// <returns>
        /// 	<c>true</c> if the specified MemberInfo can be set; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanSetMemberValue(MemberInfo member, bool nonPublic, bool canSetReadOnly)
        {
            switch (member.MemberType())
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = (FieldInfo)member;

                    if (fieldInfo.IsLiteral)
                    {
                        return false;
                    }
                    if (fieldInfo.IsInitOnly && !canSetReadOnly)
                    {
                        return false;
                    }
                    if (nonPublic)
                    {
                        return true;
                    }
                    if (fieldInfo.IsPublic)
                    {
                        return true;
                    }
                    return false;
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)member;

                    if (!propertyInfo.CanWrite)
                    {
                        return false;
                    }
                    if (nonPublic)
                    {
                        return true;
                    }
                    return (propertyInfo.GetSetMethod(nonPublic) != null);
                default:
                    return false;
            }
        }

        public static List<MemberInfo> GetFieldsAndProperties(Type type, BindingFlags bindingAttr)
        {
            List<MemberInfo> targetMembers = new List<MemberInfo>();

            targetMembers.AddRange(GetFields(type, bindingAttr));
            targetMembers.AddRange(GetProperties(type, bindingAttr));

            // for some reason .NET returns multiple members when overriding a generic member on a base class
            // http://social.msdn.microsoft.com/Forums/en-US/b5abbfee-e292-4a64-8907-4e3f0fb90cd9/reflection-overriden-abstract-generic-properties?forum=netfxbcl
            // filter members to only return the override on the topmost class
            // update: I think this is fixed in .NET 3.5 SP1 - leave this in for now...
            List<MemberInfo> distinctMembers = new List<MemberInfo>(targetMembers.Count);

            foreach (IGrouping<string, MemberInfo> groupedMember in targetMembers.GroupBy(m => m.Name))
            {
                int count = groupedMember.Count();

                if (count == 1)
                {
                    distinctMembers.Add(groupedMember.First());
                }
                else
                {
                    IList<MemberInfo> resolvedMembers = new List<MemberInfo>();
                    foreach (MemberInfo memberInfo in groupedMember)
                    {
                        // this is a bit hacky
                        // if the hiding property is hiding a base property and it is virtual
                        // then this ensures the derived property gets used
                        if (resolvedMembers.Count == 0)
                        {
                            resolvedMembers.Add(memberInfo);
                        }
                        else if (!IsOverridenGenericMember(memberInfo, bindingAttr) || memberInfo.Name == "Item")
                        {
                            resolvedMembers.Add(memberInfo);
                        }
                    }

                    distinctMembers.AddRange(resolvedMembers);
                }
            }

            return distinctMembers;
        }

        private static bool IsOverridenGenericMember(MemberInfo memberInfo, BindingFlags bindingAttr)
        {
            if (memberInfo.MemberType() != MemberTypes.Property)
            {
                return false;
            }

            PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
            if (!IsVirtual(propertyInfo))
            {
                return false;
            }

            Type declaringType = propertyInfo.DeclaringType;
            if (!declaringType.IsGenericType())
            {
                return false;
            }
            Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
            if (genericTypeDefinition == null)
            {
                return false;
            }
            MemberInfo[] members = genericTypeDefinition.GetMember(propertyInfo.Name, bindingAttr);
            if (members.Length == 0)
            {
                return false;
            }
            Type memberUnderlyingType = GetMemberUnderlyingType(members[0]);
            if (!memberUnderlyingType.IsGenericParameter)
            {
                return false;
            }

            return true;
        }

        public static T GetAttribute<T>(object attributeProvider) where T : Attribute
        {
            return GetAttribute<T>(attributeProvider, true);
        }

        public static T GetAttribute<T>(object attributeProvider, bool inherit) where T : Attribute
        {
            T[] attributes = GetAttributes<T>(attributeProvider, inherit);

            return attributes?.FirstOrDefault();
        }

        public static T[] GetAttributes<T>(object attributeProvider, bool inherit) where T : Attribute
        {
            return GetAttributes(attributeProvider, typeof(T), inherit).Cast<T>().ToArray();
        }

        public static object[] GetAttributes(object provider, Type attributeType, bool inherit)
        {
            if (provider is Type t)
            {
                return (attributeType != null)
                    ? t.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray()
                    : t.GetTypeInfo().GetCustomAttributes(inherit).ToArray();
            }

            if (provider is Assembly a)
            {
                return (attributeType != null) ? a.GetCustomAttributes(attributeType).ToArray() : a.GetCustomAttributes().ToArray();
            }

            if (provider is MemberInfo)
            {
                MemberInfo m = (MemberInfo)provider;
                return (attributeType != null) ? m.GetCustomAttributes(attributeType, inherit).ToArray() : m.GetCustomAttributes(inherit).ToArray();
            }

            if (provider is Module)
            {
                Module m = (Module)provider;
                return (attributeType != null) ? m.GetCustomAttributes(attributeType).ToArray() : m.GetCustomAttributes().ToArray();
            }

            if (provider is ParameterInfo p)
            {
                return (attributeType != null) ? p.GetCustomAttributes(attributeType, inherit).ToArray() : p.GetCustomAttributes(inherit).ToArray();
            }

            throw new Exception("Cannot get attributes from '{0}'.".FormatWith(CultureInfo.InvariantCulture, provider));
        }

        public static TypeNameKey SplitFullyQualifiedTypeName(string fullyQualifiedTypeName)
        {
            int? assemblyDelimiterIndex = GetAssemblyDelimiterIndex(fullyQualifiedTypeName);

            string typeName;
            string assemblyName;

            if (assemblyDelimiterIndex != null)
            {
                typeName = fullyQualifiedTypeName.Trim(0, assemblyDelimiterIndex.GetValueOrDefault());
                assemblyName = fullyQualifiedTypeName.Trim(assemblyDelimiterIndex.GetValueOrDefault() + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.GetValueOrDefault() - 1);
            }
            else
            {
                typeName = fullyQualifiedTypeName;
                assemblyName = null;
            }

            return new TypeNameKey(assemblyName, typeName);
        }

        private static int? GetAssemblyDelimiterIndex(string fullyQualifiedTypeName)
        {
            // we need to get the first comma following all surrounded in brackets because of generic types
            // e.g. System.Collections.Generic.Dictionary`2[[System.String, mscorlib,Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            int scope = 0;
            for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
            {
                char current = fullyQualifiedTypeName[i];
                switch (current)
                {
                    case '[':
                        scope++;
                        break;
                    case ']':
                        scope--;
                        break;
                    case ',':
                        if (scope == 0)
                        {
                            return i;
                        }
                        break;
                }
            }

            return null;
        }

        public static MemberInfo GetMemberInfoFromType(Type targetType, MemberInfo memberInfo)
        {
            const BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            switch (memberInfo.MemberType())
            {
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)memberInfo;

                    Type[] types = propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray();

                    return targetType.GetProperty(propertyInfo.Name, bindingAttr, null, propertyInfo.PropertyType, types, null);
                default:
                    return targetType.GetMember(memberInfo.Name, memberInfo.MemberType(), bindingAttr).SingleOrDefault();
            }
        }

        public static IEnumerable<FieldInfo> GetFields(Type targetType, BindingFlags bindingAttr)
        {
            ValidationUtils.ArgumentNotNull(targetType, nameof(targetType));

            List<MemberInfo> fieldInfos = new List<MemberInfo>(targetType.GetFields(bindingAttr));

            return fieldInfos.Cast<FieldInfo>();
        }

        public static IEnumerable<PropertyInfo> GetProperties(Type targetType, BindingFlags bindingAttr)
        {
            ValidationUtils.ArgumentNotNull(targetType, nameof(targetType));

            List<PropertyInfo> propertyInfos = new List<PropertyInfo>(targetType.GetProperties(bindingAttr));

            // GetProperties on an interface doesn't return properties from its interfaces
            if (targetType.IsInterface())
            {
                foreach (Type i in targetType.GetInterfaces())
                {
                    propertyInfos.AddRange(i.GetProperties(bindingAttr));
                }
            }

            GetChildPrivateProperties(propertyInfos, targetType, bindingAttr);

            // a base class private getter/setter will be inaccessible unless the property was gotten from the base class
            for (int i = 0; i < propertyInfos.Count; i++)
            {
                PropertyInfo member = propertyInfos[i];
                if (member.DeclaringType != targetType)
                {
                    PropertyInfo declaredMember = (PropertyInfo)GetMemberInfoFromType(member.DeclaringType, member);
                    propertyInfos[i] = declaredMember;
                }
            }

            return propertyInfos;
        }

        public static BindingFlags RemoveFlag(this BindingFlags bindingAttr, BindingFlags flag)
        {
            return ((bindingAttr & flag) == flag)
                ? bindingAttr ^ flag
                : bindingAttr;
        }

        private static void GetChildPrivateProperties(IList<PropertyInfo> initialProperties, Type targetType, BindingFlags bindingAttr)
        {
            // fix weirdness with private PropertyInfos only being returned for the current Type
            // find base type properties and add them to result

            // also find base properties that have been hidden by subtype properties with the same name

            while ((targetType = targetType.BaseType()) != null)
            {
                foreach (PropertyInfo propertyInfo in targetType.GetProperties(bindingAttr))
                {
                    PropertyInfo subTypeProperty = propertyInfo;

                    if (!subTypeProperty.IsVirtual())
                    {
                        if (!IsPublic(subTypeProperty))
                        {
                            // have to test on name rather than reference because instances are different
                            // depending on the type that GetProperties was called on
                            int index = initialProperties.IndexOf(p => p.Name == subTypeProperty.Name);
                            if (index == -1)
                            {
                                initialProperties.Add(subTypeProperty);
                            }
                            else
                            {
                                PropertyInfo childProperty = initialProperties[index];
                                // don't replace public child with private base
                                if (!IsPublic(childProperty))
                                {
                                    // replace nonpublic properties for a child, but gotten from
                                    // the parent with the one from the child
                                    // the property gotten from the child will have access to private getter/setter
                                    initialProperties[index] = subTypeProperty;
                                }
                            }
                        }
                        else
                        {
                            int index = initialProperties.IndexOf(p => p.Name == subTypeProperty.Name
                                                                       && p.DeclaringType == subTypeProperty.DeclaringType);

                            if (index == -1)
                            {
                                initialProperties.Add(subTypeProperty);
                            }
                        }
                    }
                    else
                    {
                        Type subTypePropertyDeclaringType = subTypeProperty.GetBaseDefinition()?.DeclaringType ?? subTypeProperty.DeclaringType;

                        int index = initialProperties.IndexOf(p => p.Name == subTypeProperty.Name
                                                                   && p.IsVirtual()
                                                                   && (p.GetBaseDefinition()?.DeclaringType ?? p.DeclaringType).IsAssignableFrom(subTypePropertyDeclaringType));

                        // don't add a virtual property that has an override
                        if (index == -1)
                        {
                            initialProperties.Add(subTypeProperty);
                        }
                    }
                }
            }
        }

        public static bool IsMethodOverridden(Type currentType, Type methodDeclaringType, string method)
        {
            bool isMethodOverriden = currentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Any(info =>
                    info.Name == method &&
                    // check that the method overrides the original on DynamicObjectProxy
                    info.DeclaringType != methodDeclaringType
                    && info.GetBaseDefinition().DeclaringType == methodDeclaringType
                );

            return isMethodOverriden;
        }

        public static object GetDefaultValue(Type type)
        {
            if (!type.IsValueType())
            {
                return null;
            }

            switch (ConvertUtils.GetTypeCode(type))
            {
                case PrimitiveTypeCode.Char:
                case PrimitiveTypeCode.SByte:
                case PrimitiveTypeCode.Byte:
                case PrimitiveTypeCode.Int16:
                case PrimitiveTypeCode.UInt16:
                case PrimitiveTypeCode.Int32:
                case PrimitiveTypeCode.UInt32:
                    return 0;
                case PrimitiveTypeCode.Int64:
                case PrimitiveTypeCode.UInt64:
                    return 0L;
                case PrimitiveTypeCode.Single:
                    return 0f;
                case PrimitiveTypeCode.Double:
                    return 0.0;
                case PrimitiveTypeCode.Decimal:
                    return 0m;
            }

            if (IsNullable(type))
            {
                return null;
            }

            // possibly use IL initobj for perf here?
            return Activator.CreateInstance(type);
        }
    }

    internal struct TypeNameKey : IEquatable<TypeNameKey>
    {
        internal readonly string AssemblyName;
        internal readonly string TypeName;

        public TypeNameKey(string assemblyName, string typeName)
        {
            AssemblyName = assemblyName;
            TypeName = typeName;
        }

        public override int GetHashCode()
        {
            return (AssemblyName?.GetHashCode() ?? 0) ^ (TypeName?.GetHashCode() ?? 0);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeNameKey))
            {
                return false;
            }

            return Equals((TypeNameKey)obj);
        }

        public bool Equals(TypeNameKey other)
        {
            return (AssemblyName == other.AssemblyName && TypeName == other.TypeName);
        }
    }
}