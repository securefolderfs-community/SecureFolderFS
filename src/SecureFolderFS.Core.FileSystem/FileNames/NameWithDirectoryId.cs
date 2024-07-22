using System;

namespace SecureFolderFS.Core.FileSystem.FileNames
{
    public sealed class NameWithDirectoryId : IEquatable<NameWithDirectoryId> // TODO: Maybe convert to struct?
    {
        public byte[] DirectoryId { get; }

        public string FileName { get; }

        public NameWithDirectoryId(byte[] directoryId, string fileName)
        {
            DirectoryId = directoryId;
            FileName = fileName;
        }

        /// <inheritdoc/>
        public bool Equals(NameWithDirectoryId? other)
        {
            if (other is null)
                return false;

            return DirectoryId.AsSpan() == other.DirectoryId.AsSpan() && FileName == other.FileName;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash *= 23 + FileName.GetHashCode();
                hash *= 23 + ComputeHash(DirectoryId);

                return hash;
            }
        }

        private static int ComputeHash(ReadOnlySpan<byte> data)
        {
            unchecked
            {
                const int p = 16777619;
                var hash = (int)2166136261;

                for (var i = 0; i < data.Length; i++)
                    hash = (hash ^ data[i]) * p;

                return hash;
            }
        }
    }
}
