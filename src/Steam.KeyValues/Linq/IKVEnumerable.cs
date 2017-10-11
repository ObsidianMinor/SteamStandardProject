using System.Collections.Generic;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Represents a collection of <see cref="KVToken"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of token.</typeparam>
    public interface IKVEnumerable<out T> : IEnumerable<T> where T : KVToken
    {
        /// <summary>
        /// Gets the <see cref="IKVEnumerable{T}"/> of <see cref="KVToken"/> with the specified key.
        /// </summary>
        /// <value></value>
        IKVEnumerable<KVToken> this[object key] { get; }
    }
}
