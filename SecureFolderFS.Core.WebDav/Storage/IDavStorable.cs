using SecureFolderFS.Sdk.Storage.ExtendableStorage;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <summary>
    /// Represents a WebDAV storable object. This is the base interface for all WebDAV file system objects.
    /// </summary>
    internal interface IDavStorable : IStorableExtended
    {
    }
}
