using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Specifies the state of the <see cref="KeyValueWriter"/>.
    /// </summary>
    public enum WriteState
    {
        /// <summary>
        /// An exception has been thrown, which has left the <see cref="KeyValueWriter"/> in an invalid state.
        /// You may call the <see cref="KeyValueWriter.Close()"/> method to put the <see cref="KeyValueWriter"/> in the <c>Closed</c> state.
        /// Any other <see cref="KeyValueWriter"/> method calls result in an <see cref="InvalidOperationException"/> being thrown.
        /// </summary>
        Error = 0,

        /// <summary>
        /// The <see cref="KeyValueWriter.Close()"/> method has been called.
        /// </summary>
        Closed = 1,

        /// <summary>
        /// An object is being written. 
        /// </summary>
        Object = 2,

        /// <summary>
        /// A property is being written.
        /// </summary>
        Property = 3,

        /// <summary>
        /// A <see cref="KeyValueWriter"/> write method has not been called.
        /// </summary>
        Start = 4
    }
}
