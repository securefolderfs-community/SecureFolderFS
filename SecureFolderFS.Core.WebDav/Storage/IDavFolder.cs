using NWebDav.Server.Stores;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <summary>
    /// Represents a WebDAV folder.
    /// </summary>
    internal interface IDavFolder : IDavStorable, ILocatableFolder, IModifiableFolder
    {
        /// <summary>
        /// Gets the depth mode for enumerating directory contents.
        /// </summary>
        InfiniteDepthMode DepthMode { get; }
    }
}
