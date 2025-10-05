using System;
using System.Collections.Generic;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents a byte sequence of a key that can be disposed.
    /// </summary>
    public interface IKey : IEnumerable<byte>, IDisposable
    {
        /// <summary>
        /// Gets the number of bytes in the key.
        /// </summary>
        int Length { get; }
    }
}
