using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <summary>
    /// Represents a WebDAV file.
    /// </summary>
    internal interface IDavFile : IDavStorable, ILocatableFile, IFileExtended // TODO: Maybe split addressability into a separate IDav.. interface?
    {
    }
}
