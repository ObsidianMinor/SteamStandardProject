using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Specifies float format handling options when writing special floating point numbers, e.g. <see cref="Double.NaN"/>,
    /// <see cref="Double.PositiveInfinity"/> and <see cref="Double.NegativeInfinity"/> with <see cref="KeyValueWriter"/>.
    /// </summary>
    public enum FloatFormatHandling
    {
        /// <summary>
        /// Write special floating point values as strings in JSON, e.g. <c>"NaN"</c>, <c>"Infinity"</c>, <c>"-Infinity"</c>.
        /// </summary>
        String = 0,

        /// <summary>
        /// Write special floating point values as symbols in JSON, e.g. <c>NaN</c>, <c>Infinity</c>, <c>-Infinity</c>.
        /// Note that this will produce non-valid JSON.
        /// </summary>
        Symbol = 1,

        /// <summary>
        /// Write special floating point values as the property's default value in JSON, e.g. 0.0 for a <see cref="Double"/> property, <c>null</c> for a <see cref="Nullable{T}"/> of <see cref="Double"/> property.
        /// </summary>
        DefaultValue = 2
    }
}
