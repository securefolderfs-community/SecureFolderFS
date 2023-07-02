using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.Buffers
{
    /// <inheritdoc cref="BufferHolder"/>
    internal sealed class DirectoryIdBuffer : BufferHolder
    {
        public DirectoryIdBuffer(byte[] buffer)
            : base(buffer)
        {
        }
    }
}
