using Steam.KeyValues.Linq;
using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Converts an object to and from JSON.
    /// </summary>
    public abstract class KeyValueConverter
    {
        /// <summary>
        /// Writes the KeyValue representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="KeyValueWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public abstract void WriteKeyValue(KeyValueWriter writer, object value, KeyValueSerializer serializer);

        /// <summary>
        /// Reads the KeyValue representation of the object.
        /// </summary>
        /// <param name="readValue">The <see cref="KVToken"/> to read from</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public abstract object ReadKeyValue(KVToken readValue, Type objectType, object existingValue, KeyValueSerializer serializer);

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool CanConvert(Type objectType);

        /// <summary>
        /// Gets a value indicating whether this <see cref="KeyValueConverter"/> can read KeyValues.
        /// </summary>
        /// <value><c>true</c> if this <see cref="KeyValueConverter"/> can read KeyValues; otherwise, <c>false</c>.</value>
        public virtual bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="KeyValueConverter"/> can write KeyValues.
        /// </summary>
        /// <value><c>true</c> if this <see cref="KeyValueConverter"/> can write KeyValues; otherwise, <c>false</c>.</value>
        public virtual bool CanWrite
        {
            get { return true; }
        }
    }
}
