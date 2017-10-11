﻿using Steam.KeyValues.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Steam.KeyValues.Utilities
{
    internal class FSharpFunction
    {
        private readonly object _instance;
        private readonly MethodCall<object, object> _invoker;

        public FSharpFunction(object instance, MethodCall<object, object> invoker)
        {
            _instance = instance;
            _invoker = invoker;
        }

        public object Invoke(params object[] args)
        {
            object o = _invoker(_instance, args);

            return o;
        }
    }

    internal static class FSharpUtils
    {
        private static readonly object Lock = new object();

        private static bool _initialized;
        private static MethodInfo _ofSeq;
        private static Type _mapType;

        public static Assembly FSharpCoreAssembly { get; private set; }
        public static MethodCall<object, object> IsUnion { get; private set; }
        public static MethodCall<object, object> GetUnionCases { get; private set; }
        public static MethodCall<object, object> PreComputeUnionTagReader { get; private set; }
        public static MethodCall<object, object> PreComputeUnionReader { get; private set; }
        public static MethodCall<object, object> PreComputeUnionConstructor { get; private set; }
        public static Func<object, object> GetUnionCaseInfoDeclaringType { get; private set; }
        public static Func<object, object> GetUnionCaseInfoName { get; private set; }
        public static Func<object, object> GetUnionCaseInfoTag { get; private set; }
        public static MethodCall<object, object> GetUnionCaseInfoFields { get; private set; }

        public const string FSharpSetTypeName = "FSharpSet`1";
        public const string FSharpListTypeName = "FSharpList`1";
        public const string FSharpMapTypeName = "FSharpMap`2";

        public static void EnsureInitialized(Assembly fsharpCoreAssembly)
        {
            if (!_initialized)
            {
                lock (Lock)
                {
                    if (!_initialized)
                    {
                        FSharpCoreAssembly = fsharpCoreAssembly;

                        Type fsharpType = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.FSharpType");

                        MethodInfo isUnionMethodInfo = GetMethodWithNonPublicFallback(fsharpType, "IsUnion", BindingFlags.Public | BindingFlags.Static);
                        IsUnion = KeyValueTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(isUnionMethodInfo);

                        MethodInfo getUnionCasesMethodInfo = GetMethodWithNonPublicFallback(fsharpType, "GetUnionCases", BindingFlags.Public | BindingFlags.Static);
                        GetUnionCases = KeyValueTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(getUnionCasesMethodInfo);

                        Type fsharpValue = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.FSharpValue");

                        PreComputeUnionTagReader = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionTagReader");
                        PreComputeUnionReader = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionReader");
                        PreComputeUnionConstructor = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionConstructor");

                        Type unionCaseInfo = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.UnionCaseInfo");

                        GetUnionCaseInfoName = KeyValueTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("Name"));
                        GetUnionCaseInfoTag = KeyValueTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("Tag"));
                        GetUnionCaseInfoDeclaringType = KeyValueTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("DeclaringType"));
                        GetUnionCaseInfoFields = KeyValueTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(unionCaseInfo.GetMethod("GetFields"));

                        Type listModule = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.ListModule");
                        _ofSeq = listModule.GetMethod("OfSeq");

                        _mapType = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.FSharpMap`2");
                        
                        _initialized = true;
                    }
                }
            }
        }

        private static MethodInfo GetMethodWithNonPublicFallback(Type type, string methodName, BindingFlags bindingFlags)
        {
            MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags);

            // if no matching method then attempt to find with nonpublic flag
            // this is required because in WinApps some methods are private but always using NonPublic breaks medium trust
            // https://github.com/JamesNK/Newtonsoft.Json/pull/649
            // https://github.com/JamesNK/Newtonsoft.Json/issues/821
            if (methodInfo == null && (bindingFlags & BindingFlags.NonPublic) != BindingFlags.NonPublic)
            {
                methodInfo = type.GetMethod(methodName, bindingFlags | BindingFlags.NonPublic);
            }

            return methodInfo;
        }

        private static MethodCall<object, object> CreateFSharpFuncCall(Type type, string methodName)
        {
            MethodInfo innerMethodInfo = GetMethodWithNonPublicFallback(type, methodName, BindingFlags.Public | BindingFlags.Static);
            MethodInfo invokeFunc = innerMethodInfo.ReturnType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);

            MethodCall<object, object> call = KeyValueTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(innerMethodInfo);
            MethodCall<object, object> invoke = KeyValueTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(invokeFunc);

            MethodCall<object, object> createFunction = (target, args) =>
            {
                object result = call(target, args);

                FSharpFunction f = new FSharpFunction(result, invoke);
                return f;
            };

            return createFunction;
        }

        public static ObjectConstructor<object> CreateSeq(Type t)
        {
            MethodInfo seqType = _ofSeq.MakeGenericMethod(t);

            return KeyValueTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(seqType);
        }

        public static ObjectConstructor<object> CreateMap(Type keyType, Type valueType)
        {
            MethodInfo creatorDefinition = typeof(FSharpUtils).GetMethod("BuildMapCreator");

            MethodInfo creatorGeneric = creatorDefinition.MakeGenericMethod(keyType, valueType);

            return (ObjectConstructor<object>)creatorGeneric.Invoke(null, null);
        }

        public static ObjectConstructor<object> BuildMapCreator<TKey, TValue>()
        {
            Type genericMapType = _mapType.MakeGenericType(typeof(TKey), typeof(TValue));
            ConstructorInfo ctor = genericMapType.GetConstructor(new[] { typeof(IEnumerable<Tuple<TKey, TValue>>) });
            ObjectConstructor<object> ctorDelegate = KeyValueTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(ctor);

            ObjectConstructor<object> creator = args =>
            {
                // convert dictionary KeyValuePairs to Tuples
                IEnumerable<KeyValuePair<TKey, TValue>> values = (IEnumerable<KeyValuePair<TKey, TValue>>)args[0];
                IEnumerable<Tuple<TKey, TValue>> tupleValues = values.Select(kv => new Tuple<TKey, TValue>(kv.Key, kv.Value));

                return ctorDelegate(tupleValues);
            };

            return creator;
        }
    }
}