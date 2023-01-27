using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage
{
    internal sealed class EncryptingDavStorageService : DavStorageService
    {
        private readonly IStreamsAccess _streamsAccess;
        private readonly IPathConverter _pathConverter;

        public EncryptingDavStorageService(ILocatableFolder baseDirectory, IStorageService storageService, IStreamsAccess streamsAccess, IPathConverter pathConverter)
            : base(baseDirectory, storageService)
        {
            _streamsAccess = streamsAccess;
            _pathConverter = pathConverter;
        }

        /// <inheritdoc/>
        public override DavFile<T> NewFile<T>(T inner)
        {
            return new EncryptingDavFile<T>(inner, _streamsAccess, _pathConverter);
        }

        /// <inheritdoc/>
        public override DavFolder<T> NewFolder<T>(T inner)
        {
            return new EncryptingDavFolder<T>(inner, _streamsAccess, _pathConverter);
        }

        /// <inheritdoc/>
        protected override string GetPathFromUri(string uriPath)
        {
            var path = base.GetPathFromUri(uriPath);
            return _pathConverter.ToCiphertext(path) ?? throw new CryptographicException("Couldn't convert to ciphertext path.");
        }
    }
}
