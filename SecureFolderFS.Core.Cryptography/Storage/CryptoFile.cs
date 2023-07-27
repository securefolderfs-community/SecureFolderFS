using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Sdk.Storage.NestedStorage;
using SecureFolderFS.Shared.Utils;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Cryptography.Storage
{
    public class CryptoFile : CryptoStorable<IFile>, IFileExtended, INestedFile
    {
        /// <inheritdoc/>
        public IFile Inner { get; }

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public CryptoFile(IFile inner)
            : base(inner)
        {
            Inner = inner;
        }

        /// <inheritdoc/>
        public virtual async Task<Stream> OpenStreamAsync(FileAccess access, CancellationToken cancellationToken = default)
        {
            var stream = await Inner.OpenStreamAsync(access, cancellationToken);
            return CreateStream(stream);
        }

        /// <inheritdoc/>
        public virtual async Task<Stream> OpenStreamAsync(FileAccess access, FileShare share, CancellationToken cancellationToken = default)
        {
            var stream = await GetStreamAsync();
            return CreateStream(stream);

            Task<Stream> GetStreamAsync()
            {
                if (Inner is IFileExtended fileExtended)
                    return fileExtended.OpenStreamAsync(access, share, cancellationToken);

                return Inner.OpenStreamAsync(access, cancellationToken);
            }
        }

        /// <summary>
        /// Creates encrypting stream instance that wraps <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The data stream to wrap.</param>
        /// <returns>An encrypting stream instance.</returns>
        protected virtual Stream CreateStream(Stream stream)
        {
            return streamsAccess.OpenCleartextStream(Inner.Id, stream);
        }
    }
}
