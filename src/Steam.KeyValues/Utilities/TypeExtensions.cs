using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Steam.KeyValues.Utilities
{
    internal static class TypeExtensions
    {
        private static BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetGetMethod(false);
        }

        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo, bool nonPublic)
        {
            MethodInfo getMethod = propertyInfo.GetMethod;
            if (getMethod != null && (getMethod.IsPublic || nonPublic))
            {
                return getMethod;
            }

            return null;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetSetMethod(false);
        }

        public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo, bool nonPublic)
        {
            MethodInfo setMethod = propertyInfo.SetMethod;
            if (setMethod != null && (setMethod.IsPublic || nonPublic))
            {
                return setMethod;
            }

            return null;
        }

        public static bool IsSubclassOf(this Type type, Type c)
        {
            return type.GetTypeInfo().IsSubclassOf(c);
        }
        
        public static bool IsAssignableFrom(this Type type, Type c)
        {
            return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
        }

        public static bool IsInstanceOfType(this Type type, object o)
        {
            if (o == null)
            {
                return false;
            }

            return type.IsAssignableFrom(o.GetType());
        }

        public static MethodInfo Method(this Delegate d)
        {
            return d.GetMethodInfo();
        }

        public static MemberTypes MemberType(this MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
            {
                return MemberTypes.Property;
            }
            else if (memberInfo is FieldInfo)
            {
                return MemberTypes.Field;
            }
            else if (memberInfo is EventInfo)
            {
                return MemberTypes.Event;
            }
            else if (memberInfo is MethodInfo)
            {
                return MemberTypes.Method;
            }
            else
            {
                return MemberTypes.Other;
            }
        }

        public static bool ContainsGenericParameters(this Type type)
        {
            return type.GetTypeInfo().ContainsGenericParameters;
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static Type BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static Assembly Assembly(this Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        public static bool IsSealed(this Type type)
        {
            return type.GetTypeInfo().IsSealed;
        }
        
        public static PropertyInfo GetProperty(this Type type, string name, BindingFlags bindingFlags, object placeholder1, Type propertyType, IList<Type> indexParameters, object placeholder2)
        {
            IEnumerable<PropertyInfo> propertyInfos = type.GetProperties(bindingFlags);

            return propertyInfos.Where(p =>
            {
                if (name != null && name != p.Name)
                {
                    return false;
                }
                if (propertyType != null && propertyType != p.PropertyType)
                {
                    return false;
                }
                if (indexParameters != null)
                {
                    if (!p.GetIndexParameters().Select(ip => ip.ParameterType).SequenceEqual(indexParameters))
                    {
                        return false;
                    }
                }

                return true;
            }).SingleOrDefault();
        }

        public static IEnumerable<MemberInfo> GetMember(this Type type, string name, MemberTypes memberType, BindingFlags bindingFlags)
        {
            return type.GetMemberInternal(name, memberType, bindingFlags);
        }
        
        public static MethodInfo GetBaseDefinition(this MethodInfo method)
        {
            return method.GetRuntimeBaseDefinition();
        }
        public static bool IsDefined(this Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().CustomAttributes.Any(a => a.AttributeType == attributeType);
        }
        
        public static MethodInfo GetMethod(this Type type, string name)
        {
            return type.GetMethod(name, DefaultFlags);
        }

        public static MethodInfo GetMethod(this Type type, string name, BindingFlags bindingFlags)
        {
            return type.GetTypeInfo().GetDeclaredMethod(name);
        }

        public static MethodInfo GetMethod(this Type type, IList<Type> parameterTypes)
        {
            return type.GetMethod(null, parameterTypes);
        }

        public static MethodInfo GetMethod(this Type type, string name, IList<Type> parameterTypes)
        {
            return type.GetMethod(name, DefaultFlags, null, parameterTypes, null);
        }

        public static MethodInfo GetMethod(this Type type, string name, BindingFlags bindingFlags, object placeHolder1, IList<Type> parameterTypes, object placeHolder2)
        {
            return MethodBinder.SelectMethod(type.GetTypeInfo().DeclaredMethods.Where(m => (name == null || m.Name == name) && TestAccessibility(m, bindingFlags)), parameterTypes);
        }

        public static IEnumerable<ConstructorInfo> GetConstructors(this Type type)
        {
            return type.GetConstructors(DefaultFlags);
        }

        public static IEnumerable<ConstructorInfo> GetConstructors(this Type type, BindingFlags bindingFlags)
        {
            return type.GetTypeInfo().DeclaredConstructors.Where(c => TestAccessibility(c, bindingFlags));
        }
        
        public static ConstructorInfo GetConstructor(this Type type, IList<Type> parameterTypes)
        {
            return type.GetConstructor(DefaultFlags, null, parameterTypes, null);
        }

        public static ConstructorInfo GetConstructor(this Type type, BindingFlags bindingFlags, object placeholder1, IList<Type> parameterTypes, object placeholder2)
        {
            return MethodBinder.SelectMethod(type.GetConstructors(bindingFlags), parameterTypes);
        }
        
        public static MemberInfo[] GetMember(this Type type, string member)
        {
            return type.GetMemberInternal(member, null, DefaultFlags);
        }

        public static MemberInfo[] GetMember(this Type type, string member, BindingFlags bindingFlags)
        {
            return type.GetMemberInternal(member, null, bindingFlags);
        }

        public static MemberInfo[] GetMemberInternal(this Type type, string member, MemberTypes? memberType, BindingFlags bindingFlags)
        {
            return type.GetTypeInfo().GetMembersRecursive().Where(m =>
                m.Name == member &&
                // test type before accessibility - accessibility doesn't support some types
                (memberType == null || m.MemberType() == memberType) &&
                TestAccessibility(m, bindingFlags)).ToArray();
        }

        public static MemberInfo GetField(this Type type, string member)
        {
            return type.GetField(member, DefaultFlags);
        }

        public static MemberInfo GetField(this Type type, string member, BindingFlags bindingFlags)
        {
            return type.GetTypeInfo().GetDeclaredField(member);
        }

        public static IEnumerable<PropertyInfo> GetProperties(this Type type, BindingFlags bindingFlags)
        {
            IList<PropertyInfo> properties = (bindingFlags.HasFlag(BindingFlags.DeclaredOnly))
                ? type.GetTypeInfo().DeclaredProperties.ToList()
                : type.GetTypeInfo().GetPropertiesRecursive();

            return properties.Where(p => TestAccessibility(p, bindingFlags));
        }

        private static IList<MemberInfo> GetMembersRecursive(this TypeInfo type)
        {
            TypeInfo t = type;
            IList<MemberInfo> members = new List<MemberInfo>();
            while (t != null)
            {
                foreach (MemberInfo member in t.DeclaredMembers)
                {
                    if (!members.Any(p => p.Name == member.Name))
                    {
                        members.Add(member);
                    }
                }
                t = (t.BaseType != null) ? t.BaseType.GetTypeInfo() : null;
            }

            return members;
        }

        private static IList<PropertyInfo> GetPropertiesRecursive(this TypeInfo type)
        {
            TypeInfo t = type;
            IList<PropertyInfo> properties = new List<PropertyInfo>();
            while (t != null)
            {
                foreach (PropertyInfo member in t.DeclaredProperties)
                {
                    if (!properties.Any(p => p.Name == member.Name))
                    {
                        properties.Add(member);
                    }
                }
                t = (t.BaseType != null) ? t.BaseType.GetTypeInfo() : null;
            }

            return properties;
        }

        private static IList<FieldInfo> GetFieldsRecursive(this TypeInfo type)
        {
            TypeInfo t = type;
            IList<FieldInfo> fields = new List<FieldInfo>();
            while (t != null)
            {
                foreach (FieldInfo member in t.DeclaredFields)
                {
                    if (!fields.Any(p => p.Name == member.Name))
                    {
                        fields.Add(member);
                    }
                }
                t = (t.BaseType != null) ? t.BaseType.GetTypeInfo() : null;
            }

            return fields;
        }

        public static IEnumerable<MethodInfo> GetMethods(this Type type, BindingFlags bindingFlags)
        {
            return type.GetTypeInfo().DeclaredMethods;
        }

        public static PropertyInfo GetProperty(this Type type, string name)
        {
            return type.GetProperty(name, DefaultFlags);
        }

        public static PropertyInfo GetProperty(this Type type, string name, BindingFlags bindingFlags)
        {
            return type.GetTypeInfo().GetDeclaredProperty(name);
        }

        public static IEnumerable<FieldInfo> GetFields(this Type type)
        {
            return type.GetFields(DefaultFlags);
        }

        public static IEnumerable<FieldInfo> GetFields(this Type type, BindingFlags bindingFlags)
        {
            IList<FieldInfo> fields = (bindingFlags.HasFlag(BindingFlags.DeclaredOnly))
                ? type.GetTypeInfo().DeclaredFields.ToList()
                : type.GetTypeInfo().GetFieldsRecursive();

            return fields.Where(f => TestAccessibility(f, bindingFlags)).ToList();
        }

        private static bool TestAccessibility(PropertyInfo member, BindingFlags bindingFlags)
        {
            if (member.GetMethod != null && TestAccessibility(member.GetMethod, bindingFlags))
            {
                return true;
            }

            if (member.SetMethod != null && TestAccessibility(member.SetMethod, bindingFlags))
            {
                return true;
            }

            return false;
        }

        private static bool TestAccessibility(MemberInfo member, BindingFlags bindingFlags)
        {
            if (member is FieldInfo)
            {
                return TestAccessibility((FieldInfo) member, bindingFlags);
            }
            else if (member is MethodBase)
            {
                return TestAccessibility((MethodBase) member, bindingFlags);
            }
            else if (member is PropertyInfo)
            {
                return TestAccessibility((PropertyInfo) member, bindingFlags);
            }

            throw new Exception("Unexpected member type.");
        }

        private static bool TestAccessibility(FieldInfo member, BindingFlags bindingFlags)
        {
            bool visibility = (member.IsPublic && bindingFlags.HasFlag(BindingFlags.Public)) ||
                              (!member.IsPublic && bindingFlags.HasFlag(BindingFlags.NonPublic));

            bool instance = (member.IsStatic && bindingFlags.HasFlag(BindingFlags.Static)) ||
                            (!member.IsStatic && bindingFlags.HasFlag(BindingFlags.Instance));

            return visibility && instance;
        }

        private static bool TestAccessibility(MethodBase member, BindingFlags bindingFlags)
        {
            bool visibility = (member.IsPublic && bindingFlags.HasFlag(BindingFlags.Public)) ||
                              (!member.IsPublic && bindingFlags.HasFlag(BindingFlags.NonPublic));

            bool instance = (member.IsStatic && bindingFlags.HasFlag(BindingFlags.Static)) ||
                            (!member.IsStatic && bindingFlags.HasFlag(BindingFlags.Instance));

            return visibility && instance;
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static IEnumerable<Type> GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        public static IEnumerable<MethodInfo> GetMethods(this Type type)
        {
            return type.GetTypeInfo().DeclaredMethods;
        }

        public static bool IsAbstract(this Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        public static bool IsVisible(this Type type)
        {
            return type.GetTypeInfo().IsVisible;
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        public static bool AssignableToTypeName(this Type type, string fullTypeName, bool searchInterfaces, out Type match)
        {
            Type current = type;

            while (current != null)
            {
                if (string.Equals(current.FullName, fullTypeName, StringComparison.Ordinal))
                {
                    match = current;
                    return true;
                }

                current = current.BaseType();
            }

            if (searchInterfaces)
            {
                foreach (Type i in type.GetInterfaces())
                {
                    if (string.Equals(i.Name, fullTypeName, StringComparison.Ordinal))
                    {
                        match = type;
                        return true;
                    }
                }
            }

            match = null;
            return false;
        }

        public static bool AssignableToTypeName(this Type type, string fullTypeName, bool searchInterfaces)
        {
            Type match;
            return type.AssignableToTypeName(fullTypeName, searchInterfaces, out match);
        }

        public static bool ImplementInterface(this Type type, Type interfaceType)
        {
            for (Type currentType = type; currentType != null; currentType = currentType.BaseType())
            {
                IEnumerable<Type> interfaces = currentType.GetInterfaces();
                foreach (Type i in interfaces)
                {
                    if (i == interfaceType || (i != null && i.ImplementInterface(interfaceType)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
