using System;

namespace SecureFolderFS.Core.FileSystem.Directories
{
    /// <summary>
    /// Represents an ID of directory.
    /// </summary>
    /// <param name="Id">Byte GUID representation of directory id.</param>
    public sealed record DirectoryId(byte[] Id)
    {
        /// <summary>
        /// Gets a <see cref="DirectoryId"/> which is empty.
        /// </summary>
        public static DirectoryId Empty { get; } = new(Array.Empty<byte>());

        /// <summary>
        /// Creates a new directory ID with unique GUID.
        /// </summary>
        /// <returns>A new directory ID.</returns>
        public static DirectoryId CreateNew()
        {
            return new DirectoryId(Guid.NewGuid().ToByteArray());
        }

        public static implicit operator ReadOnlySpan<byte>(DirectoryId directoryId) => directoryId.Id;
        public static implicit operator ReadOnlyMemory<byte>(DirectoryId directoryId) => directoryId.Id;
    }
}
