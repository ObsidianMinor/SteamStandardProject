using Steam.KeyValues.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Represents a collection of <see cref="KVToken"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of token.</typeparam>
    public struct KVEnumerable<T> : IKVEnumerable<T>, IEquatable<KVEnumerable<T>> where T : KVToken
    {
        /// <summary>
        /// An empty collection of <see cref="KVToken"/> objects.
        /// </summary>
        public static readonly KVEnumerable<T> Empty = new KVEnumerable<T>(Enumerable.Empty<T>());

        private readonly IEnumerable<T> _enumerable;

        /// <summary>
        /// Initializes a new instance of the <see cref="KVEnumerable{T}"/> struct.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        public KVEnumerable(IEnumerable<T> enumerable)
        {
            ValidationUtils.ArgumentNotNull(enumerable, nameof(enumerable));

            _enumerable = enumerable;
        }

        /// <summary>
        /// Returns an enumerator that can be used to iterate through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return (_enumerable ?? Empty).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="IKVEnumerable{T}"/> of <see cref="KVToken"/> with the specified key.
        /// </summary>
        /// <value></value>
        public IKVEnumerable<KVToken> this[object key]
        {
            get
            {
                if (_enumerable == null)
                {
                    return KVEnumerable<KVToken>.Empty;
                }

                return new KVEnumerable<KVToken>(_enumerable.Values<T, KVToken>(key));
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="KVEnumerable{T}"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="KVEnumerable{T}"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="KVEnumerable{T}"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(KVEnumerable<T> other)
        {
            return Equals(_enumerable, other._enumerable);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is KVEnumerable<T>)
            {
                return Equals((KVEnumerable<T>)obj);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            if (_enumerable == null)
            {
                return 0;
            }

            return _enumerable.GetHashCode();
        }
    }
}
