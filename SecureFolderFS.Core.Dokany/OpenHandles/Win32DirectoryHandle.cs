using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.Dokany.OpenHandles
{
    /// <inheritdoc cref="DirectoryHandle"/>
    internal sealed class Win32DirectoryHandle : DirectoryHandle
    {
        /// <inheritdoc/>
        public override void Dispose()
        {
            // TODO: Close hFolder handle?
        }
    }
}
