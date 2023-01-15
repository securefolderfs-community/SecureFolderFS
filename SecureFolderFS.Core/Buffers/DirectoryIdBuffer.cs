using SecureFolderFS.Shared.Helpers;
using System;

namespace SecureFolderFS.Core.Buffers
{
    /// <inheritdoc cref="BufferHolder"/>
    internal sealed class DirectoryIdBuffer : BufferHolder
    {
        /// <summary>
        /// Gets a singleton instance of <see cref="DirectoryIdBuffer"/> which is empty.
        /// </summary>
        public static DirectoryIdBuffer Empty { get; } = new(Array.Empty<byte>());

        /// <summary>
        /// Creates a new directory ID with unique GUID.
        /// </summary>
        /// <returns>A new directory ID.</returns>
        public static DirectoryIdBuffer CreateNew()
        {
            return new DirectoryIdBuffer(Guid.NewGuid().ToByteArray());
        }

        public DirectoryIdBuffer(byte[] buffer)
            : base(buffer)
        {
        }
    }
}
