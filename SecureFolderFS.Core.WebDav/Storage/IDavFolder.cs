using SecureFolderFS.Core.WebDav.Enums;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <summary>
    /// Represents a WebDAV folder.
    /// </summary>
    internal interface IDavFolder : IDavStorable, ILocatableFolder, IModifiableFolder
    {
        EnumerationDepthMode DepthMode { get; }
    }
}
