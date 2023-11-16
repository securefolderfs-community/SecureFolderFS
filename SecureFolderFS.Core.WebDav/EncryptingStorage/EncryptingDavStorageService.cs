using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage
{
    internal sealed class EncryptingDavStorageService : DavStorageService
    {
        private readonly IStreamsAccess _streamsAccess;
        private readonly IPathConverter _pathConverter;
        private readonly DirectoryIdCache _directoryIdCache;

        public EncryptingDavStorageService(ILocatableFolder baseDirectory, IStorageService storageService, IStreamsAccess streamsAccess, IPathConverter pathConverter, DirectoryIdCache directoryIdCache, string? remoteRootDirectory = null)
            : base(baseDirectory, storageService, remoteRootDirectory)
        {
            _streamsAccess = streamsAccess;
            _pathConverter = pathConverter;
            _directoryIdCache = directoryIdCache;
        }

        /// <inheritdoc/>
        public override DavFile<T> NewFile<T>(T inner)
        {
            return new EncryptingDavFile<T>(inner, _streamsAccess, _pathConverter, _directoryIdCache);
        }

        /// <inheritdoc/>
        public override DavFolder<T> NewFolder<T>(T inner)
        {
            return new EncryptingDavFolder<T>(inner, _streamsAccess, _pathConverter, _directoryIdCache);
        }

        /// <inheritdoc/>
        protected override string GetPathFromUri(string uriPath)
        {
            var path = base.GetPathFromUri(uriPath);
            return _pathConverter.ToCiphertext(path) ?? throw new CryptographicException("Couldn't convert to ciphertext path.");
        }
    }
}
