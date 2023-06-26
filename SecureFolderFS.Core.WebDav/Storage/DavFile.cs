using NWebDav.Server.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.StorageProperties;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <inheritdoc cref="IDavFile"/>
    /// <typeparam name="TCapability">An interface that represents capabilities of this file.</typeparam>
    internal class DavFile<TCapability> : DavStorable<IDavFile, TCapability>, IDavFile
        where TCapability : IFile
    {
        /// <inheritdoc/>
        public virtual string Path => Inner.TryGetPath() ?? string.Empty;

        /// <inheritdoc/>
        protected override IDavFile Implementation => this;

        public DavFile(TCapability inner)
            : base(inner)
        {
        }

        public DavFile(TCapability inner, IBasicProperties? properties = null)
            : base(inner, properties)
        {
        }

        /// <inheritdoc/>
        public virtual Task<Stream> OpenStreamAsync(FileAccess access, CancellationToken cancellationToken = default)
        {
            return Inner.OpenStreamAsync(access, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task<Stream> OpenStreamAsync(FileAccess access, FileShare share = FileShare.None, CancellationToken cancellationToken = default)
        {
            if (Inner is IFileExtended fileExtended)
                return fileExtended.OpenStreamAsync(access, share, cancellationToken);

            return Inner.OpenStreamAsync(access, cancellationToken);
        }
    }
}
