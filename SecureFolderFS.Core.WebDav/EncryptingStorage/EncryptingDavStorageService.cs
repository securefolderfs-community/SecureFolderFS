using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage
{
    internal sealed class EncryptingDavStorageService : DavStorageService
    {
        public EncryptingDavStorageService(ILocatableFolder baseDirectory, IStorageService storageService)
            : base(baseDirectory, storageService)
        {
        }

        /// <inheritdoc/>
        public override Task<ILocatableFile> GetFileFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            return base.GetFileFromPathAsync(path, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<ILocatableFolder> GetFolderFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            return base.GetFolderFromPathAsync(path, cancellationToken);
        }
    }
}
