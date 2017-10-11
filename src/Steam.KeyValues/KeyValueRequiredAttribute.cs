using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Instructs the <see cref="JsonSerializer"/> to always serialize the member, and to require that the member has a value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class KeyValueRequiredAttribute : Attribute
    {
    }
}
