using System;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Represents a KeyValue root key
    /// </summary>
    public partial class KVRoot : KVProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KVRoot"/> class
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public KVRoot(string name, object value) : base(name, value)
        {
        }

        /// <summary>
        /// Gets or sets the base file declaration
        /// </summary>
        public string BaseFile { get; set; }

        /// <summary>
        /// Gets or sets the root object value
        /// </summary>
        public override KVToken Value
        {
            get => base.Value;
            set
            {
                if (!(value is KVObject))
                    throw new ArgumentException("Cannot set root value to something other than a KVObject");

                base.Value = value ?? new KVObject();
            }
        }

        public override KVTokenType Type => KVTokenType.Property;

        public override void WriteTo(KeyValueWriter writer, params KeyValueConverter[] converters)
        {
            if (string.IsNullOrWhiteSpace(BaseFile))
            {
                writer.WriteToken(KeyValueToken.Base);
                writer.WriteValue(BaseFile);
            }

            base.WriteTo(writer, converters);
        }
    }
}
