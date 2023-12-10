using System;
using System.Collections.Generic;

namespace SecureFolderFS.Shared.Utilities
{
    /// <summary>
    /// Represents a byte sequence of a key that can be disposed.
    /// </summary>
    public interface IKey : IEnumerable<byte>, IDisposable
    {
    }
}
