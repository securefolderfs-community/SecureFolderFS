using NWebDav.Server.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.StorageProperties;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <inheritdoc cref="IDavFile"/>
    /// <typeparam name="TCapability">An interface that represents capabilities of this file.</typeparam>
    internal sealed class DavFile<TCapability> : DavStorable<IDavFile, TCapability>, IDavFile
        where TCapability : IFile
    {
        /// <inheritdoc/>
        public string Path => StorableInternal.TryGetPath() ?? string.Empty;

        /// <inheritdoc/>
        protected override IDavFile Implementation => this;

        public DavFile(TCapability storableInternal, IBasicProperties? properties = null)
            : base(storableInternal, properties)
        {
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
        public async Task<ILocatableFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            if (StorableInternal is not ILocatableStorable locatableStorable)
                return null;

            var parentFolder = await locatableStorable.GetParentAsync(cancellationToken);
            if (parentFolder is null)
                return null;

            return new DavFolder<ILocatableFolder>(parentFolder);
        }
    }
}
