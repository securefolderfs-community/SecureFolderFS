using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Storage.StorageProperties;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.FileShareOptions;
using SecureFolderFS.Storage.StorageProperties;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Storage
{
    /// <inheritdoc cref="IFile"/>
    public class CryptoFile : CryptoStorable<IFile>, IFileOpenShare, IChildFile, ICreatedAt, ILastModifiedAt, ISizeOf
    {
        /// <inheritdoc/>
        public ICreatedAtProperty CreatedAt => field ??= new CryptoCreatedAtProperty(Id, (Inner as ICreatedAt)?.CreatedAt);

        /// <inheritdoc/>
        public ILastModifiedAtProperty LastModifiedAt => field ??= new CryptoLastModifiedAtProperty(Id, (Inner as ILastModifiedAt)?.LastModifiedAt);

        /// <inheritdoc/>
        public ISizeOfProperty SizeOf => field ??= new CryptoSizeOfProperty(Id, specifics, (Inner as ISizeOf)?.SizeOf);

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

        public async Task<Stream> OpenStreamAsync(FileAccess accessMode, FileShare shareMode, CancellationToken cancellationToken = default)
        {
            if (specifics.Options.IsReadOnly && accessMode.HasFlag(FileAccess.Write))
                throw FileSystemExceptions.FileSystemReadOnly;

            var stream = await Inner.OpenStreamAsync(accessMode, shareMode, cancellationToken);
            if (stream.CanRead || stream is { CanSeek: true, Length: <= 0 })
                return CreatePlaintextStream(stream, null);

            var readingStream = await Inner.OpenStreamAsync(FileAccess.Read, shareMode, cancellationToken);
            return CreatePlaintextStream(stream, readingStream);
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
