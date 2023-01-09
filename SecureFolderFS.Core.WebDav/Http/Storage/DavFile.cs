using NWebDav.Server.Storage;
using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.StorageProperties;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Storage
{
    /// <inheritdoc cref="IDavFile"/>
    internal sealed class DavFile : DavStorable<IDavFile, ILocatableFile>, IDavFile
    {
        /// <inheritdoc/>
        public string Path { get; }

        /// <inheritdoc/>
        protected override IDavFile Implementation => this;

        public DavFile(ILocatableFile storableInternal, IBasicProperties? properties)
            : base(storableInternal, properties)
        {
            Path = storableInternal.Path;
        }

        /// <inheritdoc/>
        public Task<Stream> OpenStreamAsync(FileAccess access, CancellationToken cancellationToken = default)
        {
            return StorableInternal.OpenStreamAsync(access, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<Stream> OpenStreamAsync(FileAccess access, FileShare share = FileShare.None, CancellationToken cancellationToken = default)
        {
            if (StorableInternal is IFileExtended fileExtended)
                return fileExtended.OpenStreamAsync(access, share, cancellationToken);

            return StorableInternal.OpenStreamAsync(access, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<ILocatableFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return StorableInternal.GetParentAsync(cancellationToken);
        }
    }
}
