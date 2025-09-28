using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Storage.StorageProperties;
using SecureFolderFS.Storage.StorageProperties;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
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
            if (specifics.Options.IsReadOnly && access.HasFlag(FileAccess.Write))
                throw FileSystemExceptions.FileSystemReadOnly;

            var stream = await Inner.OpenStreamAsync(access, cancellationToken);
            if (stream.CanRead || stream is { CanSeek: true, Length: <= 0 })
                return CreatePlaintextStream(stream, null);

            await using var readingStream = await Inner.OpenReadAsync(cancellationToken);
            return CreatePlaintextStream(stream, readingStream);
        }

        /// <inheritdoc/>
        public override async Task<IBasicProperties> GetPropertiesAsync()
        {
            if (Inner is not IStorableProperties storableProperties)
                throw new NotSupportedException($"Properties on {nameof(CryptoFile)}.{nameof(Inner)} are not supported.");

            var innerProperties = await storableProperties.GetPropertiesAsync();
            properties ??= new CryptoFileProperties(specifics, innerProperties);

            return properties;
        }

        /// <summary>
        /// Creates encrypting stream instance that wraps <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The data stream to wrap.</param>
        /// <param name="readingStream">The additional reading access stream.</param>
        /// <returns>An encrypting <see cref="Stream"/> instance.</returns>
        protected virtual Stream CreatePlaintextStream(Stream stream, Stream? readingStream)
        {
            return specifics.StreamsAccess.OpenPlaintextStream(Inner.Id, stream, readingStream);
        }
    }
}
