using System.Globalization;
using System.IO;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Represents a raw KeyValue string.
    /// </summary>
    public partial class KVRaw : KVValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KVRaw"/> class from another <see cref="KVRaw"/> object.
        /// </summary>
        /// <param name="other">A <see cref="KVRaw"/> object to copy from.</param>
        public KVRaw(KVRaw other)
            : base(other)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KVRaw"/> class.
        /// </summary>
        /// <param name="rawKeyValue">The raw KeyValue.</param>
        public KVRaw(object rawKeyValue)
            : base(rawKeyValue, KVTokenType.Raw)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="KVRaw"/> with the content of the reader's current token.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>An instance of <see cref="KVRaw"/> with the content of the reader's current token.</returns>
        public static KVRaw Create(KeyValueReader reader)
        {
            using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
            using (KeyValueTextWriter KeyValueWriter = new KeyValueTextWriter(sw))
            {
                KeyValueWriter.WriteToken(reader);

                return new KVRaw(sw.ToString());
            }
        }

        internal override KVToken CloneToken()
        {
            return new KVRaw(this);
        }
    }
}
