using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Steam.KeyValues.Serialization
{
    /// <summary>
    /// A collection of <see cref="KeyValueProperty"/> objects.
    /// </summary>
    public class KeyValuePropertyCollection : KeyedCollection<(string, Conditional), KeyValueProperty>
    {
        private readonly Type _type;
        private readonly List<KeyValueProperty> _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePropertyCollection"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public KeyValuePropertyCollection(Type type)
            : base(KeyValuePropertyComparer.Instance)
        {
            ValidationUtils.ArgumentNotNull(type, "type");
            _type = type;

            // foreach over List<T> to avoid boxing the Enumerator
            _list = (List<KeyValueProperty>)Items;
        }

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override (string, Conditional) GetKeyForItem(KeyValueProperty item)
        {
            return (item.PropertyName, item.Condition);
        }

        /// <summary>
        /// Adds a <see cref="KeyValueProperty"/> object.
        /// </summary>
        /// <param name="property">The property to add to the collection.</param>
        public void AddProperty(KeyValueProperty property)
        {
            if (Contains((property.PropertyName, property.Condition)))
            {
                // don't overwrite existing property with ignored property
                if (property.Ignored)
                {
                    return;
                }

                KeyValueProperty existingProperty = this[(property.PropertyName, property.Condition)];
                bool duplicateProperty = true;

                if (existingProperty.Ignored)
                {
                    // remove ignored property so it can be replaced in collection
                    Remove(existingProperty);
                    duplicateProperty = false;
                }
                else
                {
                    if (property.DeclaringType != null && existingProperty.DeclaringType != null)
                    {
                        if (property.DeclaringType.IsSubclassOf(existingProperty.DeclaringType)
                            || (existingProperty.DeclaringType.IsInterface() && property.DeclaringType.ImplementInterface(existingProperty.DeclaringType)))
                        {
                            // current property is on a derived class and hides the existing
                            Remove(existingProperty);
                            duplicateProperty = false;
                        }
                        if (existingProperty.DeclaringType.IsSubclassOf(property.DeclaringType)
                            || (property.DeclaringType.IsInterface() && existingProperty.DeclaringType.ImplementInterface(property.DeclaringType)))
                        {
                            // current property is hidden by the existing so don't add it
                            return;
                        }
                    }
                }

                if (duplicateProperty)
                {
                    throw new KeyValueSerializationException("A member with the name '{0}' already exists on '{1}'. Use the KeyValuePropertyAttribute to specify another name.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, _type));
                }
            }

            Add(property);
        }

        /// <summary>
        /// Gets the closest matching <see cref="KeyValueProperty"/> object.
        /// First attempts to get an exact case match of <paramref name="propertyName"/> with condition and then
        /// a case insensitive match with condition, then the same search without conditions
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="condition">The condition of the property</param>
        /// <returns>A matching property if found.</returns>
        public KeyValueProperty GetClosestMatchProperty(string propertyName, Conditional condition)
        {
            return GetProperty(propertyName, StringComparison.Ordinal, condition)
                ?? GetProperty(propertyName, StringComparison.OrdinalIgnoreCase, condition)
                ?? GetProperty(propertyName, StringComparison.Ordinal) // slightly slow
                ?? GetProperty(propertyName, StringComparison.OrdinalIgnoreCase); // slightly slow
        }
        
        private bool TryGetValue(string key, Conditional key2, out KeyValueProperty item)
        {
            if (Dictionary == null)
            {
                item = default(KeyValueProperty);
                return false;
            }

            return Dictionary.TryGetValue((key, key2), out item);
        }

        /// <summary>
        /// Gets a property by property name and condition
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <param name="comparisonType">Type property name string comparison.</param>
        /// <param name="condition">The condition for this property</param>
        /// <returns>A matching property if found.</returns>
        public KeyValueProperty GetProperty(string propertyName, StringComparison comparisonType, Conditional condition)
        {
            // KeyedCollection has an ordinal comparer
            if (comparisonType == StringComparison.Ordinal)
            {
                if(TryGetValue(propertyName, condition, out KeyValueProperty property))
                {
                    return property;
                }

                return null;
            }

            for (int i = 0; i < _list.Count; i++)
            {
                KeyValueProperty property = _list[i];
                if (string.Equals(propertyName, property.PropertyName, comparisonType) && property.Condition == condition)
                {
                    return property;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a property by property name
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <param name="comparisonType">Type property name string comparison.</param>
        /// <returns>A matching property if found.</returns>
        public KeyValueProperty GetProperty(string propertyName, StringComparison comparisonType)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                KeyValueProperty property = _list[i];
                if (string.Equals(propertyName, property.PropertyName, StringComparison.Ordinal) || string.Equals(propertyName, property.PropertyName, comparisonType))
                {
                    return property;
                }
            }

            return null;
        }
    }

    internal class KeyValuePropertyComparer : IEqualityComparer<(string, Conditional)>
    {
        internal static KeyValuePropertyComparer Instance = new KeyValuePropertyComparer();

        public bool Equals((string, Conditional) x, (string, Conditional) y)
        {
            return x.Item1.Equals(y.Item1, StringComparison.Ordinal) && x.Item2 == y.Item2;
        }

        public int GetHashCode((string, Conditional) obj)
        {
            return obj.Item1.GetHashCode() + obj.Item2.GetHashCode(); // todo: check if this can't overflow
        }
    }
}
