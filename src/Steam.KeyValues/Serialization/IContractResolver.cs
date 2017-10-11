using System;

namespace Steam.KeyValues.Serialization
{
    /// <summary>
    /// Used by <see cref="KeyValueSerializer"/> to resolve a <see cref="KeyValueContract"/> for a given <see cref="Type"/>.
    /// </summary>
    /// <example>
    ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeContractResolverObject" title="IContractResolver Class" />
    ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeContractResolverExample" title="IContractResolver Example" />
    /// </example>
    public interface IContractResolver
    {
        /// <summary>
        /// Resolves the contract for a given type.
        /// </summary>
        /// <param name="type">The type to resolve a contract for.</param>
        /// <returns>The contract for a given type.</returns>
        KeyValueContract ResolveContract(Type type);
    }
}
