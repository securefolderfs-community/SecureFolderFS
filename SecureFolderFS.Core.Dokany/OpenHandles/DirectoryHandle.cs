namespace SecureFolderFS.Core.Dokany.OpenHandles
{
    /// <summary>
    /// Represents a directory handle on the virtual file system.
    /// </summary>
    internal sealed class DirectoryHandle : ObjectHandle
    {
        /// <inheritdoc/>
        public override void Dispose()
        {
            // TODO: Close hFolder handle?
        }
    }
}
