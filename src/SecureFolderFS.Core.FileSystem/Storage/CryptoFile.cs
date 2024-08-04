using OwlCore.Storage;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Storage
{
    /// <inheritdoc cref="IFile"/>
    public class CryptoFile : CryptoStorable<IFile>, IChildFile
    {
        public CryptoFile(string plaintextId, IFile inner, FileSystemSpecifics specifics, CryptoFolder? parent = null)
            : base(plaintextId, inner, specifics, parent)
        {
        }

        /// <inheritdoc/>
        public virtual async Task<Stream> OpenStreamAsync(FileAccess access, CancellationToken cancellationToken = default)
        {
            var stream = await Inner.OpenStreamAsync(access, cancellationToken);
            return CreateCleartextStream(stream);
        }

        /// <summary>
        /// Creates encrypting stream instance that wraps <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The data stream to wrap.</param>
        /// <returns>An encrypting <see cref="Stream"/> instance.</returns>
        protected virtual Stream CreateCleartextStream(Stream stream)
        {
            return specifics.StreamsAccess.OpenPlaintextStream(Inner.Id, stream);
        }
    }
}
