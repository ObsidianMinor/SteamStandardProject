using System;

namespace Steam.KeyValues.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="Type"/> used by the <see cref="KeyValueSerializer"/>.
    /// </summary>
    public class KeyValueContainerContract : KeyValueContract
    {
        private KeyValueContract _itemContract;
        private KeyValueContract _finalItemContract;

        // will be null for containers that don't have an item type (e.g. IList) or for complex objects
        internal KeyValueContract ItemContract
        {
            get { return _itemContract; }
            set
            {
                _itemContract = value;
                if (_itemContract != null)
                {
                    _finalItemContract = (_itemContract.UnderlyingType.IsSealed) ? _itemContract : null;
                }
                else
                {
                    _finalItemContract = null;
                }
            }
        }

        // the final (i.e. can't be inherited from like a sealed class or valuetype) item contract
        internal KeyValueContract FinalItemContract
        {
            get { return _finalItemContract; }
        }

        /// <summary>
        /// Gets or sets the default collection items <see cref="KeyValueConverter" />.
        /// </summary>
        /// <value>The converter.</value>
        public KeyValueConverter ItemConverter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the collection items preserve object references.
        /// </summary>
        /// <value><c>true</c> if collection items preserve object references; otherwise, <c>false</c>.</value>
        public bool? ItemIsReference { get; set; }

        /// <summary>
        /// Gets or sets the collection item reference loop handling.
        /// </summary>
        /// <value>The reference loop handling.</value>
        public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

        /// <summary>
        /// Gets or sets the collection item type name handling.
        /// </summary>
        /// <value>The type name handling.</value>
        public TypeNameHandling? ItemTypeNameHandling { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueContainerContract"/> class.
        /// </summary>
        /// <param name="underlyingType">The underlying type for the contract.</param>
        internal KeyValueContainerContract(Type underlyingType)
            : base(underlyingType)
        {
            KeyValueContainerAttribute KeyValueContainerAttribute = KeyValueTypeReflector.GetCachedAttribute<KeyValueContainerAttribute>(underlyingType);

            if (KeyValueContainerAttribute != null)
            {
                if (KeyValueContainerAttribute.ItemConverterType != null)
                {
                    ItemConverter = KeyValueTypeReflector.CreateKeyValueConverterInstance(
                        KeyValueContainerAttribute.ItemConverterType,
                        KeyValueContainerAttribute.ItemConverterParameters);
                }

                ItemIsReference = KeyValueContainerAttribute._itemIsReference;
                ItemReferenceLoopHandling = KeyValueContainerAttribute._itemReferenceLoopHandling;
                ItemTypeNameHandling = KeyValueContainerAttribute._itemTypeNameHandling;
            }
        }
    }
}
