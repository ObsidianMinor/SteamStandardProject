using System;

namespace Steam.KeyValues.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="Type"/> used by the <see cref="KeyValueSerializer"/>
    /// </summary>
    public class KeyValueStringContract : KeyValuePrimitiveContract
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueStringContract"/> class.
        /// </summary>
        /// <param name="underlyingType"></param>
        public KeyValueStringContract(Type underlyingType) : base(underlyingType)
        {
            ContractType = KeyValueContractType.String;
        }
    }
}
