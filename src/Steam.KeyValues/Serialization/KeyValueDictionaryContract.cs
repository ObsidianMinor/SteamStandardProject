using Steam.KeyValues.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace Steam.KeyValues.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="System.Type"/> used by the <see cref="KeyValueSerializer"/>.
    /// </summary>
    public class KeyValueDictionaryContract : KeyValueContainerContract
    {
        /// <summary>
        /// Gets or sets the dictionary key resolver.
        /// </summary>
        /// <value>The dictionary key resolver.</value>
        public Func<string, string> DictionaryKeyResolver { get; set; }

        /// <summary>
        /// Gets the <see cref="System.Type"/> of the dictionary keys.
        /// </summary>
        /// <value>The <see cref="System.Type"/> of the dictionary keys.</value>
        public Type DictionaryKeyType { get; }

        /// <summary>
        /// Gets the <see cref="System.Type"/> of the dictionary values.
        /// </summary>
        /// <value>The <see cref="System.Type"/> of the dictionary values.</value>
        public Type DictionaryValueType { get; }

        internal KeyValueContract KeyContract { get; set; }

        private readonly Type _genericCollectionDefinitionType;

        private Type _genericWrapperType;
        private ObjectConstructor<object> _genericWrapperCreator;

        private Func<object> _genericTemporaryDictionaryCreator;

        internal bool ShouldCreateWrapper { get; }

        private readonly ConstructorInfo _parameterizedConstructor;

        private ObjectConstructor<object> _overrideCreator;
        private ObjectConstructor<object> _parameterizedCreator;

        internal ObjectConstructor<object> ParameterizedCreator
        {
            get
            {
                if (_parameterizedCreator == null)
                {
                    _parameterizedCreator = KeyValueTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(_parameterizedConstructor);
                }

                return _parameterizedCreator;
            }
        }

        /// <summary>
        /// Gets or sets the function used to create the object. When set this function will override <see cref="KeyValueContract.DefaultCreator"/>.
        /// </summary>
        /// <value>The function used to create the object.</value>
        public ObjectConstructor<object> OverrideCreator
        {
            get { return _overrideCreator; }
            set { _overrideCreator = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the creator has a parameter with the dictionary values.
        /// </summary>
        /// <value><c>true</c> if the creator has a parameter with the dictionary values; otherwise, <c>false</c>.</value>
        public bool HasParameterizedCreator { get; set; }

        internal bool HasParameterizedCreatorInternal
        {
            get { return (HasParameterizedCreator || _parameterizedCreator != null || _parameterizedConstructor != null); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueDictionaryContract"/> class.
        /// </summary>
        /// <param name="underlyingType">The underlying type for the contract.</param>
        public KeyValueDictionaryContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = KeyValueContractType.Dictionary;

            Type keyType;
            Type valueType;

            if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IDictionary<,>), out _genericCollectionDefinitionType))
            {
                keyType = _genericCollectionDefinitionType.GetGenericArguments()[0];
                valueType = _genericCollectionDefinitionType.GetGenericArguments()[1];

                if (ReflectionUtils.IsGenericDefinition(UnderlyingType, typeof(IDictionary<,>)))
                {
                    CreatedType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                }
                
                IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(underlyingType, typeof(ReadOnlyDictionary<,>));
            }

            else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IReadOnlyDictionary<,>), out _genericCollectionDefinitionType))
            {
                keyType = _genericCollectionDefinitionType.GetGenericArguments()[0];
                valueType = _genericCollectionDefinitionType.GetGenericArguments()[1];

                if (ReflectionUtils.IsGenericDefinition(UnderlyingType, typeof(IReadOnlyDictionary<,>)))
                {
                    CreatedType = typeof(ReadOnlyDictionary<,>).MakeGenericType(keyType, valueType);
                }

                IsReadOnlyOrFixedSize = true;
            }
            else
            {
                ReflectionUtils.GetDictionaryKeyValueTypes(UnderlyingType, out keyType, out valueType);

                if (UnderlyingType == typeof(IDictionary))
                {
                    CreatedType = typeof(Dictionary<object, object>);
                }
            }

            if (keyType != null && valueType != null)
            {
                _parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(
                    CreatedType,
                    typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType),
                    typeof(IDictionary<,>).MakeGenericType(keyType, valueType));
                
                if (!HasParameterizedCreatorInternal && underlyingType.Name == FSharpUtils.FSharpMapTypeName)
                {
                    FSharpUtils.EnsureInitialized(underlyingType.Assembly());
                    _parameterizedCreator = FSharpUtils.CreateMap(keyType, valueType);
                }
            }

            ShouldCreateWrapper = !typeof(IDictionary).IsAssignableFrom(CreatedType);

            DictionaryKeyType = keyType;
            DictionaryValueType = valueType;

            if (ImmutableCollectionsUtils.TryBuildImmutableForDictionaryContract(underlyingType, DictionaryKeyType, DictionaryValueType, out Type immutableCreatedType, out ObjectConstructor<object> immutableParameterizedCreator))
            {
                CreatedType = immutableCreatedType;
                _parameterizedCreator = immutableParameterizedCreator;
                IsReadOnlyOrFixedSize = true;
            }
        }

        internal IWrappedDictionary CreateWrapper(object dictionary)
        {
            if (_genericWrapperCreator == null)
            {
                _genericWrapperType = typeof(DictionaryWrapper<,>).MakeGenericType(DictionaryKeyType, DictionaryValueType);

                ConstructorInfo genericWrapperConstructor = _genericWrapperType.GetConstructor(new[] { _genericCollectionDefinitionType });
                _genericWrapperCreator = KeyValueTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(genericWrapperConstructor);
            }

            return (IWrappedDictionary)_genericWrapperCreator(dictionary);
        }

        internal IDictionary CreateTemporaryDictionary()
        {
            if (_genericTemporaryDictionaryCreator == null)
            {
                Type temporaryDictionaryType = typeof(Dictionary<,>).MakeGenericType(DictionaryKeyType ?? typeof(object), DictionaryValueType ?? typeof(object));

                _genericTemporaryDictionaryCreator = KeyValueTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(temporaryDictionaryType);
            }

            return (IDictionary)_genericTemporaryDictionaryCreator();
        }
    }
}
