using OwlCore.Storage;
using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Cryptography.Storage
{
    public class CryptoFile : CryptoStorable<IFile>, IChildFile
    {
        public CryptoFile(IFile inner, IStreamsAccess streamsAccess, IPathConverter pathConverter, DirectoryIdCache directoryIdCache)
            : base(inner, streamsAccess, pathConverter, directoryIdCache)
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
            return streamsAccess.OpenCleartextStream(Inner.Id, stream);
        }
    }
}
